namespace camera_show {
    partial class Form1 {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.setCameraToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.closeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setDebugToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.setPortToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addStepComparToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.configLeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.backgroundWorker1 = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pictureBox1
            // 
            this.pictureBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.pictureBox1.Location = new System.Drawing.Point(1, 1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(260, 237);
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Paint += new System.Windows.Forms.PaintEventHandler(this.pictureBox1_Paint);
            this.pictureBox1.MouseDown += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseDown);
            this.pictureBox1.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseMove);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.pictureBox1_MouseUp);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.setCameraToolStripMenuItem,
            this.closeToolStripMenuItem,
            this.setDebugToolStripMenuItem,
            this.setPortToolStripMenuItem,
            this.addStepComparToolStripMenuItem,
            this.configLeToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(172, 136);
            // 
            // setCameraToolStripMenuItem
            // 
            this.setCameraToolStripMenuItem.Name = "setCameraToolStripMenuItem";
            this.setCameraToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.setCameraToolStripMenuItem.Text = "set camera";
            this.setCameraToolStripMenuItem.Click += new System.EventHandler(this.setCameraToolStripMenuItem_Click);
            // 
            // closeToolStripMenuItem
            // 
            this.closeToolStripMenuItem.Name = "closeToolStripMenuItem";
            this.closeToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.closeToolStripMenuItem.Text = "close";
            this.closeToolStripMenuItem.Click += new System.EventHandler(this.closeToolStripMenuItem_Click);
            // 
            // setDebugToolStripMenuItem
            // 
            this.setDebugToolStripMenuItem.Name = "setDebugToolStripMenuItem";
            this.setDebugToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.setDebugToolStripMenuItem.Text = "set debug";
            this.setDebugToolStripMenuItem.Click += new System.EventHandler(this.setDebugToolStripMenuItem_Click);
            // 
            // setPortToolStripMenuItem
            // 
            this.setPortToolStripMenuItem.Name = "setPortToolStripMenuItem";
            this.setPortToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.setPortToolStripMenuItem.Text = "set port";
            this.setPortToolStripMenuItem.Click += new System.EventHandler(this.setPortToolStripMenuItem_Click);
            // 
            // addStepComparToolStripMenuItem
            // 
            this.addStepComparToolStripMenuItem.Name = "addStepComparToolStripMenuItem";
            this.addStepComparToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.addStepComparToolStripMenuItem.Text = "add step compar";
            this.addStepComparToolStripMenuItem.Click += new System.EventHandler(this.addStepComparToolStripMenuItem_Click);
            // 
            // configLeToolStripMenuItem
            // 
            this.configLeToolStripMenuItem.Name = "configLeToolStripMenuItem";
            this.configLeToolStripMenuItem.Size = new System.Drawing.Size(171, 22);
            this.configLeToolStripMenuItem.Text = "config length cam";
            this.configLeToolStripMenuItem.Click += new System.EventHandler(this.configLeToolStripMenuItem_Click);
            // 
            // backgroundWorker1
            // 
            this.backgroundWorker1.WorkerReportsProgress = true;
            this.backgroundWorker1.WorkerSupportsCancellation = true;
            this.backgroundWorker1.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorker1_DoWork);
            this.backgroundWorker1.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.backgroundWorker1_ProgressChanged);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(278, 247);
            this.ControlBox = false;
            this.Controls.Add(this.pictureBox1);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Form1";
            this.TopMost = true;
            this.TransparencyKey = System.Drawing.Color.DarkRed;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem setCameraToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem closeToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setDebugToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setPortToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addStepComparToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem configLeToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker backgroundWorker1;
    }
}

