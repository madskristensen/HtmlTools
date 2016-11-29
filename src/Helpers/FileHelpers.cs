using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.VisualStudio.Shell;

namespace HtmlTools
{
    public static class FileHelpers
    {
        ///<summary>Gets the currently selected point within a specific buffer type, or null if there is no selection or if the selection is in a different buffer.</summary>
        ///<param name="view">The TextView containing the selection</param>
        ///<param name="contentType">The ContentType to filter the selection by.</param>
        public static SnapshotPoint? GetSelection(this ITextView view, string contentType)
        {
            return view.BufferGraph.MapDownToInsertionPoint(view.Caret.Position.BufferPosition, PointTrackingMode.Positive, ts => ts.ContentType.IsOfType(contentType));
        }

        public static string ShowDialog(string extension, string fileName = "file.")
        {
            var initialPath = Path.GetDirectoryName(ProjectHelpers.DTE.ActiveDocument.FullName);

            using (var dialog = new SaveFileDialog())
            {
                dialog.FileName = fileName + extension;
                dialog.DefaultExt = extension;
                dialog.Filter = extension.ToUpperInvariant() + " files | *." + extension;
                dialog.InitialDirectory = initialPath;

                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    return dialog.FileName;
                }
            }

            return null;
        }

        public static string GetExtension(string mimeType)
        {
            switch (mimeType)
            {
                case "image/png":
                    return "png";

                case "image/jpg":
                case "image/jpeg":
                    return "jpg";

                case "image/gif":
                    return "gif";

                case "image/svg":
                    return "svg";

                case "font/x-woff":
                    return "woff";

                case "font/x-woff2":
                    return "woff2";

                case "font/otf":
                    return "otf";

                case "application/vnd.ms-fontobject":
                    return "eot";

                case "application/octet-stream":
                    return "ttf";
            }

            return null;
        }

        private static string GetMimeTypeFromFileExtension(string extension)
        {
            string ext = extension.TrimStart('.');

            switch (ext)
            {
                case "jpg":
                case "jpeg":
                    return "image/jpeg";
                case "svg":
                    return "image/svg+xml";
                case "png":
                case "gif":
                case "tiff":
                case "webp":
                case "bmp":
                    return "image/" + ext;

                case "woff":
                    return "font/x-woff";

                case "woff2":
                    return "font/x-woff2";

                case "otf":
                    return "font/otf";

                case "eot":
                    return "application/vnd.ms-fontobject";

                case "ttf":
                    return "application/octet-stream";

                default:
                    return "text/plain";
            }
        }

        public static bool SaveDataUriToFile(string dataUri, string filePath)
        {
            try
            {
                int index = dataUri.IndexOf("base64,", StringComparison.Ordinal) + 7;
                byte[] imageBytes = Convert.FromBase64String(dataUri.Substring(index));
                File.WriteAllBytes(filePath, imageBytes);
                ProjectHelpers.AddFileToActiveProject(filePath);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetMimeTypeFromBase64(string base64)
        {
            int end = base64.IndexOf(';');

            if (end > -1)
            {
                return base64.Substring(5, end - 5);
            }

            return string.Empty;
        }

        //public static string ConvertToBase64(string fileName)
        //{
        //    if (!File.Exists(fileName))
        //        return string.Empty;

        //    string format = "data:{0};base64,{1}";
        //    byte[] buffer = File.ReadAllBytes(fileName);
        //    string extension = Path.GetExtension(fileName).Substring(1);
        //    string contentType = GetMimeTypeFromFileExtension(extension);

        //    return string.Format(CultureInfo.InvariantCulture, format, contentType, Convert.ToBase64String(buffer));
        //}

        static char[] pathSplit = { '/', '\\' };

        public static string RelativePath(string absolutePath, string relativeTo)
        {
            relativeTo = relativeTo.Replace("\\/", "\\");

            string[] absDirs = absolutePath.Split(pathSplit);
            string[] relDirs = relativeTo.Split(pathSplit);

            // Get the shortest of the two paths
            int len = Math.Min(absDirs.Length, relDirs.Length);

            // Use to determine where in the loop we exited
            int lastCommonRoot = -1;
            int index;

            // Find common root
            for (index = 0; index < len; index++)
            {
                if (absDirs[index].Equals(relDirs[index], StringComparison.OrdinalIgnoreCase)) lastCommonRoot = index;
                else break;
            }

            // If we didn't find a common prefix then throw
            if (lastCommonRoot == -1)
            {
                return relativeTo;
            }

            // Build up the relative path
            StringBuilder relativePath = new StringBuilder();

            // Add on the ..
            for (index = lastCommonRoot + 2; index < absDirs.Length; index++)
            {
                if (absDirs[index].Length > 0) relativePath.Append("..\\");
            }

            // Add on the folders
            for (index = lastCommonRoot + 1; index < relDirs.Length - 1; index++)
            {
                relativePath.Append(relDirs[index] + "\\");
            }
            relativePath.Append(relDirs[relDirs.Length - 1]);

            return relativePath.Replace('\\', '/').ToString();
        }
    }
}
