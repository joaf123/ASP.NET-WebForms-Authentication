# Verify FormsAuthentication cookie on request using a custom attribute

ASP.NET Core inspired request authentication, using a custom attribute & FormsAuthentication in legacy ASP.NET Website Projects!
Supports WebMethods, Page Classes, MasterPage Classes & WebService Classes

##  Setup:

### Prerequisite
IIS needs to be running in integrated mode. 
Classic mode is not supported due to the nature of the classic IIS pipeline

### Web.config
```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <system.webServer>
        <!--RequireAuthentication Module-->
        <modules>
            <add name="AttributeBasedFormsAuthenticationModule" type="AttributeBasedFormsAuthenticationModule" preCondition="integratedMode" />
        </modules>
    </system.webServer>
</configuration>
```

### Global.asax:
#### VB syntax
```asp
<%@ Application Language="VB" %>

<script RunAt="server">
    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        Me.UseAuthentication()
    End Sub
</script>
```
#### C# syntax
```asp
<%@ Application Language="C#" %>

<script RunAt="server">
    void Application_Start(object sender, EventArgs e)
    {
        this.UseAuthentication();
    }
</script>
```

## Supported Use:
### WebService Classes:
```csharp 
[RequiresAuthentication]
[System.Web.Script.Services.ScriptService()]
[WebService(Namespace = "http://localhost:8080/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
public class AspAjaxWebService : System.Web.Services.WebService
```
### HttpHandler Classes:
```csharp
[RequiresAuthentication]
public class dropzone_FileUpload : IHttpHandler, System.Web.SessionState.IRequiresSessionState
```
### Page Classes:
```csharp 
[RequiresAuthentication]
public partial class WebFormsPage : System.Web.UI.Page
```
### MasterPage Classes:
```csharp 
[RequiresAuthentication]
public partial class WebFormsMasterPage : System.Web.UI.MasterPage
```
### WebMethods inside Page Classes & WebMethods inside WebService Classes:
```csharp 
[RequiresAuthentication]
[WebMethod]
public string AjaxMethod() {
``` 
