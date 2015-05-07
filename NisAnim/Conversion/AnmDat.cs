using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.ComponentModel;

using NisAnim.IO;

namespace NisAnim.Conversion
{
    /* Data 0 */
    [DisplayName("Animation")]
    public class AnimationData
    {
        [Browsable(false)]
        public InfoBlockData ParentInfoBlock { get; private set; }

        [DisplayName("Animation ID")]
        public ushort AnimationID { get; private set; }
        [DisplayName("First Node ID")]
        public ushort FirstNodeID { get; private set; }

        [Browsable(false)]
        public AnimationNodeData FirstNode { get; set; }

        public AnimationData(InfoBlockData parent, EndianBinaryReader reader)
        {
            ParentInfoBlock = parent;

            AnimationID = reader.ReadUInt16();
            FirstNodeID = reader.ReadUInt16();
        }
    }

    /* Data 1 */
    [DisplayName("Animation Node")]
    public class AnimationNodeData
    {
        [Browsable(false)]
        public InfoBlockData ParentInfoBlock { get; private set; }

        [DisplayName("Unknown 0x00")]
        public uint Unknown0x00 { get; private set; }
        [DisplayName("Unknown 0x04")]
        public ushort Unknown0x04 { get; private set; }
        [DisplayName("Unknown 0x06")]
        public ushort Unknown0x06 { get; private set; }
        [DisplayName("First Animation Frame ID")]
        public short FirstAnimationFrameID { get; private set; }
        [DisplayName("Number of Animation Frames")]
        public ushort NumAnimationFrames { get; private set; }
        [DisplayName("Sibling Node ID")]
        public short SiblingNodeID { get; private set; }
        [DisplayName("Child (Lower) Node ID")]
        public short ChildNodeID { get; private set; }

        [DisplayName("Animation Frames")]
        public AnimationFrameData[] AnimationFrames { get; set; }
        [Browsable(false)]
        public AnimationNodeData SiblingNode { get; set; }
        [Browsable(false)]
        public AnimationNodeData ChildNode { get; set; }

        public AnimationNodeData(InfoBlockData parent, EndianBinaryReader reader)
        {
            ParentInfoBlock = parent;

            Unknown0x00 = reader.ReadUInt32();
            Unknown0x04 = reader.ReadUInt16();
            Unknown0x06 = reader.ReadUInt16();
            FirstAnimationFrameID = reader.ReadInt16();
            NumAnimationFrames = reader.ReadUInt16();
            SiblingNodeID = reader.ReadInt16();
            ChildNodeID = reader.ReadInt16();
        }
    }

    /* Data 2 */
    [DisplayName("Animation Frame")]
    public class AnimationFrameData
    {
        [Browsable(false)]
        public InfoBlockData ParentInfoBlock { get; private set; }

        [DisplayName("Frame Time")]
        public ushort FrameTime { get; private set; }
        [DisplayName("Unknown 0x02")]
        public byte Unknown0x02 { get; private set; }
        [DisplayName("Unknown 0x03")]
        public byte Unknown0x03 { get; private set; }
        [DisplayName("Sprite ID")]
        public ushort SpriteID { get; private set; }
        [DisplayName("Transform ID")]
        public ushort TransformID { get; private set; }
        [DisplayName("Unknown 0x08")]
        public ushort Unknown0x08 { get; private set; }
        [DisplayName("Unknown 0x0A")]
        public ushort Unknown0x0A { get; private set; }
        [DisplayName("Frame Time")]

        [Browsable(false)]
        public SpriteData Sprite { get; set; }
        [Browsable(false)]
        public TransformData Transform { get; set; }

        public AnimationFrameData(InfoBlockData parent, EndianBinaryReader reader)
        {
            ParentInfoBlock = parent;

            FrameTime = reader.ReadUInt16();
            Unknown0x02 = reader.ReadByte();
            Unknown0x03 = reader.ReadByte();
            SpriteID = reader.ReadUInt16();
            TransformID = reader.ReadUInt16();
            Unknown0x08 = reader.ReadUInt16();
            Unknown0x0A = reader.ReadUInt16();
        }
    }

