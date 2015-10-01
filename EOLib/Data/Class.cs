﻿using System;
using System.IO;
using System.Text;

namespace EOLib.Data
{
	public class ClassRecord : IDataRecord
	{
		public int ID { get; set; }
		public string Name { get; set; }
		public int NameCount { get; private set; }

		public byte Base { get; set; }
		public byte Type { get; set; }

		public short Str { get; set; }
		public short Int { get; set; }
		public short Wis { get; set; }
		public short Agi { get; set; }
		public short Con { get; set; }
		public short Cha { get; set; }

		public ClassRecord(int id)
		{
			ID = id;
		}

		public override string ToString()
		{
			return ID + ": " + Name;
		}

		public void SetNames(params string[] names)
		{
			if (names.Length != NameCount)
				throw new ArgumentException("Error: item record has invalid number of names");

			Name = names[0];
		}

		public void DeserializeFromByteArray(int version, byte[] rawData)
		{
			if (version != 0)
				throw new FileLoadException("Unable to Load file with invalid version: " + version);

			Base = (byte) Packet.DecodeNumber(new[] {rawData[0]});
			Type = (byte) Packet.DecodeNumber(new[] {rawData[1]});
			Str = (short) Packet.DecodeNumber(rawData.SubArray(2, 2));
			Int = (short) Packet.DecodeNumber(rawData.SubArray(4, 2));
			Wis = (short) Packet.DecodeNumber(rawData.SubArray(6, 2));
			Agi = (short) Packet.DecodeNumber(rawData.SubArray(8, 2));
			Con = (short) Packet.DecodeNumber(rawData.SubArray(10, 2));
			Cha = (short) Packet.DecodeNumber(rawData.SubArray(12, 2));
		}

		public byte[] SerializeToByteArray()
		{
			byte[] ret = new byte[ClassFile.DATA_SIZE + 1 + Name.Length];
			for (int i = 0; i < ret.Length; ++i)
				ret[i] = 254;

			using (MemoryStream mem = new MemoryStream(ret))
			{
				mem.WriteByte(Packet.EncodeNumber(Name.Length, 1)[0]);
				byte[] name = Encoding.ASCII.GetBytes(Name);
				mem.Write(name, 0, name.Length);

				mem.WriteByte(Base);
				mem.WriteByte(Type);

				mem.Write(Packet.EncodeNumber(Str, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Int, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Wis, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Agi, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Con, 2), 0, 2);
				mem.Write(Packet.EncodeNumber(Cha, 2), 0, 2);
			}

			return ret;
		}
	}

	public class ClassFile : EODataFile
	{
		public const int DATA_SIZE = 14;

		public ClassFile()
			: base(new ClassRecordFactory())
		{
			Load(FilePath = Constants.ClassFilePath);
		}
		public ClassFile(string path)
			: base(new ClassRecordFactory())
		{
			Load(FilePath = path);
		}

		protected override int GetDataSize()
		{
			return DATA_SIZE;
		}
	}
}
