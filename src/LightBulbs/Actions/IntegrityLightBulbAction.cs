using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.Html.Editor.SuggestedActions;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using System;
using System.Net;
using System.Security.Cryptography;
using System.Threading;
using System.Windows;

namespace HtmlTools
{
    internal class IntegrityLightBulbAction : HtmlSuggestedActionBase
    {
        private static Guid _guid = new Guid("10fd1d05-9285-4d39-b086-3280f247f759");

        public IntegrityLightBulbAction(ITextView textView, ITextBuffer textBuffer, ElementNode element)
            : base(textView, textBuffer, element, "Calculate integrity", _guid)
        { }

        public override void Invoke(CancellationToken cancellationToken)
        {
            AttributeNode src = Element.GetAttribute("src") ?? Element.GetAttribute("href") ?? Element.GetAttribute("abp-src") ?? Element.GetAttribute("abp-href");
            AttributeNode integrity = Element.GetAttribute("integrity");
            AttributeNode crossorigin = Element.GetAttribute("crossorigin");

            string url = src.Value;

            if (url.StartsWith("//"))
            {
                url = "http:" + url;
            }

            string hash = CalculateHash(url);

            if (string.IsNullOrEmpty(hash))
            {
                MessageBox.Show("Could not resolve the URL to generate the hash", "Web Essentials", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                ProjectHelpers.DTE.UndoContext.Open(DisplayText);
                using (ITextEdit edit = TextBuffer.CreateEdit())
                {
                    if (integrity != null)
                    {
                        Span span = new Span(integrity.ValueRangeUnquoted.Start, integrity.ValueRangeUnquoted.Length);
                        edit.Replace(span, hash);
                    }
                    else
                    {
                        edit.Insert(src.ValueRange.End, " integrity=\"" + hash + "\"");
                    }

                    if (crossorigin == null)
                        edit.Insert(src.ValueRange.End, " crossorigin=\"anonymous\"");

                    edit.Apply();
                }
            }
            finally
            {
                ProjectHelpers.DTE.UndoContext.Close();
            }
        }

        private static string CalculateHash(string url)
        {
            try
            {
                using (WebClient client = new WebClient())
                {
                    byte[] bytes = client.DownloadData(new Uri(url));

                    HashAlgorithm sha = SHA384.Create();
                    string hash = Convert.ToBase64String(sha.ComputeHash(bytes));
                    return $"sha384-{hash}";
                }
            }
            catch
            {
                return null;
            }
        }
    }
}