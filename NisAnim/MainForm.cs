using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.IO;
using System.Diagnostics;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using NisAnim.Conversion;
using NisAnim.IO;
using NisAnim.OpenGL;

namespace NisAnim
{
    public partial class MainForm : Form
    {
        const string oglAxisMarkerObjectName = "axisMarkerObj";
        const string oglLightMarkerObjectName = "lightMarkerObj";

        const string oglEmptyTextureName = "emptyTex";

        const string oglDefaultShaderName = "defaultShader";
        const string oglLightMarkerShaderName = "lightMarkerShader";
        const string oglLightLineShaderName = "lightLineShader";

        const float lightStep = 5.0f;

        GLHelper glHelper;
        bool render3D;
        Stopwatch stopWatch;

        Matrix4 currentMatrix;
        List<string> glObjectNames;

        Vector3 lightPosition;
        float lightRotation;

        BaseFile loadedFile;
        object selectedObj { get { return pgObject.SelectedObject; } }

        bool mouseDown;
        Vector2 mouseCenter, imageOffset;

        Timer timer;
        int animCounter, maxCounter;

        public MainForm()
        {
            InitializeComponent();

            glHelper = new GLHelper(glControl, new Action(Render)) { ClearColor = Color.SkyBlue, };
            glControl.Resize += ((s, e) =>
            {
                glHelper.Viewport = (s as GLControl).ClientRectangle;
            });
            glControl.Load += ((s, e) =>
            {
                glHelper.Textures.AddTexture(oglEmptyTextureName, Properties.Resources.Empty);
                glHelper.Shaders.AddProgramWithShaders(oglDefaultShaderName,
                    File.ReadAllText("Data\\Default.vert"),
                    File.ReadAllText("Data\\Default.frag"));

                glHelper.Shaders.AddProgramWithShaders(oglLightMarkerShaderName,
                    File.ReadAllText("Data\\LightMarker.vert"),
                    File.ReadAllText("Data\\LightMarker.frag"),
                    File.ReadAllText("Data\\LightMarker.geom"));

                glHelper.Shaders.AddProgramWithShaders(oglLightLineShaderName,
                    File.ReadAllText("Data\\LightMarker.vert"),
                    File.ReadAllText("Data\\LightMarker.frag"),
                    File.ReadAllText("Data\\LightLine.geom"));

                glHelper.Buffers.AddVertices(oglAxisMarkerObjectName, new GLVertex[]
                {
                    new GLVertex(new Vector3(-300.0f, 0.0f, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.Black, Vector2.Zero),
                    new GLVertex(new Vector3(300.0f, 0.0f, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.Black, Vector2.Zero),
                    new GLVertex(new Vector3(0.0f, 300.0f, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.Black, Vector2.Zero),
                    new GLVertex(new Vector3(0.0f, -300.0f, 0.0f), Vector3.Zero, OpenTK.Graphics.Color4.Black, Vector2.Zero)
                });
                glHelper.Buffers.AddIndices(oglAxisMarkerObjectName, new uint[] { 0, 1, 2, 3 }, PrimitiveType.Lines);

                glHelper.Buffers.AddVertices(oglLightMarkerObjectName, new GLVertex[]
                {
                    new GLVertex(Vector3.Zero, Vector3.Zero, OpenTK.Graphics.Color4.White, Vector2.Zero)
                });
                glHelper.Buffers.AddIndices(oglLightMarkerObjectName, new uint[] { 0 }, PrimitiveType.Points);
            });

            render3D = false;
            stopWatch = new Stopwatch();

            currentMatrix = Matrix4.Identity;
            glObjectNames = new List<string>();

            lightPosition = new Vector3(0.0f, 20.0f, 0.0f);
            lightRotation = 0.0f;

            SetFormTitle();

            debugDrawToolStripMenuItem.Checked = Properties.Settings.Default.DebugDraw;
            enableLightingToolStripMenuItem.Checked = Properties.Settings.Default.EnableLighting;

            mouseCenter = imageOffset = Vector2.Zero;

            animCounter = 0;
            maxCounter = 0;

            /* TODO uh, make better */
            timer = new Timer();
            timer.Interval = 15;
            timer.Tick += ((s, e) =>
            {
                animCounter++;
                if (animCounter >= maxCounter) animCounter = 0;
            });

            timer.Start();

            Application.Idle += ((s, e) =>
            {
                stopWatch.Start();

                if (glControl.IsIdle)
                    glControl.Invalidate();

                stopWatch.Stop();

                double milliseconds = stopWatch.Elapsed.TotalMilliseconds;
                stopWatch.Reset();

                if (Properties.Settings.Default.EnableLighting)
                {
                    lightRotation += (float)milliseconds / 10.0f;
                    if (lightRotation >= 360.0f) lightRotation = 0.0f;
                }
            });

            tsslStatus.Text = "Ready";
        }

        private void SetFormTitle()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(Application.ProductName);
            if (loadedFile != null) builder.AppendFormat(" - [{0}]", Path.GetFileName(loadedFile.FilePath));

            Text = builder.ToString();
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.LastDat != string.Empty)
            {
                ofdDataFile.InitialDirectory = Path.GetDirectoryName(Properties.Settings.Default.LastDat);
                ofdDataFile.FileName = Path.GetFileName(Properties.Settings.Default.LastDat);
            }

            if (ofdDataFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                tvObject.Enabled = false;
                pgObject.SelectedObject = null;

                ClearObjects();

                BackgroundWorker fileWorker = new BackgroundWorker();
                fileWorker.DoWork += ((s, ev) =>
                {
                    Type fileImplType = null;
                    using (EndianBinaryReader reader = new EndianBinaryReader(File.Open(ofdDataFile.FileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite), Endian.BigEndian))
                    {
                        fileImplType = FileHelpers.IdentifyFile(reader, ofdDataFile.FileName);
                    }

                    if (fileImplType == null)
                    {
                        MessageBox.Show("Could not identify file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        ev.Result = false;
                    }
                    else
                    {
                        loadedFile = (BaseFile)Activator.CreateInstance(fileImplType, new object[] { (Properties.Settings.Default.LastDat = ofdDataFile.FileName) });
                        this.Invoke(new Action(() => { SetFormTitle(); }));

                        ev.Result = true;
                    }
                });

                fileWorker.RunWorkerCompleted += ((s, ev) =>
                {
                    if ((bool)ev.Result == false)
                    {
                        tsslStatus.Text = "Ready";
                        tvObject.Enabled = true;
                        return;
                    }

                    /* TODO have partial treeview updates? */
                    BackgroundWorker treeWorker = new BackgroundWorker();
                    treeWorker.DoWork += ((s2, ev2) =>
                    {
                        tvObject.Invoke(new Action(() => { tvObject.Nodes.Clear(); }));
                        ev2.Result = FileHelpers.TraverseObject(null, Path.GetFileName(loadedFile.FilePath), loadedFile, true);
                    });
                    treeWorker.RunWorkerCompleted += ((s2, ev2) =>
                    {
                        tvObject.Enabled = true;
                        tvObject.Focus();
                        tvObject.Nodes.Add((TreeNode)ev2.Result);

                        tsslStatus.Text = "File loaded";
                    });

                    tsslStatus.Text = "Generating tree...";
                    treeWorker.RunWorkerAsync();
                });

                tsslStatus.Text = "Loading file...";
                fileWorker.RunWorkerAsync();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void enableLightingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.EnableLighting = (sender as ToolStripMenuItem).Checked;
        }

        private void debugDrawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DebugDraw = (sender as ToolStripMenuItem).Checked;
        }

        private void resetTranslationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageOffset = Vector2.Zero;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("NisAnim - NIS Animation Viewer\n\nWritten 2015 by xdaniel - https://github.com/xdanieldzd/", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (sfdDataFile.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                if (selectedObj is NisPackFile)
                {
                    NisPackFile file = (selectedObj as NisPackFile);
                    file.ParentFile.ExtractFile(file, sfdDataFile.FileName);
                }
                else if (selectedObj is ImageInformation)
                {
                    (selectedObj as ImageInformation).Bitmap.Save(sfdDataFile.FileName);
                }
                else if (selectedObj is SpriteData)
                {
                    (selectedObj as SpriteData).Image.Save(sfdDataFile.FileName);
                }

                sfdDataFile.FileName = Path.GetFileName(sfdDataFile.FileName);
            }
        }

        private void tvObject_AfterSelect(object sender, TreeViewEventArgs e)
        {
            pgObject.SelectedObject = e.Node.Tag;

            ClearObjects();

            if (selectedObj is NisPackFile)
            {
                NisPackFile file = (selectedObj as NisPackFile);

                e.Node.ContextMenuStrip = cmsTreeNode;
                sfdDataFile.Filter = "All Files (*.*)|*.*";
                sfdDataFile.FileName = file.DecompressedFilename;

                if (e.Node.Nodes.Count == 0)
                {
                    if (file.DetectedFileType != null)
                    {
                        string path = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "_" + file.DecompressedFilename);

                        file.ParentFile.ExtractFile(file, path);
                        object tempObject = (BaseFile)Activator.CreateInstance(file.DetectedFileType, new object[] { path });
                        e.Node.Nodes.Add(FileHelpers.TraverseObject(e.Node, file.DecompressedFilename, tempObject, true));

                        if (File.Exists(path))
                            File.Delete(path);
                    }
                }
            }
            else if (selectedObj is PacFile)
            {
                PacFile file = (selectedObj as PacFile);

                e.Node.ContextMenuStrip = cmsTreeNode;
                sfdDataFile.Filter = "All Files (*.*)|*.*";
                sfdDataFile.FileName = file.Filename;

                if (e.Node.Nodes.Count == 0)
                {
                    if (file.DetectedFileType != null)
                    {
                        string path = Path.Combine(Path.GetTempPath(), Path.GetFileNameWithoutExtension(Path.GetRandomFileName()) + "_" + file.Filename);

                        file.ParentFile.ExtractFile(file, path);
                        object tempObject = (BaseFile)Activator.CreateInstance(file.DetectedFileType, new object[] { path });
                        e.Node.Nodes.Add(FileHelpers.TraverseObject(e.Node, file.Filename, tempObject, true));

                        if (File.Exists(path))
                            File.Delete(path);
                    }
                }
            }
            else if (selectedObj is ImageInformation)
            {
                e.Node.ContextMenuStrip = cmsTreeNode;
                sfdDataFile.Filter = "Image Files (*.png)|*.png|All Files (*.*)|*.*";

                ImageInformation image = (selectedObj as ImageInformation);
                glObjectNames.Add(image.PrepareRender(glHelper));

                render3D = false;
            }
            else if (selectedObj is SpriteData)
            {
                e.Node.ContextMenuStrip = cmsTreeNode;
                sfdDataFile.Filter = "Image Files (*.png)|*.png|All Files (*.*)|*.*";

                SpriteData sprite = (selectedObj as SpriteData);
                glObjectNames.Add(sprite.PrepareRender(glHelper));

                render3D = false;
            }
            else if (selectedObj is ObfPrimitiveListEntry)
            {
                ObfPrimitiveListEntry primitiveListEntry = (selectedObj as ObfPrimitiveListEntry);
                glObjectNames.Add(primitiveListEntry.PrepareRender(glHelper));

                render3D = true;
            }
            else if (selectedObj is ObfObjectListEntry)
            {
                ObfObjectListEntry objectListEntry = (selectedObj as ObfObjectListEntry);
                glObjectNames.AddRange(objectListEntry.PrepareRender(glHelper));

                render3D = true;
            }

            animCounter = 0;
            maxCounter = 0;
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();

            if (loadedFile != null)
                loadedFile.Dispose();

            if (glHelper != null)
                glHelper.Dispose();
        }

        private void glControl_MouseMove(object sender, MouseEventArgs e)
        {
            if (glHelper.ProjectionType == ProjectionType.Perspective)
            {
                if (Convert.ToBoolean(glHelper.Camera.MouseButtonsHeld & MouseButtons.Left))
                    glHelper.Camera.MousePosition = e.Location;
            }
            else
            {
                if (mouseDown)
                {
                    imageOffset.X += -(mouseCenter.X - e.X);
                    imageOffset.Y += -(mouseCenter.Y - e.Y);
                    mouseCenter = new Vector2(e.Location.X, e.Location.Y);
                }
            }
        }

        private void glControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (glHelper.ProjectionType == ProjectionType.Perspective)
            {
                glHelper.Camera.MouseButtonsHeld &= ~e.Button;
            }
            else
            {
                mouseDown = false;
            }
        }

