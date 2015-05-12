using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.ComponentModel;

using OpenTK;
using OpenTK.Graphics;
using OGL = OpenTK.Graphics.OpenGL;

using NisAnim.IO;
using NisAnim.OpenGL;

namespace NisAnim.Conversion
{
    /* Dxt reference: https://www.opengl.org/registry/specs/EXT/texture_compression_s3tc.txt */

    public enum TxfDataFormat : byte
    {
        Argb8888 = 0x00,
        RgbaDxt1 = 0x02,
        RgbaDxt3 = 0x04,
        RgbaDxt5 = 0x06,
        Indexed8bpp = 0x09,
        Argb1555 = 0x0B,
        Argb4444 = 0x0C,
        Rgb565 = 0x0D   /* assumed */
    };

    [DisplayName("Txf Header")]
    public class TxfHeader
    {
        public const int Size = 0x10;

        [DisplayName("Format")]
        public TxfDataFormat Format { get; private set; }
        [DisplayName("Unknown 0x01")]
        public byte Unknown0x01 { get; private set; }
        [DisplayName("Unknown 0x02")]
        public ushort Unknown0x02 { get; private set; }
        [DisplayName("Width")]
        public ushort Width { get; private set; }
        [DisplayName("Height")]
        public ushort Height { get; private set; }
        [DisplayName("Unknown 0x08")]
        public ushort Unknown0x08 { get; private set; }
        [DisplayName("Unknown 0x0A")]
        public ushort Unknown0x0A { get; private set; }
        [DisplayName("Offset")]
        public uint Offset { get; private set; }

        [Browsable(false)]
        public bool IsIndexedImage { get { return (Format == TxfDataFormat.Indexed8bpp); } }

        /* Note: Offset is not an "offset" if this is the only block header in the file; then Offset is actually the total size of the data */

        public TxfHeader(EndianBinaryReader reader)
        {
            Format = (TxfDataFormat)reader.ReadByte();
            Unknown0x01 = reader.ReadByte();
            Unknown0x02 = reader.ReadUInt16();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            Unknown0x08 = reader.ReadUInt16();
            Unknown0x0A = reader.ReadUInt16();
            Offset = reader.ReadUInt32();
        }
    }

    [DisplayName("Converted Image Information")]
    public class ImageInformation
    {
        [DisplayName("Pixel Data Header")]
        public TxfHeader PixelDataHeader { get; private set; }
        [DisplayName("Palette Data Header")]
        public TxfHeader PaletteDataHeader { get; private set; }
        [DisplayName("Converted Bitmap")]
        public Bitmap Bitmap { get; private set; }

        [DisplayName("Image Offset")]
        public long ImageOffset { get; set; }
        [DisplayName("Palette Offset")]
        public long PaletteOffset { get; set; }

        public ImageInformation(TxfHeader pixelDataHeader, TxfHeader paletteHeader, Bitmap bitmap)
        {
            PixelDataHeader = pixelDataHeader;
            PaletteDataHeader = paletteHeader;
            Bitmap = bitmap;
        }

