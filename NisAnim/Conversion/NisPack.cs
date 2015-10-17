using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

using NisAnim.IO;

namespace NisAnim.Conversion
{
    [DisplayName("NISPACK File Entry")]
    public class NisPackFile
    {
        [Browsable(false)]
        public NisPack ParentFile { get; private set; }

        [DisplayName("Filename")]
        public string Filename { get; private set; }
        [DisplayName("Offset to File Data")]
        public uint Offset { get; private set; }
        [DisplayName("File Size")]
        public uint FileSize { get; private set; }
        [DisplayName("Hash or Checksum?")]
        public uint HashOrChecksum { get; private set; }

        [DisplayName("Is Lzs-compressed?")]
        public bool IsLzsCompressed { get { return (Path.GetExtension(Filename) == ".lzs"); } }
        [DisplayName("Name of Decompressed File")]
        public string DecompressedFilename { get; private set; }
        [DisplayName("Detected File Type")]
        public Type DetectedFileType { get; private set; }

        public NisPackFile(NisPack parentFile, EndianBinaryReader reader)
        {
            ParentFile = parentFile;

            Filename = Encoding.ASCII.GetString(reader.ReadBytes(0x20)).TrimEnd('\0');
            Offset = reader.ReadUInt32();
            FileSize = reader.ReadUInt32();
            HashOrChecksum = reader.ReadUInt32();

            long position = reader.BaseStream.Position;

            reader.BaseStream.Seek(Offset, SeekOrigin.Begin);

            using (MemoryStream rawStream = new MemoryStream(reader.ReadBytes((int)FileSize)))
            {
                reader.BaseStream.Seek(-FileSize, SeekOrigin.Current);
                if (IsLzsCompressed)
                {
                    using (LzsStream lzsStream = new LzsStream(rawStream))
                    {
                        using (EndianBinaryReader tempReader = new EndianBinaryReader(lzsStream))
                        {
                            DecompressedFilename = Path.GetFileNameWithoutExtension(Filename) + "." + lzsStream.FileExtension;
                            DetectedFileType = FileHelpers.IdentifyFile(tempReader, DecompressedFilename);
                        }
                    }
                }
                else
                {
                    DetectedFileType = FileHelpers.IdentifyFile(reader, DecompressedFilename = Filename);
                }
            }

            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        public MemoryStream GetStream(MemoryStream parentStream)
        {
            using (MemoryStream tempStream = new MemoryStream(parentStream.ToArray()))
            {
                tempStream.Seek(Offset, SeekOrigin.Begin);

                if (IsLzsCompressed)
                {
                    return new LzsStream(tempStream);
                }
                else
                {
                    using (EndianBinaryReader reader = new EndianBinaryReader(tempStream))
                    {
                        return new MemoryStream(reader.ReadBytes((int)FileSize));
                    }
                }
            }
        }
    }

    [DisplayName("NISPACK File")]
    public class NisPack : BaseFile
    {
        public const string MagicNumber = "NISPACK\0";

        [DisplayName("Magic Number")]
        public string Magic { get; private set; }
        [DisplayName("Unknown 0x08")]
        public uint Unknown0x08 { get; private set; }
        [DisplayName("Number of Files")]
        public uint NumFiles { get; private set; }

        [DisplayName("File Entries")]
        public NisPackFile[] Files { get; private set; }

        MemoryStream rawData;

        bool disposed = false;

        public NisPack(string filePath)
            : base(filePath)
        {
            using (FileStream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                stream.Seek(0x0C, SeekOrigin.Begin);
                byte[] numFilesRaw = new byte[4];
                stream.Read(numFilesRaw, 0, numFilesRaw.Length);
                stream.Seek(0, SeekOrigin.Begin);

                /* Crappy endianness detection GO! */
                Endian endian = Endian.BigEndian;
                if (numFilesRaw[0] != 0 && numFilesRaw[3] == 0)
                    endian = Endian.LittleEndian;

                using (EndianBinaryReader reader = new EndianBinaryReader(stream, endian))
                {
                    //OutOfMemoryException on 32bit, on files > 256MB?
                    rawData = new MemoryStream(reader.ReadBytes((int)reader.BaseStream.Length), 0, (int)reader.BaseStream.Length, false, true);

                    reader.BaseStream.Seek(0, SeekOrigin.Begin);

                    Magic = Encoding.ASCII.GetString(reader.ReadBytes(0x08));
                    Unknown0x08 = reader.ReadUInt32();
                    NumFiles = reader.ReadUInt32();

                    Files = new NisPackFile[NumFiles];
                    for (int i = 0; i < Files.Length; i++) Files[i] = new NisPackFile(this, reader);

                    Files = Files.OrderBy(x => x.Filename).ToArray();
                }
            }
        }

        public void ExtractFile(NisPackFile file, string path)
        {
            using (MemoryStream stream = file.GetStream(rawData))
            {
                stream.Seek(0, SeekOrigin.Begin);
                using (EndianBinaryReader reader = new EndianBinaryReader(stream))
                {
                    File.WriteAllBytes(path, reader.ReadBytes((int)reader.BaseStream.Length));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (rawData != null)
                        rawData.Dispose();
                }

                this.disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
