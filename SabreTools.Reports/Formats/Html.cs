using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;

using SabreTools.Logging;

namespace SabreTools.Reports.Formats
{
    /// <summary>
    /// HTML report format
    /// </summary>
    /// TODO: Make output standard width, without making the entire thing a table
    internal class Html : BaseReport
    {
        /// <summary>
        /// Create a new report from the filename
        /// </summary>
        /// <param name="statsList">List of statistics objects to set</param>
        public Html(List<DatStatistics> statsList)
            : base(statsList)
        {
        }

        /// <inheritdoc/>
        public override bool WriteToFile(string outfile, bool baddumpCol, bool nodumpCol, bool throwOnError = false)
        {
            InternalStopwatch watch = new InternalStopwatch($"Writing statistics to '{outfile}");

            try
            {
                // Try to create the output file
                FileStream fs = File.Create(outfile);
                if (fs == null)
                {
                    logger.Warning($"File '{outfile}' could not be created for writing! Please check to see if the file is writable");
                    return false;
                }

                XmlTextWriter xtw = new XmlTextWriter(fs, Encoding.UTF8)
                {
                    Formatting = Formatting.Indented,
                    IndentChar = '\t',
                    Indentation = 1
                };

                // Write out the header
                WriteHeader(xtw, baddumpCol, nodumpCol);

                // Now process each of the statistics
                for (int i = 0; i < Statistics.Count; i++)
                {
                    // Get the current statistic
                    DatStatistics stat = Statistics[i];

                    // If we have a directory statistic
                    if (stat.IsDirectory)
                    {
                        WriteMidSeparator(xtw, baddumpCol, nodumpCol);
                        WriteIndividual(xtw, stat, baddumpCol, nodumpCol);
                        
                        // If we have anything but the last value, write the separator
                        if (i < Statistics.Count - 1)
                        {
                            WriteFooterSeparator(xtw, baddumpCol, nodumpCol);
                            WriteMidHeader(xtw, baddumpCol, nodumpCol);
                        }
                    }

                    // If we have a normal statistic
                    else
                    {
                        WriteIndividual(xtw, stat, baddumpCol, nodumpCol);
                    }
                }

                WriteFooter(xtw);
                xtw.Dispose();
                fs.Dispose();
            }
            catch (Exception ex) when (!throwOnError)
            {
                logger.Error(ex);
                return false;
            }
            finally
            {
                watch.Stop();
            }

            return true;
        }