        public string PrepareRender(GLHelper glHelper)
        {
            string objectName = string.Format("{0}_hash-{1}", this.GetType().Name, this.GetHashCode());

            glHelper.Textures.AddTexture(objectName, Bitmap);
            glHelper.Buffers.AddVertices(objectName, new GLVertex[]
            {
                new GLVertex(new Vector3(0.0f, 0.0f, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.White, new Vector2(0.0f, 0.0f)),
                new GLVertex(new Vector3(0.0f, Bitmap.Height, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.White, new Vector2(0.0f, 1.0f)),
                new GLVertex(new Vector3(Bitmap.Width, Bitmap.Height, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.White, new Vector2(1.0f, 1.0f)),
                new GLVertex(new Vector3(Bitmap.Width, 0.0f, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.White, new Vector2(1.0f, 0.0f))
            });

            glHelper.Buffers.AddIndices(objectName, new uint[] { 0, 1, 2, 2, 3, 0 }, OGL.PrimitiveType.Triangles);

            return objectName;
        }
    }

    [DisplayName("Txf Image File")]
    public class Txf : BaseFile
    {
        public const string FileNamePattern = "(.*?)\\.(txf)$";

        public static Dictionary<TxfDataFormat, int> BitsPerPixel = new Dictionary<TxfDataFormat, int>()
        {
            { TxfDataFormat.Argb8888, 32 },
            { TxfDataFormat.RgbaDxt1, 2 },
            { TxfDataFormat.RgbaDxt3, 4 },
            { TxfDataFormat.RgbaDxt5, 4 },
            { TxfDataFormat.Indexed8bpp, 8 },
            { TxfDataFormat.Argb1555, 16 },
            { TxfDataFormat.Argb4444, 16 },
            { TxfDataFormat.Rgb565, 16 },
        };

        [DisplayName("Pixel Data Headers")]
        public List<TxfHeader> PixelDataHeaders { get; private set; }
        [DisplayName("Palette Data Headers")]
        public List<TxfHeader> PaletteDataHeaders { get; private set; }
        [DisplayName("Converted Images")]
        public List<ImageInformation> Images { get; private set; }

        [Browsable(false)]
        public long StartPosition { get; private set; }

        MemoryStream rawData;
        List<ColorPalette> palettes;

        bool disposed = false;

        public Txf(string filePath)
            : base(filePath)
        {
            Initialize();

            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.BigEndian))
            {
                rawData = new MemoryStream(reader.ReadBytes((int)reader.BaseStream.Length), 0, (int)reader.BaseStream.Length, false, true);

                using (MemoryStream tempStream = new MemoryStream(rawData.ToArray()))
                {
                    using (EndianBinaryReader tempReader = new EndianBinaryReader(tempStream, Endian.BigEndian))
                    {
                        StartPosition = tempReader.BaseStream.Position;

                        /* Assume a single header, sure hope only anm .dat files have more than one... */
                        PixelDataHeaders.Add(new TxfHeader(tempReader));
                        ConvertImage(tempReader, PixelDataHeaders.FirstOrDefault(), null);
                    }
                }
            }
        }

        public Txf(EndianBinaryReader reader)
        {
            Initialize();

            StartPosition = reader.BaseStream.Position;
            PixelDataHeaders.Add(new TxfHeader(reader));
            ConvertImage(reader, PixelDataHeaders.FirstOrDefault(), null);

            reader.BaseStream.Seek(StartPosition + PixelDataHeaders.FirstOrDefault().Offset, SeekOrigin.Begin);
        }

        public Txf(EndianBinaryReader reader, int imageDataSize, int numPixelHeaders, int numPaletteHeaders)
        {
            Initialize();

            rawData = new MemoryStream(reader.ReadBytes(imageDataSize), 0, imageDataSize, false, true);

            using (MemoryStream tempStream = new MemoryStream(rawData.ToArray()))
            {
                using (EndianBinaryReader tempReader = new EndianBinaryReader(tempStream, Endian.BigEndian))
                {
                    StartPosition = tempReader.BaseStream.Position;

                    for (int i = 0; i < numPixelHeaders; i++) PixelDataHeaders.Add(new TxfHeader(tempReader));
                    for (int i = 0; i < numPaletteHeaders; i++) PaletteDataHeaders.Add(new TxfHeader(tempReader));
                }
            }
        }

        private void Initialize()
        {
            PixelDataHeaders = new List<TxfHeader>();
            PaletteDataHeaders = new List<TxfHeader>();
            Images = new List<ImageInformation>();

            StartPosition = 0;
            palettes = new List<ColorPalette>();
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (rawData != null)
                        rawData.Close();

                    foreach (ImageInformation imageInfos in Images)
                        imageInfos.Bitmap.Dispose();
                }

                this.disposed = true;
            }

            base.Dispose(disposing);
        }

        public ImageInformation GetImageInformation(TxfHeader pixelDataHeader, TxfHeader paletteDataHeader)
        {
            ImageInformation image = Images.FirstOrDefault(x => x.PixelDataHeader == pixelDataHeader && x.PaletteDataHeader == paletteDataHeader);

            if (image != null) return image;

            using (MemoryStream tempStream = new MemoryStream(rawData.ToArray()))
            {
                using (EndianBinaryReader tempReader = new EndianBinaryReader(tempStream, Endian.BigEndian))
                {
                    return ConvertImage(tempReader, pixelDataHeader, paletteDataHeader);
                }
            }
        }

