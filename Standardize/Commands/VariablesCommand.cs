using Community.VisualStudio.Toolkit;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text.Editor;
using System.Linq;
using System.Text.RegularExpressions;
using static Microsoft.VisualStudio.VSConstants;
using Microsoft.VisualStudio.PlatformUI;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using static System.Net.Mime.MediaTypeNames;

namespace Standardize.Commands
{
    [Command(PackageIds.VariablesCommand)]
    internal sealed class VariablesCommand : BaseCommand<VariablesCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            var textBuffer = docView?.TextBuffer;
            var textSnapshot = textBuffer.CurrentSnapshot;

            var selection = docView?.TextView.Selection.SelectedSpans.FirstOrDefault();

            using (var edit = textBuffer.CreateEdit())
            {
                string replacedText = selection.Value.GetText();

                Regex shortPrefixRegex = new Regex(@"(?<=\bshort\s)\b(?!s)(\w+)\b");
                Regex intPrefixRegex = new Regex(@"(?<=\bint\s)(?!.*\[\])\b(?!n)(\w+)\b(?!.*\[\w+\])");
                Regex longPrefixRegex = new Regex(@"(?<=\blong\s)\b(?!l)(\w+)\b");
                Regex unsignedintPrefixRegex = new Regex(@"(?<=\bunsigned int\s)(?!un)(\w+)\b");
                Regex charPrefixRegex = new Regex(@"(?<=\bchar\s)(?!.*\[\])\b(?!c)(\w+)\b(?!.*\[\w+\])");
                Regex unsignedcharPrefixRegex = new Regex(@"(?<=\bunsigned char\s)\b(?!uc)(\w+)\b");
                Regex boolPrefixRegex = new Regex(@"(?<=\bbool\s)\b(?!b)(\w+)\b");
                Regex intarrayPrefixRegex = new Regex(@"(?<=\bint(?!\*\*)\s)(?!.*arrn.*)(\w+)\s*\[\w*\]");
                Regex chararrayPrefixRegex = new Regex(@"(?<=\bchar\s)(\w+)\s*\[\w*\]");
                Regex stringPrefixRegex = new Regex(@"(?<=\bsz\s)(?!.*arrc.*)(\w+)\s*\[\w*\]");
                Regex pointertointPrefixRegex = new Regex(@"(?<=\bint\*(?!\*)\s)\b(\w+)\b");
                Regex pointertopointertointPrefixRegex = new Regex(@"(?<=\bint\*\*\s)\b(?!ppn)(\w+)\b");
                Regex pointertocharPrefixRegex = new Regex(@"(?<=\bchar\*(?!\*)\s)\b(\w+)\b");
                Regex pointertopointertocharPrefixRegex = new Regex(@"(?<=\bchar\*\*\s)\b(\w+)\b");
                Regex filePrefixRegex = new Regex(@"(?<=\bFILE(?!\*)\s)\b(?!fs)(\w+)\b");
                Regex pointertofilePrefixRegex = new Regex(@"(?<=\bFILE\*\s)\b(?!pfs)(\w+)\b");
                Regex globalPrefixRegex = new Regex(@"(?<=\bextern\s)\b(\w+\s)(?!g_)(\w+)\b");

                string[] lines = replacedText.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];

                    line = intPrefixRegex.Replace(line, "n$0");
                    line = shortPrefixRegex.Replace(line, "s$0");
                    line = longPrefixRegex.Replace(line, "l$0");
                    line = unsignedintPrefixRegex.Replace(line, "u$0");
                    line = charPrefixRegex.Replace(line, "c$0");
                    line = unsignedcharPrefixRegex.Replace(line, "u$0");
                    line = boolPrefixRegex.Replace(line, "b$0");
                    line = intarrayPrefixRegex.Replace(line, "arrn$0");
                    line = filePrefixRegex.Replace(line, "fs$0");
                    line = pointertofilePrefixRegex.Replace(line, "pfs$0");
                    line = globalPrefixRegex.Replace(line, "$1g_$2");