    /* Data 3 */
    [DisplayName("Palette Reference")]
    public class PaletteReferenceData
    {
        [Browsable(false)]
        public InfoBlockData ParentInfoBlock { get; private set; }

        [DisplayName("Offset or Size?")]
        public uint OffsetOrSize { get; private set; }
        [DisplayName("Color Count")]
        public uint ColorCount { get; private set; }

        public PaletteReferenceData(InfoBlockData parent, EndianBinaryReader reader)
        {
            ParentInfoBlock = parent;

            OffsetOrSize = reader.ReadUInt32();
            ColorCount = reader.ReadUInt32();
        }
    }

    /* Data 4 */
    [DisplayName("Image Reference")]
    public class ImageReferenceData
    {
        [Browsable(false)]
        public InfoBlockData ParentInfoBlock { get; private set; }

        [DisplayName("Offset or Size?")]
        public uint OffsetOrSize { get; private set; }
        [DisplayName("Width")]
        public ushort Width { get; private set; }
        [DisplayName("Height")]
        public ushort Height { get; private set; }
        [DisplayName("Unknown 0x08")]
        public ushort Unknown0x08 { get; private set; }
        [DisplayName("Unknown 0x0A")]
        public ushort Unknown0x0A { get; private set; }

        public ImageReferenceData(InfoBlockData parent, EndianBinaryReader reader)
        {
            ParentInfoBlock = parent;

            OffsetOrSize = reader.ReadUInt32();
            Width = reader.ReadUInt16();
            Height = reader.ReadUInt16();
            Unknown0x08 = reader.ReadUInt16();
            Unknown0x0A = reader.ReadUInt16();
        }
    }

    /* Data 5 */
    [DisplayName("Sprite")]
    public class SpriteData
    {
        [Browsable(false)]
        public InfoBlockData ParentInfoBlock { get; private set; }

        [DisplayName("Image Number")]
        public ushort ImageNumber { get; private set; }
        [DisplayName("Palette Number")]
        public ushort PaletteNumber { get; private set; }
        [DisplayName("Unknown 0x04")]
        public ushort Unknown0x04 { get; private set; }
        [DisplayName("Unknown 0x06")]
        public ushort Unknown0x06 { get; private set; }
        [DisplayName("Rectangle")]
        public Rectangle Rectangle { get; private set; }

        [Browsable(false)]
        public Bitmap Image { get; set; }

        public SpriteData(InfoBlockData parent, EndianBinaryReader reader)
        {
            ParentInfoBlock = parent;

            ImageNumber = reader.ReadUInt16();
            PaletteNumber = reader.ReadUInt16();
            Unknown0x04 = reader.ReadUInt16();
            Unknown0x06 = reader.ReadUInt16();
            Rectangle = new Rectangle(reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16(), reader.ReadInt16());
        }

        public void GenerateBitmap(Txf txf)
        {
            //ImageReferenceData imgref = ParentInfoBlock.ImageReferences[ImageNumber];
            // wrong? index out of range sometimes? also not used yet b/c i don't know how that thing works

            ImageInformation image = txf.GetImageInformation(txf.PixelDataHeaders[ImageNumber], txf.PaletteDataHeaders[PaletteNumber]);

            if (image == null || Rectangle.Width == 0 || Rectangle.Height == 0)
            {
                Image = new Bitmap(4, 4);
                return;
            }

            Image = new Bitmap(Rectangle.Width, Rectangle.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            using (Graphics g = Graphics.FromImage(Image))
            {
                g.DrawImage(image.Bitmap, new Rectangle(0, 0, Image.Width, Image.Height), Rectangle, GraphicsUnit.Pixel);
            }

            /*System.Drawing.Imaging.BitmapData bmpd = Image.LockBits(new Rectangle(0, 0, Image.Width, Image.Height), System.Drawing.Imaging.ImageLockMode.ReadWrite, Image.PixelFormat);
            byte[] tmp = new byte[bmpd.Height * bmpd.Stride];
            System.Runtime.InteropServices.Marshal.Copy(bmpd.Scan0, tmp, 0, tmp.Length);
            Image.UnlockBits(bmpd);*/
        }
    }

