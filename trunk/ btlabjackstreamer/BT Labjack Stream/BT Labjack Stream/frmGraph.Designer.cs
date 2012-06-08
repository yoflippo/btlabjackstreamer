namespace BT_Labjack_Stream
{
    partial class frmGraph
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmGraph));
            this.zg1 = new ZedGraph.ZedGraphControl();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.gbx_Settings = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.nudGraphY = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.nudGraphX = new System.Windows.Forms.NumericUpDown();
            this.gbx_Settings.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudGraphY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGraphX)).BeginInit();
            this.SuspendLayout();
            // 
            // zg1
            // 
            this.zg1.AutoSize = true;
            this.zg1.IsAutoScrollRange = true;
            this.zg1.IsScrollY2 = true;
            this.zg1.Location = new System.Drawing.Point(12, 12);
            this.zg1.Name = "zg1";
            this.zg1.ScrollGrace = 0D;
            this.zg1.ScrollMaxX = 0D;
            this.zg1.ScrollMaxY = 0D;
            this.zg1.ScrollMaxY2 = 0D;
            this.zg1.ScrollMinX = 0D;
            this.zg1.ScrollMinY = 0D;
            this.zg1.ScrollMinY2 = 0D;
            this.zg1.Size = new System.Drawing.Size(880, 451);
            this.zg1.TabIndex = 13;
            this.zg1.ZoomStepFraction = 1D;
            this.zg1.ZoomEvent += new ZedGraph.ZedGraphControl.ZoomEventHandler(this.zg1_ZoomEvent);
            // 
            // timer1
            // 
            this.timer1.Enabled = true;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // gbx_Settings
            // 
            this.gbx_Settings.Controls.Add(this.label1);
            this.gbx_Settings.Controls.Add(this.nudGraphY);
            this.gbx_Settings.Controls.Add(this.label2);
            this.gbx_Settings.Controls.Add(this.nudGraphX);
            this.gbx_Settings.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.gbx_Settings.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.gbx_Settings.Location = new System.Drawing.Point(12, 498);
            this.gbx_Settings.Name = "gbx_Settings";
            this.gbx_Settings.Size = new System.Drawing.Size(298, 69);
            this.gbx_Settings.TabIndex = 20;
            this.gbx_Settings.TabStop = false;
            this.gbx_Settings.Text = "Settings";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label1.Location = new System.Drawing.Point(144, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(36, 16);
            this.label1.TabIndex = 23;
            this.label1.Text = "Y-as";
            // 
            // nudGraphY
            // 
            this.nudGraphY.Location = new System.Drawing.Point(147, 36);
            this.nudGraphY.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudGraphY.Name = "nudGraphY";
            this.nudGraphY.Size = new System.Drawing.Size(120, 22);
            this.nudGraphY.TabIndex = 22;
            this.nudGraphY.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudGraphY.ValueChanged += new System.EventHandler(this.nudGraphY_ValueChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label2.Location = new System.Drawing.Point(6, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(35, 16);
            this.label2.TabIndex = 21;
            this.label2.Text = "X-as";
            // 
            // nudGraphX
            // 
            this.nudGraphX.Location = new System.Drawing.Point(6, 36);
            this.nudGraphX.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudGraphX.Name = "nudGraphX";
            this.nudGraphX.Size = new System.Drawing.Size(120, 22);
            this.nudGraphX.TabIndex = 20;
            this.nudGraphX.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // frmGraph
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.DarkRed;
            this.ClientSize = new System.Drawing.Size(905, 579);
            this.Controls.Add(this.gbx_Settings);
            this.Controls.Add(this.zg1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "frmGraph";
            this.Text = "Grafiek";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmGraph_FormClosing);
            this.VisibleChanged += new System.EventHandler(this.frmGraph_VisibleChanged);
            this.Resize += new System.EventHandler(this.frmGraph_Resize);
            this.gbx_Settings.ResumeLayout(false);
            this.gbx_Settings.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.nudGraphY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGraphX)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private ZedGraph.ZedGraphControl zg1;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.GroupBox gbx_Settings;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.NumericUpDown nudGraphY;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown nudGraphX;
    }
}