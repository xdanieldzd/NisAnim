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
using System.Text.RegularExpressions;

using NisAnim.Conversion;

namespace NisAnim
{
    public partial class MainForm : Form
    {
        /* TODO add lzs decomp support? add support for separate txf files? */
        BaseFile loadedFile;
        object selectedObj { get { return pgObject.SelectedObject; } }

        bool mouseDown;
        Point mouseCenter, imageOffset;

        Timer timer;
        int animCounter, maxCounter;

        public MainForm()
        {
            InitializeComponent();

            SetFormTitle();

            debugDrawToolStripMenuItem.Checked = Properties.Settings.Default.DebugDraw;

            mouseCenter = imageOffset = Point.Empty;

            animCounter = 0;
            maxCounter = 0;

            timer = new Timer();
            timer.Interval = 15;
            timer.Tick += ((s, e) =>
            {
                animCounter++;
                if (animCounter >= maxCounter) animCounter = 0;

                pnlRender.Invalidate();
            });

            timer.Start();

            tsslStatus.Text = "Ready";
        }

        private void SetFormTitle()
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(Application.ProductName);
            if (loadedFile != null) builder.AppendFormat(" - [{0}]", Path.GetFileName(loadedFile.FilePath));

            Text = builder.ToString();
        }

        private Type IdentifyFile(string fileName)
        {
            Type baseType = typeof(BaseFile);
            foreach (Type t in baseType.Assembly.GetTypes().Where(x => x.BaseType == baseType))
            {
                FieldInfo fi = t.GetField("FileNamePattern", BindingFlags.Public | BindingFlags.Static);
                if (fi == null) continue;

                string pat = (fi.GetValue(null) as string);
                Regex reg = new Regex(pat);

                if (reg.IsMatch(Path.GetFileName(fileName))) return t;
            }

            return null;
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
                pnlRender.Invalidate();

                Type fileImplType = IdentifyFile(ofdDataFile.FileName);
                if (fileImplType == null)
                {
                    MessageBox.Show("Could not identify file!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                loadedFile = (BaseFile)Activator.CreateInstance(fileImplType, new object[] { (Properties.Settings.Default.LastDat = ofdDataFile.FileName) });

                /* TODO have partial treeview updates? */
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += ((s, ev) =>
                {
                    tvObject.Invoke(new Action(() => { tvObject.Nodes.Clear(); }));
                    ev.Result = TraverseObject(null, Path.GetFileName(loadedFile.FilePath), loadedFile, true);
                });
                worker.RunWorkerCompleted += ((s, ev) =>
                {
                    tvObject.Enabled = true;
                    tvObject.Focus();
                    tvObject.Nodes.Add((TreeNode)ev.Result);

                    tsslStatus.Text = "File loaded";
                });

                tsslStatus.Text = "Generating tree...";
                worker.RunWorkerAsync();

                SetFormTitle();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void debugDrawToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.DebugDraw = (sender as ToolStripMenuItem).Checked;
        }

        private void resetTranslationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            imageOffset = Point.Empty;
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("NisAnim - NIS Animation Viewer\nWritten 2015 by xdaniel - https://github.com/xdanieldzd/", "About", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private TreeNode TraverseObject(TreeNode parentNode, string name, object obj, bool recurse = false)
        {
            if (obj == null) return null;

            DisplayNameAttribute objDisplayName = (obj.GetType().GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(DisplayNameAttribute)) as DisplayNameAttribute);
            TreeNode node = new TreeNode(objDisplayName != null ? string.Format("{0} [{1}]", name, objDisplayName.DisplayName) : name) { Tag = obj };

            if (!(obj is System.Collections.ICollection))
            {
                var props = obj.GetType()
                    .GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public)
                    .Where(x => x.PropertyType.IsArray ||
                        (x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(List<>)) ||
                        x.GetCustomAttributes(false).FirstOrDefault(y => (y is EditorBrowsableAttribute) && (y as EditorBrowsableAttribute).State != EditorBrowsableState.Never) != null);

                foreach (var prop in props)
                {
                    var val = prop.GetValue(obj, null);
                    if (val == null) continue;

                    DisplayNameAttribute propDisplayName = (prop.GetCustomAttributes(false).FirstOrDefault(x => x.GetType() == typeof(DisplayNameAttribute)) as DisplayNameAttribute);
                    if (((val is System.Collections.ICollection) &&
                        (!(val as System.Collections.ICollection).AsQueryable().ElementType.IsValueType) && (val as System.Collections.ICollection).Count > 0) ||
                        (!(val is System.Collections.ICollection)))
                    {
                        if (recurse)
                            node.Nodes.Add(TraverseObject(node, propDisplayName != null ? propDisplayName.DisplayName : prop.Name, val, recurse));
                        else
                            node.Nodes.Add(new TreeNode(propDisplayName != null ? propDisplayName.DisplayName : prop.Name) { Tag = val });
                    }
                }
            }
            else
            {
                foreach (var e in (obj as IEnumerable<object>).Select((Value, Index) => new { Index, Value }))
                {
                    if (recurse)
                        node.Nodes.Add(TraverseObject(node, string.Format("{0}", e.Index), e.Value, recurse));
                    else
                        node.Nodes.Add(new TreeNode(string.Format("{0}", e.Index)) { Tag = e });
                }
            }

            return node;
        }

        private void tvObject_AfterSelect(object sender, TreeViewEventArgs e)
        {
            pgObject.SelectedObject = e.Node.Tag;
            pnlRender.Invalidate();

            animCounter = 0;
            maxCounter = 0;
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            pnlRender.Invalidate();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Properties.Settings.Default.Save();

            if (loadedFile != null)
                loadedFile.Dispose();
        }

        private void pnlRender_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                imageOffset.X += -(mouseCenter.X - e.X);
                imageOffset.Y += -(mouseCenter.Y - e.Y);
                mouseCenter = e.Location;
                pnlRender.Invalidate();
            }
        }

        private void pnlRender_MouseUp(object sender, MouseEventArgs e)
        {
            mouseDown = false;
        }

        private void pnlRender_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button.HasFlag(MouseButtons.Left))
            {
                mouseDown = true;
                mouseCenter = e.Location;
            }
        }

        private void pnlRender_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.Clear((sender as Control).BackColor);

            e.Graphics.DrawString(imageOffset.ToString(), SystemFonts.CaptionFont, Brushes.Black, Point.Empty);
            e.Graphics.TranslateTransform(e.ClipRectangle.Width / 2, e.ClipRectangle.Height / 2);
            e.Graphics.TranslateTransform(imageOffset.X, imageOffset.Y);

            e.Graphics.DrawLine(Pens.Black, -300, 0, 300, 0);
            e.Graphics.DrawLine(Pens.Black, 0, -300, 0, 300);

            /* TODO move to individual txf and anmdat classes? */
            if (selectedObj is ImageInformation)
            {
                Bitmap image = (selectedObj as ImageInformation).Bitmap;
                e.Graphics.DrawImage(image, new Point(-(image.Width / 2), -(image.Height / 2)));
            }
            else if (selectedObj is AnimationData)
            {
                AnimationData anim = (selectedObj as AnimationData);
                if (anim.FirstNode != null) DrawAnimationNode(e.Graphics, anim.FirstNode);
            }
            else if (selectedObj is AnimationFrameData)
            {
                DrawAnimationFrame(e.Graphics, (selectedObj as AnimationFrameData), Point.Empty);
            }
            else if (selectedObj is SpriteData)
            {
                SpriteData sprite = (selectedObj as SpriteData);
                e.Graphics.DrawImage(sprite.Image, new Point(-(sprite.Image.Width / 2), -(sprite.Image.Height / 2)));
            }

            e.Graphics.ResetTransform();
        }

        private void DrawAnimationNode(Graphics g, AnimationNodeData node)
        {
            Matrix prevTransform = g.Transform;

            if (node.ChildNode != null)
                DrawAnimationNode(g, node.ChildNode);

            if (node.FirstAnimationFrameID != -1)
            {
                AnimationFrameData animFrame = node.AnimationFrames.LastOrDefault(x => animCounter >= x.FrameTime);

                maxCounter = Math.Max(maxCounter, node.AnimationFrames.Max(x => x.FrameTime) * 3);

                if (animFrame != null)
                {
                    //dunno, probably not...
                    //if (animFrame.Unknown0x02 == 1) g.Transform = prevTransform;

                    Point nodeOffset = Point.Empty;
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

                    DrawAnimationFrame(g, animFrame, nodeOffset);
                }
            }

            if (node.SiblingNode != null)
                DrawAnimationNode(g, node.SiblingNode);

            g.Transform = prevTransform;
        }

        private void DrawAnimationFrame(Graphics g, AnimationFrameData animFrame, Point offset)
        {
            /* 09016 title screen */
            /* 00005 senate */
            /* 00050 desco battle */
            /* 24001 fuka convo A */

            float scaleX = (animFrame.Transform.Scale.X / 100.0f);
            float scaleY = (animFrame.Transform.Scale.Y / 100.0f);

            if (scaleX == 0.0f || scaleY == 0.0f) return;

            Point framePosition = new Point(animFrame.Transform.BaseOffset.X + offset.X, animFrame.Transform.BaseOffset.Y + offset.Y);

            /* likely wrong? */
            //if (animFrame.Sprite.Rectangle.Width == 0) framePosition.X += (animFrame.Sprite.Rectangle.X + animFrame.Sprite.Rectangle.Y) / 2;
            //if (animFrame.Sprite.Rectangle.Height == 0) framePosition.Y += (animFrame.Sprite.Rectangle.X + animFrame.Sprite.Rectangle.Y) / 2;

            g.TranslateTransform(framePosition.X, framePosition.Y);

            if (animFrame.Unknown0x02 != 1)
                g.TranslateTransform(animFrame.Sprite.Rectangle.Width / 2, animFrame.Sprite.Rectangle.Height / 2);
            g.RotateTransform(animFrame.Transform.RotationAngle);
            g.ScaleTransform(scaleX, scaleY);
            if (animFrame.Unknown0x02 != 1)
                g.TranslateTransform(-(animFrame.Sprite.Rectangle.Width / 2), -(animFrame.Sprite.Rectangle.Height / 2));

            g.DrawImage(animFrame.Sprite.Image, Point.Empty);

            if (Properties.Settings.Default.DebugDraw)
            {
                Pen debugRectPen = ((animFrame.Transform.RotationAngle != 0 || scaleX != 1.0f || scaleY != 1.0f) ? Pens.OrangeRed : Pens.Yellow);
                g.DrawRectangle(debugRectPen, new Rectangle(0, 0, animFrame.Sprite.Rectangle.Width, animFrame.Sprite.Rectangle.Height));
                g.DrawString(string.Format("{{X={0}, Y={1}}}", framePosition.X, framePosition.Y), SystemFonts.StatusFont, Brushes.Blue, Point.Empty);
            }
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
