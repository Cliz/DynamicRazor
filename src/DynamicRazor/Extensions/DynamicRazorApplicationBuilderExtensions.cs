using DynamicRazor;
using DynamicRazor.Interface;
using DynamicRazor.Internal;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.AspNetCore.Builder
{
    public static class DynamicRazorApplicationBuilderExtensions
    {
        public static IServiceCollection AddDynamicRazor(this IServiceCollection services)
        {
            services.AddSingleton<DynamicRazorEngine>();
            services.AddSingleton<IDynamicRazorProjectCacheProvider, DefaultProjectCacheProvider>();

            return services;
        }
    }
}
