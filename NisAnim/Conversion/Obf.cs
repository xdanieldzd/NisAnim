using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

using NisAnim.IO;

namespace NisAnim.Conversion
{
    [DisplayName("Txf List Entry")]
    public class ObfTxfListEntry
    {
        [DisplayName("Unknown 0x00")]
        public uint Unknown0x00 { get; private set; }
        [DisplayName("Txf Data")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Txf TxfData { get; private set; }

        public ObfTxfListEntry(EndianBinaryReader reader)
        {
            Unknown0x00 = reader.ReadUInt32();
            TxfData = new Txf(reader);
        }
    }

    [DisplayName("Txf List")]
    public class ObfTxfList
    {
        [DisplayName("Number of Textures")]
        public uint NumTextures { get; private set; }
        [DisplayName("Texture Offsets")]
        public uint[] TextureOffsets { get; private set; }

        [DisplayName("Textures")]
        public ObfTxfListEntry[] Textures { get; private set; }

        public ObfTxfList(EndianBinaryReader reader)
        {
            long position = reader.BaseStream.Position;

            NumTextures = reader.ReadUInt32();

            TextureOffsets = new uint[NumTextures];
            for (int i = 0; i < TextureOffsets.Length; i++) TextureOffsets[i] = reader.ReadUInt32();

            Textures = new ObfTxfListEntry[NumTextures];
            for (int i = 0; i < Textures.Length; i++)
            {
                reader.BaseStream.Seek(position + TextureOffsets[i], SeekOrigin.Begin);
                Textures[i] = new ObfTxfListEntry(reader);
            }
        }
    }

    [DisplayName("Obf File")]
    public class Obf : BaseFile
    {
        public const string FileNamePattern = "(.*?)\\.(obf)$";

        [DisplayName("File Size")]
        public uint FileSize { get; private set; }
        [DisplayName("Unknown 0x04")]
        public uint Unknown0x04 { get; private set; }
        [DisplayName("Unknown Offset 0x08")]
        public uint UnknownOffset0x08 { get; private set; }
        [DisplayName("Unknown Offset 0x0C")]
        public uint UnknownOffset0x0C { get; private set; }
        [DisplayName("Unknown Offset 0x10")]
        public uint UnknownOffset0x10 { get; private set; }
        [DisplayName("Unknown Offset 0x14")]
        public uint UnknownOffset0x14 { get; private set; }
        [DisplayName("Unknown Offset 0x18")]
        public uint UnknownOffset0x18 { get; private set; }
        [DisplayName("Unknown Offset 0x1C")]
        public uint UnknownOffset0x1C { get; private set; }
        [DisplayName("Unknown Offset 0x20")]
        public uint UnknownOffset0x20 { get; private set; }
        [DisplayName("Unknown Offset 0x24")]
        public uint UnknownOffset0x24 { get; private set; }
        [DisplayName("Unknown Offset 0x28")]
        public uint UnknownOffset0x28 { get; private set; }
        [DisplayName("Txf List Offset")]
        public uint TxfListOffset { get; private set; }

        [DisplayName("Txf List")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public ObfTxfList TxfList { get; private set; }

        bool disposed = false;

        public Obf(string filePath)
            : base(filePath)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.BigEndian))
            {
                FileSize = reader.ReadUInt32();
                Unknown0x04 = reader.ReadUInt32();
                UnknownOffset0x08 = reader.ReadUInt32();
                UnknownOffset0x0C = reader.ReadUInt32();
                UnknownOffset0x10 = reader.ReadUInt32();
                UnknownOffset0x14 = reader.ReadUInt32();
                UnknownOffset0x18 = reader.ReadUInt32();
                UnknownOffset0x1C = reader.ReadUInt32();
                UnknownOffset0x20 = reader.ReadUInt32();
                UnknownOffset0x24 = reader.ReadUInt32();
                UnknownOffset0x28 = reader.ReadUInt32();
                TxfListOffset = reader.ReadUInt32();

                reader.BaseStream.Seek(TxfListOffset, SeekOrigin.Begin);
                TxfList = new ObfTxfList(reader);
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