    /* Data 6 */
    [DisplayName("Transform")]
    public class TransformData
    {
        [Browsable(false)]
        public InfoBlockData ParentInfoBlock { get; private set; }

        [DisplayName("Base Offset")]
        public Point BaseOffset { get; private set; }
        [DisplayName("Unknown 0x04")]
        public ushort Unknown0x04 { get; private set; }
        [DisplayName("Transform Offset ID")]
        public ushort TransformOffsetID { get; private set; }
        [DisplayName("Scale")]
        public Point Scale { get; private set; }
        [DisplayName("Rotation Angle")]
        public int RotationAngle { get; private set; }

        [Browsable(false)]
        public TransformOffsetData TransformOffset { get; set; }

        public TransformData(InfoBlockData parent, EndianBinaryReader reader)
        {
            ParentInfoBlock = parent;

            BaseOffset = new Point(reader.ReadInt16(), reader.ReadInt16());
            Unknown0x04 = reader.ReadUInt16();
            TransformOffsetID = reader.ReadUInt16();
            Scale = new Point(reader.ReadInt16(), reader.ReadInt16());
            RotationAngle = reader.ReadInt32();
        }
    }

    /* Data 7 */
    [DisplayName("Transform Offset")]
    public class TransformOffsetData
    {
        [Browsable(false)]
        public InfoBlockData ParentInfoBlock { get; private set; }

        [DisplayName("Offset")]
        public Point Offset { get; private set; }

        public TransformOffsetData(InfoBlockData parent, EndianBinaryReader reader)
        {
            ParentInfoBlock = parent;

            Offset = new Point(reader.ReadInt16(), reader.ReadInt16());
        }
    }

    /* Data 8 */
    [DisplayName("Color Set")]
    public class ColorSetData
    {
        [Browsable(false)]
        public InfoBlockData ParentInfoBlock { get; private set; }

        [DisplayName("Color 1")]
        public Color Color1 { get; private set; }
        [DisplayName("Color 2")]
        public Color Color2 { get; private set; }
        [DisplayName("Color 3")]
        public Color Color3 { get; private set; }
        [DisplayName("Color 4")]
        public Color Color4 { get; private set; }
        [DisplayName("Color 5")]
        public Color Color5 { get; private set; }

        public ColorSetData(InfoBlockData parent, EndianBinaryReader reader)
        {
            ParentInfoBlock = parent;

            byte r, g, b, a;

            r = reader.ReadByte();
            g = reader.ReadByte();
            b = reader.ReadByte();
            a = reader.ReadByte();
            Color1 = Color.FromArgb(a, r, g, b);

            r = reader.ReadByte();
            g = reader.ReadByte();
            b = reader.ReadByte();
            a = reader.ReadByte();
            Color2 = Color.FromArgb(a, r, g, b);

            r = reader.ReadByte();
            g = reader.ReadByte();
            b = reader.ReadByte();
            a = reader.ReadByte();
            Color3 = Color.FromArgb(a, r, g, b);

            r = reader.ReadByte();
            g = reader.ReadByte();
            b = reader.ReadByte();
            a = reader.ReadByte();
            Color4 = Color.FromArgb(a, r, g, b);

            r = reader.ReadByte();
            g = reader.ReadByte();
            b = reader.ReadByte();
            a = reader.ReadByte();
            Color5 = Color.FromArgb(a, r, g, b);
        }
    }

    /* Data 9 */
    [DisplayName("Unknown Signed Int")]
    public class UnknownSignedIntData
    {
        [Browsable(false)]
        public InfoBlockData ParentInfoBlock { get; private set; }

        [DisplayName("Unknown 0x00")]
        public int Unknown0x00 { get; private set; }

        public UnknownSignedIntData(InfoBlockData parent, EndianBinaryReader reader)
        {
            ParentInfoBlock = parent;

            Unknown0x00 = reader.ReadInt32();
        }
    }

    [DisplayName("Info Block")]
    public class InfoBlockData
    {
        [Browsable(false)]
        public AnmDat ParentAnmFile { get; private set; }

