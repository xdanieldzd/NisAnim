using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

using NisAnim.IO;

namespace NisAnim.Conversion
{
    [DisplayName("Item")]
    public class ItemData
    {
        static Encoding sjis = Encoding.GetEncoding(932);

        [DisplayName("[Parent]")]
        public MItemDat Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        public uint Price { get; private set; }
        public short HPModifier { get; private set; }
        public short SPModifier { get; private set; }
        public short AtkModifier { get; private set; }
        public short DefModifier { get; private set; }
        public short IntModifier { get; private set; }
        public short SpdModifier { get; private set; }
        public short HitModifier { get; private set; }
        public short ResModifier { get; private set; }
        public ushort ID { get; private set; }
        public ushort Unknown0x16 { get; private set; }
        public ushort Unknown0x18 { get; private set; }
        public ushort MoveModifier { get; private set; }
        public ushort Specialist { get; private set; }
        public ushort Unknown0x1E { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; }
        public ushort Unknown0xD0 { get; private set; }
        public ushort Unknown0xD2 { get; private set; }
        public ushort Unknown0xD4 { get; private set; }
        public ushort Unknown0xD6 { get; private set; }
        public ushort Unknown0xD8 { get; private set; }
        public ushort Unknown0xDA { get; private set; }
        public ushort Rank { get; private set; }
        public byte Range { get; private set; }
        public byte JumpModifier { get; private set; }
        public ushort CriticalHit { get; private set; }
        public ushort Unknown0xE2 { get; private set; }
        public ushort Unknown0xE4 { get; private set; }
        public ushort Unknown0xE6 { get; private set; }
        public ushort Unknown0xE8 { get; private set; }
        public ushort Unknown0xEA { get; private set; }

        public ItemData(MItemDat parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            Price = reader.ReadUInt32();
            HPModifier = reader.ReadInt16();
            SPModifier = reader.ReadInt16();
            AtkModifier = reader.ReadInt16();
            DefModifier = reader.ReadInt16();
            IntModifier = reader.ReadInt16();
            SpdModifier = reader.ReadInt16();
            HitModifier = reader.ReadInt16();
            ResModifier = reader.ReadInt16();
            ID = reader.ReadUInt16();
            Unknown0x16 = reader.ReadUInt16();
            Unknown0x18 = reader.ReadUInt16();
            MoveModifier = reader.ReadUInt16();
            Specialist = reader.ReadUInt16();
            Unknown0x1E = reader.ReadUInt16();
            Name = sjis.GetString(reader.ReadBytes(0x30)).TrimEnd('\0');
            Description = sjis.GetString(reader.ReadBytes(0x80)).TrimEnd('\0');
            Unknown0xD0 = reader.ReadUInt16();
            Unknown0xD2 = reader.ReadUInt16();
            Unknown0xD4 = reader.ReadUInt16();
            Unknown0xD6 = reader.ReadUInt16();
            Unknown0xD8 = reader.ReadUInt16();
            Unknown0xDA = reader.ReadUInt16();
            Rank = reader.ReadUInt16();
            Range = reader.ReadByte();
            JumpModifier = reader.ReadByte();
            CriticalHit = reader.ReadUInt16();
            Unknown0xE2 = reader.ReadUInt16();
            Unknown0xE4 = reader.ReadUInt16();
            Unknown0xE6 = reader.ReadUInt16();
            Unknown0xE8 = reader.ReadUInt16();
            Unknown0xEA = reader.ReadUInt16();
        }
    }

    [DisplayName("Item Data File")]
    [FileNamePattern("(mitem.dat)$")]
    public class MItemDat : BaseFile
    {
        public ushort NumItems { get; private set; }
        public ushort Unknown0x02 { get; private set; }

        public ItemData[] Items { get; private set; }

        bool disposed = false;

        public MItemDat(string filePath)
            : base(filePath)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.BigEndian))
            {
                NumItems = reader.ReadUInt16();
                Unknown0x02 = reader.ReadUInt16();

                Items = new ItemData[NumItems];
                for (int i = 0; i < Items.Length; i++) Items[i] = new ItemData(this, reader);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    //
                }

                this.disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