        private ImageInformation ConvertImage(EndianBinaryReader reader, TxfHeader pixelDataHeader, TxfHeader paletteDataHeader)
        {
            PixelFormat pixelFormat;
            byte[] pixelData = null, paletteData = null;
            long pixelDataOffset = -1, paletteDataOffset = -1;

            /* Seek to the beginning of the pixel data block */
            pixelDataOffset = (StartPosition + (PixelDataHeaders.Count * TxfHeader.Size) + (PaletteDataHeaders.Count * TxfHeader.Size) + (pixelDataHeader.IsIndexedImage ? pixelDataHeader.Offset : 0));
            reader.BaseStream.Seek(pixelDataOffset, SeekOrigin.Begin);

            /* Determine pixel format & read/convert pixel data */
            switch (pixelDataHeader.Format)
            {
                case TxfDataFormat.Argb8888:
                    {
                        pixelFormat = PixelFormat.Format32bppArgb;
                        pixelData = GetPixelData(reader, pixelDataHeader.Format, CalculatePixelDataSize(pixelDataHeader.Format, pixelDataHeader.Width, pixelDataHeader.Height));
                    }
                    break;

                case TxfDataFormat.RgbaDxt1:
                case TxfDataFormat.RgbaDxt3:
                case TxfDataFormat.RgbaDxt5:
                    {
                        pixelFormat = PixelFormat.Format32bppArgb;
                        pixelData = DecompressDxt(reader, pixelDataHeader.Format, pixelDataHeader.Width, pixelDataHeader.Height);
                    }
                    break;

                case TxfDataFormat.Indexed8bpp:
                    {
                        pixelFormat = PixelFormat.Format8bppIndexed;
                        pixelData = GetPixelData(reader, pixelDataHeader.Format, CalculatePixelDataSize(pixelDataHeader.Format, pixelDataHeader.Width, pixelDataHeader.Height));
                    }
                    break;

                case TxfDataFormat.Argb1555:
                    {
                        pixelFormat = PixelFormat.Format16bppArgb1555;
                        pixelData = GetPixelData(reader, pixelDataHeader.Format, pixelDataHeader.Offset);
                    }
                    break;

                case TxfDataFormat.Argb4444:
                    {
                        pixelFormat = PixelFormat.Format32bppArgb;
                        byte[] tempData = GetPixelData(reader, pixelDataHeader.Format, pixelDataHeader.Offset);
                        pixelData = new byte[tempData.Length << 1];
                        for (int i = 0, j = 0; i < tempData.Length; i += 2, j += 4)
                        {
                            pixelData[j + 0] = (byte)((tempData[i] & 0x0F) | ((tempData[i] & 0x0F) << 4));
                            pixelData[j + 1] = (byte)((tempData[i] & 0xF0) | ((tempData[i] & 0xF0) >> 4));
                            pixelData[j + 2] = (byte)((tempData[i + 1] & 0x0F) | ((tempData[i + 1] & 0x0F) << 4));
                            pixelData[j + 3] = (byte)((tempData[i + 1] & 0xF0) | ((tempData[i + 1] & 0xF0) >> 4));
                        }
                    }
                    break;

                case TxfDataFormat.Rgb565:
                    {
                        pixelFormat = PixelFormat.Format16bppRgb565;
                        pixelData = GetPixelData(reader, pixelDataHeader.Format, pixelDataHeader.Offset);
                    }
                    break;

                default:
                    throw new Exception("Unsupported pixel data format.");
            }

            /* If we were given a palette data block header, seek to & read the palette data, too! */
            if (paletteDataHeader != null)
            {
                paletteDataOffset = StartPosition + (PixelDataHeaders.Count * TxfHeader.Size) + (PaletteDataHeaders.Count * TxfHeader.Size) + paletteDataHeader.Offset;
                reader.BaseStream.Seek(paletteDataOffset, SeekOrigin.Begin);
                paletteData = GetPixelData(reader, paletteDataHeader.Format, CalculatePaletteDataSize(pixelDataHeader.Format, paletteDataHeader.Format));
            }

            /* Generate the final bitmap */
            Bitmap bitmap = new Bitmap(pixelDataHeader.Width, pixelDataHeader.Height, pixelFormat);
            BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
            Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);
            bitmap.UnlockBits(bmpData);

