// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Razor.Compilation;
using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading;

namespace DynamicRazor.Internal
{
    internal class DynamicRazorViewCompilerProvider : IViewCompilerProvider
    {
        private readonly RazorTemplateEngine _razorTemplateEngine;
        private readonly ApplicationPartManager _applicationPartManager;
        private readonly IRazorViewEngineFileProviderAccessor _fileProviderAccessor;
        private readonly CSharpCompiler _csharpCompiler;
        private readonly RazorViewEngineOptions _viewEngineOptions;
        private readonly ILogger<RazorViewCompiler> _logger;
        private readonly Func<IViewCompiler> _createCompiler;
        private readonly IMemoryCache _memoryCache;

        private object _initializeLock = new object();
        private bool _initialized;
        private IViewCompiler _compiler;

        public DynamicRazorViewCompilerProvider(
            ApplicationPartManager applicationPartManager,
            RazorTemplateEngine razorTemplateEngine,
            IRazorViewEngineFileProviderAccessor fileProviderAccessor,
            CSharpCompiler csharpCompiler,
            IMemoryCache memoryCache,
            IOptions<RazorViewEngineOptions> viewEngineOptionsAccessor,
            ILoggerFactory loggerFactory)
        {
            _applicationPartManager = applicationPartManager;
            _razorTemplateEngine = razorTemplateEngine;
            _fileProviderAccessor = fileProviderAccessor;
            _csharpCompiler = csharpCompiler;
            _viewEngineOptions = viewEngineOptionsAccessor.Value;
            _memoryCache = memoryCache;

            _logger = loggerFactory.CreateLogger<RazorViewCompiler>();
            _createCompiler = CreateCompiler;
        }

        public IViewCompiler GetCompiler()
        {
            var fileProvider = _fileProviderAccessor.FileProvider;
            if (fileProvider is NullFileProvider)
            {
                throw new InvalidOperationException("FormatFileProvidersAreRequired");
            }

            return LazyInitializer.EnsureInitialized(
                ref _compiler,
                ref _initialized,
                ref _initializeLock,
                _createCompiler);
        }

        private IViewCompiler CreateCompiler()
        {
            var feature = new ViewsFeature();
            _applicationPartManager.PopulateFeature(feature);

            return new DynamicRazorViewCompiler(
                _fileProviderAccessor.FileProvider,
                _razorTemplateEngine,
                _csharpCompiler,
                _viewEngineOptions.CompilationCallback,
                feature.ViewDescriptors,
                _memoryCache,
                _logger);
        }
    }
}
