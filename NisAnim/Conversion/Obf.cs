using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

using OpenTK;
using OpenTK.Graphics;

using NisAnim.IO;
using NisAnim.OpenGL;

namespace NisAnim.Conversion
{
    [DisplayName("Model ID List Entry")]
    public class ObfModelIDListEntry
    {
        [DisplayName("[Parent]")]
        public ObfModelIDList Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Value 1")]
        public short ModelID { get; private set; }
        [DisplayName("Value 2")]
        public short ObjectIndex { get; private set; }

        public ObfModelIDListEntry(ObfModelIDList parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            ModelID = reader.ReadInt16();
            ObjectIndex = reader.ReadInt16();
        }
    }

    [DisplayName("Model ID List")]
    public class ObfModelIDList
    {
        [DisplayName("[Parent]")]
        public Obf Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Number of Model IDs")]
        public uint NumModelIDs { get; private set; }
        [DisplayName("Model IDs")]
        public ObfModelIDListEntry[] ModelIDs { get; private set; }

        public ObfModelIDList(Obf parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            NumModelIDs = reader.ReadUInt32();

            ModelIDs = new ObfModelIDListEntry[NumModelIDs];
            for (int i = 0; i < ModelIDs.Length; i++) ModelIDs[i] = new ObfModelIDListEntry(this, reader);
        }
    }

    [DisplayName("Object List Entry")]
    public class ObfObjectListEntry
    {
        [DisplayName("[Parent]")]
        public ObfObjectList Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Number of Node Indices")]
        public uint NumNodeIndices { get; private set; }
        [DisplayName("Unknown 0x04")]
        public uint Unknown0x04 { get; private set; }
        [DisplayName("Unknown 0x08")]
        public uint Unknown0x08 { get; private set; }
        [DisplayName("Unknown 0x0C")]
        public uint Unknown0x0C { get; private set; }
        [DisplayName("Unknown 0x10")]
        public uint Unknown0x10 { get; private set; }
        [DisplayName("Unknown 0x14")]
        public uint Unknown0x14 { get; private set; } //01000000
        [DisplayName("Unknown 0x18")]
        public uint Unknown0x18 { get; private set; } //18FD1200
        [DisplayName("Node Indices")]
        public uint[] NodeIndices { get; private set; }

        public ObfNodeListEntry[] Nodes { get; set; }

        public ObfObjectListEntry(ObfObjectList parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            NumNodeIndices = reader.ReadUInt32();
            Unknown0x04 = reader.ReadUInt32();
            Unknown0x08 = reader.ReadUInt32();
            Unknown0x0C = reader.ReadUInt32();
            Unknown0x10 = reader.ReadUInt32();
            Unknown0x14 = reader.ReadUInt32();
            Unknown0x18 = reader.ReadUInt32();

            NodeIndices = new uint[NumNodeIndices];
            for (int i = 0; i < NodeIndices.Length; i++) NodeIndices[i] = reader.ReadUInt32();
        }
    }

    [DisplayName("Object List")]
    public class ObfObjectList
    {
        [DisplayName("[Parent]")]
        public Obf Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Number of Objects")]
        public uint NumObjects { get; private set; }
        [DisplayName("Object Offsets")]
        public uint[] ObjectOffsets { get; private set; }

        public ObfObjectListEntry[] Objects { get; private set; }

        public ObfObjectList(Obf parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            NumObjects = reader.ReadUInt32();

            ObjectOffsets = new uint[NumObjects];
            for (int i = 0; i < ObjectOffsets.Length; i++) ObjectOffsets[i] = reader.ReadUInt32();

            Objects = new ObfObjectListEntry[NumObjects];
            for (int i = 0; i < Objects.Length; i++)
            {
                reader.BaseStream.Seek(Offset + ObjectOffsets[i], SeekOrigin.Begin);
                Objects[i] = new ObfObjectListEntry(this, reader);
            }
        }
    }

