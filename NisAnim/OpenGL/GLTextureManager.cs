using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace NisAnim.OpenGL
{
    public class GLTextureManager : IDisposable
    {
        GLHelper glHelper;

        Dictionary<string, int> textureCache;

        bool disposed;

        protected GLTextureManager()
        {
            this.disposed = false;
        }

        public GLTextureManager(GLHelper glHelper)
            : this()
        {
            if ((this.glHelper = glHelper) == null) throw new GLException(string.Format("{0}: glHelper cannot be null", System.Reflection.MethodBase.GetCurrentMethod()));

            this.textureCache = new Dictionary<string, int>();
        }

        ~GLTextureManager()
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
            foreach (KeyValuePair<string, int> texture in this.textureCache.Where(x => GL.IsTexture(x.Value)))
                GL.DeleteTexture(texture.Value);

            this.textureCache.Clear();
        }

        public void AddTexture(string name, Bitmap image)
        {
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", System.Reflection.MethodBase.GetCurrentMethod()));
            if (image == null) throw new GLException(string.Format("{0}: image cannot be null", System.Reflection.MethodBase.GetCurrentMethod()));

            int newId = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, newId);

            Bitmap newImage = new Bitmap(image.Width, image.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(newImage))
            {
                g.DrawImageUnscaled(image, Point.Empty);
            }

            BitmapData bmpData = newImage.LockBits(new Rectangle(0, 0, newImage.Width, newImage.Height), ImageLockMode.ReadOnly, newImage.PixelFormat);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, newImage.Width, newImage.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, bmpData.Scan0);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            newImage.UnlockBits(bmpData);

            this.textureCache.Add(name, newId);
        }

        public void AddTexture(string name, int id)
        {
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", System.Reflection.MethodBase.GetCurrentMethod()));
            if (!GL.IsTexture(id)) throw new GLException(string.Format("{0}: id must be a texture", System.Reflection.MethodBase.GetCurrentMethod()));

            this.textureCache.Add(name, id);
        }

        public void RemoveTexture(string name)
        {
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", System.Reflection.MethodBase.GetCurrentMethod()));

            if (this.textureCache.ContainsKey(name) && GL.IsTexture(this.textureCache[name]))
            {
                GL.DeleteTexture(this.textureCache[name]);
                this.textureCache.Remove(name);
            }
            else
                throw new GLException(string.Format("{0}: Cannot remove texture '{1}'; texture not found", System.Reflection.MethodBase.GetCurrentMethod(), name));
        }

        public int GetTexture(string name)
        {
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", System.Reflection.MethodBase.GetCurrentMethod()));

            if (this.textureCache.ContainsKey(name) && GL.IsTexture(this.textureCache[name]))
                return this.textureCache[name];
            else
                throw new GLException(string.Format("{0}: Cannot get texture '{1}'; texture not found", System.Reflection.MethodBase.GetCurrentMethod(), name));
        }

        public bool HasTexture(string name)
        {
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", System.Reflection.MethodBase.GetCurrentMethod()));

            return (this.textureCache.ContainsKey(name) && GL.IsTexture(this.textureCache[name]));
        }

        public void ActivateTexture(string name, TextureUnit unit)
        {
            if (name == string.Empty) throw new GLException(string.Format("{0}: name cannot be empty", System.Reflection.MethodBase.GetCurrentMethod()));

            if (this.textureCache.ContainsKey(name) && GL.IsTexture(this.textureCache[name]))
            {
                GL.ActiveTexture(unit);
                GL.BindTexture(TextureTarget.Texture2D, this.textureCache[name]);
            }
            else
                throw new GLException(string.Format("{0}: Cannot activate texture '{1}'; texture not found", System.Reflection.MethodBase.GetCurrentMethod(), name));
        }
    }
}
