using System;
using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.Tools;

#if MONO
using System.IO;
#else
using BinaryReader = System.IO.BinaryReader;
using SeekOrigin = System.IO.SeekOrigin;
using Stream = System.IO.Stream;
#endif

namespace SabreTools.Library.FileTypes
{
	/// <summary>
	/// This is code adapted from chd.h and chd.cpp in MAME
	/// </summary>
	/// <remrks>
	/// ----------------------------------------------
	/// Common CHD Header:
	/// 0x00-0x07 - CHD signature
	/// 0x08-0x0B - Header size
	/// 0x0C-0x0F - CHD version
	/// ----------------------------------------------
	/// CHD v3 header layout:
	/// 0x10-0x13 - Flags (1: Has parent SHA-1, 2: Disallow writes)
	/// 0x14-0x17 - Compression
	/// 0x18-0x1B - Hunk count
	/// 0x1C-0x23 - Logical Bytes
	/// 0x24-0x2C - Metadata Offset
	/// ...
	/// 0x4C-0x4F - Hunk Bytes
	/// 0x50-0x63 - SHA-1
	/// 0x64-0x77 - Parent SHA-1
	/// 0x78-0x87 - Map
	/// ----------------------------------------------
	/// CHD v4 header layout:
	/// 0x10-0x13 - Flags (1: Has parent SHA-1, 2: Disallow writes)
	/// 0x14-0x17 - Compression
	/// 0x18-0x1B - Hunk count
	/// 0x1C-0x23 - Logical Bytes
	/// 0x24-0x2C - Metadata Offset
	/// ...
	/// 0x2C-0x2F - Hunk Bytes
	/// 0x30-0x43 - SHA-1
	/// 0x44-0x57 - Parent SHA-1
	/// 0x58-0x6b - Raw SHA-1
	/// 0x6c-0x7b - Map
	/// ----------------------------------------------
	/// CHD v5 header layout:
	/// 0x10-0x13 - Compression format 1
	/// 0x14-0x17 - Compression format 2
	/// 0x18-0x1B - Compression format 3
	/// 0x1C-0x1F - Compression format 4
	/// 0x20-0x27 - Logical Bytes
	/// 0x28-0x2F - Map Offset
	/// 0x30-0x37 - Metadata Offset
	/// 0x38-0x3B - Hunk Bytes
	/// 0x3C-0x3F - Unit Bytes
	/// 0x40-0x53 - Raw SHA-1
	/// 0x54-0x67 - SHA-1
	/// 0x68-0x7b - Parent SHA-1
	/// ----------------------------------------------
	/// </remrks>
	public class CHDFile : IDisposable
	{
		#region Private instance variables

		// Core parameters from the header
		private byte[] m_signature;       // signature
		private uint m_headersize;       // size of the header
		private uint m_version;          // version of the header
		private ulong m_logicalbytes;    // logical size of the raw CHD data in bytes
		private ulong m_mapoffset;       // offset of map
		private ulong m_metaoffset;      // offset to first metadata bit
		private uint m_hunkbytes;        // size of each raw hunk in bytes
		private ulong m_hunkcount;       // number of hunks represented
		private uint m_unitbytes;        // size of each unit in bytes
		private ulong m_unitcount;       // number of units represented
		private CHDCodecType[] m_compression = new CHDCodecType[4];   // array of compression types used

		// map information
		private uint m_mapentrybytes;    // length of each entry in a map

		// additional required vars
		private BinaryReader m_br;       // Binary reader representing the CHD stream

		#endregion

		#region Constructors

		/// <summary>
		/// Create a new CHDFile from an input stream
		/// </summary>
		/// <param name="chdstream">Stream representing the CHD file</param>
		public CHDFile(Stream chdstream)
		{
			m_br = new BinaryReader(chdstream);
		}

		/// <summary>
		/// Dispose of the CHDFile
		/// </summary>
		public void Dispose()
		{
			m_br.Dispose();
		}