    [DisplayName("Node List Entry")]
    public class ObfNodeListEntry
    {
        [DisplayName("[Parent]")]
        public ObfNodeList Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Group Index")]
        public int GroupIndex { get; private set; }
        [DisplayName("Parent Node ID")]
        public short ParentNodeID { get; private set; }
        [DisplayName("Unknown 0x06")]
        public ushort Unknown0x06 { get; private set; }
        [DisplayName("Number of Transform Indices")]
        public uint NumTransformIndices { get; private set; }
        [DisplayName("Transform Indices")]
        public uint[] TransformIndices { get; private set; }

        public ObfGroupListEntry Group { get; set; }
        public ObfNodeListEntry ParentNode { get; set; }
        public ObfTransformationEntry[] Transformations { get; set; }

        public ObfNodeListEntry(ObfNodeList parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            GroupIndex = reader.ReadInt32();
            ParentNodeID = reader.ReadInt16();
            Unknown0x06 = reader.ReadUInt16();
            NumTransformIndices = reader.ReadUInt32();

            TransformIndices = new uint[NumTransformIndices];
            for (int i = 0; i < TransformIndices.Length; i++) TransformIndices[i] = reader.ReadUInt32();
        }

        public Matrix4 GetTransformationMatrix(int transformationIndex)
        {
            Matrix4 matrix = Matrix4.Identity;

            matrix *= Matrix4.CreateScale(Transformations[transformationIndex].Scale);
            matrix *= Matrix4.CreateRotationX(Transformations[transformationIndex].Rotation.X);
            matrix *= Matrix4.CreateRotationY(Transformations[transformationIndex].Rotation.Y);
            matrix *= Matrix4.CreateRotationZ(Transformations[transformationIndex].Rotation.Z);
            matrix *= Matrix4.CreateTranslation(Transformations[transformationIndex].Translation * 10.0f);

            if (ParentNode != null)
                matrix *= ParentNode.GetTransformationMatrix(transformationIndex);

            return matrix;
        }
    }

    [DisplayName("Node List")]
    public class ObfNodeList
    {
        [DisplayName("[Parent]")]
        public Obf Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Number of Nodes")]
        public uint NumNodes { get; private set; }
        [DisplayName("Node Offsets")]
        public uint[] NodeOffsets { get; private set; }

        public ObfNodeListEntry[] Nodes { get; private set; }

        public ObfNodeList(Obf parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            NumNodes = reader.ReadUInt32();

            NodeOffsets = new uint[NumNodes];
            for (int i = 0; i < NodeOffsets.Length; i++) NodeOffsets[i] = reader.ReadUInt32();

            Nodes = new ObfNodeListEntry[NumNodes];
            for (int i = 0; i < Nodes.Length; i++)
            {
                reader.BaseStream.Seek(Offset + NodeOffsets[i], SeekOrigin.Begin);
                Nodes[i] = new ObfNodeListEntry(this, reader);
            }
        }
    }

    [DisplayName("Transformation Entry")]
    public class ObfTransformationEntry
    {
        [DisplayName("[Parent]")]
        public Obf Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        public int TranslationXIndex { get; private set; }
        public int TranslationYIndex { get; private set; }
        public int TranslationZIndex { get; private set; }
        public int RotationXIndex { get; private set; }
        public int RotationYIndex { get; private set; }
        public int RotationZIndex { get; private set; }
        public int ScaleXIndex { get; private set; }
        public int ScaleYIndex { get; private set; }
        public int ScaleZIndex { get; private set; }
        public uint Unknown0x24 { get; private set; }

        public Vector3 Translation { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }

        public ObfTransformationEntry(Obf parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            TranslationXIndex = reader.ReadInt32();
            TranslationYIndex = reader.ReadInt32();
            TranslationZIndex = reader.ReadInt32();
            RotationXIndex = reader.ReadInt32();
            RotationYIndex = reader.ReadInt32();
            RotationZIndex = reader.ReadInt32();
            ScaleXIndex = reader.ReadInt32();
            ScaleYIndex = reader.ReadInt32();
            ScaleZIndex = reader.ReadInt32();
            Unknown0x24 = reader.ReadUInt32();
        }
    }

