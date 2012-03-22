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
            this.zg1 = new ZedGraph.ZedGraphControl();
            this.nudGraphY = new System.Windows.Forms.NumericUpDown();
            this.nudGraphX = new System.Windows.Forms.NumericUpDown();
            ((System.ComponentModel.ISupportInitialize)(this.nudGraphY)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGraphX)).BeginInit();
            this.SuspendLayout();
            // 
            // zg1
            // 
            this.zg1.IsAutoScrollRange = true;
            this.zg1.IsScrollY2 = true;
            this.zg1.Location = new System.Drawing.Point(19, 12);
            this.zg1.Name = "zg1";
            this.zg1.ScrollGrace = 0D;
            this.zg1.ScrollMaxX = 0D;
            this.zg1.ScrollMaxY = 0D;
            this.zg1.ScrollMaxY2 = 0D;
            this.zg1.ScrollMinX = 0D;
            this.zg1.ScrollMinY = 0D;
            this.zg1.ScrollMinY2 = 0D;
            this.zg1.Size = new System.Drawing.Size(594, 332);
            this.zg1.TabIndex = 13;
            this.zg1.ZoomStepFraction = 1D;
            // 
            // nudGraphY
            // 
            this.nudGraphY.Location = new System.Drawing.Point(19, 367);
            this.nudGraphY.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudGraphY.Name = "nudGraphY";
            this.nudGraphY.Size = new System.Drawing.Size(120, 20);
            this.nudGraphY.TabIndex = 14;
            this.nudGraphY.Value = new decimal(new int[] {
            2,
            0,
            0,
            0});
            // 
            // nudGraphX
            // 
            this.nudGraphX.Location = new System.Drawing.Point(177, 367);
            this.nudGraphX.Maximum = new decimal(new int[] {
            1000000,
            0,
            0,
            0});
            this.nudGraphX.Name = "nudGraphX";
            this.nudGraphX.Size = new System.Drawing.Size(120, 20);
            this.nudGraphX.TabIndex = 15;
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
            this.ClientSize = new System.Drawing.Size(625, 465);
            this.Controls.Add(this.nudGraphX);
            this.Controls.Add(this.nudGraphY);
            this.Controls.Add(this.zg1);
            this.Name = "frmGraph";
            this.Text = "frmGraph";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.frmGraph_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.nudGraphY)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGraphX)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private ZedGraph.ZedGraphControl zg1;
        private System.Windows.Forms.NumericUpDown nudGraphY;
        private System.Windows.Forms.NumericUpDown nudGraphX;
    }
}