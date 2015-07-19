using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.IO;

using NisAnim.IO;

namespace NisAnim.Conversion
{
    [DisplayName("Unit")]
    public class MapChrUnit
    {
        [DisplayName("[Parent]")]
        public MapChr Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        public ushort CharaID { get; private set; } //model ID?
        public ushort Unknown0x02 { get; private set; }
        public ushort Unknown0x04 { get; private set; }
        public short PositionX { get; private set; }
        public short PositionY { get; private set; }    //invert to match obf models? i.e. -12 -> 12
        public short PositionZ { get; private set; }    // ""
        public ushort Unknown0x0C { get; private set; }
        public ushort Unknown0x0E { get; private set; }
        public ushort Unknown0x10 { get; private set; }
        public ushort Unknown0x12 { get; private set; }
        public ushort Unknown0x14 { get; private set; }
        public ushort Unknown0x16 { get; private set; }
        public ushort Unknown0x18 { get; private set; }
        public ushort Unknown0x1A { get; private set; }
        public ushort Unknown0x1C { get; private set; }
        public ushort Unknown0x1E { get; private set; }
        public uint Unknown0x20 { get; private set; }
        public uint Unknown0x24 { get; private set; }
        public uint Unknown0x28 { get; private set; }
        public uint Unknown0x2C { get; private set; }
        public uint Unknown0x30 { get; private set; }
        public int Unknown0x34 { get; private set; }    //-1
        public int Unknown0x38 { get; private set; }    //-1
        public uint Unknown0x3C { get; private set; }
        public short Unknown0x40 { get; private set; } //some ID?
        public ushort Unknown0x42 { get; private set; }
        public short Unknown0x44 { get; private set; }
        public ushort Unknown0x46 { get; private set; }
        public short Unknown0x48 { get; private set; }
        public ushort Unknown0x4A { get; private set; }
        public short Unknown0x4C { get; private set; }
        public ushort Unknown0x4E { get; private set; }
        public short Unknown0x50 { get; private set; }
        public ushort Unknown0x52 { get; private set; }
        public short Unknown0x54 { get; private set; }
        public ushort Unknown0x56 { get; private set; }
        public short Unknown0x58 { get; private set; }
        public ushort Unknown0x5A { get; private set; }
        public short Unknown0x5C { get; private set; }
        public ushort Unknown0x5E { get; private set; }
        public short Unknown0x60 { get; private set; }
        public ushort Unknown0x62 { get; private set; }
        public short Unknown0x64 { get; private set; }
        public ushort Unknown0x66 { get; private set; }
        public short Unknown0x68 { get; private set; }
        public ushort Unknown0x6A { get; private set; }
        public short Unknown0x6C { get; private set; }
        public ushort Unknown0x6E { get; private set; }
        public short Unknown0x70 { get; private set; }
        public ushort Unknown0x72 { get; private set; }
        public short Unknown0x74 { get; private set; }
        public ushort Unknown0x76 { get; private set; }
        public short Unknown0x78 { get; private set; }
        public ushort Unknown0x7A { get; private set; }
        public short Unknown0x7C { get; private set; }
        public ushort Unknown0x7E { get; private set; }
        public ushort Unknown0x80 { get; private set; } //some other ID? items?
        public ushort Unknown0x82 { get; private set; }
        public uint Unknown0x84 { get; private set; }
        public ushort Unknown0x88 { get; private set; } //some other ID? items?
        public ushort Unknown0x8A { get; private set; }
        public uint Unknown0x8C { get; private set; }
        public ushort Unknown0x90 { get; private set; } //some other ID? items?
        public ushort Unknown0x92 { get; private set; }
        public uint Unknown0x94 { get; private set; }
        public ushort Unknown0x98 { get; private set; } //some other ID? items?
        public ushort Unknown0x9A { get; private set; }
        public uint Unknown0x9C { get; private set; }
        public ushort Unknown0xA0 { get; private set; } //some other ID? items?
        public ushort Unknown0xA2 { get; private set; }
        public uint Unknown0xA4 { get; private set; }
        public ushort Unknown0xA8 { get; private set; } //some other ID? items?
        public ushort Unknown0xAA { get; private set; }
        public uint Unknown0xAC { get; private set; }
        public ushort Unknown0xB0 { get; private set; } //some other ID? items?
        public ushort Unknown0xB2 { get; private set; }
        public uint Unknown0xB4 { get; private set; }
        public ushort Unknown0xB8 { get; private set; } //some other ID? items?
        public ushort Unknown0xBA { get; private set; }
        public uint Unknown0xBC { get; private set; }

