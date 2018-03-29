using Microsoft.AspNetCore.Razor.Language;
using System;
using System.Collections.Generic;
using System.Text;

namespace DynamicRazor
{
    public abstract class DynamicRazorProjectItem : RazorProjectItem
    {
        public string Key { get; protected set; }
    }
}
