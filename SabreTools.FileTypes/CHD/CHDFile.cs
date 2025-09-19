using System;
using System.IO;
using System.Text;
using SabreTools.IO.Extensions;
using SabreTools.Models.CHD;

namespace SabreTools.FileTypes.CHD
{
    public class CHDFile : BaseFile
    {
        #region Fields

        /// <summary>
        /// Internal MD5 hash of the file
        /// </summary>
        public byte[]? InternalMD5 { get; set; }

        /// <summary>
        /// Internal SHA-1 hash of the file
        /// </summary>
        public byte[]? InternalSHA1 { get; set; }

        #endregion

        #region Private instance variables

        /// <summary>
        /// Model representing the correct CHD header
        /// </summary>
        protected Header? _header;

        #endregion

        #region Constructors

        /// <summary>
        /// Create a new CHDFile from an input file
        /// </summary>
        /// <param name="filename">Filename respresenting the CHD file</param>
        public static CHDFile? Create(string filename)
        {
            using Stream fs = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return Create(fs);
        }

        /// <summary>
        /// Create a new CHDFile from an input stream
        /// </summary>
        /// <param name="stream">Stream representing the CHD file</param>
        public static CHDFile? Create(Stream stream)
        {
            try
            {
                var header = Deserialize(stream);
                return header switch
                {
                    HeaderV1 v1 => new CHDFile
                    {
                        _header = header,
                        InternalMD5 = v1.MD5,
                    },
                    HeaderV2 v2 => new CHDFile
                    {
                        _header = header,
                        InternalMD5 = v2.MD5,
                    },
                    HeaderV3 v3 => new CHDFile
                    {
                        _header = header,
                        InternalMD5 = v3.MD5,
                        InternalSHA1 = v3.SHA1,
                    },
                    HeaderV4 v4 => new CHDFile
                    {
                        _header = header,
                        InternalSHA1 = v4.SHA1,
                    },
                    HeaderV5 v5 => new CHDFile
                    {
                        _header = header,
                        InternalSHA1 = v5.SHA1,
                    },
                    _ => null,
                };
            }
            catch
            {
                return null;
            }
        }

        // TODO: Replace this when Serialization is updated again
        #region Serialization

        private static Header? Deserialize(Stream data)
        {
            // If the data is invalid
            if (data == null || !data.CanRead)
                return null;

            try
            {
                // Cache the current offset
                long initialOffset = data.Position;

                // Determine the header version
                uint version = GetVersion(data, initialOffset);

                // Read and return the current CHD
                switch (version)
                {
                    case 1:
                        var headerV1 = ParseHeaderV1(data);

                        if (headerV1.Tag != Models.CHD.Constants.SignatureString)
                            return null;
                        if (headerV1.Length != Models.CHD.Constants.HeaderV1Size)
                            return null;
                        if (headerV1.Compression > CompressionType.CHDCOMPRESSION_ZLIB)
                            return null;

                        return headerV1;

                    case 2:
                        var headerV2 = ParseHeaderV2(data);

                        if (headerV2.Tag != Models.CHD.Constants.SignatureString)
                            return null;
                        if (headerV2.Length != Models.CHD.Constants.HeaderV2Size)
                            return null;
                        if (headerV2.Compression > CompressionType.CHDCOMPRESSION_ZLIB)
                            return null;

                        return headerV2;

                    case 3:
                        var headerV3 = ParseHeaderV3(data);

                        if (headerV3.Tag != Models.CHD.Constants.SignatureString)
                            return null;
                        if (headerV3.Length != Models.CHD.Constants.HeaderV3Size)
                            return null;
                        if (headerV3.Compression > CompressionType.CHDCOMPRESSION_ZLIB_PLUS)
                            return null;

                        return headerV3;

                    case 4:
                        var headerV4 = ParseHeaderV4(data);

                        if (headerV4?.Tag != Models.CHD.Constants.SignatureString)
                            return null;
                        if (headerV4.Length != Models.CHD.Constants.HeaderV4Size)
                            return null;
                        if (headerV4.Compression > CompressionType.CHDCOMPRESSION_AV)
                            return null;

                        return headerV4;

                    case 5:
                        var headerV5 = ParseHeaderV5(data);

                        if (headerV5.Tag != Models.CHD.Constants.SignatureString)
                            return null;
                        if (headerV5.Length != Models.CHD.Constants.HeaderV5Size)
                            return null;

                        return headerV5;

                    default:
                        return null;

                }
            }
            catch
            {
                // Ignore the actual error
                return null;
            }
        }

        private static uint GetVersion(Stream data, long initialOffset)
        {
            // Read the header values
            byte[] tagBytes = data.ReadBytes(8);
            string tag = Encoding.ASCII.GetString(tagBytes);
            uint length = data.ReadUInt32BigEndian();
            uint version = data.ReadUInt32BigEndian();

            // Seek back to start
            data.SeekIfPossible(initialOffset);

            // Check the signature
            if (!string.Equals(tag, Models.CHD.Constants.SignatureString, StringComparison.Ordinal))
                return 0;

            // Match the version to header length
#if NET472_OR_GREATER || NETCOREAPP || NETSTANDARD2_0_OR_GREATER
            return (version, length) switch
            {
                (1, Models.CHD.Constants.HeaderV1Size) => version,
                (2, Models.CHD.Constants.HeaderV2Size) => version,
                (3, Models.CHD.Constants.HeaderV3Size) => version,
                (4, Models.CHD.Constants.HeaderV4Size) => version,
                (5, Models.CHD.Constants.HeaderV5Size) => version,
                _ => 0,
            };
#else
            return version switch
            {
                1 => length == Models.CHD.Constants.HeaderV1Size ? version : 0,
                2 => length == Models.CHD.Constants.HeaderV2Size ? version : 0,
                3 => length == Models.CHD.Constants.HeaderV3Size ? version : 0,
                4 => length == Models.CHD.Constants.HeaderV4Size ? version : 0,
                5 => length == Models.CHD.Constants.HeaderV5Size ? version : 0,
                _ => 0,
            };
#endif
        }

