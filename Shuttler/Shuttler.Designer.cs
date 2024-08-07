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
            this.lbRunningTransits = new System.Windows.Forms.ListBox();
            this.label6 = new System.Windows.Forms.Label();
            this.lblActiveTransitID = new System.Windows.Forms.Label();
            this.lblActiveTransitName = new System.Windows.Forms.Label();
            this.lblSignalAspect = new System.Windows.Forms.Label();
            this.lblSignalReason = new System.Windows.Forms.Label();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.lblSpeedReason = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(926, 20);
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
            this.btnStart.Location = new System.Drawing.Point(1036, 20);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(1036, 57);
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
            this.cbAvailableTransits.Location = new System.Drawing.Point(147, 41);
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
            this.label4.Location = new System.Drawing.Point(144, 20);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Availeble transits";
            // 
            // btnTransitsReload
            // 
            this.btnTransitsReload.Location = new System.Drawing.Point(278, 15);
            this.btnTransitsReload.Name = "btnTransitsReload";
            this.btnTransitsReload.Size = new System.Drawing.Size(75, 23);
            this.btnTransitsReload.TabIndex = 13;
            this.btnTransitsReload.Text = "Reload";
            this.btnTransitsReload.UseVisualStyleBackColor = true;
            this.btnTransitsReload.Click += new System.EventHandler(this.btnTransitsReload_Click);
            // 
            // btnStartTransit
            // 
            this.btnStartTransit.Location = new System.Drawing.Point(244, 118);
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
            this.cbTransitTrainDirection.Location = new System.Drawing.Point(147, 91);
            this.cbTransitTrainDirection.Name = "cbTransitTrainDirection";
            this.cbTransitTrainDirection.Size = new System.Drawing.Size(205, 21);
            this.cbTransitTrainDirection.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(147, 69);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Train Direction";
            // 
            // lbOutput
            // 
            this.lbOutput.FormattingEnabled = true;
            this.lbOutput.Location = new System.Drawing.Point(12, 234);
            this.lbOutput.Name = "lbOutput";
            this.lbOutput.Size = new System.Drawing.Size(1124, 199);
            this.lbOutput.TabIndex = 17;
            // 
            // lbRunningTransits
            // 
            this.lbRunningTransits.FormattingEnabled = true;
            this.lbRunningTransits.Location = new System.Drawing.Point(1142, 39);
            this.lbRunningTransits.Name = "lbRunningTransits";
            this.lbRunningTransits.Size = new System.Drawing.Size(152, 394);
            this.lbRunningTransits.TabIndex = 18;
            this.lbRunningTransits.SelectedIndexChanged += new System.EventHandler(this.lbRunningTransits_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(1142, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Active transits";
            // 
            // lblActiveTransitID
            // 
            this.lblActiveTransitID.AutoSize = true;
            this.lblActiveTransitID.Location = new System.Drawing.Point(24, 99);
            this.lblActiveTransitID.Name = "lblActiveTransitID";
            this.lblActiveTransitID.Size = new System.Drawing.Size(18, 13);
            this.lblActiveTransitID.TabIndex = 20;
            this.lblActiveTransitID.Text = "ID";
            // 
            // lblActiveTransitName
            // 
            this.lblActiveTransitName.AutoSize = true;
            this.lblActiveTransitName.Location = new System.Drawing.Point(24, 118);
            this.lblActiveTransitName.Name = "lblActiveTransitName";
            this.lblActiveTransitName.Size = new System.Drawing.Size(44, 13);
            this.lblActiveTransitName.TabIndex = 21;
            this.lblActiveTransitName.Text = "Journey";
            // 
            // lblSignalAspect
            // 
            this.lblSignalAspect.AutoSize = true;
            this.lblSignalAspect.Location = new System.Drawing.Point(24, 160);
            this.lblSignalAspect.Name = "lblSignalAspect";
            this.lblSignalAspect.Size = new System.Drawing.Size(36, 13);
            this.lblSignalAspect.TabIndex = 22;
            this.lblSignalAspect.Text = "Signal";
            // 
            // lblSignalReason
            // 
            this.lblSignalReason.AutoSize = true;
            this.lblSignalReason.Location = new System.Drawing.Point(115, 160);
            this.lblSignalReason.Name = "lblSignalReason";
            this.lblSignalReason.Size = new System.Drawing.Size(71, 13);
            this.lblSignalReason.TabIndex = 23;
            this.lblSignalReason.Text = "Signal reason";
            // 
            // lblSpeed
            // 
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Location = new System.Drawing.Point(29, 212);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(38, 13);
            this.lblSpeed.TabIndex = 24;
            this.lblSpeed.Text = "Speed";
            // 
            // lblSpeedReason
            // 
            this.lblSpeedReason.AutoSize = true;
            this.lblSpeedReason.Location = new System.Drawing.Point(118, 212);
            this.lblSpeedReason.Name = "lblSpeedReason";
            this.lblSpeedReason.Size = new System.Drawing.Size(73, 13);
            this.lblSpeedReason.TabIndex = 25;
            this.lblSpeedReason.Text = "Speed reason";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(24, 196);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(43, 13);
            this.label7.TabIndex = 26;
            this.label7.Text = "Speed";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(24, 141);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 13);
            this.label8.TabIndex = 27;
            this.label8.Text = "Signal";
            // 
            // Shuttler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1904, 450);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.lblSpeedReason);
            this.Controls.Add(this.lblSpeed);
            this.Controls.Add(this.lblSignalReason);
            this.Controls.Add(this.lblSignalAspect);
            this.Controls.Add(this.lblActiveTransitName);
            this.Controls.Add(this.lblActiveTransitID);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lbRunningTransits);
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
        private System.Windows.Forms.ListBox lbRunningTransits;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblActiveTransitID;
        private System.Windows.Forms.Label lblActiveTransitName;
        private System.Windows.Forms.Label lblSignalAspect;
        private System.Windows.Forms.Label lblSignalReason;
        private System.Windows.Forms.Label lblSpeed;
        private System.Windows.Forms.Label lblSpeedReason;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Label label8;
    }
}