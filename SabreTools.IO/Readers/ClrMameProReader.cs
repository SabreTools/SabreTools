using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SabreTools.IO.Readers
{
    public class ClrMameProReader : IDisposable
    {
        #region Constants

        public const string HeaderPatternCMP = @"(^.*?) \($";
        public const string InternalPatternCMP = @"(^\S*?) (\(.+\))$";
        public const string InternalPatternAttributesCMP = @"[^\s""]+|""[^""]*""";
        //public const string InternalPatternAttributesCMP = @"([^\s]*""[^""]+""[^\s]*)|[^""]?\w+[^""]?";
        public const string ItemPatternCMP = @"^\s*(\S*?) (.*)";
        public const string EndPatternCMP = @"^\s*\)\s*$";

        #endregion

        /// <summary>
        /// Internal stream reader for inputting
        /// </summary>
        private readonly StreamReader sr;

        /// <summary>
        /// Contents of the current line, unprocessed
        /// </summary>
        public string CurrentLine { get; private set; } = string.Empty;

        /// <summary>
        /// Get the current line number
        /// </summary>
        public long LineNumber { get; private set; } = 0;

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
        /// Get if quotes should surround attribute values
        /// </summary>
        /// <remarks>
        /// If this is disabled, then a special bit of code will be
        /// invoked to deal with unquoted, multi-part names. This can
        /// backfire in a lot of circumstances, so don't disable this
        /// unless you know what you're doing
        /// </remarks>
        public bool Quotes { get; set; } = true;

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
        }

        /// <summary>
        /// Constructor for opening a write from a stream and encoding
        /// </summary>
        public ClrMameProReader(Stream stream, Encoding encoding)
        {
            sr = new StreamReader(stream, encoding);
        }

        /// <summary>
        /// Read the next line in the file
        /// </summary>
        public bool ReadNextLine()
        {
            if (!(sr.BaseStream?.CanRead ?? false) || sr.EndOfStream)
                return false;

            CurrentLine = sr.ReadLine().Trim();
            LineNumber++;
            ProcessLine();
            return true;
        }

        /// <summary>
        /// Process the current line and extract out values
        /// </summary>
        private void ProcessLine()
        {
            // Standalone (special case for DC dats)
            if (CurrentLine.StartsWith("Name:"))
            {
                string temp = CurrentLine["Name:".Length..].Trim();
                CurrentLine = $"Name: {temp}";
            }

            // Comment
            if (CurrentLine.StartsWith("#"))
            {
                Internal = null;
                InternalName = null;
                RowType = CmpRowType.Comment;
                Standalone = null;
            }

            // Top-level
            else if (Regex.IsMatch(CurrentLine, HeaderPatternCMP))
            {
                GroupCollection gc = Regex.Match(CurrentLine, HeaderPatternCMP).Groups;
                string normalizedValue = gc[1].Value.ToLowerInvariant();

                Internal = null;
                InternalName = null;
                RowType = CmpRowType.TopLevel;
                Standalone = null;
                TopLevel = normalizedValue;
            }

            // Internal
            else if (Regex.IsMatch(CurrentLine, InternalPatternCMP))
            {
                GroupCollection gc = Regex.Match(CurrentLine, InternalPatternCMP).Groups;
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
                            while (++i < linegc.Length
                                && linegc[i] != "size"
                                && linegc[i] != "date"
                                && linegc[i] != "crc")
                            {
                                value += $" {linegc[i]}";
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
                    // Special case for assumed unquoted values (only affects `name`)
                    else if (!Quotes && key == "name")
                    {
                        while (++i < linegc.Length
                                && linegc[i] != "merge"
                                && linegc[i] != "size"
                                && linegc[i] != "crc"
                                && linegc[i] != "md5"
                                && linegc[i] != "sha1")
                        {
                            value += $" {linegc[i]}";
                        }

                        value = value.Trim();
                        i--;
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
                            try
                            {
                                value = linegc[++i].Replace("\"", string.Empty);
                            }
                            catch (Exception ex)
                            {
                                // no useful line found
                            }
                        }
                    }

                    Internal[key] = value;
                    RowType = CmpRowType.Internal;
                    Standalone = null;
                }    

                InternalName = normalizedValue;
            }

            // Standalone
            else if (Regex.IsMatch(CurrentLine, ItemPatternCMP))
            {
                GroupCollection gc = Regex.Match(CurrentLine, ItemPatternCMP).Groups;
                string itemval = gc[2].Value.Replace("\"", string.Empty);

                Internal = null;
                InternalName = null;
                RowType = CmpRowType.Standalone;
                Standalone = new KeyValuePair<string, string>(gc[1].Value, itemval);
            }

            // End section
            else if (Regex.IsMatch(CurrentLine, EndPatternCMP))
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
                .Matches(s, InternalPatternAttributesCMP)
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