		#endregion

		#region Header Parsing

		/// <summary>
		/// Validate the initial signature, version, and header size
		/// </summary>
		/// <returns>Unsigned int containing the version number, null if invalid</returns>
		public uint? ValidateHeaderVersion()
		{
			// Seek to the beginning to make sure we're reading the correct bytes
			m_br.BaseStream.Seek(0, SeekOrigin.Begin);

			// Read and verify the CHD signature
			m_signature = m_br.ReadBytes(8);
			for(int i = 0; i < 8; i++)
			{
				if (m_signature[i] != Constants.CHDSignatureBytes[i])
				{
					// throw CHDERR_INVALID_FILE;
					return null;
				}
			}

			// Get the header size and version
			m_headersize = m_br.ReadUInt32Reverse();
			m_version = m_br.ReadUInt32Reverse();

			// If we have an invalid combination of size and version
			if ((m_version == 3 && m_headersize != Constants.CHD_V3_HEADER_SIZE)
				|| (m_version == 4 && m_headersize != Constants.CHD_V4_HEADER_SIZE)
				|| (m_version == 5 && m_headersize != Constants.CHD_V5_HEADER_SIZE)
				|| (m_version < 3 || m_version > 5))
			{
				// throw CHDERR_UNSUPPORTED_VERSION;
				return null;
			}

			return m_version;
		}

		/// <summary>
		/// Get the internal SHA-1 from the CHD
		/// </summary>
		/// <returns>SHA-1 as a byte array, null on error</returns>
		public byte[] GetSHA1FromHeader()
		{
			// Validate the header by default just in case
			uint? version = ValidateHeaderVersion();

			// Now get the SHA-1 hash, if possible
			byte[] sha1 = new byte[20];

			// Now parse the rest of the header according to the version
			switch (version)
			{
				case 3:
					sha1 = ParseCHDv3Header();
					break;
				case 4:
					sha1 = ParseCHDv4Header();
					break;
				case 5:
					sha1 = ParseCHDv5Header();
					break;
				case null:
				default:
					// throw CHDERR_INVALID_FILE;
					return null;
			}

			return sha1;
		}

		/// <summary>
		/// Parse a CHD v3 header
		/// </summary>
		/// <returns>The extracted SHA-1 on success, null otherwise</returns>
		private byte[] ParseCHDv3Header()
		{
			// Seek to after the signature to make sure we're reading the correct bytes
			m_br.BaseStream.Seek(16, SeekOrigin.Begin);

			// Set the blank SHA-1 hash
			byte[] sha1 = new byte[20];

			// Set offsets and defaults
			m_mapoffset = 120;
			m_mapentrybytes = 16;

			// Read the CHD flags
			uint flags = m_br.ReadUInt32Reverse();

			// Determine compression
			switch (m_br.ReadUInt32())
			{
				case 0: m_compression[0] = CHDCodecType.CHD_CODEC_NONE; break;
				case 1: m_compression[0] = CHDCodecType.CHD_CODEC_ZLIB; break;
				case 2: m_compression[0] = CHDCodecType.CHD_CODEC_ZLIB; break;
				case 3: m_compression[0] = CHDCodecType.CHD_CODEC_AVHUFF; break;
				default: /* throw CHDERR_UNKNOWN_COMPRESSION; */ return null;
			}

			m_compression[1] = m_compression[2] = m_compression[3] = CHDCodecType.CHD_CODEC_NONE;

			m_hunkcount = m_br.ReadUInt32Reverse();
			m_logicalbytes = m_br.ReadUInt64Reverse();
			m_metaoffset = m_br.ReadUInt32Reverse();

			m_br.BaseStream.Seek(76, SeekOrigin.Begin);
			m_hunkbytes = m_br.ReadUInt32Reverse();

			m_br.BaseStream.Seek(Constants.CHDv3SHA1Offset, SeekOrigin.Begin);
			sha1 = m_br.ReadBytes(20);

			// guess at the units based on snooping the metadata
			// m_unitbytes = guess_unitbytes();
			m_unitcount = (m_logicalbytes + m_unitbytes - 1) / m_unitbytes;

			return sha1;
		}

