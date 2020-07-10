using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

using SabreTools.Library.Data;
using SabreTools.Library.Tools;

namespace SabreTools.Library.Readers
{
    public class ClrMameProReader : IDisposable
    {
        /// <summary>
        /// Internal stream reader for inputting
        /// </summary>
        private StreamReader sr;

        /// <summary>
        /// Get if at end of stream
        /// </summary>
        public bool EndOfStream
        {
            get
            {
                return sr?.EndOfStream ?? true;
            }
        }

        /// <summary>
        /// Contents of the currently read line as an internal item
        /// </summary>
        public Dictionary<string, string> Internal { get; private set; } = new Dictionary<string, string>();

        /// <summary>
        /// Current internal item name
        /// </summary>
        public string InternalName { get; private set; } = null;

        /// <summary>
        /// Get if we should be making DosCenter exceptions
        /// </summary>
        public bool DosCenter { get; set; } = false;

        /// <summary>
        /// Current row type
        /// </summary>
        public CmpRowType RowType { get; private set; } = CmpRowType.None;

        /// <summary>
        /// Contents of the currently read line as a standalone item
        /// </summary>
        public KeyValuePair<string, string>? Standalone { get; private set; } = null;

        /// <summary>
        /// Current top-level being read
        /// </summary>
        public string TopLevel { get; private set; } = string.Empty;

        /// <summary>
        /// Constructor for opening a write from a file
        /// </summary>
        public ClrMameProReader(string filename)
        {
            sr = new StreamReader(filename);
            DosCenter = true;
        }

        /// <summary>
        /// Constructor for opening a write from a stream and encoding
        /// </summary>
        public ClrMameProReader(Stream stream, Encoding encoding)
        {
            sr = new StreamReader(stream, encoding);
            DosCenter = true;
        }

        /// <summary>
        /// Read the next line in the file
        /// </summary>
        public bool ReadNextLine()
        {
            if (!(sr.BaseStream?.CanRead ?? false) || sr.EndOfStream)
                return false;

            string line = sr.ReadLine().Trim();
            ProcessLine(line);
            return true;
        }

        /// <summary>
        /// Process the current line and extract out values
        /// </summary>
        private void ProcessLine(string line)
        {
            // Standalone (special case for DC dats)
            if (line.StartsWith("Name:"))
            {
                string temp = line.Substring("Name:".Length).Trim();
                line = $"Name: {temp}";
            }

            // Comment
            if (line.StartsWith("#"))
            {
                Internal = null;
                InternalName = null;
                RowType = CmpRowType.Comment;
                Standalone = null;
            }

            // Top-level
            else if (Regex.IsMatch(line, Constants.HeaderPatternCMP))
            {
                GroupCollection gc = Regex.Match(line, Constants.HeaderPatternCMP).Groups;
                string normalizedValue = gc[1].Value.ToLowerInvariant();

                Internal = null;
                InternalName = null;
                RowType = CmpRowType.TopLevel;
                Standalone = null;
                TopLevel = normalizedValue;
            }

            // Internal
            else if (Regex.IsMatch(line, Constants.InternalPatternCMP))
            {
                GroupCollection gc = Regex.Match(line, Constants.InternalPatternCMP).Groups;
                string normalizedValue = gc[1].Value.ToLowerInvariant();
                string[] linegc = SplitLineAsCMP(gc[2].Value);

                Internal = new Dictionary<string, string>();
                for (int i = 0; i < linegc.Length; i++)
                {
                    string key = linegc[i].Replace("\"", string.Empty);
                    if (string.IsNullOrWhiteSpace(key))
                        continue;

                    string value = string.Empty;

                    // Special case for DC-style dats, only a few known fields
                    if (DosCenter)
                    {
                        // If we have a name
                        if (key == "name")
                        {
                            while (++i < linegc.Length && linegc[i] != "size" && linegc[i] != "date" && linegc[i] != "crc")
                            {
                                value += $"{linegc[i]}";
                            }

                            value = value.Trim();
                            i--;
                        }
                        // If we have a date (split into 2 parts)
                        else if (key == "date")
                        {
                            value = $"{linegc[++i].Replace("\"", string.Empty)} {linegc[++i].Replace("\"", string.Empty)}";
                        }
                        // Default case
                        else
                        {
                            value = linegc[++i].Replace("\"", string.Empty);
                        }
                    }
                    else
                    {
                        // Special cases for standalone statuses
                        if (key == "baddump" || key == "good" || key == "nodump" || key == "verified")
                        {
                            value = key;
                            key = "status";
                        }
                        // Special case for standalone sample
                        else if (normalizedValue == "sample")
                        {
                            value = key;
                            key = "name";
                        }
                        // Default case
                        else
                        {
                            value = linegc[++i].Replace("\"", string.Empty);
                        }
                    }

                    Internal[key] = value;
                    RowType = CmpRowType.Internal;
                    Standalone = null;
                }    

                InternalName = normalizedValue;
            }

            // Standalone
            else if (Regex.IsMatch(line, Constants.ItemPatternCMP))
            {
                GroupCollection gc = Regex.Match(line, Constants.ItemPatternCMP).Groups;
                string itemval = gc[2].Value.Replace("\"", string.Empty);

                Internal = null;
                InternalName = null;
                RowType = CmpRowType.Standalone;
                Standalone = new KeyValuePair<string, string>(gc[1].Value, itemval);
            }

            // End section
            else if (Regex.IsMatch(line, Constants.EndPatternCMP))
            {
                Internal = null;
                InternalName = null;
                RowType = CmpRowType.EndTopLevel;
                Standalone = null;
                TopLevel = null;
            }

            // Invalid (usually whitespace)
            else
            {
                Internal = null;
                InternalName = null;
                RowType = CmpRowType.None;
                Standalone = null;
            }
        }

        /// <summary>
        /// Split a line as if it were a CMP rom line
        /// </summary>
        /// <param name="s">Line to split</param>
        /// <returns>Line split</returns>
        /// <remarks>Uses code from http://stackoverflow.com/questions/554013/regular-expression-to-split-on-spaces-unless-in-quotes</remarks>
        private string[] SplitLineAsCMP(string s)
        {
            // Get the opening and closing brace locations
            int openParenLoc = s.IndexOf('(');
            int closeParenLoc = s.LastIndexOf(')');

            // Now remove anything outside of those braces, including the braces
            s = s.Substring(openParenLoc + 1, closeParenLoc - openParenLoc - 1);
            s = s.Trim();

            // Now we get each string, divided up as cleanly as possible
            string[] matches = Regex
                .Matches(s, Constants.InternalPatternAttributesCMP)
                .Cast<Match>()
                .Select(m => m.Groups[0].Value)
                .ToArray();

            return matches;
        }

        /// <summary>
        /// Dispose of the underlying reader
        /// </summary>
        public void Dispose()
        {
            sr.Dispose();
        }
    }
}