        private void glControl_MouseDown(object sender, MouseEventArgs e)
        {
            if (glHelper.ProjectionType == ProjectionType.Perspective)
            {
                glHelper.Camera.MouseButtonsHeld |= e.Button;
                if (Convert.ToBoolean(glHelper.Camera.MouseButtonsHeld & MouseButtons.Left)) glHelper.Camera.MousePosition = glHelper.Camera.MouseCenter = e.Location;
            }
            else
            {
                if (e.Button.HasFlag(MouseButtons.Left))
                {
                    mouseDown = true;
                    mouseCenter = new Vector2(e.Location.X, e.Location.Y);
                }
            }
        }

        private void glControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (glHelper.ProjectionType == ProjectionType.Perspective)
            {
                glHelper.Camera.KeysHeld.Add(e.KeyCode);

                switch (e.KeyCode)
                {
                    case Keys.NumPad8: lightPosition.Z -= lightStep; break;
                    case Keys.NumPad2: lightPosition.Z += lightStep; break;
                    case Keys.NumPad4: lightPosition.X -= lightStep; break;
                    case Keys.NumPad6: lightPosition.X += lightStep; break;
                    case Keys.NumPad7: lightPosition.Y += lightStep; break;
                    case Keys.NumPad9: lightPosition.Y -= lightStep; break;
                }
            }
        }

