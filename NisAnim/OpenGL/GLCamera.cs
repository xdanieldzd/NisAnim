using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace NisAnim.OpenGL
{
    /* Modified/extended from http://neokabuto.blogspot.de/2014/01/opentk-tutorial-5-basic-camera.html */

    /* TODO "GLHelper"-ify!! */
    public class GLCamera
    {
        public const float MoveSpeed = 0.2f;
        public const float MouseSensitivity = 0.005f;

        public Vector3 Position { get { return position; } }
        public Vector3 Orientation { get { return orientation; } }

        Vector3 position, orientation;

        KeyConfigStruct keyConfig;
        public KeyConfigStruct KeyConfiguration
        {
            get { return keyConfig; }
            set { keyConfig = value; }
        }

        public HashSet<Keys> KeysHeld { get; set; }
        public MouseButtons MouseButtonsHeld { get; set; }
        public Point MouseCenter { get; set; }
        public Point MousePosition { get; set; }

        public GLCamera()
        {
            Reset();

            keyConfig = new KeyConfigStruct();
            keyConfig.MoveForward = Keys.W;
            keyConfig.MoveBackward = Keys.S;
            keyConfig.StrafeLeft = Keys.A;
            keyConfig.StrafeRight = Keys.D;

            KeysHeld = new HashSet<Keys>();
        }

        public void Reset()
        {
            position = Vector3.Zero;
            orientation = new Vector3((float)Math.PI, 0.0f, 0.0f);
        }

        public Matrix4 GetViewMatrix()
        {
            Vector3 lookat = new Vector3();

            lookat.X = (float)(Math.Sin(orientation.X) * Math.Cos(orientation.Y));
            lookat.Y = (float)Math.Sin(orientation.Y);
            lookat.Z = (float)(Math.Cos(orientation.X) * Math.Cos(orientation.Y));

            return Matrix4.LookAt(position, position + lookat, Vector3.UnitY);
        }

        public void Update()
        {
            if (KeysHeld.Contains(keyConfig.MoveForward)) this.Move(0.0f, 0.1f, 0.0f);
            if (KeysHeld.Contains(keyConfig.MoveBackward)) this.Move(0.0f, -0.1f, 0.0f);
            if (KeysHeld.Contains(keyConfig.StrafeLeft)) this.Move(-0.1f, 0.0f, 0.0f);
            if (KeysHeld.Contains(keyConfig.StrafeRight)) this.Move(0.1f, 0.0f, 0.0f);

            Point delta = new Point(MouseCenter.X - MousePosition.X, MouseCenter.Y - MousePosition.Y);
            this.AddRotation(delta.X, delta.Y);

            MouseCenter = MousePosition;
        }

        private void Move(float x, float y, float z)
        {
            Vector3 offset = new Vector3();

            Vector3 forward = new Vector3((float)Math.Sin(orientation.X), (float)Math.Sin(orientation.Y), (float)Math.Cos(orientation.X));
            Vector3 right = new Vector3(-forward.Z, 0.0f, forward.X);

            offset += x * right;
            offset += y * forward;
            offset.Y += z;

            offset.NormalizeFast();
            offset = Vector3.Multiply(offset, MoveSpeed);

            position += offset;
        }

        private void AddRotation(double x, double y)
        {
            x = x * MouseSensitivity;
            y = y * MouseSensitivity;

            orientation.X = (float)((orientation.X + x) % (Math.PI * 2.0f));
            orientation.Y = (float)Math.Max(Math.Min(orientation.Y + y, Math.PI / 2.0f - 0.1f), -Math.PI / 2.0f + 0.1f);
        }

        public struct KeyConfigStruct
        {
            public Keys MoveForward { get; set; }
            public Keys MoveBackward { get; set; }
            public Keys StrafeLeft { get; set; }
            public Keys StrafeRight { get; set; }
        }
    }
}
