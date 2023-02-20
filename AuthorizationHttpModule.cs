using System;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Security;
using System.Web.UI;
/// <summary>
/// Verify authorization on call to WebMethods or Pages with the attribute: &lt;RequiresAuthentication&gt; (VB) | [RequiresAuthentication] (C#).
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

        if (context.Handler is Page page) {
            if (page?.GetType().GetCustomAttribute<RequiresAuthenticationAttribute>() != null) {
                if (!request.IsAuthenticated || request.Cookies[FormsAuthentication.FormsCookieName] == null) {
                    DenyAccess(context);
                }
            }
            if (request.HttpMethod == "POST") {
                var methodName = GetWebMethodNameFromRequest(request);
                if (!string.IsNullOrEmpty(methodName)) {
                    var pageType = page?.GetType();
                    var methodInfo = pageType?.GetMethod(methodName, BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
                    if (methodInfo?.GetCustomAttribute<RequiresAuthenticationAttribute>() != null) {
                        if (!request.IsAuthenticated || request.Cookies[FormsAuthentication.FormsCookieName] == null) {
                            DenyAccess(context);
                        }
                    }
                }
            }
        }
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
