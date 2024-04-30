using System;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Compilation;
using System.Web.Security;
using System.Web.UI;

/// <summary>
/// Verify authentication on call to WebMethods or Pages with the attribute: &lt;RequiresAuthentication&gt; (VB) | [RequiresAuthentication] (C#).
/// </summary>
[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequiresAuthenticationAttribute : Attribute { }

/// <summary>
/// Attribute based forms authentication verification module.
/// </summary>
public class AttributeBasedFormsAuthenticationModule : IHttpModule {
    public static bool useAuthentication = false;
    /// <summary>
    /// Inits the AttributeBasedFormsAuthentication Module.
    /// </summary>
    /// <param name="application">HttpApplication Parameter</param>
    public void Init(HttpApplication application) {
        // Check if it should be initialized
        if (useAuthentication) {
            // Your initialization logic here
            application.PostMapRequestHandler += OnPostAuthorizeRequest;
        }
    }

    /// <summary>
    /// Disposes the AttributeBasedFormsAuthentication Module
    /// </summary>
    public void Dispose() {
        // Clean up resources, if any
    }

    /// <summary>
    /// A request has hit IIS. This events handles authorization for the given request.
    /// </summary>
    /// <param name="sender">Sender Parameter</param>
    /// <param name="e">EventArgs Parameter</param>
    private void OnPostAuthorizeRequest(object sender, EventArgs e) {
        var app = (HttpApplication)sender;
        var context = app.Context;
        var request = context.Request;
        var requestUrl = context.Request.Url.ToString();
    
        //The request is for a Page Class:
        if (context.Handler is Page page) {
            var pageType = page.GetType();
            //Check if the MasterPage requires authentication:
            if (!requestUrl.ToLower().Contains(".axd?")) {
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
    
            //Check if the Page requires authentication:
            if (pageType.GetCustomAttribute<RequiresAuthenticationAttribute>() != null) {
                if (!request.IsAuthenticated || request.Cookies?[FormsAuthentication.FormsCookieName] == null) {
                    DenyAccess(context);
                    return;
                }
            }
    
            //The request is for a WebMethod inside a Page Class:
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
        };
    
        //The request is for a WebService Class:
        if (!(context.Handler is Page) & requestUrl.ToLower().Contains(".asmx/")) {
            var segments = requestUrl.Split(new[] { ".asmx/" }, StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length > 1) {
                string methodName = segments[1]; // Extract the part after .asmx/ as the method name
                string asmxUrlWithoutMethod = requestUrl.Replace("/" + methodName, ""); // Remove the method name from the URL
                var codeBehindType = BuildManager.GetCompiledType(GetRelativeVirtualPath(asmxUrlWithoutMethod));
    
                if (codeBehindType != null) {
                    //Check if the main WebService Class requires authentication:
                    if (codeBehindType.GetCustomAttribute<RequiresAuthenticationAttribute>() != null) {
                        if (!request.IsAuthenticated || request.Cookies?[FormsAuthentication.FormsCookieName] == null) {
                            DenyAccess(context);
                            return;
                        }
                    }
    
                    //Check if the WebMethod requres authentication:
                    if (!string.IsNullOrEmpty(methodName)) {
                        var methodInfo = codeBehindType.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
                        if (methodInfo?.GetCustomAttribute<RequiresAuthenticationAttribute>() != null) {
                            if (!request.IsAuthenticated || request.Cookies?[FormsAuthentication.FormsCookieName] == null) {
                                DenyAccess(context);
                            }
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Get absolute path as AppRelative path
    /// </summary>
    /// <param name="absolutePath">Path to map out as string</param>
    /// <returns>Returns absolute path as AppRelative path</returns>
    private string GetRelativeVirtualPath(string absolutePath) {
        Uri uri = new Uri(absolutePath);
        string rootPath = HttpContext.Current.Request.Url.GetLeftPart(UriPartial.Authority);
        if (uri.AbsoluteUri.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase)) {
            return "~" + uri.AbsolutePath.Replace("\\", "/");
        }
        return string.Empty;
    }
    
    /// <summary>
    /// Get the MasterPage path from the markup as virutal appRelative path.
    /// </summary>
    /// <param name="virtualPath">Path to map out as string</param>
    /// <returns>Path as string</returns>
    private string GetMasterPagePathFromMarkup(string virtualPath) {
        string markup = File.ReadAllText(HttpContext.Current.Server.MapPath(virtualPath));
        using (StringReader reader = new StringReader(markup)) {
            string line;
            while ((line = reader.ReadLine()) != null) {
                Match match = Regex.Match(line, "MasterPageFile\\s*=\\s*\"(.+?)\"");
                if (match.Success) {
                    string masterPagePath = match.Groups[1].Value;
    
                    if (!VirtualPathUtility.IsAppRelative(masterPagePath)) {
                        masterPagePath = VirtualPathUtility.Combine(virtualPath, masterPagePath);
                    }
    
                    return masterPagePath;
                }
            }
        }
        return string.Empty;
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

public static class HttpApplicationExtensions {
    public static void UseAuthentication(this HttpApplication application) {
        AttributeBasedFormsAuthenticationModule.useAuthentication = true;
    }
}
