# Verify FormsAuthentication cookie on request through custom attribute

> httpModule for FormsAuthentication verification on requests using custom attributes.



## Currently supported use cases - C#:

### Decorated on spesific PageMethod's or Ajax WebMethod's:

```csharp 
[RequiresAuthentication]
[WebMethod]
public string AjaxMethod() {
``` 


### Decorated on Page Class:
```csharp 
[RequiresAuthentication]
public partial class WebFormsPage : System.Web.UI.Page
```
### Decorated on MasterPage Class:
```csharp 
[RequiresAuthentication]
public partial class WebFormsMasterPage : System.Web.UI.MasterPage
```
### Decorated on WebService Class:
```csharp 
[RequiresAuthentication]
[System.Web.Script.Services.ScriptService()]
[WebService(Namespace = "http://localhost:8080/")]
[WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
public class AspAjaxWebService : System.Web.Services.WebService
{
}
```

---

## Currently supported use cases - VB.NET:

### Decorated on spesific PageMethod's or Ajax WebMethod's:
```vb
<RequiresAuthentication>
<WebMethod>
public function AjaxMethod() as string 
``` 

### Decorated on Page Class:
```vb 
<RequiresAuthentication>
Partial Class WebFormsPage
    Inherits System.Web.UI.Page
```

### Decorated on MasterPage Class:
```vb 
<RequiresAuthentication>
Partial Class WebFormsMasterPage
    Inherits System.Web.UI.MasterPage
```

### Decorated on WebService Class:
```vb
<RequiresAuthentication>
<System.Web.Script.Services.ScriptService()>
<WebService([Namespace]:="http://localhost:8080/")>
<WebServiceBinding(ConformsTo:=WsiProfiles.BasicProfile1_1)>
<[global].Microsoft.VisualBasic.CompilerServices.DesignerGenerated()>
Public Class AspAjaxWebService
    Inherits System.Web.Services.WebService
End Class
```

<br/>

---

## DO NOT FORGET BEFORE USING: 

* Make sure your FormsAuthentication cookie is set correctly 
* Verify/change the assembly metadata for System.Web.Extensions which is used to get the Type definition for RestHandlerWithSession:
```csharp
Type _RestHandlerWithSessionType = Type.GetType("System.Web.Script.Services.RestHandlerWithSession, System.Web.Extensions, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");

// the "Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" part must match YOUR spesific scenario
```