        private void glControl_KeyUp(object sender, KeyEventArgs e)
        {
            if (glHelper.ProjectionType == ProjectionType.Perspective)
            {
                glHelper.Camera.KeysHeld.Remove(e.KeyCode);
            }
        }

        private void glControl_Leave(object sender, EventArgs e)
        {
            if (glHelper.ProjectionType == ProjectionType.Perspective)
            {
                glHelper.Camera.KeysHeld.Clear();
            }
        }

        private void ClearObjects()
        {
            if (glObjectNames.Count == 0) return;

            foreach (string glObjectName in glObjectNames)
            {
                glHelper.Buffers.RemoveVertices(glObjectName);
                glHelper.Buffers.RemoveIndices(glObjectName);
                glHelper.Textures.RemoveTexture(glObjectName);
            }

            glObjectNames.Clear();
        }

        private void Render()
        {
            if (render3D)
            {
                if (selectedObj != null)
                {
                    glHelper.Camera.Update();

                    Matrix4 lightMatrix = Matrix4.Mult(Matrix4.CreateRotationY(lightRotation), Matrix4.CreateTranslation(lightPosition));
                    Vector3 rotatedLightPosition = Vector3.Transform(new Vector3(30.0f, 0.0f, 0.0f), lightMatrix);

                    /* Set projection, modelview */
                    glHelper.ProjectionType = ProjectionType.Perspective;
                    glHelper.ZNear = 0.01f;
                    glHelper.ZFar = 1000.0f;

                    /* Activate default shader */
                    glHelper.Shaders.ActivateProgram(oglDefaultShaderName);
                    /* Set shader uniforms */
                    glHelper.Shaders.SetUniformMatrix(oglDefaultShaderName, "projectionMatrix", false, glHelper.CreateProjectionMatrix());
                    glHelper.Shaders.SetUniformMatrix(oglDefaultShaderName, "modelviewMatrix", false, Matrix4.Identity);
                    glHelper.Shaders.SetUniformMatrix(oglDefaultShaderName, "objectMatrix", false, Matrix4.Identity);
                    glHelper.Shaders.SetUniform(oglDefaultShaderName, "enableLight", Convert.ToInt32(Properties.Settings.Default.EnableLighting));
                    glHelper.Shaders.SetUniform(oglDefaultShaderName, "lightPosition", rotatedLightPosition);

                    /* Render each known object */
                    foreach (string glObjectName in glObjectNames)
                    {
                        /* Activate object's texture */
                        glHelper.Textures.ActivateTexture(glObjectName, TextureUnit.Texture0);

                        /* Render */
                        glHelper.Buffers.Render(glObjectName);
                    }

                    if (Properties.Settings.Default.EnableLighting)
                    {
                        /* Render light marker line */
                        glHelper.Shaders.ActivateProgram(oglLightLineShaderName);
                        glHelper.Shaders.SetUniformMatrix(oglLightLineShaderName, "projectionMatrix", false, glHelper.CreateProjectionMatrix());
                        glHelper.Shaders.SetUniformMatrix(oglLightLineShaderName, "modelviewMatrix", false, Matrix4.Identity);
                        glHelper.Shaders.SetUniformMatrix(oglLightLineShaderName, "objectMatrix", false, Matrix4.CreateTranslation(rotatedLightPosition));
                        glHelper.Shaders.SetUniformMatrix(oglLightLineShaderName, "baseMatrix", false, Matrix4.CreateTranslation(lightPosition));
                        glHelper.Shaders.SetUniform(oglLightLineShaderName, "surfaceColor", new OpenTK.Graphics.Color4(0.0f, 0.0f, 0.0f, 0.5f));
                        glHelper.Buffers.Render(oglLightMarkerObjectName);

                        /* Render light marker */
                        glHelper.Shaders.ActivateProgram(oglLightMarkerShaderName);
                        glHelper.Shaders.SetUniformMatrix(oglLightMarkerShaderName, "projectionMatrix", false, glHelper.CreateProjectionMatrix());
                        glHelper.Shaders.SetUniformMatrix(oglLightMarkerShaderName, "modelviewMatrix", false, Matrix4.Identity);
                        glHelper.Shaders.SetUniformMatrix(oglLightMarkerShaderName, "objectMatrix", false, Matrix4.CreateTranslation(rotatedLightPosition));
                        glHelper.Shaders.SetUniformMatrix(oglLightMarkerShaderName, "baseMatrix", false, Matrix4.CreateTranslation(lightPosition));
                        glHelper.Shaders.SetUniform(oglLightMarkerShaderName, "surfaceColor", new OpenTK.Graphics.Color4(1.0f, 1.0f, 1.0f, 0.5f));
                        glHelper.Buffers.Render(oglLightMarkerObjectName);

                        /* Render light marker center */
                        glHelper.Shaders.ActivateProgram(oglLightMarkerShaderName);
                        glHelper.Shaders.SetUniformMatrix(oglLightMarkerShaderName, "objectMatrix", false, Matrix4.CreateTranslation(lightPosition));
                        glHelper.Shaders.SetUniformMatrix(oglLightMarkerShaderName, "baseMatrix", false, Matrix4.Identity);
                        glHelper.Shaders.SetUniform(oglLightMarkerShaderName, "surfaceColor", new OpenTK.Graphics.Color4(0.5f, 0.5f, 0.5f, 0.5f));
                        glHelper.Buffers.Render(oglLightMarkerObjectName);
                    }
                }
            }
            else
            {
                Matrix4 modelviewMatrix2D = Matrix4.CreateTranslation((glHelper.Viewport.Width / 2), (glHelper.Viewport.Height / 2), 0.0f);
                Matrix4 modelviewWithImageOffset = Matrix4.Mult(modelviewMatrix2D, Matrix4.CreateTranslation(imageOffset.X, imageOffset.Y, 0.0f));

                /* Set projection, modelview */
                glHelper.ProjectionType = ProjectionType.Orthographic;
                glHelper.ZNear = -10.0f;
                glHelper.ZFar = 10.0f;

                /* Activate empty dummy texture */
                glHelper.Textures.ActivateTexture(oglEmptyTextureName, TextureUnit.Texture0);

                /* Activate default shader */
                glHelper.Shaders.ActivateProgram(oglDefaultShaderName);

                /* Set shader uniforms */
                glHelper.Shaders.SetUniformMatrix(oglDefaultShaderName, "projectionMatrix", false, glHelper.CreateProjectionMatrix());
                glHelper.Shaders.SetUniformMatrix(oglDefaultShaderName, "modelviewMatrix", false, modelviewMatrix2D);
                glHelper.Shaders.SetUniformMatrix(oglDefaultShaderName, "objectMatrix", false, Matrix4.Identity);
                glHelper.Shaders.SetUniform(oglDefaultShaderName, "enableLight", 0);
                glHelper.Shaders.SetUniform(oglDefaultShaderName, "lightPosition", Vector3.Zero);

                /* Render */
                glHelper.Buffers.Render(oglAxisMarkerObjectName);

                if (selectedObj != null)
                {
                    /* Set shader uniforms */
                    glHelper.Shaders.SetUniformMatrix(oglDefaultShaderName, "modelviewMatrix", false, modelviewWithImageOffset);

                    if (selectedObj is AnimationData)
                    {
                        /* Render animation */
                        AnimationData anim = (selectedObj as AnimationData);
                        if (anim.FirstNode != null)
                        {
                            currentMatrix = Matrix4.Identity;
                            RenderAnimationNode(anim.FirstNode);
                        }
                    }
                    else if (selectedObj is AnimationFrameData)
                    {
                        AnimationFrameData animFrame = (selectedObj as AnimationFrameData);
                        currentMatrix = Matrix4.Identity;
                        RenderAnimationFrame(animFrame, Vector2.Zero);
                    }
                    else if (selectedObj is ImageInformation || selectedObj is SpriteData)
                    {
                        foreach (string glObjectName in glObjectNames)
                        {
                            /* Activate object's texture */
                            glHelper.Textures.ActivateTexture(glObjectName, TextureUnit.Texture0);

                            /* Set matrices */
                            Matrix4 translationMatrix = Matrix4.Identity;
                            if (selectedObj is ImageInformation)
                            {
                                ImageInformation image = (selectedObj as ImageInformation);
                                translationMatrix = Matrix4.CreateTranslation(-(image.Bitmap.Width / 2), -(image.Bitmap.Height / 2), 0.0f);
                            }
                            else if (selectedObj is SpriteData)
                            {
                                SpriteData sprite = (selectedObj as SpriteData);
                                translationMatrix = Matrix4.CreateTranslation(-(sprite.Image.Width / 2), -(sprite.Image.Height / 2), 0.0f);
                            }
                            glHelper.Shaders.SetUniformMatrix(oglDefaultShaderName, "objectMatrix", false, translationMatrix);

                            /* Render object */
                            glHelper.Buffers.Render(glObjectName);
                        }
                    }
                }
            }
        }

