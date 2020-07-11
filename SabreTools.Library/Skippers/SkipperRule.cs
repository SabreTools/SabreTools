using System;
using System.Collections.Generic;
using System.IO;

using SabreTools.Library.Data;
using SabreTools.Library.Tools;

namespace SabreTools.Library.Skippers
{
    public class SkipperRule
    {
        #region Fields

        /// <summary>
        /// Starting offset for applying rule
        /// </summary>
        public long? StartOffset { get; set; } // null is EOF

        /// <summary>
        /// Ending offset for applying rule
        /// </summary>
        public long? EndOffset { get; set; } // null if EOF

        /// <summary>
        /// Byte manipulation operation
        /// </summary>
        public HeaderSkipOperation Operation { get; set; }

        /// <summary>
        /// List of matching tests in a rule
        /// </summary>
        public List<SkipperTest> Tests { get; set; }

        /// <summary>
        /// Filename the skipper rule lives in
        /// </summary>
        public string SourceFile { get; set; }

        #endregion

        /// <summary>
        /// Transform an input file using the given rule
        /// </summary>
        /// <param name="input">Input file name</param>
        /// <param name="output">Output file name</param>
        /// <returns>True if the file was transformed properly, false otherwise</returns>
        public bool TransformFile(string input, string output)
        {
            // If the input file doesn't exist, fail
            if (!File.Exists(input))
            {
                Globals.Logger.Error($"I'm sorry but '{input}' doesn't exist!");
                return false;
            }

            // Create the output directory if it doesn't already
            DirectoryExtensions.Ensure(Path.GetDirectoryName(output));

            Globals.Logger.User($"Attempting to apply rule to '{input}'");
            bool success = TransformStream(FileExtensions.TryOpenRead(input), FileExtensions.TryCreate(output));

            // If the output file has size 0, delete it
            if (new FileInfo(output).Length == 0)
            {
                FileExtensions.TryDelete(output);
                success = false;
            }

            return success;
        }

        /// <summary>
        /// Transform an input stream using the given rule
        /// </summary>
        /// <param name="input">Input stream</param>
        /// <param name="output">Output stream</param>
        /// <param name="keepReadOpen">True if the underlying read stream should be kept open, false otherwise</param>
        /// <param name="keepWriteOpen">True if the underlying write stream should be kept open, false otherwise</param>
        /// <returns>True if the file was transformed properly, false otherwise</returns>
        public bool TransformStream(Stream input, Stream output, bool keepReadOpen = false, bool keepWriteOpen = false)
        {
            bool success = true;

            // If the sizes are wrong for the values, fail
            long extsize = input.Length;
            if ((Operation > HeaderSkipOperation.Bitswap && (extsize % 2) != 0)
                || (Operation > HeaderSkipOperation.Byteswap && (extsize % 4) != 0)
                || (Operation > HeaderSkipOperation.Bitswap && (StartOffset == null || StartOffset % 2 == 0)))
            {
                Globals.Logger.Error("The stream did not have the correct size to be transformed!");
                return false;
            }

            // Now read the proper part of the file and apply the rule
            BinaryWriter bw = null;
            BinaryReader br = null;
            try
            {
                Globals.Logger.User("Applying found rule to input stream");
                bw = new BinaryWriter(output);
                br = new BinaryReader(input);

                // Seek to the beginning offset
                if (StartOffset == null)
                    success = false;

                else if (Math.Abs((long)StartOffset) > input.Length)
                    success = false;

                else if (StartOffset > 0)
                    input.Seek((long)StartOffset, SeekOrigin.Begin);

                else if (StartOffset < 0)
                    input.Seek((long)StartOffset, SeekOrigin.End);

                // Then read and apply the operation as you go
                if (success)
                {
                    byte[] buffer = new byte[4];
                    int pos = 0;
                    while (input.Position < (EndOffset ?? input.Length)
                        && input.Position < input.Length)
                    {
                        byte b = br.ReadByte();
                        switch (Operation)
                        {
                            case HeaderSkipOperation.Bitswap:
                                // http://stackoverflow.com/questions/3587826/is-there-a-built-in-function-to-reverse-bit-order
                                uint r = b;
                                int s = 7;
                                for (b >>= 1; b != 0; b >>= 1)
                                {
                                    r <<= 1;
                                    r |= (byte)(b & 1);
                                    s--;
                                }
                                r <<= s;
                                buffer[pos] = (byte)r;
                                break;

                            case HeaderSkipOperation.Byteswap:
                                if (pos % 2 == 1)
                                {
                                    buffer[pos - 1] = b;
                                }
                                if (pos % 2 == 0)
                                {
                                    buffer[pos + 1] = b;
                                }
                                break;

                            case HeaderSkipOperation.Wordswap:
                                buffer[3 - pos] = b;
                                break;

                            case HeaderSkipOperation.WordByteswap:
                                buffer[(pos + 2) % 4] = b;
                                break;

                            case HeaderSkipOperation.None:
                            default:
                                buffer[pos] = b;
                                break;
                        }

                        // Set the buffer position to default write to
                        pos = (pos + 1) % 4;

                        // If we filled a buffer, flush to the stream
                        if (pos == 0)
                        {
                            bw.Write(buffer);
                            bw.Flush();
                            buffer = new byte[4];
                        }
                    }

                    // If there's anything more in the buffer, write only the left bits
                    for (int i = 0; i < pos; i++)
                    {
                        bw.Write(buffer[i]);
                    }
                }
            }
            catch (Exception ex)
            {
                Globals.Logger.Error(ex.ToString());
                return false;
            }
            finally
            {
                // If we're not keeping the read stream open, dispose of the binary reader
                if (!keepReadOpen)
                    br?.Dispose();

                // If we're not keeping the write stream open, dispose of the binary reader
                if (!keepWriteOpen)
                    bw?.Dispose();
            }

            return success;
        }
    }

    /// <summary>
    /// Intermediate class for storing Skipper Test information
    /// </summary>
    public struct SkipperTest
    {
        /// <summary>
        /// Type of test to be run
        /// </summary>
        public HeaderSkipTest Type { get; set; }

        /// <summary>
        /// File offset to run the test
        /// </summary>
        public long? Offset { get; set; } // null is EOF

        /// <summary>
        /// Static value to be checked at the offset
        /// </summary>
        public byte[] Value { get; set; }

        /// <summary>
        /// Determines whether a pass or failure is expected
        /// </summary>
        public bool Result { get; set; }

        /// <summary>
        /// Byte mask to be applied to the tested bytes
        /// </summary>
        public byte[] Mask { get; set; }

        /// <summary>
        /// Expected size of the input byte array, used with the Operator
        /// </summary>
        public long? Size { get; set; } // null is PO2, "power of 2" filesize

        /// <summary>
        /// Expected range value for the input byte array size, used with Size
        /// </summary>
        public HeaderSkipTestFileOperator Operator { get; set; }
    }
}