        [DisplayName("Offset in File")]
        public long Position { get; private set; }

        [DisplayName("Info Block Size")]
        public uint InfoEntrySize { get; private set; }
        [DisplayName("Animation Number")]
        public ushort AnmNumber { get; private set; }

        [DisplayName("Array Element Counts")]
        public ushort[] ElementCounts { get; private set; }
        [DisplayName("Unknown 0x3A")]
        public ushort Unknown0x3A { get; private set; }
        [DisplayName("Array Element Offsets")]
        public uint[] ElementOffsets { get; private set; }

        object[][] elementData;

        Type[] elementDataTypes = new Type[]
        {
            typeof(AnimationData),
            typeof(AnimationNodeData),
            typeof(AnimationFrameData),
            typeof(PaletteReferenceData),
            typeof(ImageReferenceData),
            typeof(SpriteData),
            typeof(TransformData),
            typeof(TransformOffsetData),
            typeof(ColorSetData),
            typeof(UnknownSignedIntData)
        };

        [DisplayName("Animations")]
        public AnimationData[] Animations { get { return (AnimationData[])elementData[0]; } }
        [DisplayName("Animation Nodes")]
        public AnimationNodeData[] AnimationNodes { get { return (AnimationNodeData[])elementData[1]; } }
        [DisplayName("Animation Frames")]
        public AnimationFrameData[] AnimationFrames { get { return (AnimationFrameData[])elementData[2]; } }
        [DisplayName("Palette References")]
        public PaletteReferenceData[] PaletteReferences { get { return (PaletteReferenceData[])elementData[3]; } }
        [DisplayName("Image References")]
        public ImageReferenceData[] ImageReferences { get { return (ImageReferenceData[])elementData[4]; } }
        [DisplayName("Sprites")]
        public SpriteData[] Sprites { get { return (SpriteData[])elementData[5]; } }
        [DisplayName("Transforms")]
        public TransformData[] Transforms { get { return (TransformData[])elementData[6]; } }
        [DisplayName("Transform Offsets")]
        public TransformOffsetData[] TransformOffsets { get { return (TransformOffsetData[])elementData[7]; } }
        [DisplayName("Color Sets")]
        public ColorSetData[] ColorSets { get { return (ColorSetData[])elementData[8]; } }
        [DisplayName("Unknown Signed Ints")]
        public UnknownSignedIntData[] UnknownSignedInts { get { return (UnknownSignedIntData[])elementData[9]; } }

        public InfoBlockData(AnmDat anmFile, EndianBinaryReader reader)
        {
            ParentAnmFile = anmFile;

            Position = reader.BaseStream.Position;

            InfoEntrySize = reader.ReadUInt32();
            AnmNumber = reader.ReadUInt16();

            ElementCounts = new ushort[10];
            for (int i = 0; i < ElementCounts.Length; i++) ElementCounts[i] = reader.ReadUInt16();
            Unknown0x3A = reader.ReadUInt16();
            ElementOffsets = new uint[10];
            for (int i = 0; i < ElementOffsets.Length; i++) ElementOffsets[i] = reader.ReadUInt32();

            elementData = new object[10][];
            for (int i = 0; i < elementData.Length; i++)
            {
                elementData[i] = (object[])Array.CreateInstance(elementDataTypes[i], ElementCounts[i]);
                for (int j = 0; j < elementData[i].Length; j++)
                    elementData[i][j] = (object)Activator.CreateInstance(elementDataTypes[i], new object[] { this, reader });
            }

            /* TEMP TESTING */
            uint[] elementSizes = new uint[10];
            for (int i = 0; i < elementSizes.Length; i++)
            {
                uint size = ((i == 9 ? InfoEntrySize : ElementOffsets[i + 1]) - ElementOffsets[i]);
                if (ElementCounts[i] != 0)
                    elementSizes[i] = size / ElementCounts[i];
            }
        }
    }

    [DisplayName("Animation Data File")]
    public class AnmDat : BaseFile
    {
        public const string FileNamePattern = "(anm)(.*?)\\.(dat)$";