        private void RenderAnimationNode(AnimationNodeData node)
        {
            Matrix4 lastMatrix = currentMatrix;

            if (node.ChildNode != null)
                RenderAnimationNode(node.ChildNode);

            if (node.FirstAnimationFrameID != -1)
            {
                AnimationFrameData animFrame = node.AnimationFrames.LastOrDefault(x => animCounter >= x.FrameTime);

                maxCounter = Math.Max(maxCounter, node.AnimationFrames.Max(x => x.FrameTime) * 3);

                if (animFrame != null)
                {
                    Vector2 nodeOffset = Vector2.Zero;
                    if ((node.Unknown0x06 & 0x01) == 0x01)
                    {
                        nodeOffset.X = animFrame.Transform.TransformOffset.Offset.X;
                        nodeOffset.Y = animFrame.Transform.TransformOffset.Offset.Y;
                    }
                    else
                    {
                        nodeOffset.X = -animFrame.Transform.TransformOffset.Offset.X;
                        nodeOffset.Y = -animFrame.Transform.TransformOffset.Offset.Y;
                    }

                    RenderAnimationFrame(animFrame, nodeOffset);
                }
            }

            if (node.SiblingNode != null)
                RenderAnimationNode(node.SiblingNode);

            currentMatrix = lastMatrix;
        }

