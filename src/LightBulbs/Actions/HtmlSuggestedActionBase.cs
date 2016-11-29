using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace MadsKristensen.EditorExtensions.Html
{
    internal abstract class HtmlSuggestedActionBase : SuggestedActionBase
    {
        public ElementNode Element { get; private set; }
        public AttributeNode Attribute { get; private set; }

        protected HtmlSuggestedActionBase(ITextView textView, ITextBuffer textBuffer, ElementNode element, string displayText)
            : this(textView, textBuffer, element, null, displayText)
        {
        }

        protected HtmlSuggestedActionBase(ITextView textView, ITextBuffer textBuffer, ElementNode element, AttributeNode attribute, string displayText)
            : base(textBuffer, textView, displayText)
        {
            Element = element;
            Attribute = attribute;
        }
    }
}
