# Verify FormsAuthentication cookie on request through custom attribute
httpModule for verification of FormsAuthentication cookie on request, using a custom attribute.

Currently supported use cases:
```csharp 
[RequiresAuthentication]
[WebMethod]
public string AjaxMethod() {
``` 

```csharp 
[RequiresAuthentication]
public partial class WebFormsPage : System.Web.UI.Page
```
<br/>
Remember to set the FormsAuthentication cookie!
