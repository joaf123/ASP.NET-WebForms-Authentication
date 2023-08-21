# Verify FormsAuthentication cookie on request through custom attribute
httpModule for verification of FormsAuthentication cookie on request, using a custom attribute.

## Currently supported use cases:

### C#:

#### Decorated on a PageMethod or ASP.NET Ajax WebMethod:

```csharp 
[RequiresAuthentication]
[WebMethod]
public string AjaxMethod() {
``` 


#### Decorated on a Page, MasterPage or ASP.NET Ajax WebService Class:
##### Page Class:
```csharp 
[RequiresAuthentication]
public partial class WebFormsPage : System.Web.UI.Page
```
##### MasterPage Class:
```csharp 
[RequiresAuthentication]
public partial class WebFormsMasterPage : System.Web.UI.MasterPage
```
##### WebService Class:
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

### VB:

#### Decorated on a PageMethod or ASP.NET Ajax WebMethod:
```vb
<RequiresAuthentication>
<WebMethod>
public function AjaxMethod() as string 
``` 

#### Decorated on a Page, MasterPage or ASP.NET Ajax WebService Class
##### Page Class:
```vb 
<RequiresAuthentication>
Partial Class WebFormsPage
    Inherits System.Web.UI.Page
```

##### MasterPage Class:
```vb 
<RequiresAuthentication>
Partial Class WebFormsMasterPage
    Inherits System.Web.UI.MasterPage
```

##### WebService Class:
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
Remember to set the FormsAuthentication cookie!
