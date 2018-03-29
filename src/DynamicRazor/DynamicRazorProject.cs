using DynamicRazor.Internal;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.Extensions.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace DynamicRazor
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicRazorProject : RazorProject
    {
        private bool _isComputed = false;
        private int _computedHash = 0;

        private static readonly EmptyProjectItem _empty = new EmptyProjectItem();

        private IDictionary<string, DynamicRazorProjectItem> _dic = new ConcurrentDictionary<string, DynamicRazorProjectItem>();

        /// <summary>
        /// 
        /// </summary>
        public DynamicRazorProject()
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="basePath"></param>
        /// <returns></returns>
        public override IEnumerable<RazorProjectItem> EnumerateItems(string basePath)
        {
            return _dic.Values;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public override RazorProjectItem GetItem(string path)
        {
            if (_dic.TryGetValue(path, out var item))
            {
                return item;
            }

            return _empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Add(DynamicRazorProjectItem item)
        {
            _dic.Add(item.Key, item);
            _isComputed = false;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        public void Remove(DynamicRazorProjectItem item)
        {
            _dic.Remove(item.Key);
            _isComputed = false;
        }
        /// <summary>
        /// 
        /// </summary>
        public void Clear()
        {
            _dic.Clear();
            _isComputed = false;
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

                foreach (var item in _dic)
                {
                    hashCodeCombiner.Add(item.GetHashCode());
                }

                _computedHash = hashCodeCombiner;
                _isComputed = true;
            }

            return _computedHash;
        }
    }
}
