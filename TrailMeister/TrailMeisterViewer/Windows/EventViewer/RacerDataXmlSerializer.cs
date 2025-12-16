using System.Xml.Serialization;
using System.IO;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Xsl;
using System.Xml.Linq;
using System.Linq;
using TrailMeisterDb;

namespace TrailMeisterViewer.Windows.EventViewer
{
    internal class RacerDataXmlSerializer
    {
        internal static void ExportEventToHtml(DbEvent ev, List<RacerData> racers)
        {
            ExportEventToHtml(ev, racers, "AllRacers.html");

            foreach (var racer in racers)
            {
                List<RacerData> list = new List<RacerData>();
                list.Add(racer);

                ExportEventToHtml(
                    ev, 
                    list, 
                    "-" + racer.Person.FirstName + "_" 
                    + racer.Person.LastName + " (" 
                    + racer.Person.NickName 
                    + ").html");
            }
        }

        internal static void ExportEventToHtml(DbEvent ev, List<RacerData> racers, string htmlFileName)
        {
            XDocument doc = new XDocument(
                                new XElement("Event",
                                    new XElement("EventName", ev.EventName),
                                    new XElement("EventDate", ev.EventDate),
                                        new XElement("Racers",
                                        racers.Select(r => new XElement("Racer",
                                            new XElement("FirstName", r.Person.FirstName),
                                            new XElement("LastName", r.Person.LastName),
                                            new XElement("NickName", r.Person.NickName),
                                            new XElement("Association", r.Person.Association),
                                            new XElement("EventLaps",
                                                r.EventLaps.Select(lap => new XElement("Lap",
                                                    new XElement("LapNumber", lap.LapCount),
                                                    new XElement("LapTime", lap.LapTime)
                                                ))
                                            )
                                        ))
                                        )
                                     ));
            string xsltPath = Path.Combine(
                                System.AppContext.BaseDirectory,
                                "Windows",
                                "EventViewer",
                                "Event.xslt");
            TransformXmlToHtml(
                doc, 
                xsltPath, 
                Path.Combine(
                    @"C:\Users\davem\",
                    ev.EventName + "_" + htmlFileName));
        }

        private static void TransformXmlToHtml(XDocument xmlDoc, string xsltPath, string outputHtmlPath)
        {
            var xslt = new XslCompiledTransform();
            xslt.Load(xsltPath);

            using var xmlReader = xmlDoc.CreateReader();
            using (var writer = XmlWriter.Create(outputHtmlPath, xslt.OutputSettings))
            {
                xslt.Transform(xmlReader, writer);
            }
        }
    }
}

