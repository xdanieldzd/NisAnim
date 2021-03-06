﻿namespace NisAnim
{
    partial class MainForm
    {
        /// <summary>
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Windows Form-Designer generierter Code

        /// <summary>
        /// Erforderliche Methode für die Designerunterstützung.
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.pgObject = new System.Windows.Forms.PropertyGrid();
            this.msMainMenu = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.optionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.debugDrawToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripSeparator();
            this.resetTranslationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.aboutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ofdDataFile = new System.Windows.Forms.OpenFileDialog();
            this.ssMainStatus = new System.Windows.Forms.StatusStrip();
            this.tsslStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.cmsTreeNode = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.sfdDataFile = new System.Windows.Forms.SaveFileDialog();
            this.glControl = new OpenTK.GLControl();
            this.enableLightingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.tvObject = new NisAnim.TreeViewEx();
            this.msMainMenu.SuspendLayout();
            this.ssMainStatus.SuspendLayout();
            this.cmsTreeNode.SuspendLayout();
            this.SuspendLayout();
            // 
            // pgObject
            // 
            this.pgObject.Dock = System.Windows.Forms.DockStyle.Right;
            this.pgObject.Location = new System.Drawing.Point(989, 24);
            this.pgObject.Name = "pgObject";
            this.pgObject.Size = new System.Drawing.Size(275, 636);
            this.pgObject.TabIndex = 2;
            // 
            // msMainMenu
            // 
            this.msMainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.optionsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.msMainMenu.Location = new System.Drawing.Point(0, 0);
            this.msMainMenu.Name = "msMainMenu";
            this.msMainMenu.Size = new System.Drawing.Size(1264, 24);
            this.msMainMenu.TabIndex = 3;
            this.msMainMenu.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openToolStripMenuItem,
            this.toolStripMenuItem1,
            this.exitToolStripMenuItem});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileToolStripMenuItem.Text = "&File";
            // 
            // openToolStripMenuItem
            // 
            this.openToolStripMenuItem.Name = "openToolStripMenuItem";
            this.openToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.openToolStripMenuItem.Text = "&Open";
            this.openToolStripMenuItem.Click += new System.EventHandler(this.openToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(100, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(103, 22);
            this.exitToolStripMenuItem.Text = "E&xit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // optionsToolStripMenuItem
            // 
            this.optionsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.enableLightingToolStripMenuItem,
            this.debugDrawToolStripMenuItem,
            this.toolStripMenuItem2,
            this.resetTranslationToolStripMenuItem});
            this.optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            this.optionsToolStripMenuItem.Size = new System.Drawing.Size(61, 20);
            this.optionsToolStripMenuItem.Text = "&Options";
            // 
            // debugDrawToolStripMenuItem
            // 
            this.debugDrawToolStripMenuItem.CheckOnClick = true;
            this.debugDrawToolStripMenuItem.Name = "debugDrawToolStripMenuItem";
            this.debugDrawToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.debugDrawToolStripMenuItem.Text = "&Debug Draw";
            this.debugDrawToolStripMenuItem.Click += new System.EventHandler(this.debugDrawToolStripMenuItem_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(161, 6);
            // 
            // resetTranslationToolStripMenuItem
            // 
            this.resetTranslationToolStripMenuItem.Name = "resetTranslationToolStripMenuItem";
            this.resetTranslationToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.resetTranslationToolStripMenuItem.Text = "&Reset Translation";
            this.resetTranslationToolStripMenuItem.Click += new System.EventHandler(this.resetTranslationToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.aboutToolStripMenuItem});
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(44, 20);
            this.helpToolStripMenuItem.Text = "&Help";
            // 
            // aboutToolStripMenuItem
            // 
            this.aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            this.aboutToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.aboutToolStripMenuItem.Text = "&About";
            this.aboutToolStripMenuItem.Click += new System.EventHandler(this.aboutToolStripMenuItem_Click);
            // 
            // ofdDataFile
            // 
            this.ofdDataFile.Filter = "All Files (*.*)|*.*";
            // 
            // ssMainStatus
            // 
            this.ssMainStatus.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tsslStatus});
            this.ssMainStatus.Location = new System.Drawing.Point(0, 660);
            this.ssMainStatus.Name = "ssMainStatus";
            this.ssMainStatus.Size = new System.Drawing.Size(1264, 22);
            this.ssMainStatus.TabIndex = 4;
            this.ssMainStatus.Text = "statusStrip1";
            // 
            // tsslStatus
            // 
            this.tsslStatus.Name = "tsslStatus";
            this.tsslStatus.Size = new System.Drawing.Size(22, 17);
            this.tsslStatus.Text = "---";
            // 
            // cmsTreeNode
            // 
            this.cmsTreeNode.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToolStripMenuItem});
            this.cmsTreeNode.Name = "cmsTreeNode";
            this.cmsTreeNode.Size = new System.Drawing.Size(108, 26);
            // 
            // saveToolStripMenuItem
            // 
            this.saveToolStripMenuItem.Name = "saveToolStripMenuItem";
            this.saveToolStripMenuItem.Size = new System.Drawing.Size(107, 22);
            this.saveToolStripMenuItem.Text = "&Save...";
            this.saveToolStripMenuItem.Click += new System.EventHandler(this.saveToolStripMenuItem_Click);
            // 
            // glControl
            // 
            this.glControl.BackColor = System.Drawing.Color.Black;
            this.glControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.glControl.Location = new System.Drawing.Point(275, 24);
            this.glControl.Name = "glControl";
            this.glControl.Size = new System.Drawing.Size(714, 636);
            this.glControl.TabIndex = 0;
            this.glControl.VSync = false;
            this.glControl.KeyDown += new System.Windows.Forms.KeyEventHandler(this.glControl_KeyDown);
            this.glControl.KeyUp += new System.Windows.Forms.KeyEventHandler(this.glControl_KeyUp);
            this.glControl.Leave += new System.EventHandler(this.glControl_Leave);
            this.glControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseDown);
            this.glControl.MouseMove += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseMove);
            this.glControl.MouseUp += new System.Windows.Forms.MouseEventHandler(this.glControl_MouseUp);
            // 
            // enableLightingToolStripMenuItem
            // 
            this.enableLightingToolStripMenuItem.CheckOnClick = true;
            this.enableLightingToolStripMenuItem.Name = "enableLightingToolStripMenuItem";
            this.enableLightingToolStripMenuItem.Size = new System.Drawing.Size(164, 22);
            this.enableLightingToolStripMenuItem.Text = "&Enable Lighting";
            this.enableLightingToolStripMenuItem.Click += new System.EventHandler(this.enableLightingToolStripMenuItem_Click);
            // 
            // tvObject
            // 
            this.tvObject.Dock = System.Windows.Forms.DockStyle.Left;
            this.tvObject.HideSelection = false;
            this.tvObject.Location = new System.Drawing.Point(0, 24);
            this.tvObject.Name = "tvObject";
            this.tvObject.Size = new System.Drawing.Size(275, 636);
            this.tvObject.TabIndex = 0;
            this.tvObject.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.tvObject_AfterSelect);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1264, 682);
            this.Controls.Add(this.glControl);
            this.Controls.Add(this.pgObject);
            this.Controls.Add(this.tvObject);
            this.Controls.Add(this.ssMainStatus);
            this.Controls.Add(this.msMainMenu);
            this.MainMenuStrip = this.msMainMenu;
            this.Name = "MainForm";
            this.Text = "NisAnim";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.Deactivate += new System.EventHandler(this.MainForm_Deactivate);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.msMainMenu.ResumeLayout(false);
            this.msMainMenu.PerformLayout();
            this.ssMainStatus.ResumeLayout(false);
            this.ssMainStatus.PerformLayout();
            this.cmsTreeNode.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private TreeViewEx tvObject;
        private System.Windows.Forms.PropertyGrid pgObject;
        private System.Windows.Forms.MenuStrip msMainMenu;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog ofdDataFile;
        private System.Windows.Forms.ToolStripMenuItem optionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem debugDrawToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem2;
        private System.Windows.Forms.ToolStripMenuItem resetTranslationToolStripMenuItem;
        private System.Windows.Forms.StatusStrip ssMainStatus;
        private System.Windows.Forms.ToolStripStatusLabel tsslStatus;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem aboutToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip cmsTreeNode;
        private System.Windows.Forms.ToolStripMenuItem saveToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog sfdDataFile;
        private OpenTK.GLControl glControl;
        private System.Windows.Forms.ToolStripMenuItem enableLightingToolStripMenuItem;
    }
}

