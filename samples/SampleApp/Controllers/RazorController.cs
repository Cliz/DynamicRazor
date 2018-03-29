using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicRazor;
using Microsoft.AspNetCore.Mvc;
using SampleApp.Models;

namespace SampleApp.Controllers
{
    [Route("api/[controller]")]
    public class RazorController : Controller
    {
        private DynamicRazorEngine _dynamicRazorEngine;

        public RazorController(DynamicRazorEngine dynamicRazorEngine)
        {
            _dynamicRazorEngine = dynamicRazorEngine;
        }

        // GET api/<controller>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var project = GetDefaultProject();
            project.AddStringItem("View.cshtml", GetViewText());

            var model = new PersonModel();
            model.Name = "Hello Sample";
            model.Age = 20;

            return new ContentResult
            {
                ContentType = "text/html",
                Content = await _dynamicRazorEngine.CompileRenderAsync(project, "View.cshtml", model),
                StatusCode = 200
            };
        }

        
        private DynamicRazorProject GetDefaultProject()
        {
            var project = new DynamicRazorProject();
            project.AddStringItem("_ViewStart.cshtml", GetViewStartText());
            project.AddStringItem("_ViewImports.cshtml", GetViewImportsText());
            project.AddStringItem("_Layout.cshtml", GetLayoutText());

            return project;
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
        private string GetViewText()
        {
            var sb = new StringBuilder();

            sb.AppendLine("@model SampleApp.Models.PersonModel");
            sb.AppendLine("<div>");
            sb.AppendLine("<input type=\"text\" asp-for=\"Name\" />");
            sb.AppendLine("</div>");
            sb.AppendLine("<div>");
            sb.AppendLine("<input type=\"text\" asp-for=\"Age\" />");
            sb.AppendLine("</div>");

            return sb.ToString();
        }
    }
}
