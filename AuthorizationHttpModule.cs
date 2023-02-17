using System;
using System.Reflection;
using System.Web;
using System.Web.Compilation;
using System.Web.Security;
using System.Web.UI;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class RequiresAuthenticationAttribute : Attribute { }

public class AttributeBasedFormsAuthenticationModule : IHttpModule {
    public void Init(HttpApplication application) {
        application.PostMapRequestHandler += OnPostAuthorizeRequest;
    }

    public void Dispose() {
        // Clean up resources, if any
    }

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

    private static void DenyAccess(HttpContext context) {
        context.Response.StatusCode = 401;
        context.Response.SuppressContent = true;
        context.Response.End();
    }

    private static string GetWebMethodNameFromRequest(HttpRequest request) {
        var pathInfo = request.PathInfo.TrimStart('/');
        var slashIndex = pathInfo.IndexOf('/');
        return slashIndex >= 0 ? pathInfo.Substring(0, slashIndex) : pathInfo;
    }
}