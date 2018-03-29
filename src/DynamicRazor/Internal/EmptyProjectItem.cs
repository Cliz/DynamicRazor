using Microsoft.AspNetCore.Razor.Language;
using System.IO;

namespace DynamicRazor.Internal
{
    internal class EmptyProjectItem : RazorProjectItem
    {
        public override string BasePath => string.Empty;

        public override string FilePath => string.Empty;

        public override string PhysicalPath => string.Empty;

        public override bool Exists => false;

        public override Stream Read()
        {
            return null;
        }
    }
}
