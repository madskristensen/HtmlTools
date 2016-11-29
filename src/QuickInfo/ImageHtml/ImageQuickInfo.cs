using Microsoft.Html.Core.Tree.Nodes;
using Microsoft.Html.Editor.Document;
using Microsoft.Html.Editor.Tree;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.Web.Editor.EditorHelpers;
using Microsoft.Web.Editor.Host;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace HtmlTools
{
    internal class ImageHtmlQuickInfo : IQuickInfoSource
    {
        static readonly BitmapFrame noPreview = Freeze(BitmapFrame.Create(new Uri("pack://application:,,,/HtmlTools;component/Resources/nopreview.png")));

        static T Freeze<T>(T obj) where T : Freezable { obj.Freeze(); return obj; }

        public void AugmentQuickInfoSession(IQuickInfoSession session, IList<object> qiContent, out ITrackingSpan applicableToSpan)
        {
            applicableToSpan = null;

            SnapshotPoint? point = session.GetTriggerPoint(session.TextView.TextBuffer.CurrentSnapshot);

            if (!point.HasValue)
                return;

            HtmlEditorTree tree = HtmlEditorDocument.TryFromTextView(session.TextView).HtmlEditorTree;

            if (tree == null)
                return;

            tree.GetPositionElement(point.Value.Position, out var node, out var attr);

            if (node == null || (!node.Name.Equals("img", StringComparison.OrdinalIgnoreCase) && !node.Name.Equals("source", StringComparison.OrdinalIgnoreCase)))
                return;
            if (attr == null || !attr.Name.Equals("src", StringComparison.OrdinalIgnoreCase))
                return;

            string url = GetFullUrl(attr.Value, session.TextView.TextBuffer);
            if (string.IsNullOrEmpty(url))
                return;

            applicableToSpan = session.TextView.TextBuffer.CurrentSnapshot.CreateTrackingSpan(point.Value.Position, 1, SpanTrackingMode.EdgeNegative);

            AddImageContent(qiContent, url);
        }

        public static string GetFullUrl(string text, ITextBuffer sourceBuffer)
        {
            return GetFullUrl(text, sourceBuffer.GetFileName() ?? ProjectHelpers.DTE.ActiveDocument.FullName);
        }

        public static string GetFullUrl(string text, string sourceFilename)
        {
            if (string.IsNullOrEmpty(text))
                return null;

            text = text.Trim(new[] { '\'', '"', '~' });

            if (text.StartsWith("//", StringComparison.Ordinal))
                text = "http:" + text;

            if (text.Contains("://") || text.StartsWith("data:", StringComparison.Ordinal))
                return text;

            if (String.IsNullOrEmpty(sourceFilename))
                return null;

            text = Uri.UnescapeDataString(text);
            return ProjectHelpers.ToAbsoluteFilePath(text, sourceFilename);
        }

        public static void AddImageContent(IList<object> qiContent, string url)
        {
            BitmapSource source;
            try
            {
                source = LoadImage(url);
            }
            catch (Exception ex)
            {
                qiContent.Add(new Image { Source = noPreview });
                qiContent.Add(ex.Message);
                return;
            }

            if (source == null)
            {
                qiContent.Add(new Image { Source = noPreview });
                qiContent.Add("Couldn't locate " + url);
                return;
            }

            // HWNDs are always 32-bit.
            // https://twitter.com/Schabse/status/406159104697049088
            // http://msdn.microsoft.com/en-us/library/aa384203.aspx
            var screen = Screen.FromHandle(new IntPtr(ProjectHelpers.DTE.ActiveWindow.HWnd));
            Image image = new Image
            {
                Source = source,
                MaxWidth = screen.WorkingArea.Width / 2,
                MaxHeight = screen.WorkingArea.Height / 2,
                Stretch = Stretch.Uniform,
                StretchDirection = StretchDirection.DownOnly
            };
            qiContent.Add(image);

            // Use a TextBuffer to show dynamic text with
            // the correct default styling. The presenter
            // uses the same technique to show strings in
            // QuickInfoItemView.CreateTextBuffer().
            // Base64Tagger assumes that text from base64
            // images will never change. If that changes,
            // you must change that to handle changes.
            var size = WebEditor.ExportProvider.GetExport<ITextBufferFactoryService>().Value.CreateTextBuffer();
            size.SetText("Loading...");

            source.OnDownloaded(() => size.SetText(source.PixelWidth + "×" + source.PixelHeight));
            if (source.IsDownloading)
            {
                EventHandler<System.Windows.Media.ExceptionEventArgs> failure = (s, e) =>
                {
                    image.Source = noPreview;
                    size.SetText("Couldn't load image: " + e.ErrorException.Message);
                };
                source.DecodeFailed += failure;
                source.DownloadFailed += failure;
            }

            qiContent.Add(size);
        }

        private static BitmapFrame LoadImage(string url)
        {
            try
            {
                if (url.StartsWith("data:", StringComparison.Ordinal))
                {
                    int index = url.IndexOf("base64,", StringComparison.Ordinal) + 7;
                    byte[] imageBytes = Convert.FromBase64String(url.Substring(index));

                    using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
                    {
                        // Must cache OnLoad before the stream is disposed
                        return BitmapFrame.Create(ms, BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                    }
                }
                else if (url.Contains("://") || File.Exists(url))
                {
                    return BitmapFrame.Create(new Uri(url), BitmapCreateOptions.None, BitmapCacheOption.OnLoad);
                }
            }
            catch { }

            return null;
        }

        public void Dispose()
        {
            // Nothing to dispose
        }
    }
}