    [DisplayName("Group List Entry")]
    public class ObfGroupListEntry
    {
        [DisplayName("[Parent]")]
        public ObfGroupList Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Number of Primitives")]
        public uint NumPrimitives { get; private set; }
        [DisplayName("Unknown Float 0x04")]
        public float UnknownFloat0x04 { get; private set; }
        [DisplayName("Unknown Float 0x08")]
        public float UnknownFloat0x08 { get; private set; }
        [DisplayName("Unknown 0x0C")]
        public ushort Unknown0x0C { get; private set; }
        [DisplayName("Unknown 0x0E")]
        public ushort Unknown0x0E { get; private set; }
        [DisplayName("Unknown 0x10")]
        public uint Unknown0x10 { get; private set; }
        [DisplayName("Unknown 0x14")]
        public uint Unknown0x14 { get; private set; }
        [DisplayName("Unknown 0x18")]
        public uint Unknown0x18 { get; private set; }
        [DisplayName("Primitive IDs")]
        public uint[] PrimitiveIDs { get; private set; }

        public ObfPrimitiveListEntry[] Primitives { get; set; }

        public ObfGroupListEntry(ObfGroupList parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            NumPrimitives = reader.ReadUInt32();
            UnknownFloat0x04 = reader.ReadSingle();
            UnknownFloat0x08 = reader.ReadSingle();
            Unknown0x0C = reader.ReadUInt16();
            Unknown0x0E = reader.ReadUInt16();
            Unknown0x10 = reader.ReadUInt32();
            Unknown0x14 = reader.ReadUInt32();
            Unknown0x18 = reader.ReadUInt32();

            PrimitiveIDs = new uint[NumPrimitives];
            for (int i = 0; i < PrimitiveIDs.Length; i++) PrimitiveIDs[i] = reader.ReadUInt32();
        }

        public List<string> PrepareRender(GLHelper glHelper)
        {
            List<string> glNames = new List<string>();

            for (int i = 0; i < Primitives.Length; i++)
                glNames.Add(Primitives[i].PrepareRender(glHelper));

            return glNames;
        }
    }

    [DisplayName("Group List")]
    public class ObfGroupList
    {
        [DisplayName("[Parent]")]
        public Obf Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Number of Groups")]
        public uint NumGroups { get; private set; }
        [DisplayName("Group Offsets")]
        public uint[] GroupOffsets { get; private set; }

        [DisplayName("Groups")]
        public ObfGroupListEntry[] Groups { get; private set; }

        public ObfGroupList(Obf parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            NumGroups = reader.ReadUInt32();

            GroupOffsets = new uint[NumGroups];
            for (int i = 0; i < GroupOffsets.Length; i++) GroupOffsets[i] = reader.ReadUInt32();

            Groups = new ObfGroupListEntry[NumGroups];
            for (int i = 0; i < Groups.Length; i++)
            {
                reader.BaseStream.Seek(Offset + GroupOffsets[i], SeekOrigin.Begin);
                Groups[i] = new ObfGroupListEntry(this, reader);
            }
        }
    }

