namespace Shuttler
{
    partial class Shuttler
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
            this.btnTest = new System.Windows.Forms.Button();
            this.lbRoster = new System.Windows.Forms.ListBox();
            this.btnStart = new System.Windows.Forms.Button();
            this.btnStop = new System.Windows.Forms.Button();
            this.lbStartBlocks = new System.Windows.Forms.ListBox();
            this.lbDestinationBlocks = new System.Windows.Forms.ListBox();
            this.cbAvailableTransits = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.btnReloadStartBlocks = new System.Windows.Forms.Button();
            this.btnReloadDestBlocks = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.btnTransitsReload = new System.Windows.Forms.Button();
            this.btnStartTransit = new System.Windows.Forms.Button();
            this.cbTransitTrainDirection = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.lbOutput = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(661, 29);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 0;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // lbRoster
            // 
            this.lbRoster.FormattingEnabled = true;
            this.lbRoster.Location = new System.Drawing.Point(1690, 39);
            this.lbRoster.Name = "lbRoster";
            this.lbRoster.Size = new System.Drawing.Size(202, 394);
            this.lbRoster.TabIndex = 1;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(1219, 13);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(1219, 54);
            this.btnStop.Name = "btnStop";
            this.btnStop.Size = new System.Drawing.Size(75, 23);
            this.btnStop.TabIndex = 3;
            this.btnStop.Text = "Stop";
            this.btnStop.UseVisualStyleBackColor = true;
            this.btnStop.Click += new System.EventHandler(this.btnStop_Click);
            // 
            // lbStartBlocks
            // 
            this.lbStartBlocks.FormattingEnabled = true;
            this.lbStartBlocks.Location = new System.Drawing.Point(1300, 39);
            this.lbStartBlocks.Name = "lbStartBlocks";
            this.lbStartBlocks.Size = new System.Drawing.Size(192, 394);
            this.lbStartBlocks.TabIndex = 4;
            // 
            // lbDestinationBlocks
            // 
            this.lbDestinationBlocks.FormattingEnabled = true;
            this.lbDestinationBlocks.Location = new System.Drawing.Point(1499, 39);
            this.lbDestinationBlocks.Name = "lbDestinationBlocks";
            this.lbDestinationBlocks.Size = new System.Drawing.Size(185, 394);
            this.lbDestinationBlocks.TabIndex = 5;
            // 
            // cbAvailableTransits
            // 
            this.cbAvailableTransits.FormattingEnabled = true;
            this.cbAvailableTransits.Location = new System.Drawing.Point(1088, 125);
            this.cbAvailableTransits.Name = "cbAvailableTransits";
            this.cbAvailableTransits.Size = new System.Drawing.Size(206, 21);
            this.cbAvailableTransits.TabIndex = 6;
            this.cbAvailableTransits.SelectedIndexChanged += new System.EventHandler(this.cbAvailableTransits_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1300, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Start blocks";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1499, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Destination blocks";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1690, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Roster";
            // 
            // btnReloadStartBlocks
            // 
            this.btnReloadStartBlocks.Location = new System.Drawing.Point(1417, 13);
            this.btnReloadStartBlocks.Name = "btnReloadStartBlocks";
            this.btnReloadStartBlocks.Size = new System.Drawing.Size(75, 23);
            this.btnReloadStartBlocks.TabIndex = 10;
            this.btnReloadStartBlocks.Text = "Reload";
            this.btnReloadStartBlocks.UseVisualStyleBackColor = true;
            this.btnReloadStartBlocks.Click += new System.EventHandler(this.btnReloadStartBlocks_Click);
            // 
            // btnReloadDestBlocks
            // 
            this.btnReloadDestBlocks.Location = new System.Drawing.Point(1609, 13);
            this.btnReloadDestBlocks.Name = "btnReloadDestBlocks";
            this.btnReloadDestBlocks.Size = new System.Drawing.Size(75, 23);
            this.btnReloadDestBlocks.TabIndex = 11;
            this.btnReloadDestBlocks.Text = "Reload";
            this.btnReloadDestBlocks.UseVisualStyleBackColor = true;
            this.btnReloadDestBlocks.Click += new System.EventHandler(this.btnReloadDestBlocks_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1085, 104);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Availeble transits";
            // 
            // btnTransitsReload
            // 
            this.btnTransitsReload.Location = new System.Drawing.Point(1219, 99);
            this.btnTransitsReload.Name = "btnTransitsReload";
            this.btnTransitsReload.Size = new System.Drawing.Size(75, 23);
            this.btnTransitsReload.TabIndex = 13;
            this.btnTransitsReload.Text = "Reload";
            this.btnTransitsReload.UseVisualStyleBackColor = true;
            this.btnTransitsReload.Click += new System.EventHandler(this.btnTransitsReload_Click);
            // 
            // btnStartTransit
            // 
            this.btnStartTransit.Location = new System.Drawing.Point(1185, 202);
            this.btnStartTransit.Name = "btnStartTransit";
            this.btnStartTransit.Size = new System.Drawing.Size(109, 23);
            this.btnStartTransit.TabIndex = 14;
            this.btnStartTransit.Text = "Start transit";
            this.btnStartTransit.UseVisualStyleBackColor = true;
            this.btnStartTransit.Click += new System.EventHandler(this.btnStartTransit_Click);
            // 
            // cbTransitTrainDirection
            // 
            this.cbTransitTrainDirection.FormattingEnabled = true;
            this.cbTransitTrainDirection.Items.AddRange(new object[] {
            "Forward",
            "Reverse"});
            this.cbTransitTrainDirection.Location = new System.Drawing.Point(1088, 175);
            this.cbTransitTrainDirection.Name = "cbTransitTrainDirection";
            this.cbTransitTrainDirection.Size = new System.Drawing.Size(205, 21);
            this.cbTransitTrainDirection.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1088, 153);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Train Direction";
            // 
            // lbOutput
            // 
            this.lbOutput.FormattingEnabled = true;
            this.lbOutput.Location = new System.Drawing.Point(630, 231);
            this.lbOutput.Name = "lbOutput";
            this.lbOutput.Size = new System.Drawing.Size(664, 199);
            this.lbOutput.TabIndex = 17;
            // 
            // Shuttler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1904, 450);
            this.Controls.Add(this.lbOutput);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.cbTransitTrainDirection);
            this.Controls.Add(this.btnStartTransit);
            this.Controls.Add(this.btnTransitsReload);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnReloadDestBlocks);
            this.Controls.Add(this.btnReloadStartBlocks);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.cbAvailableTransits);
            this.Controls.Add(this.lbDestinationBlocks);
            this.Controls.Add(this.lbStartBlocks);
            this.Controls.Add(this.btnStop);
            this.Controls.Add(this.btnStart);
            this.Controls.Add(this.lbRoster);
            this.Controls.Add(this.btnTest);
            this.Name = "Shuttler";
            this.Text = "Shuttler";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.ListBox lbRoster;
        private System.Windows.Forms.Button btnStart;
        private System.Windows.Forms.Button btnStop;
        private System.Windows.Forms.ListBox lbStartBlocks;
        private System.Windows.Forms.ListBox lbDestinationBlocks;
        private System.Windows.Forms.ComboBox cbAvailableTransits;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnReloadStartBlocks;
        private System.Windows.Forms.Button btnReloadDestBlocks;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnTransitsReload;
        private System.Windows.Forms.Button btnStartTransit;
        private System.Windows.Forms.ComboBox cbTransitTrainDirection;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ListBox lbOutput;
    }
}