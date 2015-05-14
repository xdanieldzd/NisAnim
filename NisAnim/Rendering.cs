using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using NisAnim.OpenGL;

namespace NisAnim
{
    /* Ugly on purpose! */
    public static class Rendering
    {
        static GLHelper glHelper;

        public struct LightStruct
        {
            public bool Enabled;
            public Vector4 Position;
            public Vector3 Intensities;
            public float Attenuation;
            public float AmbientCoefficient;
            public float ConeAngle;
            public Vector3 ConeDirection;
        };

        public const string AxisMarkerObjectName = "axisMarkerObj";
        public const string LightMarkerObjectName = "lightMarkerObj";

        public const string EmptyTextureName = "emptyTex";

        public const string DefaultShaderName = "defaultShader";
        public const string LightMarkerShaderName = "lightMarkerShader";
        public const string LightLineShaderName = "lightLineShader";

        public const float LightStep = 5.0f;

        public static LightStruct[] Lights;
        public static Vector3 LightOffset;
        public static float LightRotation;

        static Rendering()
        {
            Lights = new LightStruct[5];
            LightOffset = new Vector3(0.0f, 20.0f, 0.0f);
            LightRotation = 0.0f;
        }

        /* More or less universal functions... */
        public static void Initialize(GLHelper glHelper)
        {
            Rendering.glHelper = glHelper;

            glHelper.Textures.AddTexture(EmptyTextureName, Properties.Resources.Empty);
            glHelper.Shaders.AddProgramWithShaders(DefaultShaderName,
                File.ReadAllText("Data\\Default.vert"),
                File.ReadAllText("Data\\Default.frag"));

            glHelper.Shaders.AddProgramWithShaders(LightMarkerShaderName,
                File.ReadAllText("Data\\LightMarker.vert"),
                File.ReadAllText("Data\\LightMarker.frag"),
                File.ReadAllText("Data\\LightMarker.geom"));

            glHelper.Shaders.AddProgramWithShaders(LightLineShaderName,
                File.ReadAllText("Data\\LightMarker.vert"),
                File.ReadAllText("Data\\LightMarker.frag"),
                File.ReadAllText("Data\\LightLine.geom"));

            glHelper.Buffers.AddVertices(AxisMarkerObjectName, new GLVertex[]
            {
                new GLVertex(new Vector3(-300.0f, 0.0f, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.Black, Vector2.Zero),
                new GLVertex(new Vector3(300.0f, 0.0f, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.Black, Vector2.Zero),
                new GLVertex(new Vector3(0.0f, 300.0f, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.Black, Vector2.Zero),
                new GLVertex(new Vector3(0.0f, -300.0f, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.Black, Vector2.Zero)
            });
            glHelper.Buffers.AddIndices(AxisMarkerObjectName, new uint[] { 0, 1, 2, 3 }, PrimitiveType.Lines);

            glHelper.Buffers.AddVertices(LightMarkerObjectName, new GLVertex[]
            {
                new GLVertex(Vector3.Zero, Vector3.Zero, OpenTK.Graphics.Color4.White, Vector2.Zero)
            });
            glHelper.Buffers.AddIndices(LightMarkerObjectName, new uint[] { 0 }, PrimitiveType.Points);

            Lights[0] = new LightStruct();
            Lights[0].Enabled = true;
            Lights[0].Position = Vector4.Zero;
            Lights[0].Intensities = new Vector3(0.75f, 0.75f, 0.75f);
            Lights[0].AmbientCoefficient = 0.5f;

            Lights[1] = new LightStruct();
            Lights[1].Enabled = true;
            Lights[1].Intensities = new Vector3(0.4f, 0.7f, 1.0f);
            Lights[1].AmbientCoefficient = 0.05f;

            Lights[2] = new LightStruct();
            Lights[2].Enabled = true;
            Lights[2].Intensities = new Vector3(1.0f, 0.7f, 0.4f);
            Lights[2].AmbientCoefficient = 0.05f;

            Lights[3] = new LightStruct();
            Lights[3].Enabled = true;
            Lights[3].Intensities = new Vector3(0.4f, 0.1f, 0.7f);
            Lights[3].AmbientCoefficient = 0.05f;

            Lights[4] = new LightStruct();
            Lights[4].Enabled = true;
            Lights[4].Intensities = new Vector3(0.7f, 1.0f, 0.4f);
            Lights[4].AmbientCoefficient = 0.05f;
        }

        public static void UpdateLightUniforms(string shaderName, string numLightsVarName, string lightsVarName)
        {
            glHelper.Shaders.SetUniform(shaderName, numLightsVarName, Lights.Length);

            for (int i = 0; i < Lights.Length; i++)
            {
                glHelper.Shaders.SetUniform(shaderName, string.Format("{0}[{1}].enabled", lightsVarName, i), Convert.ToInt32(Lights[i].Enabled));
                glHelper.Shaders.SetUniform(shaderName, string.Format("{0}[{1}].position", lightsVarName, i), Lights[i].Position);
                glHelper.Shaders.SetUniform(shaderName, string.Format("{0}[{1}].intensities", lightsVarName, i), Lights[i].Intensities);
                glHelper.Shaders.SetUniform(shaderName, string.Format("{0}[{1}].attenuation", lightsVarName, i), Lights[i].Attenuation);
                glHelper.Shaders.SetUniform(shaderName, string.Format("{0}[{1}].ambientCoefficient", lightsVarName, i), Lights[i].AmbientCoefficient);
                glHelper.Shaders.SetUniform(shaderName, string.Format("{0}[{1}].coneAngle", lightsVarName, i), Lights[i].ConeAngle);
                glHelper.Shaders.SetUniform(shaderName, string.Format("{0}[{1}].coneDirection", lightsVarName, i), Lights[i].ConeDirection);
            }
        }

        /* ...aaanndd project-specific functions that are just code moved over from MainForm */
        public static void RotateLights(double milliseconds)
        {
            LightRotation += (float)milliseconds / 5.0f;
            if (LightRotation >= 360.0f) LightRotation = 0.0f;
        }

        public static void ApplyLightRotation()
        {
            Matrix4 lightMatrix = Matrix4.CreateRotationY(LightRotation);
            Vector4 rotatedLightPosition = Vector4.Transform(new Vector4(40.0f, 0.0f, 0.0f, 0.0f), lightMatrix);

            Lights[0].Position = new Vector4(glHelper.Camera.Position, 0.0f);

            Lights[1].Position = rotatedLightPosition + new Vector4(LightOffset, 0.0f);
            Lights[2].Position = new Vector4(-rotatedLightPosition.X, rotatedLightPosition.Y, -rotatedLightPosition.Z, 0.0f) + new Vector4(LightOffset, 0.0f);
            Lights[3].Position = new Vector4(rotatedLightPosition.X, rotatedLightPosition.Z, rotatedLightPosition.Y, 0.0f) + new Vector4(LightOffset, 0.0f);
            Lights[4].Position = new Vector4(-rotatedLightPosition.X, -rotatedLightPosition.Z, rotatedLightPosition.Y, 0.0f) + new Vector4(LightOffset, 0.0f);
        }

        public static void HandleLightKeydowns(Keys keyCode)
        {
            switch (keyCode)
            {
                case Keys.D1: Lights[0].Enabled = !Lights[0].Enabled; break;
                case Keys.D2: Lights[1].Enabled = !Lights[1].Enabled; break;
                case Keys.D3: Lights[2].Enabled = !Lights[2].Enabled; break;
                case Keys.D4: Lights[3].Enabled = !Lights[3].Enabled; break;
                case Keys.D5: Lights[4].Enabled = !Lights[4].Enabled; break;

                case Keys.NumPad8: LightOffset.Z -= LightStep; break;
                case Keys.NumPad2: LightOffset.Z += LightStep; break;
                case Keys.NumPad4: LightOffset.X -= LightStep; break;
                case Keys.NumPad6: LightOffset.X += LightStep; break;
                case Keys.NumPad7: LightOffset.Y += LightStep; break;
                case Keys.NumPad9: LightOffset.Y -= LightStep; break;
            }
        }

        public static void RenderLightVisualization()
        {
            for (int i = 1; i < Lights.Length; i++)
            {
                if (!Lights[i].Enabled) continue;

                /* Render light marker line */
                glHelper.Shaders.ActivateProgram(LightLineShaderName);
                glHelper.Shaders.SetUniformMatrix(LightLineShaderName, "projectionMatrix", false, glHelper.CreateProjectionMatrix());
                glHelper.Shaders.SetUniformMatrix(LightLineShaderName, "modelviewMatrix", false, Matrix4.Identity);
                glHelper.Shaders.SetUniformMatrix(LightLineShaderName, "objectMatrix", false, Matrix4.CreateTranslation(Lights[i].Position.Xyz));
                glHelper.Shaders.SetUniformMatrix(LightLineShaderName, "baseMatrix", false, Matrix4.CreateTranslation(LightOffset));
                glHelper.Shaders.SetUniform(LightLineShaderName, "surfaceColor", new OpenTK.Graphics.Color4(0.0f, 0.0f, 0.0f, 0.5f));
                glHelper.Buffers.Render(LightMarkerObjectName);

                /* Render light marker */
                glHelper.Shaders.ActivateProgram(LightMarkerShaderName);
                glHelper.Shaders.SetUniformMatrix(LightMarkerShaderName, "projectionMatrix", false, glHelper.CreateProjectionMatrix());
                glHelper.Shaders.SetUniformMatrix(LightMarkerShaderName, "modelviewMatrix", false, Matrix4.Identity);
                glHelper.Shaders.SetUniformMatrix(LightMarkerShaderName, "objectMatrix", false, Matrix4.CreateTranslation(Lights[i].Position.Xyz));
                glHelper.Shaders.SetUniformMatrix(LightMarkerShaderName, "baseMatrix", false, Matrix4.CreateTranslation(LightOffset));
                glHelper.Shaders.SetUniform(LightMarkerShaderName, "surfaceColor", new OpenTK.Graphics.Color4(Lights[i].Intensities.X, Lights[i].Intensities.Y, Lights[i].Intensities.Z, 0.75f));
                glHelper.Buffers.Render(LightMarkerObjectName);

                /* Render light marker center */
                glHelper.Shaders.ActivateProgram(LightMarkerShaderName);
                glHelper.Shaders.SetUniformMatrix(LightMarkerShaderName, "objectMatrix", false, Matrix4.CreateTranslation(LightOffset));
                glHelper.Shaders.SetUniformMatrix(LightMarkerShaderName, "baseMatrix", false, Matrix4.Identity);
                glHelper.Shaders.SetUniform(LightMarkerShaderName, "surfaceColor", new OpenTK.Graphics.Color4(0.5f, 0.5f, 0.5f, 0.5f));
                glHelper.Buffers.Render(LightMarkerObjectName);
            }
        }
    }
}