    [DisplayName("Primitive List Entry")]
    public class ObfPrimitiveListEntry
    {
        [DisplayName("[Parent]")]
        public ObfPrimitiveList Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Number of Vertex Indices")]
        public uint NumVertexIndices { get; private set; }
        [DisplayName("Texture ID")]
        public uint TextureID { get; private set; }
        [DisplayName("Unknown 0x08")]
        public int Unknown0x08 { get; private set; }
        [DisplayName("Position Scale")]
        public float PositionScale { get; private set; }
        [DisplayName("Normal Scale")]
        public float NormalScale { get; private set; }
        [DisplayName("Unknown Vector Scale")]
        public float UnknownVectorScale { get; private set; }
        [DisplayName("Color Scale")]
        public float ColorScale { get; private set; }
        [DisplayName("Texture Coord Scale")]
        public float TexCoordScale { get; private set; }
        [DisplayName("Unknown 0x20")]
        public byte Unknown0x20 { get; private set; }
        [DisplayName("Unknown 0x21")]
        public byte Unknown0x21 { get; private set; }
        [DisplayName("Unknown 0x22")]
        public byte Unknown0x22 { get; private set; }
        [DisplayName("Unknown 0x23")]
        public byte Unknown0x23 { get; private set; }
        [DisplayName("Unknown 0x24")]
        public byte Unknown0x24 { get; private set; }
        [DisplayName("Unknown 0x25")]
        public byte Unknown0x25 { get; private set; }
        [DisplayName("Unknown 0x26")]
        public byte Unknown0x26 { get; private set; }
        [DisplayName("Unknown 0x27")]
        public byte Unknown0x27 { get; private set; }
        [DisplayName("Unknown 0x28")]
        public byte Unknown0x28 { get; private set; }
        [DisplayName("Unknown 0x29")]
        public byte Unknown0x29 { get; private set; }
        [DisplayName("Unknown 0x2A")]
        public byte Unknown0x2A { get; private set; }
        [DisplayName("Unknown 0x2B")]
        public byte Unknown0x2B { get; private set; }
        [DisplayName("Unknown 0x2C")]
        public int Unknown0x2C { get; private set; }
        [DisplayName("Vertex Indices")]
        public uint[] VertexIndices { get; private set; }

        [DisplayName("Vertices")]
        public ObfVertex[] Vertices { get; set; }

        public ObfPrimitiveListEntry(ObfPrimitiveList parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            NumVertexIndices = reader.ReadUInt32();
            TextureID = reader.ReadUInt32();
            Unknown0x08 = reader.ReadInt32();

            //pos,norm,unk,col,tex
            // RAAAAAAHHHHHHHHHHHHHH
            PositionScale = reader.ReadSingle();
            NormalScale = reader.ReadSingle();
            UnknownVectorScale = reader.ReadSingle();
            ColorScale = reader.ReadSingle();
            TexCoordScale = reader.ReadSingle();

            Unknown0x20 = reader.ReadByte();
            Unknown0x21 = reader.ReadByte();
            Unknown0x22 = reader.ReadByte();
            Unknown0x23 = reader.ReadByte();
            Unknown0x24 = reader.ReadByte();
            Unknown0x25 = reader.ReadByte();
            Unknown0x26 = reader.ReadByte();
            Unknown0x27 = reader.ReadByte();
            Unknown0x28 = reader.ReadByte();
            Unknown0x29 = reader.ReadByte();
            Unknown0x2A = reader.ReadByte();
            Unknown0x2B = reader.ReadByte();
            Unknown0x2C = reader.ReadInt32();

            VertexIndices = new uint[NumVertexIndices];
            for (int i = 0; i < VertexIndices.Length; i++) VertexIndices[i] = reader.ReadUInt32();
        }

        public string PrepareRender(GLHelper glHelper)
        {
            string groupName = string.Format("{0}_hash-{1}", this.GetType().Name, this.GetHashCode());

            List<GLVertex> glVertices = new List<GLVertex>();
            List<uint> glIndices = new List<uint>();

            for (int i = 0; i < Vertices.Length; i++)
            {
                ObfVertex vertex = Vertices[i];

                glVertices.Add(new GLVertex(
                    vertex.Position,
                    vertex.Normals,
                    new Color4(vertex.Color.X * 2.0f, vertex.Color.Y * 2.0f, vertex.Color.Z * 2.0f, vertex.Color.W * 2.0f),
                    vertex.TextureCoord));

                glIndices.Add((uint)i);
            }

            glHelper.Buffers.AddVertices(groupName, glVertices.ToArray());
            glHelper.Buffers.AddIndices(groupName, glIndices.ToArray(), OpenTK.Graphics.OpenGL.PrimitiveType.Triangles);

            glHelper.Textures.AddTexture(groupName, Parent.Parent.TxfList.Textures[TextureID].TxfData.Images.FirstOrDefault().Bitmap);

            return groupName;
        }
    }