            /* Apply palette if necessary */
            if (paletteDataHeader != null && paletteData != null)
                ApplyColorPalette(bitmap, paletteData, paletteDataHeader.Format);

            ImageInformation imageInfos = new ImageInformation(pixelDataHeader, paletteDataHeader, bitmap) { ImageOffset = pixelDataOffset, PaletteOffset = paletteDataOffset };
            Images.Add(imageInfos);

            return imageInfos;
        }

        private byte[] GetPixelData(EndianBinaryReader reader, TxfDataFormat format, uint dataSize)
        {
            byte[] pixelData = new byte[dataSize];
            int bytesPerPixel = (BitsPerPixel[format] >> 3);

            for (int i = 0; i < pixelData.Length; i += bytesPerPixel)
                for (int j = bytesPerPixel - 1; j >= 0; j--)
                    pixelData[i + j] = reader.ReadByte();

            return pixelData;
        }

        private byte[] DecompressDxt(EndianBinaryReader reader, TxfDataFormat format, int width, int height)
        {
            byte[] pixelData = new byte[CalculatePixelDataSize(TxfDataFormat.Argb8888, width, height)];

            for (int y = 0; y < height; y += 4)
            {
                for (int x = 0; x < width; x += 4)
                {
                    byte[] decompressedBlock = DecompressDxtBlock(reader, format);

                    for (int py = 0; py < 4; py++)
                    {
                        for (int px = 0; px < 4; px++)
                        {
                            int ix = (x + px);
                            int iy = (y + py);

                            if (ix >= width || iy >= height) continue;

                            for (int c = 0; c < 4; c++)
                                pixelData[(((iy * width) + ix) * 4) + c] = decompressedBlock[(((py * 4) + px) * 4) + c];
                        }
                    }
                }
            }

            return pixelData;
        }

        private uint CalculatePixelDataSize(TxfDataFormat pixelFormat, int width, int height)
        {
            return (uint)(width * height * (BitsPerPixel[pixelFormat] >> 3));
        }

        private uint CalculatePaletteDataSize(TxfDataFormat pixelFormat, TxfDataFormat paletteFormat)
        {
            uint colorCount, bytesPerColor;
            switch (pixelFormat)
            {
                case TxfDataFormat.Indexed8bpp: colorCount = 256; break;
                default: throw new Exception(string.Format("Unknown color count for pixel format '{0}'.", pixelFormat));
            }

            switch (paletteFormat)
            {
                case TxfDataFormat.Argb8888: bytesPerColor = 4; break;
                case TxfDataFormat.Argb4444: bytesPerColor = 2; break;
                default: throw new Exception(string.Format("Unknown bytes/color for palette format '{0}'.", paletteFormat));
            }

            return (colorCount * bytesPerColor);
        }

        private void ApplyColorPalette(Bitmap bitmap, byte[] paletteData, TxfDataFormat paletteFormat)
        {
            ColorPalette palette = bitmap.Palette;
            switch (paletteFormat)
            {
                case TxfDataFormat.Argb8888:
                    {
                        for (int i = 0, j = 0; i < palette.Entries.Length; i++, j += 4)
                        {
                            palette.Entries[i] = Color.FromArgb(paletteData[j + 3], paletteData[j + 2], paletteData[j + 1], paletteData[j]);
                        }
                    }
                    break;

                case TxfDataFormat.Argb4444:
                    {
                        for (int i = 0, j = 0; i < palette.Entries.Length; i++, j += 2)
                        {
                            palette.Entries[i] = Color.FromArgb(
                                (byte)((paletteData[j] & 0xF0) | ((paletteData[j] & 0xF0) >> 4)),
                                (byte)((paletteData[j + 1] & 0x0F) | ((paletteData[j + 1] & 0x0F) << 4)),
                                (byte)((paletteData[j + 1] & 0xF0) | ((paletteData[j + 1] & 0xF0) >> 4)),
                                (byte)((paletteData[j] & 0x0F) | ((paletteData[j] & 0x0F) << 4)));
                        }
                    }
                    break;

                default:
                    throw new Exception(string.Format("Cannot generate color palette from data format '{0}'.", paletteFormat));
            }
            bitmap.Palette = palette;
        }

