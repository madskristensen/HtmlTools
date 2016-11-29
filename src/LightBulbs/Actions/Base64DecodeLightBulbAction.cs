using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.Html.Editor.SuggestedActions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.Web.Editor.EditorHelpers;
using Microsoft.Web.Editor.Text;
using System;
using System.Threading;

namespace HtmlTools
{
    internal class HtmlBase64DecodeLightBulbAction : HtmlSuggestedActionBase
    {
        private static Guid _guid = new Guid("10fd1d05-9285-4d39-b086-3280f247f757");

        public HtmlBase64DecodeLightBulbAction(ITextView textView, ITextBuffer textBuffer, ElementNode element, AttributeNode attribute)
            : base(textView, textBuffer, element, attribute, "Save as File...", _guid)
        { }

        public override void Invoke(CancellationToken cancellationToken)
        {
            string mimeType = FileHelpers.GetMimeTypeFromBase64(Attribute.Value);
            string extension = FileHelpers.GetExtension(mimeType) ?? "png";

            var fileName = FileHelpers.ShowDialog(extension);

            if (!string.IsNullOrEmpty(fileName) && FileHelpers.SaveDataUriToFile(Attribute.Value, fileName))
            {
                string relative = FileHelpers.RelativePath(TextBuffer.GetFileName(), fileName);

                try
                {
                    ProjectHelpers.DTE.UndoContext.Open(DisplayText);
                    using (ITextEdit edit = TextBuffer.CreateEdit())
                    {
                        edit.Replace(Attribute.ValueRangeUnquoted.ToSpan(), relative.ToLowerInvariant());
                        edit.Apply();
                    }
                }
                finally
                {
                    ProjectHelpers.DTE.UndoContext.Close();
                }
            }
        }
    }
}
