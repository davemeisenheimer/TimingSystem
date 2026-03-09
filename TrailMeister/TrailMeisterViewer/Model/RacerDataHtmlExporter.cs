using System.Diagnostics;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;

namespace TrailMeisterViewer.Model
{
    internal abstract class RacerDataHtmlExporter
    {
        protected static string OutputBaseDirectory => TrailMeisterDb.AppSettings.Current.ExportOutputDirectory;

        internal abstract void Export();

        protected static void TransformXmlToHtml(XDocument xmlDoc, string xsltPath, string outputHtmlPath)
        {
            var xslt = new XslCompiledTransform();
            xslt.Load(xsltPath);

            using var xmlReader = xmlDoc.CreateReader();
            using var writer = XmlWriter.Create(outputHtmlPath, xslt.OutputSettings);
            xslt.Transform(xmlReader, writer);
        }

        protected static void OpenOutputFolder(string folderPath)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = folderPath,
                UseShellExecute = true
            });
        }
    }
}