        private byte[] DecompressDxtBlock(EndianBinaryReader reader, TxfDataFormat format)
        {
            byte[] outputData = new byte[(4 * 4) * 4];
            byte[] colorData = null, alphaData = null;

            if (format != TxfDataFormat.RgbaDxt1)
                alphaData = DecompressDxtAlpha(reader, format);

            colorData = DecompressDxtColor(reader, format);

            for (int i = 0; i < colorData.Length; i += 4)
            {
                outputData[i] = colorData[i];
                outputData[i + 1] = colorData[i + 1];
                outputData[i + 2] = colorData[i + 2];
                outputData[i + 3] = (alphaData != null ? alphaData[i + 3] : colorData[i + 3]);
            }

            return outputData;
        }

        private byte[] DecompressDxtColor(EndianBinaryReader reader, TxfDataFormat format)
        {
            byte[] colorOut = new byte[(4 * 4) * 4];

            ushort color0 = reader.ReadUInt16(Endian.LittleEndian);
            ushort color1 = reader.ReadUInt16(Endian.LittleEndian);
            uint bits = reader.ReadUInt32(Endian.LittleEndian);

            byte c0r, c0g, c0b, c1r, c1g, c1b;
            UnpackRgb565(color0, out c0r, out c0g, out c0b);
            UnpackRgb565(color1, out c1r, out c1g, out c1b);

            byte[] bitsExt = new byte[16];
            for (int i = 0; i < bitsExt.Length; i++)
                bitsExt[i] = (byte)((bits >> (i * 2)) & 0x3);

            for (int y = 0; y < 4; y++)
            {
                for (int x = 0; x < 4; x++)
                {
                    byte code = bitsExt[(y * 4) + x];
                    int destOffset = ((y * 4) + x) * 4;

                    if (format == TxfDataFormat.RgbaDxt1)
                    {
                        colorOut[destOffset + 3] = (byte)((color0 <= color1 && code == 3) ? 0 : 0xFF);
                    }

                    if (format == TxfDataFormat.RgbaDxt1 && color0 <= color1)
                    {
                        switch (code)
                        {
                            case 0x00:
                                colorOut[destOffset + 0] = c0b;
                                colorOut[destOffset + 1] = c0g;
                                colorOut[destOffset + 2] = c0r;
                                break;

                            case 0x01:
                                colorOut[destOffset + 0] = c1b;
                                colorOut[destOffset + 1] = c1g;
                                colorOut[destOffset + 2] = c1r;
                                break;

                            case 0x02:
                                colorOut[destOffset + 0] = (byte)((c0b + c1b) / 2);
                                colorOut[destOffset + 1] = (byte)((c0g + c1g) / 2);
                                colorOut[destOffset + 2] = (byte)((c0r + c1r) / 2);
                                break;

                            case 0x03:
                                colorOut[destOffset + 0] = 0;
                                colorOut[destOffset + 1] = 0;
                                colorOut[destOffset + 2] = 0;
                                break;
                        }
                    }
                    else
                    {
                        switch (code)
                        {
                            case 0x00:
                                colorOut[destOffset + 0] = c0b;
                                colorOut[destOffset + 1] = c0g;
                                colorOut[destOffset + 2] = c0r;
                                break;

                            case 0x01:
                                colorOut[destOffset + 0] = c1b;
                                colorOut[destOffset + 1] = c1g;
                                colorOut[destOffset + 2] = c1r;
                                break;

                            case 0x02:
                                colorOut[destOffset + 0] = (byte)((2 * c0b + c1b) / 3);
                                colorOut[destOffset + 1] = (byte)((2 * c0g + c1g) / 3);
                                colorOut[destOffset + 2] = (byte)((2 * c0r + c1r) / 3);
                                break;

                            case 0x03:
                                colorOut[destOffset + 0] = (byte)((c0b + 2 * c1b) / 3);
                                colorOut[destOffset + 1] = (byte)((c0g + 2 * c1g) / 3);
                                colorOut[destOffset + 2] = (byte)((c0r + 2 * c1r) / 3);
                                break;
                        }
                    }
                }
            }

            return colorOut;
        }

