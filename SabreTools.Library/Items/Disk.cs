using System.Linq;

using SabreTools.Library.Data;
using SabreTools.Library.Tools;

namespace SabreTools.Library.Items
{
	/// <summary>
	/// Represents Compressed Hunks of Data (CHD) formatted disks which use internal hashes
	/// </summary>
	public class Disk : DatItem
	{
		#region Private instance variables

		// Disk information
		private byte[] _md5; // 16 bytes
		private byte[] _sha1; // 20 bytes
		private byte[] _sha256; // 32 bytes
		private byte[] _sha384; // 48 bytes
		private byte[] _sha512; // 64 bytes
		private ItemStatus _itemStatus;

		#endregion

		#region Publicly facing variables

		// Disk information
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
		public ItemStatus ItemStatus
		{
			get { return _itemStatus; }
			set { _itemStatus = value; }
		}

		#endregion

		#region Constructors

		/// <summary>
		/// Create a default, empty Disk object
		/// </summary>
		public Disk()
		{
			_name = "";
			_itemType = ItemType.Disk;
			_dupeType = 0x00;
			_itemStatus = ItemStatus.None;
		}

		#endregion

		#region Cloning Methods

		public override object Clone()
		{
			return new Disk()
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

				_md5 = this._md5,
				_sha1 = this._sha1,
				_sha256 = this._sha256,
				_sha384 = this._sha384,
				_sha512 = this._sha512,
				ItemStatus = this.ItemStatus,
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
			Disk newOther = (Disk)other;

			// If either is a nodump, it's never a match
			if (_itemStatus == ItemStatus.Nodump || newOther.ItemStatus == ItemStatus.Nodump)
			{
				return dupefound;
			}

			// If we can determine that the disks have no non-empty hashes in common, we return false
			if ((this._md5.IsNullOrEmpty() || newOther._md5.IsNullOrEmpty())
				&& (this._sha1.IsNullOrEmpty() || newOther._sha1.IsNullOrEmpty())
				&& (this._sha256.IsNullOrEmpty() || newOther._sha256.IsNullOrEmpty())
				&& (this._sha384.IsNullOrEmpty() || newOther._sha384.IsNullOrEmpty())
				&& (this._sha512.IsNullOrEmpty() || newOther._sha512.IsNullOrEmpty()))
			{
				dupefound = false;
			}
			else if (((this._md5.IsNullOrEmpty() || newOther._md5.IsNullOrEmpty()) || Enumerable.SequenceEqual(this._md5, newOther._md5))
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
