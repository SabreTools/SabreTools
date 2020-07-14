using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using SabreTools.Library.Data;
using SabreTools.Library.FileTypes;
using Compress.ThreadReaders;

namespace SabreTools.Library.Tools
{
    /// <summary>
    /// Extensions to Stream functionality
    /// </summary>
    public static class StreamExtensions
    {
        /// <summary>
        /// Add an aribtrary number of bytes to the inputted stream
        /// </summary>
        /// <param name="input">Stream to be appended to</param>
        /// <param name="output">Outputted stream</param>
        /// <param name="bytesToAddToHead">Bytes to be added to head of stream</param>
        /// <param name="bytesToAddToTail">Bytes to be added to tail of stream</param>
        public static void AppendBytes(Stream input, Stream output, byte[] bytesToAddToHead, byte[] bytesToAddToTail)
        {
            // Write out prepended bytes
            if (bytesToAddToHead != null && bytesToAddToHead.Count() > 0)
                output.Write(bytesToAddToHead, 0, bytesToAddToHead.Length);

            // Now copy the existing file over
            input.CopyTo(output);

            // Write out appended bytes
            if (bytesToAddToTail != null && bytesToAddToTail.Count() > 0)
                output.Write(bytesToAddToTail, 0, bytesToAddToTail.Length);
        }

        /// <summary>
        /// Retrieve file information for a single file
        /// </summary>
        /// <param name="input">Filename to get information from</param>
        /// <param name="size">Size of the input stream</param>
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated (defaults to none)</param>
        /// <param name="keepReadOpen">True if the underlying read stream should be kept open, false otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        /// <returns>Populated BaseFile object if success, empty one on error</returns>
        public static BaseFile GetInfo(this Stream input, long size = -1, Hash omitFromScan = 0x0, bool keepReadOpen = false, bool chdsAsFiles = true)
        {
            return GetInfoAsync(input, size, omitFromScan, keepReadOpen, chdsAsFiles).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Retrieve file information for a single file
        /// </summary>
        /// <param name="input">Filename to get information from</param>
        /// <param name="size">Size of the input stream</param>
        /// <param name="omitFromScan">Hash flag saying what hashes should not be calculated (defaults to none)</param>
        /// <param name="keepReadOpen">True if the underlying read stream should be kept open, false otherwise</param>
        /// <param name="chdsAsFiles">True if CHDs should be treated like regular files, false otherwise</param>
        /// <returns>Populated BaseFile object if success, empty one on error</returns>
        public static async Task<BaseFile> GetInfoAsync(Stream input, long size = -1, Hash omitFromScan = 0x0, bool keepReadOpen = false, bool chdsAsFiles = true)
        {
            // If we want to automatically set the size
            if (size == -1)
                size = input.Length;

            // We first check to see if it's a CHD if we have to
            if (!chdsAsFiles)
            {
                var chd = CHDFile.Create(input);
                input.SeekIfPossible();

                // If we found a valid CHD
                if (chd != null)
                {
                    if (!keepReadOpen)
                        input.Dispose();

                    return chd;
                }
            }

            BaseFile rom = new BaseFile()
            {
                Size = size,
            };

            try
            {
                // Get a list of hashers to run over the buffer
                List<Hasher> hashers = new List<Hasher>()
                {
                    new Hasher(Hash.CRC),
                    new Hasher(Hash.MD5),
#if NET_FRAMEWORK
                    new Hasher(Hash.RIPEMD160),
#endif
                    new Hasher(Hash.SHA1),
                    new Hasher(Hash.SHA256),
                    new Hasher(Hash.SHA384),
                    new Hasher(Hash.SHA512),
                };

                // Initialize the hashing helpers
                var loadBuffer = new ThreadLoadBuffer(input);
                int buffersize = 3 * 1024 * 1024;
                byte[] buffer0 = new byte[buffersize];
                byte[] buffer1 = new byte[buffersize];

                /*
                Please note that some of the following code is adapted from
                RomVault. This is a modified version of how RomVault does
                threaded hashing. As such, some of the terminology and code
                is the same, though variable names and comments may have
                been tweaked to better fit this code base.
                */

                // Pre load the first buffer
                int next = size > buffersize ? buffersize : (int)size;
                input.Read(buffer0, 0, next);
                int current = next;
                size -= next;
                bool bufferSelect = true;

                while (current > 0)
                {
                    // Trigger the buffer load on the second buffer
                    next = size > buffersize ? buffersize : (int)size;
                    if (next > 0)
                        loadBuffer.Trigger(bufferSelect ? buffer1 : buffer0, next);

                    byte[] buffer = bufferSelect ? buffer0 : buffer1;

                    // Run hashes in parallel
                    await Task.WhenAll(hashers.Select(h => h.Process(buffer, current)));

                    // Wait for the load buffer worker, if needed
                    if (next > 0)
                        loadBuffer.Wait();

                    // Setup for the next hashing step
                    current = next;
                    size -= next;
                    bufferSelect = !bufferSelect;
                }

                // Finalize all hashing helpers
                loadBuffer.Finish();
                await Task.WhenAll(hashers.Select(h => h.Finalize()));

                // Get the results
                if (!omitFromScan.HasFlag(Hash.CRC))
                    rom.CRC = hashers.First(h => h.HashType == Hash.CRC).GetHash();
                if (!omitFromScan.HasFlag(Hash.MD5))
                    rom.MD5 = hashers.First(h => h.HashType == Hash.MD5).GetHash();
#if NET_FRAMEWORK
                if (!omitFromScan.HasFlag(Hash.RIPEMD160))
                    rom.RIPEMD160 = hashers.First(h => h.HashType == Hash.RIPEMD160).GetHash();
#endif
                if (!omitFromScan.HasFlag(Hash.SHA1))
                    rom.SHA1 = hashers.First(h => h.HashType == Hash.SHA1).GetHash();
                if (!omitFromScan.HasFlag(Hash.SHA256))
                    rom.SHA256 = hashers.First(h => h.HashType == Hash.SHA256).GetHash();
                if (!omitFromScan.HasFlag(Hash.SHA384))
                    rom.SHA384 = hashers.First(h => h.HashType == Hash.SHA384).GetHash();
                if (!omitFromScan.HasFlag(Hash.SHA512))
                    rom.SHA512 = hashers.First(h => h.HashType == Hash.SHA512).GetHash();

                // Dispose of the hashers
                loadBuffer.Dispose();
                hashers.ForEach(h => h.Dispose());
            }
            catch (IOException)
            {
                return new BaseFile();
            }
            finally
            {
                if (!keepReadOpen)
                    input.Dispose();
                else
                    input.SeekIfPossible();
            }

            return rom;
        }

        /// <summary>
        /// Seek to a specific point in the stream, if possible
        /// </summary>
        /// <param name="input">Input stream to try seeking on</param>
        /// <param name="offset">Optional offset to seek to</param>
        private static long SeekIfPossible(this Stream input, long offset = 0)
        {
            if (input.CanSeek)
            {
                try
                {
                    if (offset < 0)
                        return input.Seek(offset, SeekOrigin.End);
                    else if (offset >= 0)
                        return input.Seek(offset, SeekOrigin.Begin);
                }
                catch (NotSupportedException)
                {
                    Globals.Logger.Verbose("Stream does not support seeking to starting offset. Stream position not changed");
                }
                catch (NotImplementedException)
                {
                    Globals.Logger.Warning("Stream does not support seeking to starting offset. Stream position not changed");
                }
            }

            return input.Position;
        }
    }
}