        private void RenderAnimationFrame(AnimationFrameData animFrame, Vector2 offset)
        {
            float scaleX = (animFrame.Transform.Scale.X / 100.0f);
            float scaleY = (animFrame.Transform.Scale.Y / 100.0f);

            if (scaleX == 0.0f || scaleY == 0.0f) return;

            Matrix4 translationMatrix = Matrix4.CreateTranslation((animFrame.Transform.BaseOffset.X + offset.X), (animFrame.Transform.BaseOffset.Y + offset.Y), 0.0f);
            Matrix4 rotationMatrix = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(animFrame.Transform.RotationAngle));
            Matrix4 scaleMatrix = Matrix4.CreateScale(scaleX, scaleY, 1.0f);

            currentMatrix = Matrix4.Mult(translationMatrix, currentMatrix);

            if (animFrame.Unknown0x02 != 1)
                currentMatrix = Matrix4.Mult(Matrix4.CreateTranslation((animFrame.Sprite.Rectangle.Width / 2), (animFrame.Sprite.Rectangle.Height / 2), 0.0f), currentMatrix);

            currentMatrix = Matrix4.Mult(rotationMatrix, currentMatrix);
            currentMatrix = Matrix4.Mult(scaleMatrix, currentMatrix);

            if (animFrame.Unknown0x02 != 1)
                currentMatrix = Matrix4.Mult(Matrix4.CreateTranslation(-(animFrame.Sprite.Rectangle.Width / 2), -(animFrame.Sprite.Rectangle.Height / 2), 0.0f), currentMatrix);

            glHelper.Shaders.SetUniformMatrix(oglDefaultShaderName, "objectMatrix", false, currentMatrix);

            string spriteName = animFrame.Sprite.PrepareRender(glHelper);
            glHelper.Textures.ActivateTexture(spriteName, TextureUnit.Texture0);
            glHelper.Buffers.Render(spriteName);
        }

        private void MainForm_Activated(object sender, EventArgs e)
        {
            timer.Start();
        }

        private void MainForm_Deactivate(object sender, EventArgs e)
        {
            timer.Stop();
        }
    }
}
