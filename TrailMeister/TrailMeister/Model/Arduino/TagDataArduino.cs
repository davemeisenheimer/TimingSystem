using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TrailMeister.Model.Data;

namespace TrailMeister.Model.Arduino
{
    internal class TagDataArduino
    {
        private class TagLines
        {
            List<string> lines;
            internal TagLines(string[] array)
            {
                this.lines = new List<string>(array);
            }

            public List<string> Lines { get { return this.lines; } }
        }

        internal static List<ReaderData> getReaderData(string msg)
        {
            // We may get data for multiple tags.  Each tag is multiple lines:
            // - the first line has the number of bytes (lines) that will be contained in the EPC
            // - the subsequent X number of lines has those bytes
            // - the last line has the timestamp of the last recorded lap
            string[] lines = msg.Split(
                new string[] { Environment.NewLine },
                StringSplitOptions.None
            );
            List<TagLines> tags = getTagLines(lines);
            return processTagList(tags);
        }

        private static List<ReaderData> processTagList(List<TagLines> tags)
        {
            List<ReaderData> result = new List<ReaderData>();

            foreach (TagLines tl in tags)
            {
                ReaderData readerData = getDataObect(tl);
                result.Add(readerData);
            }

            return result;
        }

        private static ReaderData getDataObect(TagLines tag)
        {
            //byte[] bytes = getEpcBytesFromTaglines(tag.Lines); 
            string epc = getEpcString(tag.Lines);
            string rfidLapTimeStamp = tag.Lines[tag.Lines.Count - 1];
            return new ReaderData(epc, ulong.Parse(rfidLapTimeStamp, System.Globalization.NumberStyles.HexNumber));
        }

        private static string getEpcString(List<string> lines)
        {
            string epcByteCountStr = lines[0].Trim();
            int epcByteCount = int.Parse(epcByteCountStr);

            string epc = "";

            for (int i = 1; i <= epcByteCount; i++)
            {
                epc += int.Parse(lines[i].Trim(), System.Globalization.NumberStyles.HexNumber).ToString("X2");

                //byte myByte = getByte(lines[i].Trim());
                //bytes[i - 1] = myByte;
            }
            return epc;
        }

        private static byte getByte(string line)
        {
            byte[] myByte = Encoding.Default.GetBytes(line.Trim());

            if (myByte.Length > 1)
            {
                throw new Exception("Something wrong with bytes received.  Expected each line of data to contain one byte, but got " + myByte.Length);
            }

            return myByte[0];
        }

        private static List<TagLines> getTagLines(string[] lines)
        {
            int i = 0;
            int beginTag = 0;
            List<TagLines> tagData = new List<TagLines>();

            foreach (string line in lines)
            {
                string l = line.Trim();

                if (l == ITagDataSource.END_TAG_DATA) // Match with arduino code
                {
                    // End of current tag
                    int elementCnt = i - beginTag;
                    string[] tagLines = new string[elementCnt];
                    Array.Copy(lines, beginTag, tagLines, 0, elementCnt);
                    tagData.Add(new TagLines(tagLines));
                    beginTag = i + 1;
                }
                i++;
            }

            return tagData;
        }
    }
}
