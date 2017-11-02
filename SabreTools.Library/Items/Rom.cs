using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.Tools;

namespace SabreTools.Library.Items
{
	/// <summary>
	/// Represents a generic file within a set
	/// </summary>
	public class Rom : DatItem
	{
		#region Private instance variables

		// Rom information
		private long _size;
		private byte[] _crc; // 8 bytes
		private byte[] _md5; // 16 bytes
		private byte[] _sha1; // 20 bytes
		private byte[] _sha256; // 32 bytes
		private byte[] _sha384; // 48 bytes
		private byte[] _sha512; // 64 bytes
		private string _date;
		private ItemStatus _itemStatus;

		#endregion

		#region Publicly facing variables

		// Rom information
		public long Size
		{
			get { return _size; }
			set { _size = value; }
		}
		public string CRC
		{
			get { return _crc.IsNullOrEmpty() ? null : Style.ByteArrayToString(_crc); }
			set { _crc = Style.StringToByteArray(value); }
		}
		public string MD5
		{
			get { return _md5.IsNullOrEmpty() ? null : Style.ByteArrayToString(_md5); }
			set { _md5 = Style.StringToByteArray(value); }
		}
		public string SHA1
		{
			get { return _sha1.IsNullOrEmpty() ? null : Style.ByteArrayToString(_sha1); }
			set { _sha1 = Style.StringToByteArray(value); }
		}
		public string SHA256
		{
			get { return _sha256.IsNullOrEmpty() ? null : Style.ByteArrayToString(_sha256); }
			set { _sha256 = Style.StringToByteArray(value); }
		}
		public string SHA384
		{
			get { return _sha384.IsNullOrEmpty() ? null : Style.ByteArrayToString(_sha384); }
			set { _sha384 = Style.StringToByteArray(value); }
		}
		public string SHA512
		{
			get { return _sha512.IsNullOrEmpty() ? null : Style.ByteArrayToString(_sha512); }
			set { _sha512 = Style.StringToByteArray(value); }
		}
		public string Date
		{
			get { return _date; }
			set { _date = value; }
		}
		public ItemStatus ItemStatus
		{
			get { return _itemStatus; }
			set { _itemStatus = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create a default, empty Rom object
		/// </summary>
		public Rom()
		{
			_name = "";
			_itemType = ItemType.Rom;
			_dupeType = 0x00;
			_itemStatus = ItemStatus.None;
			_date = "";
		}

		/// <summary>
		/// Create a "blank" Rom object
		/// </summary>
		/// <param name="name"></param>
		/// <param name="machineName"></param>
		/// <param name="omitFromScan"></param>
		/// <remarks>TODO: All instances of Hash.DeepHashes should be made into 0x0 eventually</remarks>
		public Rom(string name, string machineName, Hash omitFromScan = Hash.DeepHashes)
		{
			_name = name;
			_itemType = ItemType.Rom;
			_size = -1;
			if ((omitFromScan & Hash.CRC) == 0)
			{
				_crc = null;
			}
			if ((omitFromScan & Hash.MD5) == 0)
			{
				_md5 = null;
			}
			if ((omitFromScan & Hash.SHA1) == 0)
			{
				_sha1 = null;
			}
			if ((omitFromScan & Hash.SHA256) == 0)
			{
				_sha256 = null;
			}
			if ((omitFromScan & Hash.SHA384) == 0)
			{
				_sha384 = null;
			}
			if ((omitFromScan & Hash.SHA512) == 0)
			{
				_sha512 = null;
			}
			_itemStatus = ItemStatus.None;

			_machine = new Machine
			{
				Name = machineName,
				Description = machineName,
			};
		}

		#endregion

		#region Cloning Methods

		public override object Clone()
		{
			return new Rom()
			{
				Name = this.Name,
				Type = this.Type,
				Dupe = this.Dupe,

				Supported = this.Supported,
				Publisher = this.Publisher,
				Infos = this.Infos,
				PartName = this.PartName,
				PartInterface = this.PartInterface,
				Features = this.Features,
				AreaName = this.AreaName,
				AreaSize = this.AreaSize,

				MachineName = this.MachineName,
				Comment = this.Comment,
				MachineDescription = this.MachineDescription,
				Year = this.Year,
				Manufacturer = this.Manufacturer,
				RomOf = this.RomOf,
				CloneOf = this.CloneOf,
				SampleOf = this.SampleOf,
				SourceFile = this.SourceFile,
				Runnable = this.Runnable,
				Board = this.Board,
				RebuildTo = this.RebuildTo,
				Devices = this.Devices,
				MachineType = this.MachineType,

				SystemID = this.SystemID,
				System = this.System,
				SourceID = this.SourceID,
				Source = this.Source,

				Size = this.Size,
				_crc = this._crc,
				_md5 = this._md5,
				_sha1 = this._sha1,
				_sha256 = this._sha256,
				_sha384 = this._sha384,
				_sha512 = this._sha512,
				ItemStatus = this.ItemStatus,
				Date = this.Date,
			};
		}

		#endregion

		#region Comparision Methods

		public override bool Equals(DatItem other)
		{
			bool dupefound = false;

			// If we don't have a rom, return false
			if (_itemType != other.Type)
			{
				return dupefound;
			}

			// Otherwise, treat it as a rom
			Rom newOther = (Rom)other;

			// If either is a nodump, it's never a match
			if (_itemStatus == ItemStatus.Nodump || newOther.ItemStatus == ItemStatus.Nodump)
			{
				return dupefound;
			}

			// If we can determine that the roms have no non-empty hashes in common, we return false
			if ((_crc.IsNullOrEmpty() || newOther._crc.IsNullOrEmpty())
				&& (this._md5.IsNullOrEmpty() || newOther._md5.IsNullOrEmpty())
				&& (this._sha1.IsNullOrEmpty() || newOther._sha1.IsNullOrEmpty())
				&& (this._sha256.IsNullOrEmpty() || newOther._sha256.IsNullOrEmpty())
				&& (this._sha384.IsNullOrEmpty() || newOther._sha384.IsNullOrEmpty())
				&& (this._sha512.IsNullOrEmpty() || newOther._sha512.IsNullOrEmpty()))
			{
				dupefound = false;
			}
			else if ((this.Size == newOther.Size)
				&& ((this._crc.IsNullOrEmpty() || newOther._crc.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._crc, newOther._crc))
				&& ((this._md5.IsNullOrEmpty() || newOther._md5.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._md5, newOther._md5))
				&& ((this._sha1.IsNullOrEmpty() || newOther._sha1.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._sha1, newOther._sha1))
				&& ((this._sha256.IsNullOrEmpty() || newOther._sha256.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._sha256, newOther._sha256))
				&& ((this._sha384.IsNullOrEmpty() || newOther._sha384.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._sha384, newOther._sha384))
				&& ((this._sha512.IsNullOrEmpty() || newOther._sha512.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._sha512, newOther._sha512)))
			{
				dupefound = true;
			}

			return dupefound;
		}

		#endregion
	}
}
