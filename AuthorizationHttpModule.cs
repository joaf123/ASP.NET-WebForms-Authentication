using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Compilation;
using System.Web.Security;
using System.Web.UI;

/// <summary>
/// Verify authorization on requests to AJAX WebServices or Pages.
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequiresAuthenticationAttribute : Attribute { }

/// <summary>
/// Attribute based forms authentication verification module.
/// </summary>
public class AttributeBasedFormsAuthenticationModule : IHttpModule {
    /// <summary>
    /// Inits the AttributeBasedFormsAuthentication Module.
    /// </summary>
    /// <param name="application">HttpApplication Parameter</param>
    public void Init(HttpApplication application) {
        application.PostMapRequestHandler += OnPostAuthorizeRequest;
    }

    /// <summary>
    /// Disposes the AttributeBasedFormsAuthentication Module
    /// </summary>
    public void Dispose() {
        // Clean up resources, if any
    }

    /// <summary>
    /// Authorize request.
    /// </summary>
    /// <param name="sender">Sender Parameter</param>
    /// <param name="e">EventArgs Parameter</param>
    private void OnPostAuthorizeRequest(object sender, EventArgs e) {
        var app = (HttpApplication)sender;
        var context = app.Context;
        var request = context.Request;
        var requestUrl = context.Request.Url.ToString();

            if (context.Handler is Page page) {
            var pageType = page.GetType();
            if (!requestUrl.Contains(".axd?")) {
                var masterPagePath = GetMasterPagePathFromMarkup(page.AppRelativeVirtualPath);
                if (!string.IsNullOrEmpty(masterPagePath)) {
                    var masterPageType = BuildManager.GetCompiledType(masterPagePath);
                    if (masterPageType != null && masterPageType.GetCustomAttribute<RequiresAuthenticationAttribute>() != null) {
                        if (!request.IsAuthenticated || request.Cookies?[FormsAuthentication.FormsCookieName] == null) {
                            DenyAccess(context);
                            return;
                        }
                    }
                }
            }

            if (pageType.GetCustomAttribute<RequiresAuthenticationAttribute>() != null) {
                if (!request.IsAuthenticated || request.Cookies?[FormsAuthentication.FormsCookieName] == null) {
                    DenyAccess(context);
                    return;
                }
            }

            if (request.HttpMethod == "POST") {
                var methodName = GetWebMethodNameFromRequest(request);
                if (!string.IsNullOrEmpty(methodName)) {
                    var methodInfo = pageType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    if (methodInfo?.GetCustomAttribute<RequiresAuthenticationAttribute>() != null) {
                        if (!request.IsAuthenticated || request.Cookies?[FormsAuthentication.FormsCookieName] == null) {
                            DenyAccess(context);
                        }
                    }
                }
            }

        } else if (requestUrl.Contains(".asmx/")) {
            //Prøvd å hente Type via Handler._privateSomething

            var segments = requestUrl.Split(new[] { ".asmx/" }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 1) {
                string methodName = segments[1]; // Extract the part after .asmx/ as the method name
                string asmxUrlWithoutMethod = requestUrl.Replace("/" + methodName, ""); // Remove the method name from the URL

                var codeBehindPath = GetCodeBehindPathFromASMXUrl(asmxUrlWithoutMethod); 

                if (!string.IsNullOrEmpty(codeBehindPath)) {
                    dynamic _handler = context.CurrentHandler;
                    dynamic handlerWrapper = context.Handler;

                    if (handlerWrapper != null) {
                        // Retrieve original handler and its metadata
                        var originalHandlerField = handlerWrapper.GetType().GetField("_originalHandler", BindingFlags.NonPublic | BindingFlags.Instance);
                        var originalHandler = originalHandlerField?.GetValue(handlerWrapper) as dynamic;

                        if (originalHandler != null) {
                            var webServiceMethodDataField = originalHandler.GetType().GetField("_webServiceMethodData", BindingFlags.NonPublic | BindingFlags.Instance);
                            var webServiceMethodData = webServiceMethodDataField?.GetValue(originalHandler) as dynamic;

                            if (webServiceMethodData != null) {
                                var ownerField = webServiceMethodData.GetType().GetField("_owner", BindingFlags.NonPublic | BindingFlags.Instance);
                                var owner = ownerField?.GetValue(webServiceMethodData) as dynamic;

                                if (owner != null) {
                                    var typeDataField = owner.GetType().GetField("_typeData", BindingFlags.NonPublic | BindingFlags.Instance);
                                    var typeData = typeDataField?.GetValue(owner) as dynamic;

                                    if (typeData != null) {
                                        var actualTypeField = typeData.GetType().GetField("_actualType", BindingFlags.NonPublic | BindingFlags.Instance);
                                        var actualType = actualTypeField?.GetValue(typeData) as dynamic;

                                        var attributes = actualType.GetCustomAttributes(typeof(RequiresAuthenticationAttribute), true);
                                        bool requiresAuthentication = attributes.Length > 0;

                                        // Check if authentication is required for the main class
                                        if (requiresAuthentication) {
                                            if (!request.IsAuthenticated || request.Cookies?[FormsAuthentication.FormsCookieName] == null) {
                                                DenyAccess(context);
                                                return;
                                            }
                                        }

                                        // Check for authentication attribute on the requested method
                                        if (!requiresAuthentication) {
                                            var methodInfo = actualType.GetMethod(methodName);
                                            if (methodInfo != null) {
                                                var methodAttributes = methodInfo.GetCustomAttributes(typeof(RequiresAuthenticationAttribute), true);
                                                bool methodRequiresAuthentication = methodAttributes.Length > 0;


                                                if (methodRequiresAuthentication) {
                                                    if (!request.IsAuthenticated || request.Cookies?[FormsAuthentication.FormsCookieName] == null) {
                                                        DenyAccess(context);
                                                        return;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private string GetCodeBehindPathFromASMXUrl(string asmxUrl) {
        string relativePath = GetRelativeVirtualPath(asmxUrl);
        if (!string.IsNullOrEmpty(relativePath)) {
            string asmxMarkup = File.ReadAllText(HttpContext.Current.Server.MapPath(relativePath));

            Match match = Regex.Match(asmxMarkup, @"<%@ WebService Language=""VB"" CodeBehind=""(.*?)"" Class=""(.*?)"" %>");
            if (match.Success && match.Groups.Count > 1) {
                string codeBehindPath = match.Groups[1].Value;
                return codeBehindPath;
            }
        }

        return string.Empty;
    }

    private string GetRelativeVirtualPath(string absolutePath) {
        Uri uri = new Uri(absolutePath);
        string rootPath = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
        if (uri.AbsoluteUri.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase)) {
            return "~" + uri.AbsolutePath.Replace("\\", "/");
        }
        return string.Empty;
    }

    private string GetMasterPagePathFromMarkup(string virtualPath) {
        string markup = File.ReadAllText(HttpContext.Current.Server.MapPath(virtualPath));
        using (StringReader reader = new StringReader(markup)) {
            string line;
            while ((line = reader.ReadLine()) != null) {
                Match match = Regex.Match(line, "MasterPageFile\\s*=\\s*\"(.+?)\"");
                if (match.Success) {
                    return match.Groups[1].Value;
                }
            }
        }
        return null;
    }

    /// <summary>
    /// Deny access.
    /// </summary>
    /// <param name="context">The context.</param>
    private static void DenyAccess(HttpContext context) {
        context.Response.StatusCode = 401;
        context.Response.SuppressContent = true;
        context.Response.End();
    }

    /// <summary>
    /// Gets the web method name from request.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <returns>WebMethod name as string.</returns>
    private static string GetWebMethodNameFromRequest(HttpRequest request) {
        var pathInfo = request.PathInfo.TrimStart('/');
        var slashIndex = pathInfo.IndexOf('/');
        return slashIndex >= 0 ? pathInfo.Substring(0, slashIndex) : pathInfo;
    }
}