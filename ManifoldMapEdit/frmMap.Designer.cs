namespace ManifoldMapEdit
{
    partial class frmMap
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMap));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.axComponentControl1 = new AxManifold.Interop.AxComponentControl();
            this.gbDrawControls = new System.Windows.Forms.GroupBox();
            this.btnDrawArea = new System.Windows.Forms.Button();
            this.btnDrawLine = new System.Windows.Forms.Button();
            this.btnDrawPoint = new System.Windows.Forms.Button();
            this.gbEditMode = new System.Windows.Forms.GroupBox();
            this.btnEditDeleteCoordinate = new System.Windows.Forms.Button();
            this.btnEditAddCoordinate = new System.Windows.Forms.Button();
            this.btnEditMoveGeom = new System.Windows.Forms.Button();
            this.btnEditEnable = new System.Windows.Forms.Button();
            this.lblMode = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.axComponentControl1)).BeginInit();
            this.gbDrawControls.SuspendLayout();
            this.gbEditMode.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.axComponentControl1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.textBox1);
            this.splitContainer1.Panel2.Controls.Add(this.gbDrawControls);
            this.splitContainer1.Panel2.Controls.Add(this.gbEditMode);
            this.splitContainer1.Panel2.Controls.Add(this.lblMode);
            this.splitContainer1.Panel2.Paint += new System.Windows.Forms.PaintEventHandler(this.splitContainer1_Panel2_Paint);
            this.splitContainer1.Size = new System.Drawing.Size(777, 492);
            this.splitContainer1.SplitterDistance = 556;
            this.splitContainer1.TabIndex = 0;
            // 
            // axComponentControl1
            // 
            this.axComponentControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.axComponentControl1.Enabled = true;
            this.axComponentControl1.Location = new System.Drawing.Point(0, 0);
            this.axComponentControl1.Name = "axComponentControl1";
            this.axComponentControl1.OcxState = ((System.Windows.Forms.AxHost.State)(resources.GetObject("axComponentControl1.OcxState")));
            this.axComponentControl1.Size = new System.Drawing.Size(556, 492);
            this.axComponentControl1.TabIndex = 0;
            this.axComponentControl1.ClickEvent += new AxManifold.Interop.IComponentControlEvents_ClickEventHandler(this.axComponentControl1_ClickEvent);
            this.axComponentControl1.EndTrack += new AxManifold.Interop.IComponentControlEvents_EndTrackEventHandler(this.axComponentControl1_EndTrack);
            // 
            // gbDrawControls
            // 
            this.gbDrawControls.Controls.Add(this.btnDrawArea);
            this.gbDrawControls.Controls.Add(this.btnDrawLine);
            this.gbDrawControls.Controls.Add(this.btnDrawPoint);
            this.gbDrawControls.Location = new System.Drawing.Point(15, 46);
            this.gbDrawControls.Name = "gbDrawControls";
            this.gbDrawControls.Size = new System.Drawing.Size(177, 102);
            this.gbDrawControls.TabIndex = 12;
            this.gbDrawControls.TabStop = false;
            this.gbDrawControls.Text = "  Draw:  ";
            // 
            // btnDrawArea
            // 
            this.btnDrawArea.Location = new System.Drawing.Point(13, 60);
            this.btnDrawArea.Name = "btnDrawArea";
            this.btnDrawArea.Size = new System.Drawing.Size(73, 35);
            this.btnDrawArea.TabIndex = 7;
            this.btnDrawArea.Text = "Polygon";
            this.btnDrawArea.UseVisualStyleBackColor = true;
            this.btnDrawArea.Click += new System.EventHandler(this.btnDrawArea_Click);
            // 
            // btnDrawLine
            // 
            this.btnDrawLine.Location = new System.Drawing.Point(91, 19);
            this.btnDrawLine.Name = "btnDrawLine";
            this.btnDrawLine.Size = new System.Drawing.Size(73, 35);
            this.btnDrawLine.TabIndex = 6;
            this.btnDrawLine.Text = "Line";
            this.btnDrawLine.UseVisualStyleBackColor = true;
            this.btnDrawLine.Click += new System.EventHandler(this.btnDrawLine_Click);
            // 
            // btnDrawPoint
            // 
            this.btnDrawPoint.Location = new System.Drawing.Point(13, 19);
            this.btnDrawPoint.Name = "btnDrawPoint";
            this.btnDrawPoint.Size = new System.Drawing.Size(73, 35);
            this.btnDrawPoint.TabIndex = 5;
            this.btnDrawPoint.Text = "Point";
            this.btnDrawPoint.UseVisualStyleBackColor = true;
            this.btnDrawPoint.Click += new System.EventHandler(this.btnDrawPoint_Click);
            // 
            // gbEditMode
            // 
            this.gbEditMode.Controls.Add(this.btnEditDeleteCoordinate);
            this.gbEditMode.Controls.Add(this.btnEditAddCoordinate);
            this.gbEditMode.Controls.Add(this.btnEditMoveGeom);
            this.gbEditMode.Controls.Add(this.btnEditEnable);
            this.gbEditMode.Location = new System.Drawing.Point(15, 166);
            this.gbEditMode.Name = "gbEditMode";
            this.gbEditMode.Size = new System.Drawing.Size(177, 111);
            this.gbEditMode.TabIndex = 11;
            this.gbEditMode.TabStop = false;
            this.gbEditMode.Text = "  Edit Mode - Disabled:  ";
            // 
            // btnEditDeleteCoordinate
            // 
            this.btnEditDeleteCoordinate.Enabled = false;
            this.btnEditDeleteCoordinate.Location = new System.Drawing.Point(91, 19);
            this.btnEditDeleteCoordinate.Name = "btnEditDeleteCoordinate";
            this.btnEditDeleteCoordinate.Size = new System.Drawing.Size(73, 35);
            this.btnEditDeleteCoordinate.TabIndex = 22;
            this.btnEditDeleteCoordinate.Text = "Delete";
            this.btnEditDeleteCoordinate.UseVisualStyleBackColor = true;
            this.btnEditDeleteCoordinate.Click += new System.EventHandler(this.btnEditDeleteCoordinate_Click);
            // 
            // btnEditAddCoordinate
            // 
            this.btnEditAddCoordinate.Enabled = false;
            this.btnEditAddCoordinate.Location = new System.Drawing.Point(12, 60);
            this.btnEditAddCoordinate.Name = "btnEditAddCoordinate";
            this.btnEditAddCoordinate.Size = new System.Drawing.Size(73, 35);
            this.btnEditAddCoordinate.TabIndex = 12;
            this.btnEditAddCoordinate.Text = "Add";
            this.btnEditAddCoordinate.UseVisualStyleBackColor = true;
            this.btnEditAddCoordinate.Click += new System.EventHandler(this.btnEditAddCoordinate_Click);
            // 
            // btnEditMoveGeom
            // 
            this.btnEditMoveGeom.Enabled = false;
            this.btnEditMoveGeom.Location = new System.Drawing.Point(91, 60);
            this.btnEditMoveGeom.Name = "btnEditMoveGeom";
            this.btnEditMoveGeom.Size = new System.Drawing.Size(73, 35);
            this.btnEditMoveGeom.TabIndex = 13;
            this.btnEditMoveGeom.TabStop = false;
            this.btnEditMoveGeom.Text = "Move";
            this.btnEditMoveGeom.UseVisualStyleBackColor = true;
            this.btnEditMoveGeom.Click += new System.EventHandler(this.btnEditMoveGeom_Click);
            // 
            // btnEditEnable
            // 
            this.btnEditEnable.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnEditEnable.Location = new System.Drawing.Point(12, 19);
            this.btnEditEnable.Name = "btnEditEnable";
            this.btnEditEnable.Size = new System.Drawing.Size(73, 35);
            this.btnEditEnable.TabIndex = 10;
            this.btnEditEnable.Text = "Enable";
            this.btnEditEnable.UseVisualStyleBackColor = true;
            this.btnEditEnable.Click += new System.EventHandler(this.btnEditEnable_Click);
            // 
            // lblMode
            // 
            this.lblMode.AutoSize = true;
            this.lblMode.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMode.Location = new System.Drawing.Point(12, 13);
            this.lblMode.Name = "lblMode";
            this.lblMode.Size = new System.Drawing.Size(42, 13);
            this.lblMode.TabIndex = 2;
            this.lblMode.Text = "Mode:";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(15, 283);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox1.Size = new System.Drawing.Size(177, 79);
            this.textBox1.TabIndex = 13;
            this.textBox1.Text = "To enable Edit Mode - select a Line or Polygon on the map then Press \'Enable\'. Th" +
                "e default Edit mode is Move Coordinate.";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(777, 492);
            this.Controls.Add(this.splitContainer1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.axComponentControl1)).EndInit();
            this.gbDrawControls.ResumeLayout(false);
            this.gbEditMode.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private AxManifold.Interop.AxComponentControl axComponentControl1;
        private System.Windows.Forms.Label lblMode;
        private System.Windows.Forms.GroupBox gbEditMode;
        private System.Windows.Forms.Button btnEditDeleteCoordinate;
        private System.Windows.Forms.Button btnEditAddCoordinate;
        private System.Windows.Forms.Button btnEditMoveGeom;
        private System.Windows.Forms.Button btnEditEnable;
        private System.Windows.Forms.GroupBox gbDrawControls;
        private System.Windows.Forms.Button btnDrawArea;
        private System.Windows.Forms.Button btnDrawLine;
        private System.Windows.Forms.Button btnDrawPoint;
        private System.Windows.Forms.TextBox textBox1;

    }
}

