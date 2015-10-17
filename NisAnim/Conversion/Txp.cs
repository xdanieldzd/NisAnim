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
    /* HIGHLY shitty PSP TXP (ex. Z.H.P.) handling here */

    public class TxpImage
    {
        // shim thingy
        public Bitmap Image { get; private set; }

        public TxpImage(Bitmap image)
        {
            Image = image;
        }

        public string PrepareRender(GLHelper glHelper)
        {
            string objectName = string.Format("{0}_hash-{1}", this.GetType().Name, this.GetHashCode());

            glHelper.Textures.AddTexture(objectName, Image, OGL.TextureWrapMode.ClampToEdge, OGL.TextureWrapMode.ClampToEdge, OGL.TextureMinFilter.Linear, OGL.TextureMagFilter.Linear);
            glHelper.Buffers.AddVertices(objectName, new GLVertex[]
            {
                new GLVertex(new Vector3(0.0f, 0.0f, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.White, new Vector2(0.0f, 0.0f)),
                new GLVertex(new Vector3(0.0f, Image.Height, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.White, new Vector2(0.0f, 1.0f)),
                new GLVertex(new Vector3(Image.Width, Image.Height, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.White, new Vector2(1.0f, 1.0f)),
                new GLVertex(new Vector3(Image.Width, 0.0f, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.White, new Vector2(1.0f, 0.0f))
            });

            glHelper.Buffers.AddIndices(objectName, new uint[] { 0, 1, 2, 2, 3, 0 }, OGL.PrimitiveType.Triangles);

            return objectName;
        }
    }

    [DisplayName("Txf Image File")]
    [FileNamePattern("(.*?)\\.(txp)$")]
    public class Txp : BaseFile
    {
        public ushort Width { get; private set; }
        public ushort Height { get; private set; }
        public ushort ColorCount { get; private set; }
        public ushort Unknown0x06 { get; private set; }
        public ushort Unknown0x08MaybeColorCount { get; private set; }
        public ushort PaletteCount { get; private set; }
        public ushort TiledFlag { get; private set; }
        public ushort Unknown0x0E { get; private set; }

        List<Color[]> palettes;
        byte[] pixelData;

        public List<TxpImage> Images { get; private set; }

        bool disposed = false;

        public Txp(string filePath)
            : base(filePath)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.LittleEndian))
            {
                Width = reader.ReadUInt16();
                Height = reader.ReadUInt16();
                ColorCount = reader.ReadUInt16();
                Unknown0x06 = reader.ReadUInt16();
                Unknown0x08MaybeColorCount = reader.ReadUInt16();
                PaletteCount = reader.ReadUInt16();
                TiledFlag = reader.ReadUInt16();
                Unknown0x0E = reader.ReadUInt16();

                palettes = new List<Color[]>();
                for (int i = 0; i < PaletteCount; i++)
                {
                    Color[] palette = new Color[ColorCount];
                    for (int c = 0; c < ColorCount; c++)
                    {
                        byte r = reader.ReadByte();
                        byte g = reader.ReadByte();
                        byte b = reader.ReadByte();
                        byte a = reader.ReadByte();
                        palette[c] = Color.FromArgb(a, r, g, b);
                    }
                    palettes.Add(palette);
                }

                //8bpp indexed only atm!
                pixelData = new byte[Width * Height];

                if (TiledFlag == 0)
                {
                    pixelData = reader.ReadBytes(pixelData.Length);
                }
                else
                {
                    int tw = (ColorCount == 256 ? 16 : 32), th = 8;
                    for (int iy = 0; iy < Height; iy += th)
                    {
                        for (int ix = 0; ix < Width; ix += tw)
                        {
                            for (int ty = 0; ty < th; ty++)
                            {
                                if (ColorCount == 256)
                                {
                                    for (int tx = 0; tx < tw; tx++)
                                    {
                                        byte idx = reader.ReadByte();
                                        pixelData[((iy + ty) * Width) + (ix + tx)] = idx;
                                    }
                                }
                                else
                                {
                                    for (int tx = 0; tx < tw; tx += 2)
                                    {
                                        byte idx = reader.ReadByte();
                                        pixelData[((iy + ty) * Width) + (ix + tx)] = (byte)(idx & 0x0F);
                                        pixelData[((iy + ty) * Width) + (ix + tx + 1)] = (byte)((idx >> 4) & 0x0F);
                                    }
                                }
                            }
                        }
                    }
                }

                Images = new List<TxpImage>();

                for (int i = 0; i < PaletteCount; i++)
                {
                    Bitmap bitmap = new Bitmap(Width, Height, PixelFormat.Format8bppIndexed);
                    ColorPalette palette = bitmap.Palette;
                    for (int j = 0; j < ColorCount; j++) palette.Entries[j] = palettes[i][j];
                    bitmap.Palette = palette;

                    BitmapData bmpData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, bitmap.PixelFormat);
                    Marshal.Copy(pixelData, 0, bmpData.Scan0, pixelData.Length);
                    bitmap.UnlockBits(bmpData);

                    Images.Add(new TxpImage(bitmap));
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    foreach (TxpImage txpImage in Images)
                        txpImage.Image.Dispose();
                }

                this.disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
