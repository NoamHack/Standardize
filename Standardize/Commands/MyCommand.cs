using System.Text.RegularExpressions;

namespace Standardize.Commands
{
    [Command(PackageIds.MyCommand)]
    internal sealed class MyCommand : BaseCommand<MyCommand>
    {
        protected override async Task ExecuteAsync(OleMenuCmdEventArgs e)
        {
            var docView = await VS.Documents.GetActiveDocumentViewAsync();
            var textBuffer = docView?.TextBuffer;
            var textSnapshot = textBuffer.CurrentSnapshot;

            string currentDate = DateTime.Now.ToString("dd.MM.yyyy");

            string szBeforeMain = "//-----------------------------------------------------------------------------\r\n// \t\r\n// \t\t\t                     -----------\t\r\n// \r\n// General :\r\n//\r\n// Input :\r\n//\r\n// Process :\r\n//\r\n// Output :\r\n//\r\n//-----------------------------------------------------------------------------\r\n// Programmer :\r\n// Student No :\r\n// Date : " + currentDate + "\r\n//-----------------------------------------------------------------------------";
            
            string text = textSnapshot.GetText();

            // Finding return without brackets
            Regex afterreturnRegex = new Regex(@"(?<=return\s)([^;]+)(?=;)");
            // Finding comma without space
            Regex aftercommaRegex = new Regex(@",(?! )");
            // Finding struct without space
            Regex controlstructureRegex = new Regex(@"\b(while|case|if|for|switch|foreach|do)\s*\(");
            // Finding main functions
            Regex mainRegex = new Regex(@"void main\(\)|int main\(\)|void main\(void\)");
            // Finding comment without space
            Regex spaceaftercommentRegex = new Regex(@"//(?! )(?=[^-])");
            // Finding function
            Regex functionRegex = new Regex(@"(\w+[\s\*]+\w+)\s*\(([^)]*)\)\s*\{?");

            string result = controlstructureRegex.Replace(text, "$1 (");
            result = afterreturnRegex.Replace(result, "($1)");
            result = aftercommaRegex.Replace(result, ", ");
            result = spaceaftercommentRegex.Replace(result, "// ");

            // Add szBeforeMain above main functions
            result = mainRegex.Replace(result, szBeforeMain + Environment.NewLine + "$&");

            using (var edit = textBuffer.CreateEdit())
            {
                edit.Replace(0, text.Length, result);
                edit.Apply();
            }

            string GetSecondWord(string input)
            {
                string[] words = input.Split(' ');
                if (words.Length > 1)
                {
                    return words[1];
                }
                else
                {
                    return String.Empty; // או אפשר להחזיר null או להפעיל יוצא מהקנה
                }
            }

        }

    }
}
