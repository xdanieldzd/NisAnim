using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace NisAnim.OpenGL
{
    public enum ProjectionType { Perspective, Orthographic };

    public class GLHelper : IDisposable
    {
        GLControl glControl;
        Action renderAction;

        bool ready;

        public GLTextureManager Textures { get; private set; }
        public GLBufferManager Buffers { get; private set; }
        public GLShaderManager Shaders { get; private set; }
        public GLCamera Camera { get; private set; }

        public ProjectionType ProjectionType { get; set; }
        public Rectangle Viewport { get; set; }
        public float ZNear { get; set; }
        public float ZFar { get; set; }
        public Color ClearColor { get; set; }

        bool disposed;

        protected GLHelper()
        {
            disposed = false;
        }

        public GLHelper(GLControl glControl, Action renderAction)
            : this()
        {
            if ((this.glControl = glControl) == null) throw new GLException(string.Format("{0}: glControl cannot be null", System.Reflection.MethodBase.GetCurrentMethod()));
            if ((this.renderAction = renderAction) == null) throw new GLException(string.Format("{0}: renderAction cannot be null", System.Reflection.MethodBase.GetCurrentMethod()));

            this.ready = false;

            this.Textures = new GLTextureManager(this);
            this.Buffers = new GLBufferManager(this);
            this.Shaders = new GLShaderManager(this);

            this.Camera = new GLCamera();

            this.ProjectionType = OpenGL.ProjectionType.Perspective;
            this.Viewport = this.glControl.ClientRectangle;
            this.ZNear = 0.1f;
            this.ZFar = 100.0f;
            this.ClearColor = this.glControl.BackColor;

            this.glControl.Load += ((s, e) =>
            {
                SetDefaults();

                this.ready = true;
            });

            this.glControl.Paint += ((s, e) =>
            {
                if (!this.ready) return;

                GL.ClearColor(this.ClearColor);
                GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

                GL.Viewport(this.Viewport.X, this.Viewport.Y, this.Viewport.Width, this.Viewport.Height);
                GL.DepthRange(this.ZNear, this.ZFar);

                this.renderAction.Invoke();

                glControl.SwapBuffers();
            });
        }

        ~GLHelper()
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
                    if (this.Textures != null) this.Textures.Dispose();
                    if (this.Buffers != null) this.Buffers.Dispose();
                    if (this.Shaders != null) this.Shaders.Dispose();
                }

                this.disposed = true;
            }
        }

        public void Clear()
        {
            this.Textures.Clear();
            this.Buffers.Clear();
            this.Shaders.Clear();
        }

        private void SetDefaults()
        {
            /* TODO actually slim down to only what's needed? been carrying all of these around since SceneNavi, I think... */

            GL.ShadeModel(ShadingModel.Smooth);
            GL.Enable(EnableCap.PointSmooth);
            GL.Hint(HintTarget.PointSmoothHint, HintMode.Nicest);

            GL.PolygonMode(MaterialFace.FrontAndBack, PolygonMode.Fill);

            GL.ClearDepth(5.0);

            GL.DepthFunc(DepthFunction.Lequal);
            GL.Enable(EnableCap.DepthTest);
            GL.DepthMask(true);

            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            /*GL.Light(LightName.Light0, LightParameter.Ambient, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Diffuse, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Specular, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Light(LightName.Light0, LightParameter.Position, new float[] { 1.0f, 1.0f, 1.0f, 1.0f });
            GL.Enable(EnableCap.Light0);
            
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Normalize);
            */
            //GL.Enable(EnableCap.CullFace);
            //GL.CullFace(CullFaceMode.Back);

            GL.Enable(EnableCap.Blend);
            GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
        }

        public Matrix4 CreateProjectionMatrix()
        {
            if (this.Viewport.Width - this.Viewport.X <= 0 || this.Viewport.Height - this.Viewport.Y <= 0)
                throw new GLException(string.Format("{0}: Viewport cannot be negative", System.Reflection.MethodBase.GetCurrentMethod()));

            Matrix4 projectionMatrix = Matrix4.Identity;

            switch (this.ProjectionType)
            {
                case ProjectionType.Perspective:
                    double aspect = this.Viewport.Width / (double)this.Viewport.Height;
                    projectionMatrix = this.Camera.GetViewMatrix() * Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, (float)aspect, this.ZNear, this.ZFar);
                    break;

                case ProjectionType.Orthographic:
                    projectionMatrix = Matrix4.CreateOrthographicOffCenter(this.Viewport.Left, this.Viewport.Right, this.Viewport.Bottom, this.Viewport.Top, this.ZNear, this.ZFar);
                    break;
            }

            return projectionMatrix;
        }
    }
}
