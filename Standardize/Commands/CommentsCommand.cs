using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Threading;
using System.Text.RegularExpressions;

namespace Standardize.Commands
{
    [Command(PackageIds.CommentsCommand)]
    internal sealed class CommentsCommand : BaseCommand<CommentsCommand>
    {
        // Executing the main command asynchronously
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            // Switching to the main UI thread of Visual Studio
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            // Getting the current document view's textBuffer
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            var textBuffer = docView?.TextBuffer;

            // Grabbing the current snapshot of text to apply changes
            var textSnapshot = textBuffer.CurrentSnapshot;
            string text = textSnapshot.GetText();

            // Grabbing the selection context if there's one
            var selection = docView?.TextView.Selection.SelectedSpans.FirstOrDefault();

            // Beginning the batch edit on the text buffer, effectively starting a transaction
            using (var edit = textBuffer.CreateEdit())
            {
                // Replacing the selected text with processed code
                string selectedText = selection.Value.GetText();
                string processedCode = ProcessCode(selectedText);

                // Applying the processed code to the selected text
                edit.Replace(selection.Value.Span, processedCode);
                edit.Apply(); // Committing the changes
            }
        }

        // Processing code strings for better formatting
        private string ProcessCode(string code)
        {
            // Splitting the code into lines for individual processing
            string[] lines = code.Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            // Calculating the max character widths for each significant part (type, variable name, after variable name)
            int maxTypeWidth = CalculateMaxWidth(lines, 0);
            int maxNameWidth = CalculateMaxWidth(lines, 1);
            int maxAfterNameWidth = CalculateMaxWidth(lines, 2);
            string newLine;

            // Standardizing space separation after type and variable names
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                string[] components = line.Split();

                if (components.Length > 2)
                {
                    // Parsing the individual components of the code line
                    string type = components[0].Trim();
                    string name = components[1].Trim();
                    string aftername = components[2].Trim();

                    // Generating spaces string according to max calculated widths
                    string spacesType = new string(' ', maxTypeWidth - type.Length);
                    string spacesName = new string(' ', maxNameWidth - name.Length);
                    string spacesAfterName = new string(' ', maxAfterNameWidth - aftername.Length);

                    // Constructing lines based on computed widths and original components
                    if (i == 0)
                    {
                        newLine = $"{type}{spacesType} {name}{spacesName} {aftername}{spacesAfterName}";
                    }
                    else
                    {
                        newLine = $"    {type}{spacesType} {name}{spacesName} {aftername}{spacesAfterName}";
                    }
                    for (int j = 3; j < components.Length; j++) // Attaching remaining components if exist
                    {
                        newLine += " " + components[j];
                    }

                    // Replacing old line with the new formatted line 
                    lines[i] = newLine;
                }
                else if (components.Length == 2) // For lines with two components
                {
                    string type = components[0].Trim();
                    string name = components[1].Trim();

                    string spacesType = new string(' ', maxTypeWidth - type.Length);
                    string spacesName = new string(' ', maxNameWidth - name.Length);

                    lines[i] = "    " + line.Replace(type, type + spacesType).Replace(name, name + spacesName);

                    if (i == 0)
                    {
                        lines[i] = line.Replace(type, type + spacesType).Replace(name, name + spacesName);
                    }
                }
                else if (components.Length == 1) // Handling single component lines
                {
                    string type = components[0].Trim();
                    string spaces = new string(' ', maxTypeWidth - type.Length);

                    lines[i] = line.Replace(type, type + spaces);
                }
            }
             
            // Joining all lines back into a single string with environment specific line endings
            string processedCode = string.Join(Environment.NewLine, lines);

            return processedCode;
        }

        // Utility method to calculate maximum width (character count) of given index component across all lines
        private int CalculateMaxWidth(string[] lines, int index)
        {
            int maxWidth = 0;

            // Iterate through each line's component at given index
            foreach (string line in lines)
            {
                string trimmedLine = line.Trim();
                string[] components = trimmedLine.Split();

                if (components.Length > index)
                {
                    string part = components[index].Trim();
                    int width = part.Length;

                    // Update max width if current width is bigger 
                    if (width > maxWidth)
                    {
                        maxWidth = width;
                    }
                }
            }

            // Return max width of the component across all the provided lines
            return maxWidth;
        }
    }
}