		/// <summary>
		/// Parse a CHD v4 header
		/// </summary>
		/// <returns>The extracted SHA-1 on success, null otherwise</returns>
		private byte[] ParseCHDv4Header()
		{
			// Seek to after the signature to make sure we're reading the correct bytes
			m_br.BaseStream.Seek(16, SeekOrigin.Begin);

			// Set the blank SHA-1 hash
			byte[] sha1 = new byte[20];

			// Set offsets and defaults
			m_mapoffset = 108;
			m_mapentrybytes = 16;

			// Read the CHD flags
			uint flags = m_br.ReadUInt32Reverse();

			// Determine compression
			switch (m_br.ReadUInt32())
			{
				case 0: m_compression[0] = CHDCodecType.CHD_CODEC_NONE; break;
				case 1: m_compression[0] = CHDCodecType.CHD_CODEC_ZLIB; break;
				case 2: m_compression[0] = CHDCodecType.CHD_CODEC_ZLIB; break;
				case 3: m_compression[0] = CHDCodecType.CHD_CODEC_AVHUFF; break;
				default: /* throw CHDERR_UNKNOWN_COMPRESSION; */ return null;
			}

			m_compression[1] = m_compression[2] = m_compression[3] = CHDCodecType.CHD_CODEC_NONE;

			m_hunkcount = m_br.ReadUInt32Reverse();
			m_logicalbytes = m_br.ReadUInt64Reverse();
			m_metaoffset = m_br.ReadUInt32Reverse();

			m_br.BaseStream.Seek(44, SeekOrigin.Begin);
			m_hunkbytes = m_br.ReadUInt32Reverse();

			m_br.BaseStream.Seek(Constants.CHDv4SHA1Offset, SeekOrigin.Begin);
			sha1 = m_br.ReadBytes(20);

			// guess at the units based on snooping the metadata
			// m_unitbytes = guess_unitbytes();
			m_unitcount = (m_logicalbytes + m_unitbytes - 1) / m_unitbytes;
			return sha1;
		}

		/// <summary>
		/// Parse a CHD v5 header
		/// </summary>
		/// <returns>The extracted SHA-1 on success, null otherwise</returns>
		private byte[] ParseCHDv5Header()
		{
			// Seek to after the signature to make sure we're reading the correct bytes
			m_br.BaseStream.Seek(16, SeekOrigin.Begin);

			// Set the blank SHA-1 hash
			byte[] sha1 = new byte[20];

			// Determine compression
			m_compression[0] = (CHDCodecType)m_br.ReadUInt32Reverse();
			m_compression[1] = (CHDCodecType)m_br.ReadUInt32Reverse();
			m_compression[2] = (CHDCodecType)m_br.ReadUInt32Reverse();
			m_compression[3] = (CHDCodecType)m_br.ReadUInt32Reverse();

			m_logicalbytes = m_br.ReadUInt64Reverse();
			m_mapoffset = m_br.ReadUInt64Reverse();
			m_metaoffset = m_br.ReadUInt64Reverse();
			m_hunkbytes = m_br.ReadUInt32Reverse();
			m_hunkcount = (m_logicalbytes + m_hunkbytes - 1) / m_hunkbytes;
			m_unitbytes = m_br.ReadUInt32Reverse();
			m_unitcount = (m_logicalbytes + m_unitbytes - 1) / m_unitbytes;

			// m_allow_writes = !compressed();

			// determine properties of map entries
			// m_mapentrybytes = compressed() ? 12 : 4;

			m_br.BaseStream.Seek(Constants.CHDv5SHA1Offset, SeekOrigin.Begin);
			sha1 = m_br.ReadBytes(20);
			return sha1;
		}

		#endregion
	}
}