                    if (line.Contains("sz") && chararrayPrefixRegex.IsMatch(line))
                    {
                        line = line.Replace("arrc", "TEMP_PREFIX");
                        line = line.Replace("sz", "arrc");
                        line = line.Replace("TEMP_PREFIX", "sz");
                    }
                    else if (line.Contains("arrc") && chararrayPrefixRegex.IsMatch(line))
                    {
                        line = line.Replace("sz", "TEMP_PREFIX");
                        line = line.Replace("arrc", "sz");
                        line = line.Replace("TEMP_PREFIX", "arrc");
                    }
                    else if (chararrayPrefixRegex.IsMatch(line))
                    {
                        line = chararrayPrefixRegex.Replace(line, "arrc$0");
                    }

                    if (line.Contains("pn") && pointertointPrefixRegex.IsMatch(line))
                    {
                        line = line.Replace("pn", "TEMP_PREFIX");
                        line = line.Replace("arrn", "pn");
                        line = line.Replace("TEMP_PREFIX", "arrn");
                    }
                    else if (line.Contains("arrn") && pointertointPrefixRegex.IsMatch(line))
                    {
                        line = line.Replace("arrn", "TEMP_PREFIX");
                        line = line.Replace("pn", "arrn");
                        line = line.Replace("TEMP_PREFIX", "pn");
                    }
                    else
                    {
                        line = pointertointPrefixRegex.Replace(line, "pn$0");
                    }

                    if (line.Contains("parrn"))
                    {
                        line = line.Replace("ppn", "TEMP_PREFIX");
                        line = line.Replace("parrn", "ppn");
                        line = line.Replace("TEMP_PREFIX", "parrn");
                    }
                    else if (line.Contains("ppn"))
                    {
                        line = line.Replace("parrn", "TEMP_PREFIX");
                        line = line.Replace("ppn", "parrn");
                        line = line.Replace("TEMP_PREFIX", "ppn");
                    }
                    else
                    {
                        line = pointertopointertointPrefixRegex.Replace(line, "ppn$0");
                    }

                    if (line.Contains("sz") && pointertocharPrefixRegex.IsMatch(line))
                    {
                        line = line.Replace("sz", "TEMP_PREFIX");
                        line = line.Replace("pc", "sz");
                        line = line.Replace("TEMP_PREFIX", "pc");
                    }
                    else if (line.Contains("pc") && pointertocharPrefixRegex.IsMatch(line))
                    {
                        line = line.Replace("pc", "TEMP_PREFIX");
                        line = line.Replace("arrc", "pc");
                        line = line.Replace("TEMP_PREFIX", "arrc");
                    }
                    else if (line.Contains("arrc") && pointertocharPrefixRegex.IsMatch(line))
                    {
                        line = line.Replace("arrc", "TEMP_PREFIX");
                        line = line.Replace("sz", "arrc");
                        line = line.Replace("TEMP_PREFIX", "sz");
                    }
                    else
                    {
                        line = pointertocharPrefixRegex.Replace(line, "pc$0");
                    }

                    if (line.Contains("ppc") && pointertopointertocharPrefixRegex.IsMatch(line))
                    {
                        line = line.Replace("ppc", "TEMP_PREFIX");
                        line = line.Replace("parrc", "ppc");
                        line = line.Replace("TEMP_PREFIX", "parrc");
                    }
                    else if (line.Contains("parrc") && pointertopointertocharPrefixRegex.IsMatch(line))
                    {
                        line = line.Replace("parrc", "TEMP_PREFIX");
                        line = line.Replace("psz", "parrc");
                        line = line.Replace("TEMP_PREFIX", "psz");
                    }
                    else if (line.Contains("psz") && pointertopointertocharPrefixRegex.IsMatch(line))
                    {
                        line = line.Replace("psz", "TEMP_PREFIX");
                        line = line.Replace("ppc", "psz");
                        line = line.Replace("TEMP_PREFIX", "ppc");
                    }
                    else
                    {
                        line = pointertopointertocharPrefixRegex.Replace(line, "ppc$0");
                    }

                    lines[i] = line;
                }

                replacedText = string.Join(Environment.NewLine, lines);

                edit.Replace(selection.Value.Span, replacedText);
                edit.Apply();
            }
        }
    }
}


