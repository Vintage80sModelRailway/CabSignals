namespace JMRIDebugOutput
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.tbConfigLocation = new System.Windows.Forms.TextBox();
            this.btnConfigLocationBrowse = new System.Windows.Forms.Button();
            this.ofConfigFile = new System.Windows.Forms.OpenFileDialog();
            this.btnOpenFiles = new System.Windows.Forms.Button();
            this.tbDispatchPath = new System.Windows.Forms.TextBox();
            this.btnDispatchPath = new System.Windows.Forms.Button();
            this.fbDispatchesPath = new System.Windows.Forms.FolderBrowserDialog();
            this.lbOutput = new System.Windows.Forms.ListBox();
            this.cbDispatches = new System.Windows.Forms.ComboBox();
            this.btnOpenDispatch = new System.Windows.Forms.Button();
            this.tbTransit = new System.Windows.Forms.TextBox();
            this.tbStartBlock = new System.Windows.Forms.TextBox();
            this.lblTransit = new System.Windows.Forms.Label();
            this.lblStartBlock = new System.Windows.Forms.Label();
            this.tbJMRIWebServerIP = new System.Windows.Forms.TextBox();
            this.tbTrainName = new System.Windows.Forms.TextBox();
            this.lblTrainName = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.btnStartJourney = new System.Windows.Forms.Button();
            this.tbWebServerPort = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbCurrentBlock = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.tbNextBlock = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tbCurrentBlockSignalMast = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tbCurrentBlockSignalMastState = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.btnStopJourney = new System.Windows.Forms.Button();
            this.pbSignal = new System.Windows.Forms.PictureBox();
            this.btnMore = new System.Windows.Forms.Button();
            this.lbJourneyLog = new System.Windows.Forms.ListBox();
            this.tbExceptionTrace = new System.Windows.Forms.TextBox();
            this.tbCurrentSection = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.tbSectionIndex = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.lbAssignedBlocks = new System.Windows.Forms.ListBox();
            this.cbUpdateAssignedBlocks = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.pbSignal)).BeginInit();
            this.SuspendLayout();
            // 
            // tbConfigLocation
            // 
            this.tbConfigLocation.Location = new System.Drawing.Point(221, 54);
            this.tbConfigLocation.Name = "tbConfigLocation";
            this.tbConfigLocation.Size = new System.Drawing.Size(308, 20);
            this.tbConfigLocation.TabIndex = 0;
            // 
            // btnConfigLocationBrowse
            // 
            this.btnConfigLocationBrowse.Location = new System.Drawing.Point(549, 54);
            this.btnConfigLocationBrowse.Name = "btnConfigLocationBrowse";
            this.btnConfigLocationBrowse.Size = new System.Drawing.Size(75, 23);
            this.btnConfigLocationBrowse.TabIndex = 1;
            this.btnConfigLocationBrowse.Text = "Browse";
            this.btnConfigLocationBrowse.UseVisualStyleBackColor = true;
            this.btnConfigLocationBrowse.Click += new System.EventHandler(this.btnConfigLocationBrowse_Click);
            // 
            // ofConfigFile
            // 
            this.ofConfigFile.FileName = "openFileDialog1";
            // 
            // btnOpenFiles
            // 
            this.btnOpenFiles.Location = new System.Drawing.Point(802, 15);
            this.btnOpenFiles.Name = "btnOpenFiles";
            this.btnOpenFiles.Size = new System.Drawing.Size(99, 23);
            this.btnOpenFiles.TabIndex = 2;
            this.btnOpenFiles.Text = "Open Files";
            this.btnOpenFiles.UseVisualStyleBackColor = true;
            this.btnOpenFiles.Click += new System.EventHandler(this.btnOpenFiles_Click);
            // 
            // tbDispatchPath
            // 
            this.tbDispatchPath.Location = new System.Drawing.Point(221, 81);
            this.tbDispatchPath.Name = "tbDispatchPath";
            this.tbDispatchPath.Size = new System.Drawing.Size(308, 20);
            this.tbDispatchPath.TabIndex = 3;
            // 
            // btnDispatchPath
            // 
            this.btnDispatchPath.Location = new System.Drawing.Point(549, 79);
            this.btnDispatchPath.Name = "btnDispatchPath";
            this.btnDispatchPath.Size = new System.Drawing.Size(75, 23);
            this.btnDispatchPath.TabIndex = 4;
            this.btnDispatchPath.Text = "Browse";
            this.btnDispatchPath.UseVisualStyleBackColor = true;
            this.btnDispatchPath.Click += new System.EventHandler(this.btnDispatchPath_Click);
            // 
            // lbOutput
            // 
            this.lbOutput.FormattingEnabled = true;
            this.lbOutput.Location = new System.Drawing.Point(156, 255);
            this.lbOutput.Name = "lbOutput";
            this.lbOutput.Size = new System.Drawing.Size(745, 186);
            this.lbOutput.TabIndex = 5;
            // 
            // cbDispatches
            // 
            this.cbDispatches.Enabled = false;
            this.cbDispatches.FormattingEnabled = true;
            this.cbDispatches.Location = new System.Drawing.Point(221, 137);
            this.cbDispatches.Name = "cbDispatches";
            this.cbDispatches.Size = new System.Drawing.Size(308, 21);
            this.cbDispatches.TabIndex = 6;
            // 
            // btnOpenDispatch
            // 
            this.btnOpenDispatch.Enabled = false;
            this.btnOpenDispatch.Location = new System.Drawing.Point(549, 137);
            this.btnOpenDispatch.Name = "btnOpenDispatch";
            this.btnOpenDispatch.Size = new System.Drawing.Size(75, 23);
            this.btnOpenDispatch.TabIndex = 7;
            this.btnOpenDispatch.Text = "Open";
            this.btnOpenDispatch.UseVisualStyleBackColor = true;
            this.btnOpenDispatch.Click += new System.EventHandler(this.btnOpenDispatch_Click);
            // 
            // tbTransit
            // 
            this.tbTransit.Location = new System.Drawing.Point(733, 82);
            this.tbTransit.Name = "tbTransit";
            this.tbTransit.Size = new System.Drawing.Size(168, 20);
            this.tbTransit.TabIndex = 8;
            // 
            // tbStartBlock
            // 
            this.tbStartBlock.Location = new System.Drawing.Point(733, 109);
            this.tbStartBlock.Name = "tbStartBlock";
            this.tbStartBlock.Size = new System.Drawing.Size(168, 20);
            this.tbStartBlock.TabIndex = 9;
            // 
            // lblTransit
            // 
            this.lblTransit.AutoSize = true;
            this.lblTransit.Location = new System.Drawing.Point(692, 85);
            this.lblTransit.Name = "lblTransit";
            this.lblTransit.Size = new System.Drawing.Size(39, 13);
            this.lblTransit.TabIndex = 10;
            this.lblTransit.Text = "Transit";
            // 
            // lblStartBlock
            // 
            this.lblStartBlock.AutoSize = true;
            this.lblStartBlock.Location = new System.Drawing.Point(669, 112);
            this.lblStartBlock.Name = "lblStartBlock";
            this.lblStartBlock.Size = new System.Drawing.Size(58, 13);
            this.lblStartBlock.TabIndex = 11;
            this.lblStartBlock.Text = "Start block";
            // 
            // tbJMRIWebServerIP
            // 
            this.tbJMRIWebServerIP.Location = new System.Drawing.Point(221, 12);
            this.tbJMRIWebServerIP.Name = "tbJMRIWebServerIP";
            this.tbJMRIWebServerIP.Size = new System.Drawing.Size(298, 20);
            this.tbJMRIWebServerIP.TabIndex = 12;
            // 
            // tbTrainName
            // 
            this.tbTrainName.Location = new System.Drawing.Point(733, 53);
            this.tbTrainName.Name = "tbTrainName";
            this.tbTrainName.Size = new System.Drawing.Size(168, 20);
            this.tbTrainName.TabIndex = 13;
            // 
            // lblTrainName
            // 
            this.lblTrainName.AutoSize = true;
            this.lblTrainName.Location = new System.Drawing.Point(696, 56);
            this.lblTrainName.Name = "lblTrainName";
            this.lblTrainName.Size = new System.Drawing.Size(31, 13);
            this.lblTrainName.TabIndex = 14;
            this.lblTrainName.Text = "Train";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(164, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Server IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(149, 57);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 16;
            this.label2.Text = "JMRI cfg file";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(142, 85);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 17;
            this.label3.Text = "Dispatch path";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(166, 140);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(49, 13);
            this.label4.TabIndex = 18;
            this.label4.Text = "Dispatch";
            // 
            // btnStartJourney
            // 
            this.btnStartJourney.Enabled = false;
            this.btnStartJourney.Location = new System.Drawing.Point(733, 135);
            this.btnStartJourney.Name = "btnStartJourney";
            this.btnStartJourney.Size = new System.Drawing.Size(168, 23);
            this.btnStartJourney.TabIndex = 19;
            this.btnStartJourney.Text = "Start Journey";
            this.btnStartJourney.UseVisualStyleBackColor = true;
            this.btnStartJourney.Click += new System.EventHandler(this.btnStartJourney_Click);
            // 
            // tbWebServerPort
            // 
            this.tbWebServerPort.Location = new System.Drawing.Point(557, 12);
            this.tbWebServerPort.Name = "tbWebServerPort";
            this.tbWebServerPort.Size = new System.Drawing.Size(67, 20);
            this.tbWebServerPort.TabIndex = 20;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(525, 15);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(26, 13);
            this.label5.TabIndex = 21;
            this.label5.Text = "Port";
            // 
            // tbCurrentBlock
            // 
            this.tbCurrentBlock.Location = new System.Drawing.Point(221, 194);
            this.tbCurrentBlock.Name = "tbCurrentBlock";
            this.tbCurrentBlock.Size = new System.Drawing.Size(221, 20);
            this.tbCurrentBlock.TabIndex = 22;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(144, 197);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 13);
            this.label6.TabIndex = 23;
            this.label6.Text = "Current Block";
            // 
            // tbNextBlock
            // 
            this.tbNextBlock.Location = new System.Drawing.Point(221, 220);
            this.tbNextBlock.Name = "tbNextBlock";
            this.tbNextBlock.Size = new System.Drawing.Size(221, 20);
            this.tbNextBlock.TabIndex = 24;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(156, 225);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(59, 13);
            this.label7.TabIndex = 25;
            this.label7.Text = "Next Block";
            // 
            // tbCurrentBlockSignalMast
            // 
            this.tbCurrentBlockSignalMast.Location = new System.Drawing.Point(590, 197);
            this.tbCurrentBlockSignalMast.Name = "tbCurrentBlockSignalMast";
            this.tbCurrentBlockSignalMast.Size = new System.Drawing.Size(168, 20);
            this.tbCurrentBlockSignalMast.TabIndex = 26;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(455, 200);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(129, 13);
            this.label8.TabIndex = 27;
            this.label8.Text = "Current Block Signal Mast";
            // 
            // tbCurrentBlockSignalMastState
            // 
            this.tbCurrentBlockSignalMastState.Location = new System.Drawing.Point(802, 197);
            this.tbCurrentBlockSignalMastState.Name = "tbCurrentBlockSignalMastState";
            this.tbCurrentBlockSignalMastState.Size = new System.Drawing.Size(100, 20);
            this.tbCurrentBlockSignalMastState.TabIndex = 28;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(764, 201);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(32, 13);
            this.label9.TabIndex = 29;
            this.label9.Text = "State";
            // 
            // btnStopJourney
            // 
            this.btnStopJourney.Enabled = false;
            this.btnStopJourney.Location = new System.Drawing.Point(767, 225);
            this.btnStopJourney.Name = "btnStopJourney";
            this.btnStopJourney.Size = new System.Drawing.Size(135, 23);
            this.btnStopJourney.TabIndex = 30;
            this.btnStopJourney.Text = "Stop Journey";
            this.btnStopJourney.UseVisualStyleBackColor = true;
            this.btnStopJourney.Click += new System.EventHandler(this.btnStopJourney_Click);
            // 
            // pbSignal
            // 
            this.pbSignal.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.pbSignal.Location = new System.Drawing.Point(34, 27);
            this.pbSignal.Name = "pbSignal";
            this.pbSignal.Size = new System.Drawing.Size(59, 356);
            this.pbSignal.TabIndex = 32;
            this.pbSignal.TabStop = false;
            // 
            // btnMore
            // 
            this.btnMore.Location = new System.Drawing.Point(34, 406);
            this.btnMore.Name = "btnMore";
            this.btnMore.Size = new System.Drawing.Size(59, 23);
            this.btnMore.TabIndex = 33;
            this.btnMore.Text = ">>";
            this.btnMore.UseVisualStyleBackColor = true;
            this.btnMore.Click += new System.EventHandler(this.btnMore_Click);
            // 
            // lbJourneyLog
            // 
            this.lbJourneyLog.FormattingEnabled = true;
            this.lbJourneyLog.Location = new System.Drawing.Point(926, 15);
            this.lbJourneyLog.Name = "lbJourneyLog";
            this.lbJourneyLog.Size = new System.Drawing.Size(190, 420);
            this.lbJourneyLog.TabIndex = 34;
            // 
            // tbExceptionTrace
            // 
            this.tbExceptionTrace.Location = new System.Drawing.Point(1309, 40);
            this.tbExceptionTrace.Multiline = true;
            this.tbExceptionTrace.Name = "tbExceptionTrace";
            this.tbExceptionTrace.Size = new System.Drawing.Size(149, 395);
            this.tbExceptionTrace.TabIndex = 35;
            // 
            // tbCurrentSection
            // 
            this.tbCurrentSection.Location = new System.Drawing.Point(549, 220);
            this.tbCurrentSection.Name = "tbCurrentSection";
            this.tbCurrentSection.Size = new System.Drawing.Size(100, 20);
            this.tbCurrentSection.TabIndex = 36;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(465, 223);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(78, 13);
            this.label10.TabIndex = 37;
            this.label10.Text = "Current section";
            // 
            // tbSectionIndex
            // 
            this.tbSectionIndex.Location = new System.Drawing.Point(719, 220);
            this.tbSectionIndex.Name = "tbSectionIndex";
            this.tbSectionIndex.Size = new System.Drawing.Size(39, 20);
            this.tbSectionIndex.TabIndex = 38;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(672, 225);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(33, 13);
            this.label11.TabIndex = 39;
            this.label11.Text = "Index";
            // 
            // lbAssignedBlocks
            // 
            this.lbAssignedBlocks.FormattingEnabled = true;
            this.lbAssignedBlocks.Location = new System.Drawing.Point(1122, 15);
            this.lbAssignedBlocks.Name = "lbAssignedBlocks";
            this.lbAssignedBlocks.Size = new System.Drawing.Size(181, 420);
            this.lbAssignedBlocks.TabIndex = 40;
            // 
            // cbUpdateAssignedBlocks
            // 
            this.cbUpdateAssignedBlocks.AutoSize = true;
            this.cbUpdateAssignedBlocks.BackColor = System.Drawing.Color.GhostWhite;
            this.cbUpdateAssignedBlocks.Checked = true;
            this.cbUpdateAssignedBlocks.CheckState = System.Windows.Forms.CheckState.Checked;
            this.cbUpdateAssignedBlocks.Location = new System.Drawing.Point(1386, 17);
            this.cbUpdateAssignedBlocks.Name = "cbUpdateAssignedBlocks";
            this.cbUpdateAssignedBlocks.Size = new System.Drawing.Size(61, 17);
            this.cbUpdateAssignedBlocks.TabIndex = 41;
            this.cbUpdateAssignedBlocks.Text = "Update";
            this.cbUpdateAssignedBlocks.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.cbUpdateAssignedBlocks.UseVisualStyleBackColor = false;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1461, 450);
            this.Controls.Add(this.cbUpdateAssignedBlocks);
            this.Controls.Add(this.lbAssignedBlocks);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.tbSectionIndex);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.tbCurrentSection);
            this.Controls.Add(this.tbExceptionTrace);
            this.Controls.Add(this.lbJourneyLog);
            this.Controls.Add(this.btnMore);
            this.Controls.Add(this.pbSignal);
            this.Controls.Add(this.btnStopJourney);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.tbCurrentBlockSignalMastState);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.tbCurrentBlockSignalMast);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.tbNextBlock);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.tbCurrentBlock);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbWebServerPort);
            this.Controls.Add(this.btnStartJourney);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblTrainName);
            this.Controls.Add(this.tbTrainName);
            this.Controls.Add(this.tbJMRIWebServerIP);
            this.Controls.Add(this.lblStartBlock);
            this.Controls.Add(this.lblTransit);
            this.Controls.Add(this.tbStartBlock);
            this.Controls.Add(this.tbTransit);
            this.Controls.Add(this.btnOpenDispatch);
            this.Controls.Add(this.cbDispatches);
            this.Controls.Add(this.lbOutput);
            this.Controls.Add(this.btnDispatchPath);
            this.Controls.Add(this.tbDispatchPath);
            this.Controls.Add(this.btnOpenFiles);
            this.Controls.Add(this.btnConfigLocationBrowse);
            this.Controls.Add(this.tbConfigLocation);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Form1";
            this.Text = "Cab Signals";
            ((System.ComponentModel.ISupportInitialize)(this.pbSignal)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox tbConfigLocation;
        private System.Windows.Forms.Button btnConfigLocationBrowse;
        private System.Windows.Forms.OpenFileDialog ofConfigFile;
        private System.Windows.Forms.Button btnOpenFiles;
        private System.Windows.Forms.TextBox tbDispatchPath;
        private System.Windows.Forms.Button btnDispatchPath;
        private System.Windows.Forms.FolderBrowserDialog fbDispatchesPath;
        private System.Windows.Forms.ListBox lbOutput;
        private System.Windows.Forms.ComboBox cbDispatches;
        private System.Windows.Forms.Button btnOpenDispatch;
        private System.Windows.Forms.TextBox tbTransit;
        private System.Windows.Forms.TextBox tbStartBlock;
        private System.Windows.Forms.Label lblTransit;
        private System.Windows.Forms.Label lblStartBlock;
        private System.Windows.Forms.TextBox tbJMRIWebServerIP;
        private System.Windows.Forms.TextBox tbTrainName;
        private System.Windows.Forms.Label lblTrainName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btnStartJourney;
        private System.Windows.Forms.TextBox tbWebServerPort;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbCurrentBlock;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox tbNextBlock;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox tbCurrentBlockSignalMast;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox tbCurrentBlockSignalMastState;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button btnStopJourney;
        private System.Windows.Forms.PictureBox pbSignal;
        private System.Windows.Forms.Button btnMore;
        private System.Windows.Forms.ListBox lbJourneyLog;
        private System.Windows.Forms.TextBox tbExceptionTrace;
        private System.Windows.Forms.TextBox tbCurrentSection;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.TextBox tbSectionIndex;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ListBox lbAssignedBlocks;
        protected System.Windows.Forms.CheckBox cbUpdateAssignedBlocks;
    }
}

