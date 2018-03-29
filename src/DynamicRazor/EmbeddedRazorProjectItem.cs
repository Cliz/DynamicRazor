using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DynamicRazor
{
    public class EmbeddedRazorProjectItem : DynamicRazorProjectItem
    {
        private bool _isComputed = false;
        private int _computedHash = 0;

        public Assembly Assembly { get; }

        public string ResourceKey { get; }

        public EmbeddedRazorProjectItem(string path, Assembly assembly, string resourceKey)
        {
            if (Path.GetExtension(path) != ".cshtml")
            {
                throw new ArgumentException(nameof(path));
            }

            FilePath = ViewPath.NormalizePath(path);
            Key = FilePath;
            Assembly = assembly;
            ResourceKey = resourceKey;
            Exists = Assembly.GetManifestResourceNames().Any(f => f == ResourceKey);
        }

        public override string BasePath => string.Empty;

        public override string FilePath { get; }

        public override string PhysicalPath => Path.Combine(BasePath, FilePath);

        public override bool Exists { get; }

        public override Stream Read()
        {
            return Assembly.GetManifestResourceStream(ResourceKey);
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            return obj.GetHashCode() == this.GetHashCode();
        }
        public override int GetHashCode()
        {
            if (!_isComputed)
            {
                var hashCodeCombiner = HashCodeCombiner.Start();
                hashCodeCombiner.Add(FilePath);
                hashCodeCombiner.Add(Assembly.FullName);
                hashCodeCombiner.Add(ResourceKey);

                _computedHash = hashCodeCombiner;
                _isComputed = true;
            }

            return _computedHash;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static partial class DynamicRazorProjectExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="path"></param>
        /// <param name="rootType"></param>
        /// <param name="resourceKey"></param>
        public static void AddEmbeddedItem(this DynamicRazorProject project, string path, Type rootType, string resourceKey)
        {
            project.Add(new EmbeddedRazorProjectItem(path, rootType.Assembly, resourceKey));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="project"></param>
        /// <param name="path"></param>
        /// <param name="assembly"></param>
        /// <param name="resourceKey"></param>
        public static void AddEmbeddedItem(this DynamicRazorProject project, string path, Assembly assembly, string resourceKey)
        {
            project.Add(new EmbeddedRazorProjectItem(path, assembly, resourceKey));
        }
    }
}
