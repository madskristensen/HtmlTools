using EnvDTE80;
using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.Html.Editor.Document;
using Microsoft.Html.Editor.Tree;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.TextManager.Interop;
using System;

namespace HtmlTools
{
    internal class HtmlFindAllReferences : CommandTargetBase<VSConstants.VSStd97CmdID>
    {
        private readonly HtmlEditorTree _tree;
        private string _className;

        public HtmlFindAllReferences(IVsTextView adapter, IWpfTextView textView)
            : base(adapter, textView, VSConstants.VSStd97CmdID.FindReferences)
        {
            HtmlEditorDocument document = HtmlEditorDocument.TryFromTextView(textView);

            _tree = document == null ? null : document.HtmlEditorTree;
        }

        protected override bool Execute(VSConstants.VSStd97CmdID commandId, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
        {
            if (!string.IsNullOrEmpty(_className))
            {
                SearchFiles("." + _className, "*.css;*.less;*.scss;*.sass");
				return true;
            }

            return false;
        }

        public static void SearchFiles(string term, string fileTypes)
        {
            Find2 find = (Find2)ProjectHelpers.DTE.Find;
            string types = find.FilesOfType;
            bool matchCase = find.MatchCase;
            bool matchWord = find.MatchWholeWord;

            find.WaitForFindToComplete = false;
            find.Action = EnvDTE.vsFindAction.vsFindActionFindAll;
            find.Backwards = false;
            find.MatchInHiddenText = true;
            find.MatchWholeWord = true;
            find.MatchCase = true;
            find.PatternSyntax = EnvDTE.vsFindPatternSyntax.vsFindPatternSyntaxLiteral;
            find.ResultsLocation = EnvDTE.vsFindResultsLocation.vsFindResults1;
            find.SearchSubfolders = true;
            find.FilesOfType = fileTypes;
            find.Target = EnvDTE.vsFindTarget.vsFindTargetSolution;
            find.FindWhat = term;
            find.Execute();

            find.FilesOfType = types;
            find.MatchCase = matchCase;
            find.MatchWholeWord = matchWord;
        }

        private bool TryGetClassName(out string className)
        {
            int position = TextView.Caret.Position.BufferPosition.Position;
            className = null;

            ElementNode element = null;
            AttributeNode attr = null;

            _tree.GetPositionElement(position, out element, out attr);

            if (attr == null || attr.Name != "class")
                return false;

            int beginning = position - attr.ValueRangeUnquoted.Start;
            int start = attr.Value.LastIndexOf(' ', beginning) + 1;
            int length = attr.Value.IndexOf(' ', start) - start;

            if (length < 0)
                length = attr.ValueRangeUnquoted.Length - start;

            className = attr.Value.Substring(start, length);

            return true;
        }

        protected override bool IsEnabled()
        {
            return TryGetClassName(out _className);
        }
    }
}