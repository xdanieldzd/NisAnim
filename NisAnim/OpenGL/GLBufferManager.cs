using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace NisAnim.OpenGL
{
    [Serializable]
    [StructLayout(LayoutKind.Sequential)]
    public struct GLVertex
    {
        public Vector3 Position;
        public Vector3 Normal;
        public Color4 Color;
        public Vector2 TexCoord;

        public static readonly int Stride = Marshal.SizeOf(default(GLVertex));

        public static readonly int PositionOffset = Marshal.OffsetOf(typeof(GLVertex), "Position").ToInt32();
        public static readonly int NormalOffset = Marshal.OffsetOf(typeof(GLVertex), "Normal").ToInt32();
        public static readonly int ColorOffset = Marshal.OffsetOf(typeof(GLVertex), "Color").ToInt32();
        public static readonly int TexCoordOffset = Marshal.OffsetOf(typeof(GLVertex), "TexCoord").ToInt32();

        public const int PositionAttributeIndex = 0;
        public const int NormalAttributeIndex = 1;
        public const int ColorAttributeIndex = 2;
        public const int TexCoordAttributeIndex = 3;

        public GLVertex(Vector3 position, Vector3 normal, Color4 color, Vector2 texCoord)
        {
            this.Position = position;
            this.Normal = normal;
            this.Color = color;
            this.TexCoord = texCoord;
        }
    }

    public class GLBufferManager : IDisposable
    {
        GLHelper glHelper;

        List<string> cacheNameList;
        Dictionary<string, int> vertexBufferCache, elementBufferCache;
        Dictionary<string, int> elementBufferNumIndices;
        Dictionary<string, PrimitiveType> elementBufferPrimitiveTypes;

        bool disposed;

        protected GLBufferManager()
        {
            disposed = false;
        }

        public GLBufferManager(GLHelper glHelper)
            : this()
        {
            if ((this.glHelper = glHelper) == null) throw new GLException(string.Format("{0}: glHelper cannot be null", System.Reflection.MethodBase.GetCurrentMethod()));

            this.cacheNameList = new List<string>();

            this.vertexBufferCache = new Dictionary<string, int>();
            this.elementBufferCache = new Dictionary<string, int>();

            this.elementBufferNumIndices = new Dictionary<string, int>();
            this.elementBufferPrimitiveTypes = new Dictionary<string, PrimitiveType>();
        }

        ~GLBufferManager()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    Clear();
                }

                this.disposed = true;
            }
        }

        public void Clear()
        {
            this.cacheNameList.Clear();

            foreach (KeyValuePair<string, int> buffer in this.elementBufferCache.Where(x => GL.IsBuffer(x.Value)))
                GL.DeleteBuffer(buffer.Value);

            foreach (KeyValuePair<string, int> buffer in this.vertexBufferCache.Where(x => GL.IsBuffer(x.Value)))
                GL.DeleteBuffer(buffer.Value);

            this.vertexBufferCache.Clear();
            this.elementBufferCache.Clear();

            this.elementBufferNumIndices.Clear();
            this.elementBufferPrimitiveTypes.Clear();
        }

        private void AddToNameList(string name)
        {
            if (!this.cacheNameList.Contains(name)) this.cacheNameList.Add(name);
        }

        private void RemoveFromNameList(string name)
        {
            if ((this.vertexBufferCache.Count(x => x.Key == name) +
                this.elementBufferCache.Count(x => x.Key == name)) == 0)
                this.cacheNameList.Remove(name);
        }

        public void AddVertices(string name, GLVertex[] data)
        {
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", System.Reflection.MethodBase.GetCurrentMethod()));
            if (data == null) throw new GLException(string.Format("{0}: data cannot be null", System.Reflection.MethodBase.GetCurrentMethod()));

            int newBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, newBuffer);

            int expectedSize = (int)(data.Length * GLVertex.Stride);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(expectedSize), data, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ArrayBuffer, BufferParameterName.BufferSize, out size);
            if (size != expectedSize)
                throw new GLException(string.Format("{0}: Problem uploading vertices to VBO; tried {1} bytes, uploaded {2}", System.Reflection.MethodBase.GetCurrentMethod(), expectedSize, size));

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            if (!this.vertexBufferCache.ContainsKey(name))
                this.vertexBufferCache.Add(name, newBuffer);

            AddToNameList(name);
        }

        public void RemoveVertices(string name)
        {
            DeleteDataBuffer(this.vertexBufferCache, name, System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }

        public int GetVertices(string name)
        {
            return GetDataBuffer(this.vertexBufferCache, name, System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }

        public void AddIndices(string name, uint[] data, PrimitiveType primitiveType)
        {
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", System.Reflection.MethodBase.GetCurrentMethod()));
            if (data == null) throw new GLException(string.Format("{0}: data cannot be null", System.Reflection.MethodBase.GetCurrentMethod()));

            int newBuffer = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, newBuffer);

            int expectedSize = (int)(data.Length * sizeof(uint));
            GL.BufferData(BufferTarget.ElementArrayBuffer, new IntPtr(expectedSize), data, BufferUsageHint.StaticDraw);

            int size;
            GL.GetBufferParameter(BufferTarget.ElementArrayBuffer, BufferParameterName.BufferSize, out size);
            if (size != expectedSize)
                throw new GLException(string.Format("{0}: Problem uploading indices to VBO; tried {1} bytes, uploaded {2}", System.Reflection.MethodBase.GetCurrentMethod(), expectedSize, size));

            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);

            if (!this.elementBufferCache.ContainsKey(name))
                this.elementBufferCache.Add(name, newBuffer);

            if (!this.elementBufferNumIndices.ContainsKey(name))
                this.elementBufferNumIndices.Add(name, data.Length);

            if (!this.elementBufferPrimitiveTypes.ContainsKey(name))
                this.elementBufferPrimitiveTypes.Add(name, primitiveType);

            AddToNameList(name);
        }

        public void RemoveIndices(string name)
        {
            DeleteDataBuffer(this.elementBufferCache, name, System.Reflection.MethodBase.GetCurrentMethod().ToString());
            this.elementBufferNumIndices.Remove(name);
            this.elementBufferPrimitiveTypes.Remove(name);
        }

        public int GetIndices(string name)
        {
            return GetDataBuffer(this.elementBufferCache, name, System.Reflection.MethodBase.GetCurrentMethod().ToString());
        }

        private void DeleteDataBuffer(Dictionary<string, int> cache, string name, string callerFunc = default(string))
        {
            if (callerFunc == default(string)) callerFunc = System.Reflection.MethodBase.GetCurrentMethod().ToString();
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", callerFunc));

            if (cache.ContainsKey(name) && GL.IsBuffer(cache[name]))
            {
                GL.DeleteBuffer(cache[name]);
                cache.Remove(name);

                RemoveFromNameList(name);
            }
        }

        private int GetDataBuffer(Dictionary<string, int> cache, string name, string callerFunc = default(string))
        {
            if (callerFunc == default(string)) callerFunc = System.Reflection.MethodBase.GetCurrentMethod().ToString();
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", callerFunc));

            if (cache.ContainsKey(name) && GL.IsBuffer(cache[name]))
                return cache[name];
            else
                throw new GLException(string.Format("{0}: Cannot get buffer '{1}'; buffer not found", callerFunc, name));
        }

        public void Render(string name)
        {
            if (!this.vertexBufferCache.ContainsKey(name) ||
                !this.elementBufferCache.ContainsKey(name) || !this.elementBufferNumIndices.ContainsKey(name) || !this.elementBufferPrimitiveTypes.ContainsKey(name))
                throw new GLException(string.Format("{0}: Insufficient data uploaded for '{1}'", System.Reflection.MethodBase.GetCurrentMethod(), name));

            GL.BindBuffer(BufferTarget.ArrayBuffer, this.vertexBufferCache[name]);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, this.elementBufferCache[name]);

            GL.EnableVertexAttribArray(GLVertex.PositionAttributeIndex);
            GL.EnableVertexAttribArray(GLVertex.NormalAttributeIndex);
            GL.EnableVertexAttribArray(GLVertex.ColorAttributeIndex);
            GL.EnableVertexAttribArray(GLVertex.TexCoordAttributeIndex);

            GL.VertexAttribPointer(GLVertex.PositionAttributeIndex, 3, VertexAttribPointerType.Float, false, GLVertex.Stride, GLVertex.PositionOffset);
            GL.VertexAttribPointer(GLVertex.NormalAttributeIndex, 3, VertexAttribPointerType.Float, false, GLVertex.Stride, GLVertex.NormalOffset);
            GL.VertexAttribPointer(GLVertex.ColorAttributeIndex, 4, VertexAttribPointerType.Float, false, GLVertex.Stride, GLVertex.ColorOffset);
            GL.VertexAttribPointer(GLVertex.TexCoordAttributeIndex, 2, VertexAttribPointerType.Float, false, GLVertex.Stride, GLVertex.TexCoordOffset);

            GL.DrawElements(this.elementBufferPrimitiveTypes[name], this.elementBufferNumIndices[name], DrawElementsType.UnsignedInt, IntPtr.Zero);

            GL.DisableVertexAttribArray(GLVertex.PositionAttributeIndex);
            GL.DisableVertexAttribArray(GLVertex.NormalAttributeIndex);
            GL.DisableVertexAttribArray(GLVertex.ColorAttributeIndex);
            GL.DisableVertexAttribArray(GLVertex.TexCoordAttributeIndex);

            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.BindBuffer(BufferTarget.ElementArrayBuffer, 0);
        }
    }
}
