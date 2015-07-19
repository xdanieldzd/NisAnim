using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

using NisAnim.IO;

namespace NisAnim.Conversion
{
    [DisplayName("Pac File Entry")]
    public class PacFile
    {
        [Browsable(false)]
        public Pac ParentFile { get; private set; }

        [DisplayName("Offset to File Data")]
        public uint Offset { get; private set; }
        [DisplayName("Filename")]
        public string Filename { get; private set; }

        [DisplayName("Detected File Type")]
        public Type DetectedFileType { get; private set; }
        [DisplayName("Calculated Length")]
        public uint Length { get; private set; }

        public PacFile(Pac parentFile, EndianBinaryReader reader)
        {
            ParentFile = parentFile;

            Offset = reader.ReadUInt32();
            Filename = Encoding.ASCII.GetString(reader.ReadBytes(0x1C)).TrimEnd('\0');

            long position = reader.BaseStream.Position;
            reader.BaseStream.Seek(Offset, SeekOrigin.Begin);
            DetectedFileType = FileHelpers.IdentifyFile(reader, Filename);
            reader.BaseStream.Seek(position, SeekOrigin.Begin);
        }

        public void SetLength(PacFile[] files)
        {
            int nextFileIdx = (Array.IndexOf(files, this) + 1);
            if (nextFileIdx < files.Length)
                Length = files[nextFileIdx].Offset - Offset;
            else
                Length = (uint)(ParentFile.FileSize - Offset);
        }

        public MemoryStream GetStream(MemoryStream parentStream)
        {
            using (MemoryStream tempStream = new MemoryStream(parentStream.ToArray()))
            {
                tempStream.Seek(ParentFile.DataStartPosition + Offset, SeekOrigin.Begin);

                using (EndianBinaryReader reader = new EndianBinaryReader(tempStream))
                {
                    return new MemoryStream(reader.ReadBytes((int)Length));
                }
            }
        }
    }

    [DisplayName("Pac File")]
    [FileNamePattern("(.*?)\\.(pac)$")]
    public class Pac : BaseFile
    {
        [DisplayName("Number of Files")]
        public uint NumFiles { get; private set; }
        [DisplayName("Unknown 0x04")]
        public uint Unknown0x04 { get; private set; }
        [DisplayName("Unknown 0x08")]
        public uint Unknown0x08 { get; private set; }
        [DisplayName("Unknown 0x0C")]
        public uint Unknown0x0C { get; private set; }

        [DisplayName("File Entries")]
        public PacFile[] Files { get; private set; }

        [Browsable(false)]
        public long DataStartPosition { get; private set; }
        [Browsable(false)]
        public long FileSize { get; private set; }

        bool disposed = false;

        MemoryStream rawData;

        public Pac(string filePath)
            : base(filePath)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.BigEndian))
            {
                rawData = new MemoryStream(reader.ReadBytes((int)reader.BaseStream.Length), 0, (int)reader.BaseStream.Length, false, true);

                reader.BaseStream.Seek(0, SeekOrigin.Begin);

                NumFiles = reader.ReadUInt32();
                Unknown0x04 = reader.ReadUInt32();
                Unknown0x08 = reader.ReadUInt32();
                Unknown0x0C = reader.ReadUInt32();

                Files = new PacFile[NumFiles];
                for (int i = 0; i < Files.Length; i++) Files[i] = new PacFile(this, reader);

                FileSize = reader.BaseStream.Length;

                foreach (PacFile file in Files) file.SetLength(Files);

                DataStartPosition = reader.BaseStream.Position;
            }
        }

        public void ExtractFile(PacFile file, string path)
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
