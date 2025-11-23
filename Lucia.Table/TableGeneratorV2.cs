using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Lucia.Table
{
    [Generator]
    public class TableGeneratorV2 : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            List<object> fails = new();

            foreach (var table in context.AdditionalFiles.Where(x => x.Path.EndsWith(".tsv")))
            {
                try
                {
                    // --------------------------
                    // 1. Build namespace safely
                    // --------------------------
                    string rawNs = string.Join("",
                        Path.GetDirectoryName(table.Path)?
                            .Split(new string[] { "table" }, StringSplitOptions.None)
                            .Skip(1)!) ?? "";

                    rawNs = rawNs.Replace('\\', '/').Replace('/', '.');
                    string safeNs = MakeSafeNamespace("Lucia.Table.V2" + rawNs);

                    // --------------------------
                    // 2. Read file content
                    // --------------------------
                    string content = table.GetText()?.ToString()
                                     ?? throw new FormatException("File is empty!");

                    List<string> lines = content
                        .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
                        .ToList();

                    List<MemberType> members = new();
                    List<MemberType> membersRealCount = new();

                    string headLine = lines.First();
                    lines.RemoveAt(0);

                    // --------------------------
                    // 3. Parse header columns
                    // --------------------------
                    foreach (var head in headLine.Split('\t'))
                    {
                        string cleanHead = head.Split('[').First();
                        cleanHead = MakeSafeIdentifier(cleanHead);

                        if (members.Any(x => x.Name == cleanHead))
                        {
                            var m = members.First(x => x.Name == cleanHead);
                            m.Span++;
                            members[members.FindIndex(mt => mt.Name == cleanHead)] = m;
                        }
                        else
                        {
                            members.Add(new MemberType
                            {
                                Name = cleanHead,
                                Nullable = false,
                                Span = 1
                            });
                        }

                        membersRealCount.Add(members.First(x => x.Name == cleanHead));
                    }

                    // --------------------------
                    // 4. Infer types from rows
                    // --------------------------
                    for (int x = 0; x < lines.Count; x++)
                    {
                        string[] values = lines[x].Split('\t');

                        for (int y = 0; y < values.Length && y < membersRealCount.Count; y++)
                        {
                            string value = values[y];
                            MemberType memberType = membersRealCount[y];

                            if (string.IsNullOrEmpty(value))
                            {
                                memberType.Nullable = true;
                            }
                            else if (int.TryParse(value, out _))
                            {
                                if (memberType.Type != typeof(string).FullName)
                                    memberType.Type = typeof(int).FullName;
                            }
                            else
                            {
                                memberType.Type = typeof(string).FullName;
                            }

                            membersRealCount[y] = memberType;
                        }
                    }

                    // --------------------------
                    // 5. Generate property definitions
                    // --------------------------
                    string propDefs = string.Empty;

                    foreach (MemberType memberType in membersRealCount.GroupBy(x => x.Name).Select(g => g.First()))
                    {
                        if (string.IsNullOrEmpty(memberType.Type) || string.IsNullOrEmpty(memberType.Name))
                            continue;

                        string typeName = memberType.Type;
                        string propName = memberType.Name;

                        if (membersRealCount.Count(x => x.Name == memberType.Name) > 1)
                        {
                            propDefs += $"public List<{typeName}> {propName} {{ get; set; }}\r\n        ";
                        }
                        else
                        {
                            propDefs += $"public {typeName}{(memberType.Nullable ? "?" : "")} {propName} {{ get; set; }}\r\n\t";
                        }
                    }

                    // --------------------------
                    // 6. Safe class name
                    // --------------------------
                    string rawClassName = Path.GetFileNameWithoutExtension(table.Path);
                    string safeClassName = MakeSafeIdentifier(rawClassName) + "Table";

                    // --------------------------
                    // 7. Extract short resource path (without Split(string[]))
                    // --------------------------
                    string fullPath = table.Path.Replace("\\", "/");
                    string shortPath = fullPath;

                    int idx = fullPath.IndexOf("/Resources/", StringComparison.OrdinalIgnoreCase);
                    if (idx >= 0)
                    {
                        shortPath = fullPath.Substring(idx + "/Resources/".Length);
                    }

                    // --------------------------
                    // 8. Generate output file
                    // --------------------------
                    string file = $@"// <auto-generated/>
namespace {safeNs}
{{
    #nullable enable
    #pragma warning disable CS8618, CS8602
    public class {safeClassName} : global::Lucia.Common.Util.ITable
    {{
        public static string File => ""{shortPath}"";

        {propDefs}
    }}
}}";

                    context.AddSource($"{safeClassName}.g.cs", file);
                }
                catch (Exception ex)
                {
                    fails.Add(new
                    {
                        msg = ex.ToString(),
                        file = table.Path
                    });
                }
            }
        }

        public void Initialize(GeneratorInitializationContext context) { }

        // ------------------ Helpers ------------------

        private static string MakeSafeIdentifier(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return "_Empty";

            // Replace invalid chars with _
            name = Regex.Replace(name, @"[^a-zA-Z0-9_]", "_");

            // Cannot start with digit
            if (char.IsDigit(name[0]))
                name = "_" + name;

            return name;
        }

        private static string MakeSafeNamespace(string ns)
        {
            var parts = ns.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            return string.Join(".", parts.Select(MakeSafeIdentifier));
        }

        struct MemberType
        {
            public string Name;
            public string Type;
            public bool Nullable;
            public int Span;
        }
    }
}
