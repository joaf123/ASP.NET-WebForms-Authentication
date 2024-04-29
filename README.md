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

### Global.asax (vb[^1]):
```asp
<%@ Application Language="VB" %>

<script RunAt="server">
    Sub Application_Start(ByVal sender As Object, ByVal e As EventArgs)
        Me.UseAuthentication()
    End Sub
</script>
```
[^1]: `this.UseAuthentication` in C#

### DO NOT FORGET BEFORE USING: 

* Make sure your FormsAuthentication cookie is set correctly 
* Verify/change the assembly metadata for System.Web.Extensions which is used to get the Type definition for RestHandlerWithSession:
```csharp
Type _RestHandlerWithSessionType = Type.GetType("System.Web.Script.Services.RestHandlerWithSession, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

// the "Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" part must match YOUR spesific scenario
```

## Supported Use Cases:

### Decorated on spesific PageMethod's or Ajax WebMethod's:

```csharp 
[RequiresAuthentication]
[WebMethod]
public string AjaxMethod() {
``` 
or
### Decorated on the main WebService Class:
```csharp 
[RequiresAuthentication]
[System.Web.Script.Services.ScriptService()]
[WebService(Namespace = "http://localhost:8080/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
public class AspAjaxWebService : System.Web.Services.WebService
```

### Decorated on a Page Class:
```csharp 
[RequiresAuthentication]
public partial class WebFormsPage : System.Web.UI.Page
```
or
### Decorated on a MasterPage Class:
```csharp 
[RequiresAuthentication]
public partial class WebFormsMasterPage : System.Web.UI.MasterPage
```