        /// <summary>
        /// Write out the header to the stream, if any exists
        /// </summary>
        /// <param name="xtw">XmlTextWriter to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteHeader(XmlTextWriter xtw, bool baddumpCol, bool nodumpCol)
        {
            xtw.WriteDocType("html", null, null, null);
            xtw.WriteStartElement("html");

            xtw.WriteStartElement("header");
            xtw.WriteElementString("title", "DAT Statistics Report");
            xtw.WriteElementString("style", @"
body {
    background-color: lightgray;
}
.dir {
    color: #0088FF;
}");
            xtw.WriteEndElement(); // header

            xtw.WriteStartElement("body");

            xtw.WriteElementString("h2", $"DAT Statistics Report ({DateTime.Now.ToShortDateString()})");

            xtw.WriteStartElement("table");
            xtw.WriteAttributeString("border", "1");
            xtw.WriteAttributeString("cellpadding", "5");
            xtw.WriteAttributeString("cellspacing", "0");
            xtw.Flush();

            // Now write the mid header for those who need it
            WriteMidHeader(xtw, baddumpCol, nodumpCol);
        }

        /// <summary>
        /// Write out the mid-header to the stream, if any exists
        /// </summary>
        /// <param name="xtw">XmlTextWriter to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteMidHeader(XmlTextWriter xtw, bool baddumpCol, bool nodumpCol)
        {
            xtw.WriteStartElement("tr");
            xtw.WriteAttributeString("bgcolor", "gray");

            xtw.WriteElementString("th", "File Name");

            xtw.WriteStartElement("th");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString("Total Size");
            xtw.WriteEndElement(); // th

            xtw.WriteStartElement("th");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString("Games");
            xtw.WriteEndElement(); // th

            xtw.WriteStartElement("th");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString("Roms");
            xtw.WriteEndElement(); // th

            xtw.WriteStartElement("th");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString("Disks");
            xtw.WriteEndElement(); // th

            xtw.WriteStartElement("th");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString("# with CRC");
            xtw.WriteEndElement(); // th

            xtw.WriteStartElement("th");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString("# with MD5");
            xtw.WriteEndElement(); // th

            xtw.WriteStartElement("th");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString("# with SHA-1");
            xtw.WriteEndElement(); // th

            xtw.WriteStartElement("th");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString("# with SHA-256");
            xtw.WriteEndElement(); // th

            if (baddumpCol)
            {
                xtw.WriteStartElement("th");
                xtw.WriteAttributeString("align", "right");
                xtw.WriteString("Baddumps");
                xtw.WriteEndElement(); // th
            }

            if (nodumpCol)
            {
                xtw.WriteStartElement("th");
                xtw.WriteAttributeString("align", "right");
                xtw.WriteString("Nodumps");
                xtw.WriteEndElement(); // th
            }

            xtw.WriteEndElement(); // tr
            xtw.Flush();
        }

        /// <summary>
        /// Write a single set of statistics
        /// </summary>
        /// <param name="xtw">XmlTextWriter to write to</param>
        /// <param name="stat">DatStatistics object to write out</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteIndividual(XmlTextWriter xtw, DatStatistics stat, bool baddumpCol, bool nodumpCol)
        {
            bool isDirectory = stat.DisplayName.StartsWith("DIR: ");

            xtw.WriteStartElement("tr");
            if (isDirectory)
                xtw.WriteAttributeString("class", "dir");
            
            xtw.WriteElementString("td", isDirectory ? WebUtility.HtmlEncode(stat.DisplayName.Remove(0, 5)) : WebUtility.HtmlEncode(stat.DisplayName));

            xtw.WriteStartElement("td");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString(GetBytesReadable(stat.Statistics.TotalSize));
            xtw.WriteEndElement(); // td

            xtw.WriteStartElement("td");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString(stat.MachineCount.ToString());
            xtw.WriteEndElement(); // td

            xtw.WriteStartElement("td");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString(stat.Statistics.RomCount.ToString());
            xtw.WriteEndElement(); // td

            xtw.WriteStartElement("td");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString(stat.Statistics.DiskCount.ToString());
            xtw.WriteEndElement(); // td

            xtw.WriteStartElement("td");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString(stat.Statistics.CRCCount.ToString());
            xtw.WriteEndElement(); // td

            xtw.WriteStartElement("td");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString(stat.Statistics.MD5Count.ToString());
            xtw.WriteEndElement(); // td

            xtw.WriteStartElement("td");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString(stat.Statistics.SHA1Count.ToString());
            xtw.WriteEndElement(); // td

            xtw.WriteStartElement("td");
            xtw.WriteAttributeString("align", "right");
            xtw.WriteString(stat.Statistics.SHA256Count.ToString());
            xtw.WriteEndElement(); // td

            if (baddumpCol)
            {
                xtw.WriteStartElement("td");
                xtw.WriteAttributeString("align", "right");
                xtw.WriteString(stat.Statistics.BaddumpCount.ToString());
                xtw.WriteEndElement(); // td
            }

            if (nodumpCol)
            {
                xtw.WriteStartElement("td");
                xtw.WriteAttributeString("align", "right");
                xtw.WriteString(stat.Statistics.NodumpCount.ToString());
                xtw.WriteEndElement(); // td
            }

            xtw.WriteEndElement(); // tr
            xtw.Flush();
        }

        /// <summary>
        /// Write out the separator to the stream, if any exists
        /// </summary>
        /// <param name="xtw">XmlTextWriter to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteMidSeparator(XmlTextWriter xtw, bool baddumpCol, bool nodumpCol)
        {
            xtw.WriteStartElement("tr");

            xtw.WriteStartElement("td");
            xtw.WriteAttributeString("colspan", baddumpCol && nodumpCol ? "12" : (baddumpCol ^ nodumpCol ? "11" : "10"));
            xtw.WriteEndElement(); // td

            xtw.WriteEndElement(); // tr
            xtw.Flush();
        }

        /// <summary>
        /// Write out the footer-separator to the stream, if any exists
        /// </summary>
        /// <param name="xtw">XmlTextWriter to write to</param>
        /// <param name="baddumpCol">True if baddumps should be included in output, false otherwise</param>
        /// <param name="nodumpCol">True if nodumps should be included in output, false otherwise</param>
        private void WriteFooterSeparator(XmlTextWriter xtw, bool baddumpCol, bool nodumpCol)
        {
            xtw.WriteStartElement("tr");
            xtw.WriteAttributeString("border", "0");

            xtw.WriteStartElement("td");
            xtw.WriteAttributeString("colspan", baddumpCol && nodumpCol ? "12" : (baddumpCol ^ nodumpCol ? "11" : "10"));
            xtw.WriteEndElement(); // td

            xtw.WriteEndElement(); // tr
            xtw.Flush();
        }

        /// <summary>
        /// Write out the footer to the stream, if any exists
        /// </summary>
        /// <param name="xtw">XmlTextWriter to write to</param>
        private void WriteFooter(XmlTextWriter xtw)
        {
            xtw.WriteEndElement(); // table
            xtw.WriteEndElement(); // body
            xtw.WriteEndElement(); // html
            xtw.Flush();
        }
    }
}