        /// <summary>
        /// Parse a Stream into a HeaderV1
        /// </summary>
        private static HeaderV1 ParseHeaderV1(Stream data)
        {
            var obj = new HeaderV1();

            byte[] tag = data.ReadBytes(8);
            obj.Tag = Encoding.ASCII.GetString(tag);
            obj.Length = data.ReadUInt32BigEndian();
            obj.Version = data.ReadUInt32BigEndian();
            obj.Flags = (Flags)data.ReadUInt32BigEndian();
            obj.Compression = (CompressionType)data.ReadUInt32BigEndian();
            obj.HunkSize = data.ReadUInt32BigEndian();
            obj.TotalHunks = data.ReadUInt32BigEndian();
            obj.Cylinders = data.ReadUInt32BigEndian();
            obj.Heads = data.ReadUInt32BigEndian();
            obj.Sectors = data.ReadUInt32BigEndian();
            obj.MD5 = data.ReadBytes(16);
            obj.ParentMD5 = data.ReadBytes(16);

            return obj;
        }

        /// <summary>
        /// Parse a Stream into a V2 header
        /// </summary>
        private static HeaderV2 ParseHeaderV2(Stream data)
        {
            var obj = new HeaderV2();

            byte[] tag = data.ReadBytes(8);
            obj.Tag = Encoding.ASCII.GetString(tag);
            obj.Length = data.ReadUInt32BigEndian();
            obj.Version = data.ReadUInt32BigEndian();
            obj.Flags = (Flags)data.ReadUInt32BigEndian();
            obj.Compression = (CompressionType)data.ReadUInt32BigEndian();
            obj.HunkSize = data.ReadUInt32BigEndian();
            obj.TotalHunks = data.ReadUInt32BigEndian();
            obj.Cylinders = data.ReadUInt32BigEndian();
            obj.Heads = data.ReadUInt32BigEndian();
            obj.Sectors = data.ReadUInt32BigEndian();
            obj.MD5 = data.ReadBytes(16);
            obj.ParentMD5 = data.ReadBytes(16);
            obj.BytesPerSector = data.ReadUInt32BigEndian();

            return obj;
        }

        /// <summary>
        /// Parse a Stream into a V3 header
        /// </summary>
        private static HeaderV3 ParseHeaderV3(Stream data)
        {
            var obj = new HeaderV3();

            byte[] tag = data.ReadBytes(8);
            obj.Tag = Encoding.ASCII.GetString(tag);
            obj.Length = data.ReadUInt32BigEndian();
            obj.Version = data.ReadUInt32BigEndian();
            obj.Flags = (Flags)data.ReadUInt32BigEndian();
            obj.Compression = (CompressionType)data.ReadUInt32BigEndian();
            obj.TotalHunks = data.ReadUInt32BigEndian();
            obj.LogicalBytes = data.ReadUInt64BigEndian();
            obj.MetaOffset = data.ReadUInt64BigEndian();
            obj.MD5 = data.ReadBytes(16);
            obj.ParentMD5 = data.ReadBytes(16);
            obj.HunkBytes = data.ReadUInt32BigEndian();
            obj.SHA1 = data.ReadBytes(20);
            obj.ParentSHA1 = data.ReadBytes(20);

            return obj;
        }

        /// <summary>
        /// Parse a Stream into a V4 header
        /// </summary>
        private static HeaderV4 ParseHeaderV4(Stream data)
        {
            var obj = new HeaderV4();

            byte[] tag = data.ReadBytes(8);
            obj.Tag = Encoding.ASCII.GetString(tag);
            obj.Length = data.ReadUInt32BigEndian();
            obj.Version = data.ReadUInt32BigEndian();
            obj.Flags = (Flags)data.ReadUInt32BigEndian();
            obj.Compression = (CompressionType)data.ReadUInt32BigEndian();
            obj.TotalHunks = data.ReadUInt32BigEndian();
            obj.LogicalBytes = data.ReadUInt64BigEndian();
            obj.MetaOffset = data.ReadUInt64BigEndian();
            obj.HunkBytes = data.ReadUInt32BigEndian();
            obj.SHA1 = data.ReadBytes(20);
            obj.ParentSHA1 = data.ReadBytes(20);
            obj.RawSHA1 = data.ReadBytes(20);

            return obj;
        }

        /// <summary>
        /// Parse a Stream into a V5 header
        /// </summary>
        private static HeaderV5 ParseHeaderV5(Stream data)
        {
            var obj = new HeaderV5();

            byte[] tag = data.ReadBytes(8);
            obj.Tag = Encoding.ASCII.GetString(tag);
            obj.Length = data.ReadUInt32BigEndian();
            obj.Version = data.ReadUInt32BigEndian();
            obj.Compressors = new CodecType[4];
            for (int i = 0; i < 4; i++)
            {
                obj.Compressors[i] = (CodecType)data.ReadUInt32BigEndian();
            }
            obj.LogicalBytes = data.ReadUInt64BigEndian();
            obj.MapOffset = data.ReadUInt64BigEndian();
            obj.MetaOffset = data.ReadUInt64BigEndian();
            obj.HunkBytes = data.ReadUInt32BigEndian();
            obj.UnitBytes = data.ReadUInt32BigEndian();
            obj.RawSHA1 = data.ReadBytes(20);
            obj.SHA1 = data.ReadBytes(20);
            obj.ParentSHA1 = data.ReadBytes(20);

            return obj;
        }

        #endregion

        #endregion
    }
}
