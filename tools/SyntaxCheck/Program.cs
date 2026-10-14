using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace IronVeil.Tools
{
    /// <summary>
    /// Parses every C# file in the repository with Roslyn and reports syntax errors.
    /// This cannot type-check against Unity (the assemblies are not available
    /// outside the editor), but it catches broken braces, stray tokens, malformed
    /// generics and the other mistakes that would otherwise only show up when Unity
    /// imports the project.
    /// </summary>
    internal static class Program
    {
        private static int Main(string[] args)
        {
            string root = args.Length > 0 ? args[0] : Directory.GetCurrentDirectory();
            List<string> files = new List<string>();

            foreach (string directory in new[] { "Assets", "tools" })
            {
                string path = Path.Combine(root, directory);
                if (!Directory.Exists(path)) continue;
                files.AddRange(Directory.EnumerateFiles(path, "*.cs", SearchOption.AllDirectories));
            }

            files = files
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + "obj" + Path.DirectorySeparatorChar))
                .Where(f => !f.Contains(Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar))
                .OrderBy(f => f, StringComparer.Ordinal)
                .ToList();

            CSharpParseOptions options = new CSharpParseOptions(LanguageVersion.CSharp9);
            int errors = 0;
            int warnings = 0;

            foreach (string file in files)
            {
                string text;
                try
                {
                    text = File.ReadAllText(file);
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine("could not read " + file + ": " + exception.Message);
                    errors++;
                    continue;
                }

                SyntaxTree tree = CSharpSyntaxTree.ParseText(text, options, file);
                foreach (Diagnostic diagnostic in tree.GetDiagnostics())
                {
                    string location = diagnostic.Location.GetLineSpan().StartLinePosition.Line + 1 + "";
                    string relative = Path.GetRelativePath(root, file);

                    if (diagnostic.Severity == DiagnosticSeverity.Error)
                    {
                        errors++;
                        Console.Error.WriteLine(relative + "(" + location + "): error " + diagnostic.Id + ": " + diagnostic.GetMessage());
                    }
                    else if (diagnostic.Severity == DiagnosticSeverity.Warning)
                    {
                        warnings++;
                        Console.WriteLine(relative + "(" + location + "): warning " + diagnostic.Id + ": " + diagnostic.GetMessage());
                    }
                }
            }

            Console.WriteLine();
            Console.WriteLine("parsed " + files.Count + " files: " + errors + " syntax error(s), " + warnings + " warning(s)");
            return errors == 0 ? 0 : 1;
        }
    }
}
