using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Internal;
using System;
using System.IO;
using System.Text;

namespace DynamicRazor
{
    /// <summary>
    /// 
    /// </summary>
    public class StringRazorProjectItem : DynamicRazorProjectItem
    {
        private bool _isComputed = false;
        private int _computedHash = 0;

        public override string BasePath => string.Empty;

        public override string FilePath { get; }

        public override string PhysicalPath => Path.Combine(BasePath, FilePath);

        public override bool Exists => Content != null && Content.Length > 0;

        public string Content { get; }

        public StringRazorProjectItem(string path, string cshtml)
        {
            if (Path.GetExtension(path) != ".cshtml")
            {
                throw new ArgumentException(nameof(path));
            }
            
            FilePath = ViewPath.NormalizePath(path);
            Key = FilePath;
            Content = cshtml;
        }

        public override Stream Read() => new MemoryStream(Encoding.UTF8.GetBytes(Content ?? string.Empty));

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
                hashCodeCombiner.Add(Content);

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
        /// <param name="cshtml"></param>
        public static void AddStringItem(this DynamicRazorProject project, string path, string cshtml)
        {
            project.Add(new StringRazorProjectItem(path, cshtml));
        }
    }
}
