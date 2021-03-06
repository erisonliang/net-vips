namespace NetVips.Samples
{
    using System;
    using System.Text;
    using System.IO;
    using System.Security;
    using System.Runtime.InteropServices;

    public class GenerateEnums : ISample
    {
        public string Name => "Generate enums";
        public string Category => "Internal";

        [SuppressUnmanagedCodeSecurity]
        [DllImport("libvips-42.dll" /*"libvips.so.42"*/, CallingConvention = CallingConvention.Cdecl,
            EntryPoint = "vips_saveable_get_type")]
        internal static extern IntPtr SaveableGetType();

        private string RemovePrefix(string enumStr)
        {
            const string prefix = "Vips";

            return enumStr.StartsWith(prefix) ? enumStr.Substring(prefix.Length) : enumStr;
        }

        /// <summary>
        /// Generate the `Enums.Generated.cs` file.
        /// </summary>
        /// <remarks>
        /// This is used to generate the `Enums.Generated.cs` file (<see cref="Enums"/>).
        /// </remarks>
        /// <returns>The `Enums.Generated.cs` as string.</returns>
        private string Generate()
        {
            // otherwise we're missing some enums
            SaveableGetType();
            var _ = Image.Black(1, 1);

            var allEnums = NetVips.GetEnums();

            const string preamble = @"//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     libvips version: {0}
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------";

            var stringBuilder =
                new StringBuilder(string.Format(preamble,
                    $"{NetVips.Version(0)}.{NetVips.Version(1)}.{NetVips.Version(2)}"));
            stringBuilder.AppendLine()
                .AppendLine()
                .AppendLine("namespace NetVips")
                .AppendLine("{")
                .AppendLine("    using System;")
                .AppendLine()
                .AppendLine("    public  partial static class Enums")
                .AppendLine("    {")
                .AppendLine("        #region auto-generated enums")
                .AppendLine();

            foreach (var name in allEnums)
            {
                if (name.StartsWith("Gsf"))
                {
                    continue;
                }

                var gtype = NetVips.TypeFromName(name);
                var csharpName = RemovePrefix(name);

                stringBuilder.AppendLine("        /// <summary>")
                    .AppendLine($"        /// {csharpName}")
                    .AppendLine("        /// </summary>")
                    .AppendLine($"        public static class {csharpName}")
                    .AppendLine("        {");

                var enumValues = NetVips.ValuesForEnum(gtype);
                for (var i = 0; i < enumValues.Count; i++)
                {
                    var value = enumValues[i];
                    var csharpValue = value.Replace('-', '_').ToPascalCase();

                    stringBuilder.AppendLine($"            /// <summary>{csharpValue}</summary>")
                        .AppendLine($"            public const string {csharpValue} = \"{value}\";");

                    if (i != enumValues.Count - 1)
                    {
                        stringBuilder.AppendLine();
                    }
                }

                stringBuilder.AppendLine("        }").AppendLine();
            }

            stringBuilder.AppendLine("        #endregion")
                .AppendLine("    }")
                .AppendLine("}");

            return stringBuilder.ToString();
        }

        public string Execute(string[] args)
        {
            if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "This example can only be run on Windows";
            }

            File.WriteAllText("Enums.Generated.cs", Generate());

            return "See Enums.Generated.cs";
        }
    }
}