using Microsoft.Extensions.Caching.Memory;

namespace DynamicRazor.Interface
{
    public interface IDynamicRazorProjectCacheProvider
    {
        IMemoryCache GetCache(string id);
    }
}