    [DisplayName("Primitive List")]
    public class ObfPrimitiveList
    {
        [DisplayName("[Parent]")]
        public Obf Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Number of Primitives")]
        public uint NumPrimitives { get; private set; }
        [DisplayName("Primitive Offset")]
        public uint[] PrimitiveOffsets { get; private set; }

        [DisplayName("Primitives")]
        public ObfPrimitiveListEntry[] Primitives { get; private set; }

        public ObfPrimitiveList(Obf parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            NumPrimitives = reader.ReadUInt32();

            PrimitiveOffsets = new uint[NumPrimitives];
            for (int i = 0; i < PrimitiveOffsets.Length; i++) PrimitiveOffsets[i] = reader.ReadUInt32();

            Primitives = new ObfPrimitiveListEntry[NumPrimitives];
            for (int i = 0; i < Primitives.Length; i++)
            {
                reader.BaseStream.Seek(Offset + PrimitiveOffsets[i], SeekOrigin.Begin);
                Primitives[i] = new ObfPrimitiveListEntry(this, reader);
            }
        }
    }

    [DisplayName("Vertex")]
    public class ObfVertex
    {
        public const int Size = 0x3C;

        [DisplayName("[Parent]")]
        public ObfPrimitiveListEntry Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Position")]
        public Vector3 Position { get; private set; }
        [DisplayName("Normals")]
        public Vector3 Normals { get; private set; }
        [DisplayName("Unknown Vector3 0x18")]
        public Vector3 UnknownVector0x18 { get; private set; }
        [DisplayName("Color")]
        public Vector4 Color { get; private set; }
        [DisplayName("Texture Coords")]
        public Vector2 TextureCoord { get; private set; }

        public ObfVertex(ObfPrimitiveListEntry parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            /* TODO: using X position & normal as-is makes certain things appear mirrored (ex. albatross collar floor, geoblocks?); just inverting them feels wrong somehow, tho... */
            /* TEMP: invert Z, just testing */
            Position = new Vector3(reader.ReadSingle(), reader.ReadSingle(), -reader.ReadSingle());

            Normals = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
            UnknownVector0x18 = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());

            float a = reader.ReadSingle(), r = reader.ReadSingle(), g = reader.ReadSingle(), b = reader.ReadSingle();
            Color = new Vector4(r, g, b, a);

