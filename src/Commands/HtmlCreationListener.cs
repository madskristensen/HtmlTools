using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace HtmlTools
{
    [Export(typeof(IVsTextViewCreationListener))]
    [ContentType("HTMLX")]
    [TextViewRole(PredefinedTextViewRoles.PrimaryDocument)]
    public class HtmlxViewCreationListener : IVsTextViewCreationListener
    {
        [Import]
        public IVsEditorAdaptersFactoryService EditorAdaptersFactoryService { get; set; }

        [Import]
        public ICompletionBroker CompletionBroker { get; set; }

        public void VsTextViewCreated(IVsTextView textViewAdapter)
        {
            var textView = EditorAdaptersFactoryService.GetWpfTextView(textViewAdapter);

            textView.Properties.GetOrCreateSingletonProperty(() => new HtmlGoToDefinition(textViewAdapter, textView));
            textView.Properties.GetOrCreateSingletonProperty(() => new HtmlFindAllReferences(textViewAdapter, textView));
        }
    }
}
