using System.Threading;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.Html.Editor.SuggestedActions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.Web.Editor.Text;
using System;

namespace HtmlTools
{
    internal class HtmlRemoveElementLightBulbAction : HtmlSuggestedActionBase
    {
        private AttributeNode _src;
        private static Guid _guid = new Guid("5d80b651-09b5-40b3-b549-f70c56dbd0fc");
        public HtmlRemoveElementLightBulbAction(ITextView textView, ITextBuffer textBuffer, ElementNode element)
            : base(textView, textBuffer, element, element.Children.Count == 0 ? "Remove <" + element.StartTag.Name + "> tag" : "Remove <" + element.StartTag.Name + "> and Keep Children", _guid)
        {
            _src = element.GetAttribute("src", true);
        }

        public override void Invoke(CancellationToken cancellationToken)
        {
            var content = Element.GetText(Element.InnerRange).Trim();
            int start = Element.Start;
            int length = content.Length;

            try
            {
                ProjectHelpers.DTE.UndoContext.Open(DisplayText);
                using (ITextEdit edit = TextBuffer.CreateEdit())
                {
                    edit.Replace(Element.OuterRange.ToSpan(), content);
                    edit.Apply();
                }

                SnapshotSpan span = new SnapshotSpan(TextView.TextBuffer.CurrentSnapshot, start, length);

                TextView.Selection.Select(span, false);
                ProjectHelpers.DTE.ExecuteCommand("Edit.FormatSelection");
                TextView.Caret.MoveTo(new SnapshotPoint(TextView.TextBuffer.CurrentSnapshot, start));
                TextView.Selection.Clear();
            }
            finally
            {
                ProjectHelpers.DTE.UndoContext.Close();
            }
        }
    }
}
