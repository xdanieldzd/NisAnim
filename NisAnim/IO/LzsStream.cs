using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace NisAnim.IO
{
    public class LzsStream : MemoryStream
    {
        public string FileExtension { get; private set; }
        public uint UncompressedSize { get; private set; }
        public uint CompressedSize { get; private set; }
        public byte Flag { get; private set; }
        public byte Unknown0x0D { get; private set; }
        public byte Unknown0x0E { get; private set; }
        public byte Unknown0x0F { get; private set; }

        public LzsStream(Stream originalStream)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(originalStream))
            {
                FileExtension = Encoding.ASCII.GetString(reader.ReadBytes(4)).TrimEnd('\0');
                UncompressedSize = reader.ReadUInt32();
                CompressedSize = reader.ReadUInt32();
                Flag = reader.ReadByte();
                Unknown0x0D = reader.ReadByte();
                Unknown0x0E = reader.ReadByte();
                Unknown0x0F = reader.ReadByte();

                byte[] decompressed = Decompress(reader.ReadBytes((int)CompressedSize));
                base.Write(decompressed, 0, decompressed.Length);
            }
        }

        private byte[] Decompress(byte[] compressed)
        {
            byte[] decompressed = new byte[UncompressedSize];

            int inOffset = 0, outOffset = 0;

            while (outOffset < UncompressedSize)
            {
                byte data = compressed[inOffset++];
                if (data != Flag)
                {
                    decompressed[outOffset++] = data;
                }
                else
                {
                    byte distance = compressed[inOffset];
                    if (distance == Flag)
                    {
                        decompressed[outOffset++] = Flag;
                        inOffset++;
                    }
                    else
                    {
                        if (distance > Flag) distance--;

                        byte length = compressed[inOffset + 1];
                        for (int i = 0; i < length; i++) decompressed[outOffset + i] = decompressed[(outOffset - distance) + i];

                        inOffset += 2;
                        outOffset += length;
                    }
                }
            }

            return decompressed;
        }
    }
}
