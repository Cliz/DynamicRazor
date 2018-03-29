using Microsoft.AspNetCore.Mvc.Razor.Internal;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DynamicRazor
{
    public class FileSystemRazorProjectItem : DynamicRazorProjectItem
    {
        private bool _isComputed = false;
        private int _computedHash = 0;

        public FileSystemRazorProjectItem(string key, string filePath)
        {
            if (Path.GetExtension(key) != ".cshtml")
            {
                throw new ArgumentException(nameof(key));
            }
            
            Key = ViewPath.NormalizePath(key);
            //TODO Windows以外の環境への対応
            File = new FileInfo(filePath);

            if (Path.IsPathRooted(filePath))
            {
                BasePath = NormalizeAndEnsureValidPath(File.DirectoryName);
                filePath = NormalizeAndEnsureValidPath(File.Name);
            }
            else
            {
                var relativePath = NormalizeAndEnsureValidPath(filePath);

                var idx = File.FullName.LastIndexOf(filePath);

                BasePath = NormalizeAndEnsureValidPath(File.FullName.Substring(0, File.FullName.LastIndexOf(filePath)).TrimEnd(Path.DirectorySeparatorChar));
                FilePath = relativePath;
            }
        }

        public FileInfo File { get; }

        public override string BasePath { get; }

        public override string FilePath { get; }

        public override bool Exists => File.Exists;

        public override string PhysicalPath => File.FullName;

        public override Stream Read() => new FileStream(PhysicalPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);

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
                hashCodeCombiner.Add(File.FullName);

                _computedHash = hashCodeCombiner;
                _isComputed = true;
            }

            return _computedHash;
        }

        protected virtual string NormalizeAndEnsureValidPath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentException(nameof(path));
            }

            var absolutePath = path;
            if (!absolutePath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                if (path[0] == '/' || path[0] == '\\')
                {
                    path = path.Substring(1);
                }

                absolutePath = Path.Combine("/", path);
            }

            absolutePath = absolutePath.Replace('\\', '/');

            return absolutePath;
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
        /// <param name="key"></param>
        /// <param name="filePath"></param>
        public static void AddFileItem(this DynamicRazorProject project, string key, string filePath)
        {
            project.Add(new FileSystemRazorProjectItem(key, filePath));
        }
    }
}