        [DisplayName("Info Block Data Size")]
        public uint InfoBlockSize { get; private set; }
        [DisplayName("Image Data Size")]
        public uint ImageDataSize { get; private set; }
        [DisplayName("Number of Pixel Data Headers")]
        public uint NumPixelDataHeaders { get; private set; }
        [DisplayName("Number of Palette Data Headers")]
        public uint NumPaletteDataHeaders { get; private set; }
        [DisplayName("Number of Info Blocks")]
        public uint NumInfoBlocks { get; private set; }
        [DisplayName("Unknown 0x14")]
        public uint Unknown0x14 { get; private set; }
        [DisplayName("Unknown 0x18")]
        public uint Unknown0x18 { get; private set; }
        [DisplayName("Unknown 0x1C")]
        public uint Unknown0x1C { get; private set; }

        [DisplayName("Info Blocks")]
        public InfoBlockData[] InfoBlocks { get; private set; }

        [DisplayName("Txf Data")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Txf TxfData { get; private set; }

        bool disposed = false;

        protected AnmDat() { }

        public AnmDat(string filePath)
            : base(filePath)
        {
            using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.BigEndian))
            {
                InfoBlockSize = reader.ReadUInt32();
                ImageDataSize = reader.ReadUInt32();
                NumPixelDataHeaders = reader.ReadUInt32();
                NumPaletteDataHeaders = reader.ReadUInt32();
                NumInfoBlocks = reader.ReadUInt32();
                Unknown0x14 = reader.ReadUInt32();
                Unknown0x18 = reader.ReadUInt32();
                Unknown0x1C = reader.ReadUInt32();

                InfoBlocks = new InfoBlockData[NumInfoBlocks];
                for (int i = 0; i < InfoBlocks.Length; i++)
                {
                    long offset = reader.BaseStream.Position;
                    InfoBlocks[i] = new InfoBlockData(this, reader);

                    reader.BaseStream.Seek(offset + InfoBlocks[i].InfoEntrySize, SeekOrigin.Begin);
                }

                TxfData = new Txf(reader, (int)ImageDataSize, (int)NumPixelDataHeaders, (int)NumPaletteDataHeaders);

                foreach (InfoBlockData infoBlock in InfoBlocks)
                {
                    /* Post-processing */
                    foreach (AnimationData animation in infoBlock.Animations.Where(x => x != null))
                    {
                        animation.FirstNode = infoBlock.AnimationNodes[animation.FirstNodeID];
                    }

                    foreach (AnimationFrameData animationFrame in infoBlock.AnimationFrames.Where(x => x != null))
                    {
                        animationFrame.Sprite = infoBlock.Sprites[animationFrame.SpriteID];
                        animationFrame.Transform = infoBlock.Transforms[animationFrame.TransformID];
                    }

                    foreach (SpriteData sprite in infoBlock.Sprites.Where(x => x != null))
                    {
                        sprite.GenerateBitmap(TxfData);
                    }

                    foreach (TransformData transform in infoBlock.Transforms.Where(x => x != null))
                    {
                        transform.TransformOffset = infoBlock.TransformOffsets[transform.TransformOffsetID];
                    }

                    foreach (AnimationNodeData animationNode in infoBlock.AnimationNodes.Where(x => x != null))
                    {
                        if (animationNode.FirstAnimationFrameID != -1)
                        {
                            animationNode.AnimationFrames = new AnimationFrameData[animationNode.NumAnimationFrames];
                            for (int i = 0; i < animationNode.AnimationFrames.Length; i++)
                                animationNode.AnimationFrames[i] = infoBlock.AnimationFrames[animationNode.FirstAnimationFrameID + i];
                        }

                        if (animationNode.SiblingNodeID != -1)
                            animationNode.SiblingNode = infoBlock.AnimationNodes[animationNode.SiblingNodeID];

                        if (animationNode.ChildNodeID != -1)
                            animationNode.ChildNode = infoBlock.AnimationNodes[animationNode.ChildNodeID];
                    }
                }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    TxfData.Dispose();
                }

                this.disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
