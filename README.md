# DynamicRazor

Use Razor to build templates from Strings or EmbeddedResources or Files.

# Usage

## Startup.cs

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddDynamicRazor();
}
```
## Sample use cases
```csharp

public class Person
{
    public string Name { get; set; }
}

using DynamicRazor;
public SampleController : Controller
{
    private DynamicRazorEngine _dynamicRazorEngine;

    public RazorController(DynamicRazorEngine dynamicRazorEngine)
    {
        _dynamicRazorEngine = dynamicRazorEngine;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
    {
        var project = GetDefaultProject();
        project.AddStringItem("View.cshtml", GetViewText());

        var model = new Person();
        model.Name = "Bob";

        var html = await _dynamicRazorEngine.CompileRenderAsync(project, "View.cshtml", model);

        return new ContentResult
        {
            ContentType = "text/html",
            Content = html,
            StatusCode = 200
        };
    }

    private DynamicRazorProject GetDefaultProject()
	{
        var project = new DynamicRazorProject();
        project.AddStringItem("_ViewStart.cshtml", GetViewStartText()); //Option
        project.AddStringItem("_ViewImports.cshtml", GetViewImportsText()); //Option
        project.AddStringItem("_Layout.cshtml", GetLayoutText()); //Option

        return project;
    }
    private string GetViewText()
    {
        var sb = new StringBuilder();

        sb.AppendLine("@model SampleApp.Models.Person");
        sb.AppendLine("<div>");
        sb.AppendLine("<input type=\"text\" asp-for=\"Name\" />");
        sb.AppendLine("</div>");

        return sb.ToString();
    }
    private string GetViewStartText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("@{");
        sb.AppendLine("Layout = \"_Layout\";");
        sb.AppendLine("}");

        return sb.ToString();
    }
    private string GetViewImportsText()
    {
        var sb = new StringBuilder();

        sb.AppendLine("@using SampleApp");
        sb.AppendLine("@namespace SampleApp.Pages");
        sb.AppendLine("@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers");

        return sb.ToString();
    }
    private string GetLayoutText()
    {
        var sb = new StringBuilder();
        sb.AppendLine("<!DOCTYPE html>");
        sb.AppendLine("<html>");
        sb.AppendLine("<head>");
        sb.AppendLine("<meta charset=\"utf-8\" />");
        sb.AppendLine("<title>Sample</title>");
        sb.AppendLine("@RenderSection(\"Scripts\", required: false)");
        sb.AppendLine("</head>");
        sb.AppendLine("<body>");
        sb.AppendLine("@RenderBody()");
        sb.AppendLine("</body>");
        sb.AppendLine("</html>");

        return sb.ToString();
    }
}
```