        public MapChrUnit(MapChr parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            CharaID = reader.ReadUInt16();
            Unknown0x02 = reader.ReadUInt16();
            Unknown0x04 = reader.ReadUInt16();
            PositionX = reader.ReadInt16();
            PositionY = reader.ReadInt16();
            PositionZ = reader.ReadInt16();
            Unknown0x0C = reader.ReadUInt16();
            Unknown0x0E = reader.ReadUInt16();
            Unknown0x10 = reader.ReadUInt16();
            Unknown0x12 = reader.ReadUInt16();
            Unknown0x14 = reader.ReadUInt16();
            Unknown0x16 = reader.ReadUInt16();
            Unknown0x18 = reader.ReadUInt16();
            Unknown0x1A = reader.ReadUInt16();
            Unknown0x1C = reader.ReadUInt16();
            Unknown0x1E = reader.ReadUInt16();
            Unknown0x20 = reader.ReadUInt32();
            Unknown0x24 = reader.ReadUInt32();
            Unknown0x28 = reader.ReadUInt32();
            Unknown0x2C = reader.ReadUInt32();
            Unknown0x30 = reader.ReadUInt32();
            Unknown0x34 = reader.ReadInt32();
            Unknown0x38 = reader.ReadInt32();
            Unknown0x3C = reader.ReadUInt32();
            Unknown0x40 = reader.ReadInt16();
            Unknown0x42 = reader.ReadUInt16();
            Unknown0x44 = reader.ReadInt16();
            Unknown0x46 = reader.ReadUInt16();
            Unknown0x48 = reader.ReadInt16();
            Unknown0x4A = reader.ReadUInt16();
            Unknown0x4C = reader.ReadInt16();
            Unknown0x4E = reader.ReadUInt16();
            Unknown0x50 = reader.ReadInt16();
            Unknown0x52 = reader.ReadUInt16();
            Unknown0x54 = reader.ReadInt16();
            Unknown0x56 = reader.ReadUInt16();
            Unknown0x58 = reader.ReadInt16();
            Unknown0x5A = reader.ReadUInt16();
            Unknown0x5C = reader.ReadInt16();
            Unknown0x5E = reader.ReadUInt16();
            Unknown0x60 = reader.ReadInt16();
            Unknown0x62 = reader.ReadUInt16();
            Unknown0x64 = reader.ReadInt16();
            Unknown0x66 = reader.ReadUInt16();
            Unknown0x68 = reader.ReadInt16();
            Unknown0x6A = reader.ReadUInt16();
            Unknown0x6C = reader.ReadInt16();
            Unknown0x6E = reader.ReadUInt16();
            Unknown0x70 = reader.ReadInt16();
            Unknown0x72 = reader.ReadUInt16();
            Unknown0x74 = reader.ReadInt16();
            Unknown0x76 = reader.ReadUInt16();
            Unknown0x78 = reader.ReadInt16();
            Unknown0x7A = reader.ReadUInt16();
            Unknown0x7C = reader.ReadInt16();
            Unknown0x7E = reader.ReadUInt16();
            Unknown0x80 = reader.ReadUInt16();
            Unknown0x82 = reader.ReadUInt16();
            Unknown0x84 = reader.ReadUInt32();
            Unknown0x88 = reader.ReadUInt16();
            Unknown0x8A = reader.ReadUInt16();
            Unknown0x8C = reader.ReadUInt32();
            Unknown0x90 = reader.ReadUInt16();
            Unknown0x92 = reader.ReadUInt16();
            Unknown0x94 = reader.ReadUInt32();
            Unknown0x98 = reader.ReadUInt16();
            Unknown0x9A = reader.ReadUInt16();
            Unknown0x9C = reader.ReadUInt32();
            Unknown0xA0 = reader.ReadUInt16();
            Unknown0xA2 = reader.ReadUInt16();
            Unknown0xA4 = reader.ReadUInt32();
            Unknown0xA8 = reader.ReadUInt16();
            Unknown0xAA = reader.ReadUInt16();
            Unknown0xAC = reader.ReadUInt32();
            Unknown0xB0 = reader.ReadUInt16();
            Unknown0xB2 = reader.ReadUInt16();
            Unknown0xB4 = reader.ReadUInt32();
            Unknown0xB8 = reader.ReadUInt16();
            Unknown0xBA = reader.ReadUInt16();
            Unknown0xBC = reader.ReadUInt32();
        }
    }

    [DisplayName("Map Character File")]
    [FileNamePattern("(map)(.*?)\\.(chr)$")]
    public class MapChr : BaseFile
    {
        public uint NumUnits { get; private set; }
        public uint Unknown0x04 { get; private set; }
        public uint Unknown0x08 { get; private set; }
        public uint Unknown0x0C { get; private set; }

        public MapChrUnit[] Units { get; private set; }

        bool disposed = false;

        public MapChr(string filePath)
            : base(filePath)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.BigEndian))
            {
                NumUnits = reader.ReadUInt32();
                Unknown0x04 = reader.ReadUInt32();
                Unknown0x08 = reader.ReadUInt32();
                Unknown0x0C = reader.ReadUInt32();

                Units = new MapChrUnit[NumUnits];
                for (int i = 0; i < Units.Length; i++) Units[i] = new MapChrUnit(this, reader);
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