        private void UnpackRgb565(ushort rgb565, out byte r, out byte g, out byte b)
        {
            r = (byte)((rgb565 & 0xF800) >> 11);
            r = (byte)((r << 3) | (r >> 2));
            g = (byte)((rgb565 & 0x07E0) >> 5);
            g = (byte)((g << 2) | (g >> 4));
            b = (byte)(rgb565 & 0x1F);
            b = (byte)((b << 3) | (b >> 2));
        }

        private byte[] DecompressDxtAlpha(EndianBinaryReader reader, TxfDataFormat format)
        {
            byte[] alphaOut = new byte[(4 * 4) * 4];

            switch (format)
            {
                case TxfDataFormat.RgbaDxt3:
                    {
                        ulong alpha = reader.ReadUInt64();
                        for (int i = 0; i < alphaOut.Length; i += 4)
                        {
                            alphaOut[i + 3] = (byte)(((alpha & 0xF) << 4) | (alpha & 0xF));
                            alpha >>= 4;
                        }
                    }
                    break;

                case TxfDataFormat.RgbaDxt5:
                    {
                        byte alpha0 = reader.ReadByte();
                        byte alpha1 = reader.ReadByte();
                        byte bits_0 = reader.ReadByte();
                        byte bits_1 = reader.ReadByte();
                        byte bits_2 = reader.ReadByte();
                        byte bits_3 = reader.ReadByte();
                        byte bits_4 = reader.ReadByte();
                        byte bits_5 = reader.ReadByte();

                        ulong bits = (ulong)(((ulong)bits_0 << 40) | ((ulong)bits_1 << 32) | ((ulong)bits_2 << 24) | ((ulong)bits_3 << 16) | ((ulong)bits_4 << 8) | (ulong)bits_5);

                        byte[] bitsExt = new byte[16];
                        for (int i = 0; i < bitsExt.Length; i++)
                            bitsExt[i] = (byte)((bits >> (i * 3)) & 0x7);

                        for (int y = 0; y < 4; y++)
                        {
                            for (int x = 0; x < 4; x++)
                            {
                                byte code = bitsExt[(y * 4) + x];
                                int destOffset = (((y * 4) + x) * 4) + 3;

                                if (alpha0 > alpha1)
                                {
                                    switch (code)
                                    {
                                        case 0x00: alphaOut[destOffset] = alpha0; break;
                                        case 0x01: alphaOut[destOffset] = alpha1; break;
                                        case 0x02: alphaOut[destOffset] = (byte)((6 * alpha0 + 1 * alpha1) / 7); break;
                                        case 0x03: alphaOut[destOffset] = (byte)((5 * alpha0 + 2 * alpha1) / 7); break;
                                        case 0x04: alphaOut[destOffset] = (byte)((4 * alpha0 + 3 * alpha1) / 7); break;
                                        case 0x05: alphaOut[destOffset] = (byte)((3 * alpha0 + 4 * alpha1) / 7); break;
                                        case 0x06: alphaOut[destOffset] = (byte)((2 * alpha0 + 5 * alpha1) / 7); break;
                                        case 0x07: alphaOut[destOffset] = (byte)((1 * alpha0 + 6 * alpha1) / 7); break;
                                    }
                                }
                                else
                                {
                                    switch (code)
                                    {
                                        case 0x00: alphaOut[destOffset] = alpha0; break;
                                        case 0x01: alphaOut[destOffset] = alpha1; break;
                                        case 0x02: alphaOut[destOffset] = (byte)((4 * alpha0 + 1 * alpha1) / 5); break;
                                        case 0x03: alphaOut[destOffset] = (byte)((3 * alpha0 + 2 * alpha1) / 5); break;
                                        case 0x04: alphaOut[destOffset] = (byte)((2 * alpha0 + 3 * alpha1) / 5); break;
                                        case 0x05: alphaOut[destOffset] = (byte)((1 * alpha0 + 4 * alpha1) / 5); break;
                                        case 0x06: alphaOut[destOffset] = 0x00; break;
                                        case 0x07: alphaOut[destOffset] = 0xFF; break;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }

            return alphaOut;
        }
    }
}