            TextureCoord = new Vector2(reader.ReadSingle(), reader.ReadSingle());
        }
    }

    [DisplayName("Txf List Entry")]
    public class ObfTxfListEntry
    {
        [DisplayName("[Parent]")]
        public ObfTxfList Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Unknown 0x00")]
        public uint Unknown0x00 { get; private set; }
        [DisplayName("Txf Data")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public Txf TxfData { get; private set; }

        public ObfTxfListEntry(ObfTxfList parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            Unknown0x00 = reader.ReadUInt32();
            TxfData = new Txf(reader);
        }
    }

    [DisplayName("Txf List")]
    public class ObfTxfList
    {
        [DisplayName("[Parent]")]
        public Obf Parent { get; private set; }
        [DisplayName("[Offset]")]
        public long Offset { get; private set; }

        [DisplayName("Number of Textures")]
        public uint NumTextures { get; private set; }
        [DisplayName("Texture Offsets")]
        public uint[] TextureOffsets { get; private set; }

        [DisplayName("Textures")]
        public ObfTxfListEntry[] Textures { get; private set; }

        public ObfTxfList(Obf parent, EndianBinaryReader reader)
        {
            Parent = parent;
            Offset = reader.BaseStream.Position;

            NumTextures = reader.ReadUInt32();

            TextureOffsets = new uint[NumTextures];
            for (int i = 0; i < TextureOffsets.Length; i++) TextureOffsets[i] = reader.ReadUInt32();

            Textures = new ObfTxfListEntry[NumTextures];
            for (int i = 0; i < Textures.Length; i++)
            {
                reader.BaseStream.Seek(Offset + TextureOffsets[i], SeekOrigin.Begin);
                Textures[i] = new ObfTxfListEntry(this, reader);
            }
        }
    }

    [DisplayName("Obf File")]
    [FileNamePattern("(.*?)\\.(obf)$")]
    public class Obf : BaseFile
    {
        [DisplayName("File Size")]
        public uint FileSize { get; private set; }
        [DisplayName("Unknown 0x04")]
        public uint Unknown0x04 { get; private set; }
        [DisplayName("Unknown Offset 0x08")]
        public uint UnknownOffset0x08 { get; private set; }
        [DisplayName("Model ID List Offset")]
        public uint ModelIDListOffset { get; private set; }
        [DisplayName("Object List Offset")]
        public uint ObjectListOffset { get; private set; }
        [DisplayName("Node List Offset")]
        public uint NodeListOffset { get; private set; }
        [DisplayName("Transformation List Offset")]
        public uint TransformationEntriesOffset { get; private set; }
        [DisplayName("Transformation Data Offset")]
        public uint TransformationDataOffset { get; private set; }
        [DisplayName("Group List Offset")]
        public uint GroupListOffset { get; private set; }
        [DisplayName("Primitive List Offset")]
        public uint PrimitiveListOffset { get; private set; }
        [DisplayName("Vertex Data Offset")]
        public uint VertexDataOffset { get; private set; }
        [DisplayName("Txf List Offset")]
        public uint TxfListOffset { get; private set; }

        [DisplayName("Model ID List")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public ObfModelIDList ModelIDList { get; private set; }

        [DisplayName("Object List")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public ObfObjectList ObjectList { get; private set; }

        [DisplayName("Node List")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public ObfNodeList NodeList { get; private set; }

        [DisplayName("Transformation Entries")]
        public List<ObfTransformationEntry> TransformationEntries { get; private set; }

        [DisplayName("Group List")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public ObfGroupList GroupList { get; private set; }

        [DisplayName("Primitive List")]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public ObfPrimitiveList PrimitiveList { get; private set; }

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
                ModelIDListOffset = reader.ReadUInt32();
                ObjectListOffset = reader.ReadUInt32();
                NodeListOffset = reader.ReadUInt32();
                TransformationEntriesOffset = reader.ReadUInt32();
                TransformationDataOffset = reader.ReadUInt32();
                GroupListOffset = reader.ReadUInt32();
                PrimitiveListOffset = reader.ReadUInt32();
                VertexDataOffset = reader.ReadUInt32();
                TxfListOffset = reader.ReadUInt32();

                reader.BaseStream.Seek(ModelIDListOffset, SeekOrigin.Begin);
                ModelIDList = new ObfModelIDList(this, reader);

                reader.BaseStream.Seek(ObjectListOffset, SeekOrigin.Begin);
                ObjectList = new ObfObjectList(this, reader);

                reader.BaseStream.Seek(NodeListOffset, SeekOrigin.Begin);
                NodeList = new ObfNodeList(this, reader);

                reader.BaseStream.Seek(TransformationEntriesOffset, SeekOrigin.Begin);
                TransformationEntries = new List<ObfTransformationEntry>();
                while (reader.BaseStream.Position < TransformationDataOffset)
                    TransformationEntries.Add(new ObfTransformationEntry(this, reader));

                foreach (ObfTransformationEntry transformEntry in TransformationEntries)
                {
                    transformEntry.Translation = new Vector3(
                        GetTransformValue(reader, transformEntry.TranslationXIndex),
                        GetTransformValue(reader, transformEntry.TranslationYIndex),
                        -GetTransformValue(reader, transformEntry.TranslationZIndex));

                    transformEntry.Rotation = new Vector3(
                        GetTransformValue(reader, transformEntry.RotationXIndex),
                        GetTransformValue(reader, transformEntry.RotationYIndex),
                        GetTransformValue(reader, transformEntry.RotationZIndex));

                    transformEntry.Scale = new Vector3(
                        GetTransformValue(reader, transformEntry.ScaleXIndex),
                        GetTransformValue(reader, transformEntry.ScaleYIndex),
                        GetTransformValue(reader, transformEntry.ScaleZIndex));
                }

                reader.BaseStream.Seek(GroupListOffset, SeekOrigin.Begin);
                GroupList = new ObfGroupList(this, reader);

                reader.BaseStream.Seek(PrimitiveListOffset, SeekOrigin.Begin);
                PrimitiveList = new ObfPrimitiveList(this, reader);

                reader.BaseStream.Seek(TxfListOffset, SeekOrigin.Begin);
                TxfList = new ObfTxfList(this, reader);

                for (int i = 0; i < PrimitiveList.Primitives.Length; i++)
                {
                    ObfPrimitiveListEntry primitiveListEntry = PrimitiveList.Primitives[i];

                    primitiveListEntry.Vertices = new ObfVertex[primitiveListEntry.NumVertexIndices];
                    for (int j = 0; j < primitiveListEntry.Vertices.Length; j++)
                    {
                        reader.BaseStream.Seek(VertexDataOffset + (ObfVertex.Size * primitiveListEntry.VertexIndices[j]), SeekOrigin.Begin);
                        primitiveListEntry.Vertices[j] = new ObfVertex(primitiveListEntry, reader);
                    }
                }

                for (int i = 0; i < GroupList.Groups.Length; i++)
                {
                    ObfGroupListEntry groupListEntry = GroupList.Groups[i];

                    groupListEntry.Primitives = new ObfPrimitiveListEntry[groupListEntry.NumPrimitives];
                    for (int j = 0; j < groupListEntry.Primitives.Length; j++)
                    {
                        groupListEntry.Primitives[j] = PrimitiveList.Primitives[groupListEntry.PrimitiveIDs[j]];
                    }
                }

                for (int i = 0; i < NodeList.Nodes.Length; i++)
                {
                    ObfNodeListEntry nodeListEntry = NodeList.Nodes[i];

                    if (nodeListEntry.GroupIndex != -1)
                        nodeListEntry.Group = GroupList.Groups[nodeListEntry.GroupIndex];
                }

                for (int i = 0; i < ObjectList.Objects.Length; i++)
                {
                    ObfObjectListEntry objectListEntry = ObjectList.Objects[i];

                    objectListEntry.Nodes = new ObfNodeListEntry[objectListEntry.NumNodeIndices];
                    for (int j = 0; j < objectListEntry.Nodes.Length; j++)
                    {
                        objectListEntry.Nodes[j] = NodeList.Nodes[objectListEntry.NodeIndices[j]];
                    }

                    foreach (ObfNodeListEntry nodeListEntry in objectListEntry.Nodes)
                    {
                        if (nodeListEntry.ParentNodeID != -1)
                            nodeListEntry.ParentNode = objectListEntry.Nodes[nodeListEntry.ParentNodeID];

                        nodeListEntry.Transformations = new ObfTransformationEntry[nodeListEntry.NumTransformIndices];
                        for (int j = 0; j < nodeListEntry.Transformations.Length; j++)
                        {
                            nodeListEntry.Transformations[j] = TransformationEntries[(int)nodeListEntry.TransformIndices[j]];
                        }
                    }
                }
            }
        }

        public float GetTransformValue(EndianBinaryReader reader, int index)
        {
            reader.BaseStream.Seek(TransformationDataOffset + (index * 0x10), SeekOrigin.Begin);
            return reader.ReadSingle();
        }

        protected override void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    for (int i = 0; i < TxfList.Textures.Length; i++)
                        TxfList.Textures[i].TxfData.Dispose();
                }

                this.disposed = true;
            }

            base.Dispose(disposing);
        }
    }
}
