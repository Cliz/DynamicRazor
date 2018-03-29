using DynamicRazor.Interface;
using DynamicRazor.Internal;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Routing;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading;
using System.Threading.Tasks;

namespace DynamicRazor
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicRazorEngine
    {
        private RazorEngine _engine;
        private ITempDataDictionaryFactory _tempDataDictionaryFactory;
        private IRazorViewEngineFileProviderAccessor _razorViewEngineFileProviderAccessor;
        private CSharpCompiler _cSharpCompiler;
        private IOptions<RazorViewEngineOptions> _razorEngineOptions;
        private ApplicationPartManager _applicationPartManager;
        private ILoggerFactory _loggerFactory;
        private IRazorPageActivator _razorPageActivator;
        private HtmlEncoder _htmlEncoder;
        private IHttpContextAccessor _httpContextAccessor;
        private IDynamicRazorProjectCacheProvider _projectCacheProvider;
        private DiagnosticSource _diagnosticSource;
        private IOptions<MvcViewOptions> _mvcViewOptions;
        private IRazorPageFactoryProvider _razorPageFactoryProvider;
        private IServiceProvider _serviceProvider;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="engine"></param>
        /// <param name="cSharpCompiler"></param>
        /// <param name="applicationPartManager"></param>
        /// <param name="tempDataDictionaryFactory"></param>
        /// <param name="razorViewEngineFileProviderAccessor"></param>
        /// <param name="razorPageActivator"></param>
        /// <param name="htmlEncoder"></param>
        /// <param name="razorPageFactoryProvider"></param>
        /// <param name="httpContextAccessor"></param>
        /// <param name="projectCacheProvider"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="razorEngineOptions"></param>
        /// <param name="mvcViewOptions"></param>
        /// <param name="diagnosticSource"></param>
        public DynamicRazorEngine(RazorEngine engine,
            CSharpCompiler cSharpCompiler,
            ApplicationPartManager applicationPartManager,
            ITempDataDictionaryFactory tempDataDictionaryFactory,
            IRazorViewEngineFileProviderAccessor razorViewEngineFileProviderAccessor,
            IRazorPageActivator razorPageActivator,
            HtmlEncoder htmlEncoder,
            IRazorPageFactoryProvider razorPageFactoryProvider,
            IHttpContextAccessor httpContextAccessor,
            IDynamicRazorProjectCacheProvider projectCacheProvider,
            ILoggerFactory loggerFactory,
            IOptions<RazorViewEngineOptions> razorEngineOptions,
            IOptions<MvcViewOptions> mvcViewOptions,
            DiagnosticSource diagnosticSource,
            IServiceProvider serviceProvider)
        {
            _engine = engine;
            _tempDataDictionaryFactory = tempDataDictionaryFactory;
            _razorViewEngineFileProviderAccessor = razorViewEngineFileProviderAccessor;
            _cSharpCompiler = cSharpCompiler;
            _razorEngineOptions = razorEngineOptions;
            _applicationPartManager = applicationPartManager;
            _loggerFactory = loggerFactory;
            _razorPageActivator = razorPageActivator;
            _htmlEncoder = htmlEncoder;
            _httpContextAccessor = httpContextAccessor;
            _projectCacheProvider = projectCacheProvider;
            _diagnosticSource = diagnosticSource;
            _mvcViewOptions = mvcViewOptions;
            _razorPageFactoryProvider = razorPageFactoryProvider;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="key"></param>
        /// <param name="model"></param>
        /// <param name="isMainPage"></param>
        /// <returns></returns>
        public async Task<string> CompileRenderAsync(
            DynamicRazorProject project,
            string key,
            object model,
            bool isMainPage = true,
            ActionDescriptor actionDescriptor = null)
        {
            if (project == null) throw new ArgumentNullException(nameof(project));

            var templateEngine = new MvcRazorTemplateEngine(_engine, project);

            var httpContext = new CustomHttpContext(_httpContextAccessor?.HttpContext);
            httpContext.RequestServices = _serviceProvider;

            _razorEngineOptions.Value.ViewLocationFormats.Insert(0, "/{0}.cshtml");

            var projectID = project.GetHashCode().ToString();

            var viewCompilerProvider = new DynamicRazorViewCompilerProvider(
                _applicationPartManager,
                templateEngine,
                _razorViewEngineFileProviderAccessor,
                _cSharpCompiler,
                _projectCacheProvider.GetCache(projectID),
                _razorEngineOptions,
                _loggerFactory);

            var engine = new RazorViewEngine(
                new DefaultRazorPageFactoryProvider(viewCompilerProvider),
                _razorPageActivator,
                _htmlEncoder,
                _razorEngineOptions,
                project,
                _loggerFactory,
                _diagnosticSource);

            var viewResult = engine.GetView(null, ViewPath.NormalizePath(key), isMainPage);

            if (!viewResult.Success)
            {
                throw new InvalidOperationException("View Not Found");
            }

            var actionContext = new ActionContext(httpContext, new RouteData(), actionDescriptor ?? new ActionDescriptor());

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary()),
                _tempDataDictionaryFactory.GetTempData(httpContext),
                TextWriter.Null,
                _mvcViewOptions.Value.HtmlHelperOptions);

            viewContext.ViewData.Model = model;

            var sb = new StringBuilder();
            using (var sw = new StringWriter(sb))
            {
                viewContext.Writer = sw;
                await viewContext.View.RenderAsync(viewContext);
            }

            return sb.ToString();
        }
    }

    public class CustomHttpContext : DefaultHttpContext
    {
        private HttpContext _baseContext;

        public CustomHttpContext(HttpContext baseContext)
        {
            _baseContext = baseContext;
        }

        public override HttpRequest Request => _baseContext?.Request ?? base.Request;
        public override IServiceProvider RequestServices { get => _baseContext?.RequestServices ?? base.RequestServices; set => base.RequestServices = value; }
    }
}
