namespace WindowsFormsApplication1
{
    partial class Form1
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
            this.connectButton = new System.Windows.Forms.Button();
            this.listBox = new System.Windows.Forms.ListBox();
            this.comPortsList = new System.Windows.Forms.ListBox();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.UpDown = new System.Windows.Forms.NumericUpDown();
            this.save = new System.Windows.Forms.Button();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.zg1 = new ZedGraph.ZedGraphControl();
            this.startButton = new System.Windows.Forms.Button();
            this.cbxGrafiekAanUit = new System.Windows.Forms.CheckBox();
            this.btnReset = new System.Windows.Forms.Button();
            this.nudVertraging = new System.Windows.Forms.NumericUpDown();
            this.nudGrafiekYmax = new System.Windows.Forms.NumericUpDown();
            this.nudGrafiekXmax = new System.Windows.Forms.NumericUpDown();
            this.lblGrafiekYmax = new System.Windows.Forms.Label();
            this.lblGrafiekXmax = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.tstlbl_waarschuwing = new System.Windows.Forms.ToolStripStatusLabel();
            this.tstlb_BaudRate = new System.Windows.Forms.ToolStripStatusLabel();
            ((System.ComponentModel.ISupportInitialize)(this.UpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudVertraging)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGrafiekYmax)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGrafiekXmax)).BeginInit();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // connectButton
            // 
            this.connectButton.Location = new System.Drawing.Point(7, 87);
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(101, 42);
            this.connectButton.TabIndex = 0;
            this.connectButton.Text = "Connect";
            this.connectButton.UseVisualStyleBackColor = true;
            this.connectButton.Click += new System.EventHandler(this.connect_Click);
            // 
            // listBox
            // 
            this.listBox.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listBox.FormattingEnabled = true;
            this.listBox.HorizontalScrollbar = true;
            this.listBox.ItemHeight = 14;
            this.listBox.Location = new System.Drawing.Point(114, 12);
            this.listBox.Name = "listBox";
            this.listBox.ScrollAlwaysVisible = true;
            this.listBox.Size = new System.Drawing.Size(118, 466);
            this.listBox.TabIndex = 1;
            // 
            // comPortsList
            // 
            this.comPortsList.FormattingEnabled = true;
            this.comPortsList.Location = new System.Drawing.Point(7, 12);
            this.comPortsList.Name = "comPortsList";
            this.comPortsList.Size = new System.Drawing.Size(101, 69);
            this.comPortsList.TabIndex = 2;
            this.comPortsList.SelectedValueChanged += new System.EventHandler(this.comPortsList_SelectedValueChanged);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(8, 135);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(100, 20);
            this.progressBar.TabIndex = 4;
            // 
            // UpDown
            // 
            this.UpDown.Increment = new decimal(new int[] {
            200,
            0,
            0,
            0});
            this.UpDown.Location = new System.Drawing.Point(8, 206);
            this.UpDown.Maximum = new decimal(new int[] {
            2000,
            0,
            0,
            0});
            this.UpDown.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.UpDown.Name = "UpDown";
            this.UpDown.Size = new System.Drawing.Size(69, 20);
            this.UpDown.TabIndex = 6;
            this.UpDown.Value = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.UpDown.ValueChanged += new System.EventHandler(this.UpDown_ValueChanged);
            // 
            // save
            // 
            this.save.Location = new System.Drawing.Point(7, 232);
            this.save.Name = "save";
            this.save.Size = new System.Drawing.Size(101, 44);
            this.save.TabIndex = 7;
            this.save.Text = "Save";
            this.save.UseVisualStyleBackColor = true;
            this.save.Click += new System.EventHandler(this.save_Click);
            // 
            // zg1
            // 
            this.zg1.IsAutoScrollRange = true;
            this.zg1.IsScrollY2 = true;
            this.zg1.Location = new System.Drawing.Point(238, 12);
            this.zg1.Name = "zg1";
            this.zg1.ScrollGrace = 0D;
            this.zg1.ScrollMaxX = 0D;
            this.zg1.ScrollMaxY = 0D;
            this.zg1.ScrollMaxY2 = 0D;
            this.zg1.ScrollMinX = 0D;
            this.zg1.ScrollMinY = 0D;
            this.zg1.ScrollMinY2 = 0D;
            this.zg1.Size = new System.Drawing.Size(989, 473);
            this.zg1.TabIndex = 12;
            this.zg1.ZoomStepFraction = 1D;
            // 
            // startButton
            // 
            this.startButton.Location = new System.Drawing.Point(8, 282);
            this.startButton.Name = "startButton";
            this.startButton.Size = new System.Drawing.Size(101, 42);
            this.startButton.TabIndex = 13;
            this.startButton.Text = "Start";
            this.startButton.UseVisualStyleBackColor = true;
            this.startButton.Click += new System.EventHandler(this.startButton_Click);
            // 
            // cbxGrafiekAanUit
            // 
            this.cbxGrafiekAanUit.AutoSize = true;
            this.cbxGrafiekAanUit.Checked = true;
            this.cbxGrafiekAanUit.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbxGrafiekAanUit.Location = new System.Drawing.Point(7, 330);
            this.cbxGrafiekAanUit.Name = "cbxGrafiekAanUit";
            this.cbxGrafiekAanUit.Size = new System.Drawing.Size(81, 17);
            this.cbxGrafiekAanUit.TabIndex = 14;
            this.cbxGrafiekAanUit.Text = "Grafiek aan";
            this.cbxGrafiekAanUit.UseVisualStyleBackColor = true;
            // 
            // btnReset
            // 
            this.btnReset.Location = new System.Drawing.Point(7, 161);
            this.btnReset.Name = "btnReset";
            this.btnReset.Size = new System.Drawing.Size(101, 39);
            this.btnReset.TabIndex = 15;
            this.btnReset.Text = "Reset";
            this.btnReset.UseVisualStyleBackColor = true;
            this.btnReset.Click += new System.EventHandler(this.btnReset_Click);
            // 
            // nudVertraging
            // 
            this.nudVertraging.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudVertraging.Location = new System.Drawing.Point(7, 378);
            this.nudVertraging.Maximum = new decimal(new int[] {
            550,
            0,
            0,
            0});
            this.nudVertraging.Minimum = new decimal(new int[] {
            20,
            0,
            0,
            0});
            this.nudVertraging.Name = "nudVertraging";
            this.nudVertraging.Size = new System.Drawing.Size(101, 20);
            this.nudVertraging.TabIndex = 16;
            this.nudVertraging.Value = new decimal(new int[] {
            100,
            0,
            0,
            0});
            // 
            // nudGrafiekYmax
            // 
            this.nudGrafiekYmax.Increment = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.nudGrafiekYmax.Location = new System.Drawing.Point(7, 421);
            this.nudGrafiekYmax.Maximum = new decimal(new int[] {
            66000,
            0,
            0,
            0});
            this.nudGrafiekYmax.Minimum = new decimal(new int[] {
            25,
            0,
            0,
            0});
            this.nudGrafiekYmax.Name = "nudGrafiekYmax";
            this.nudGrafiekYmax.Size = new System.Drawing.Size(101, 20);
            this.nudGrafiekYmax.TabIndex = 17;
            this.nudGrafiekYmax.Value = new decimal(new int[] {
            65999,
            0,
            0,
            0});
            // 
            // nudGrafiekXmax
            // 
            this.nudGrafiekXmax.Increment = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudGrafiekXmax.Location = new System.Drawing.Point(7, 465);
            this.nudGrafiekXmax.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.nudGrafiekXmax.Minimum = new decimal(new int[] {
            5,
            0,
            0,
            0});
            this.nudGrafiekXmax.Name = "nudGrafiekXmax";
            this.nudGrafiekXmax.Size = new System.Drawing.Size(101, 20);
            this.nudGrafiekXmax.TabIndex = 18;
            this.nudGrafiekXmax.Value = new decimal(new int[] {
            400,
            0,
            0,
            0});
            // 
            // lblGrafiekYmax
            // 
            this.lblGrafiekYmax.AutoSize = true;
            this.lblGrafiekYmax.Location = new System.Drawing.Point(7, 405);
            this.lblGrafiekYmax.Name = "lblGrafiekYmax";
            this.lblGrafiekYmax.Size = new System.Drawing.Size(70, 13);
            this.lblGrafiekYmax.TabIndex = 19;
            this.lblGrafiekYmax.Text = "Grafiek Ymax";
            // 
            // lblGrafiekXmax
            // 
            this.lblGrafiekXmax.AutoSize = true;
            this.lblGrafiekXmax.Location = new System.Drawing.Point(7, 449);
            this.lblGrafiekXmax.Name = "lblGrafiekXmax";
            this.lblGrafiekXmax.Size = new System.Drawing.Size(70, 13);
            this.lblGrafiekXmax.TabIndex = 20;
            this.lblGrafiekXmax.Text = "Grafiek Xmax";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 359);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(83, 13);
            this.label1.TabIndex = 21;
            this.label1.Text = "Snelheid grafiek";
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tstlbl_waarschuwing,
            this.tstlb_BaudRate});
            this.statusStrip1.Location = new System.Drawing.Point(0, 497);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1247, 22);
            this.statusStrip1.TabIndex = 22;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // tstlbl_waarschuwing
            // 
            this.tstlbl_waarschuwing.Font = new System.Drawing.Font("Tahoma", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tstlbl_waarschuwing.ForeColor = System.Drawing.Color.Red;
            this.tstlbl_waarschuwing.Name = "tstlbl_waarschuwing";
            this.tstlbl_waarschuwing.Size = new System.Drawing.Size(0, 17);
            // 
            // tstlb_BaudRate
            // 
            this.tstlb_BaudRate.Name = "tstlb_BaudRate";
            this.tstlb_BaudRate.Size = new System.Drawing.Size(58, 17);
            this.tstlb_BaudRate.Text = "Baudrate: ";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1247, 519);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblGrafiekXmax);
            this.Controls.Add(this.lblGrafiekYmax);
            this.Controls.Add(this.nudGrafiekXmax);
            this.Controls.Add(this.nudGrafiekYmax);
            this.Controls.Add(this.nudVertraging);
            this.Controls.Add(this.btnReset);
            this.Controls.Add(this.cbxGrafiekAanUit);
            this.Controls.Add(this.startButton);
            this.Controls.Add(this.zg1);
            this.Controls.Add(this.save);
            this.Controls.Add(this.UpDown);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.comPortsList);
            this.Controls.Add(this.listBox);
            this.Controls.Add(this.connectButton);
            this.Name = "Form1";
            this.Text = "Test Capacitieve Knoppen";
            this.Load += new System.EventHandler(this.Form1_Load);
            ((System.ComponentModel.ISupportInitialize)(this.UpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudVertraging)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGrafiekYmax)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.nudGrafiekXmax)).EndInit();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button connectButton;
        private System.Windows.Forms.ListBox listBox;
        private System.Windows.Forms.ListBox comPortsList;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.NumericUpDown UpDown;
        private System.Windows.Forms.Button save;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private ZedGraph.ZedGraphControl zg1;
        private System.Windows.Forms.Button startButton;
        private System.Windows.Forms.CheckBox cbxGrafiekAanUit;
        private System.Windows.Forms.Button btnReset;
        private System.Windows.Forms.NumericUpDown nudVertraging;
        private System.Windows.Forms.NumericUpDown nudGrafiekYmax;
        private System.Windows.Forms.NumericUpDown nudGrafiekXmax;
        private System.Windows.Forms.Label lblGrafiekYmax;
        private System.Windows.Forms.Label lblGrafiekXmax;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel tstlbl_waarschuwing;
        private System.Windows.Forms.ToolStripStatusLabel tstlb_BaudRate;
    }
}

