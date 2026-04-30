namespace LayoutMonitor
{
    partial class LayoutMonitorForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LayoutMonitorForm));
            this.lbOutput = new System.Windows.Forms.ListBox();
            this.tbServerIP = new System.Windows.Forms.TextBox();
            this.tbServerPort = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnStartMonitoring = new System.Windows.Forms.Button();
            this.lblBlockWarning = new System.Windows.Forms.Label();
            this.tbConfigLocation = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.btnStopMonitoring = new System.Windows.Forms.Button();
            this.btnAcknowledgeAlert = new System.Windows.Forms.Button();
            this.lblLikelyIssue = new System.Windows.Forms.Label();
            this.lvUpdates = new System.Windows.Forms.ListView();
            this.lblBlockContainingDanger = new System.Windows.Forms.Label();
            this.ddlTrainSelector = new System.Windows.Forms.ComboBox();
            this.btnTerminateTrain = new System.Windows.Forms.Button();
            this.btnTrainInfo = new System.Windows.Forms.Button();
            this.btnCancelAllocations = new System.Windows.Forms.Button();
            this.btnRosterTest = new System.Windows.Forms.Button();
            this.lblTrainName = new System.Windows.Forms.Label();
            this.btnClearOutputLog = new System.Windows.Forms.Button();
            this.lbJourneyLog = new System.Windows.Forms.ListBox();
            this.btnClearJourneyListBox = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.tbTrainDCCID = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tbTrainName = new System.Windows.Forms.TextBox();
            this.btnUpdateTrainIDAndName = new System.Windows.Forms.Button();
            this.tbPrevDCCID = new System.Windows.Forms.TextBox();
            this.lblSpeed = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lbOutput
            // 
            this.lbOutput.FormattingEnabled = true;
            this.lbOutput.Location = new System.Drawing.Point(12, 589);
            this.lbOutput.Name = "lbOutput";
            this.lbOutput.Size = new System.Drawing.Size(1688, 147);
            this.lbOutput.TabIndex = 0;
            // 
            // tbServerIP
            // 
            this.tbServerIP.Location = new System.Drawing.Point(1188, 24);
            this.tbServerIP.Name = "tbServerIP";
            this.tbServerIP.Size = new System.Drawing.Size(125, 20);
            this.tbServerIP.TabIndex = 1;
            this.tbServerIP.Visible = false;
            // 
            // tbServerPort
            // 
            this.tbServerPort.Location = new System.Drawing.Point(1213, 50);
            this.tbServerPort.Name = "tbServerPort";
            this.tbServerPort.Size = new System.Drawing.Size(100, 20);
            this.tbServerPort.TabIndex = 2;
            this.tbServerPort.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1131, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Server IP";
            this.label1.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1181, 53);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Port";
            this.label2.Visible = false;
            // 
            // btnStartMonitoring
            // 
            this.btnStartMonitoring.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartMonitoring.Location = new System.Drawing.Point(1374, 18);
            this.btnStartMonitoring.Name = "btnStartMonitoring";
            this.btnStartMonitoring.Size = new System.Drawing.Size(89, 62);
            this.btnStartMonitoring.TabIndex = 5;
            this.btnStartMonitoring.Text = "Start";
            this.btnStartMonitoring.UseVisualStyleBackColor = true;
            this.btnStartMonitoring.Click += new System.EventHandler(this.btnStartMonitoring_Click);
            // 
            // lblBlockWarning
            // 
            this.lblBlockWarning.AutoSize = true;
            this.lblBlockWarning.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.lblBlockWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBlockWarning.Location = new System.Drawing.Point(15, 106);
            this.lblBlockWarning.Name = "lblBlockWarning";
            this.lblBlockWarning.Size = new System.Drawing.Size(283, 46);
            this.lblBlockWarning.TabIndex = 6;
            this.lblBlockWarning.Text = "Block warning";
            // 
            // tbConfigLocation
            // 
            this.tbConfigLocation.Location = new System.Drawing.Point(1045, 76);
            this.tbConfigLocation.Name = "tbConfigLocation";
            this.tbConfigLocation.Size = new System.Drawing.Size(268, 20);
            this.tbConfigLocation.TabIndex = 7;
            this.tbConfigLocation.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1002, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Config";
            this.label3.Visible = false;
            // 
            // btnStopMonitoring
            // 
            this.btnStopMonitoring.BackColor = System.Drawing.Color.Red;
            this.btnStopMonitoring.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStopMonitoring.ForeColor = System.Drawing.Color.White;
            this.btnStopMonitoring.Location = new System.Drawing.Point(1469, 15);
            this.btnStopMonitoring.Name = "btnStopMonitoring";
            this.btnStopMonitoring.Size = new System.Drawing.Size(104, 66);
            this.btnStopMonitoring.TabIndex = 9;
            this.btnStopMonitoring.Text = "Stop";
            this.btnStopMonitoring.UseVisualStyleBackColor = false;
            this.btnStopMonitoring.Click += new System.EventHandler(this.btnStopMonitoring_Click);
            // 
            // btnAcknowledgeAlert
            // 
            this.btnAcknowledgeAlert.BackColor = System.Drawing.Color.LimeGreen;
            this.btnAcknowledgeAlert.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAcknowledgeAlert.ForeColor = System.Drawing.Color.White;
            this.btnAcknowledgeAlert.Location = new System.Drawing.Point(1579, 15);
            this.btnAcknowledgeAlert.Name = "btnAcknowledgeAlert";
            this.btnAcknowledgeAlert.Size = new System.Drawing.Size(121, 66);
            this.btnAcknowledgeAlert.TabIndex = 10;
            this.btnAcknowledgeAlert.Text = "Ack";
            this.btnAcknowledgeAlert.UseVisualStyleBackColor = false;
            this.btnAcknowledgeAlert.Click += new System.EventHandler(this.btnAcknowledgeAlert_Click);
            // 
            // lblLikelyIssue
            // 
            this.lblLikelyIssue.AutoSize = true;
            this.lblLikelyIssue.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.lblLikelyIssue.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLikelyIssue.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
            this.lblLikelyIssue.Location = new System.Drawing.Point(12, 253);
            this.lblLikelyIssue.Name = "lblLikelyIssue";
            this.lblLikelyIssue.Size = new System.Drawing.Size(336, 46);
            this.lblLikelyIssue.TabIndex = 11;
            this.lblLikelyIssue.Text = "Issue description";
            // 
            // lvUpdates
            // 
            this.lvUpdates.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvUpdates.ForeColor = System.Drawing.Color.White;
            this.lvUpdates.HideSelection = false;
            this.lvUpdates.Location = new System.Drawing.Point(12, 332);
            this.lvUpdates.MultiSelect = false;
            this.lvUpdates.Name = "lvUpdates";
            this.lvUpdates.Size = new System.Drawing.Size(1688, 252);
            this.lvUpdates.TabIndex = 12;
            this.lvUpdates.UseCompatibleStateImageBehavior = false;
            this.lvUpdates.View = System.Windows.Forms.View.Details;
            this.lvUpdates.SelectedIndexChanged += new System.EventHandler(this.lvUpdates_SelectedIndexChanged);
            // 
            // lblBlockContainingDanger
            // 
            this.lblBlockContainingDanger.AutoSize = true;
            this.lblBlockContainingDanger.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.lblBlockContainingDanger.Font = new System.Drawing.Font("Microsoft Sans Serif", 30F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBlockContainingDanger.ForeColor = System.Drawing.Color.Black;
            this.lblBlockContainingDanger.Location = new System.Drawing.Point(15, 179);
            this.lblBlockContainingDanger.Name = "lblBlockContainingDanger";
            this.lblBlockContainingDanger.Size = new System.Drawing.Size(125, 46);
            this.lblBlockContainingDanger.TabIndex = 13;
            this.lblBlockContainingDanger.Text = "Block";
            // 
            // ddlTrainSelector
            // 
            this.ddlTrainSelector.Font = new System.Drawing.Font("Microsoft Sans Serif", 15F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ddlTrainSelector.FormattingEnabled = true;
            this.ddlTrainSelector.Location = new System.Drawing.Point(1374, 92);
            this.ddlTrainSelector.Name = "ddlTrainSelector";
            this.ddlTrainSelector.Size = new System.Drawing.Size(321, 33);
            this.ddlTrainSelector.TabIndex = 16;
            this.ddlTrainSelector.SelectedIndexChanged += new System.EventHandler(this.ddlTrainSelector_SelectedIndexChanged);
            // 
            // btnTerminateTrain
            // 
            this.btnTerminateTrain.BackColor = System.Drawing.Color.Red;
            this.btnTerminateTrain.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTerminateTrain.ForeColor = System.Drawing.Color.White;
            this.btnTerminateTrain.Location = new System.Drawing.Point(1529, 131);
            this.btnTerminateTrain.Name = "btnTerminateTrain";
            this.btnTerminateTrain.Size = new System.Drawing.Size(166, 44);
            this.btnTerminateTrain.TabIndex = 17;
            this.btnTerminateTrain.Text = "Terminate";
            this.btnTerminateTrain.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.btnTerminateTrain.UseVisualStyleBackColor = false;
            this.btnTerminateTrain.Click += new System.EventHandler(this.btnTerminateTrain_Click);
            // 
            // btnTrainInfo
            // 
            this.btnTrainInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnTrainInfo.Location = new System.Drawing.Point(1427, 131);
            this.btnTrainInfo.Name = "btnTrainInfo";
            this.btnTrainInfo.Size = new System.Drawing.Size(96, 44);
            this.btnTrainInfo.TabIndex = 18;
            this.btnTrainInfo.Text = "Info";
            this.btnTrainInfo.UseVisualStyleBackColor = true;
            // 
            // btnCancelAllocations
            // 
            this.btnCancelAllocations.BackColor = System.Drawing.Color.Gold;
            this.btnCancelAllocations.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancelAllocations.Location = new System.Drawing.Point(1407, 182);
            this.btnCancelAllocations.Name = "btnCancelAllocations";
            this.btnCancelAllocations.Size = new System.Drawing.Size(288, 43);
            this.btnCancelAllocations.TabIndex = 19;
            this.btnCancelAllocations.Text = "Cancel Allocations";
            this.btnCancelAllocations.UseVisualStyleBackColor = false;
            this.btnCancelAllocations.Click += new System.EventHandler(this.btnCancelAllocations_Click);
            // 
            // btnRosterTest
            // 
            this.btnRosterTest.Location = new System.Drawing.Point(1620, 231);
            this.btnRosterTest.Name = "btnRosterTest";
            this.btnRosterTest.Size = new System.Drawing.Size(75, 23);
            this.btnRosterTest.TabIndex = 20;
            this.btnRosterTest.Text = "RosterTest";
            this.btnRosterTest.UseVisualStyleBackColor = true;
            this.btnRosterTest.Click += new System.EventHandler(this.btnRosterTest_Click);
            // 
            // lblTrainName
            // 
            this.lblTrainName.AutoSize = true;
            this.lblTrainName.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.lblTrainName.Font = new System.Drawing.Font("Microsoft Sans Serif", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrainName.Location = new System.Drawing.Point(12, 14);
            this.lblTrainName.Name = "lblTrainName";
            this.lblTrainName.Size = new System.Drawing.Size(302, 63);
            this.lblTrainName.TabIndex = 21;
            this.lblTrainName.Text = "Loco name";
            // 
            // btnClearOutputLog
            // 
            this.btnClearOutputLog.Location = new System.Drawing.Point(1582, 742);
            this.btnClearOutputLog.Name = "btnClearOutputLog";
            this.btnClearOutputLog.Size = new System.Drawing.Size(75, 23);
            this.btnClearOutputLog.TabIndex = 22;
            this.btnClearOutputLog.Text = "Clear";
            this.btnClearOutputLog.UseVisualStyleBackColor = true;
            this.btnClearOutputLog.Click += new System.EventHandler(this.btnClearOutputLog_Click);
            // 
            // lbJourneyLog
            // 
            this.lbJourneyLog.FormattingEnabled = true;
            this.lbJourneyLog.Location = new System.Drawing.Point(1718, 147);
            this.lbJourneyLog.Name = "lbJourneyLog";
            this.lbJourneyLog.Size = new System.Drawing.Size(174, 589);
            this.lbJourneyLog.TabIndex = 23;
            // 
            // btnClearJourneyListBox
            // 
            this.btnClearJourneyListBox.Location = new System.Drawing.Point(1777, 742);
            this.btnClearJourneyListBox.Name = "btnClearJourneyListBox";
            this.btnClearJourneyListBox.Size = new System.Drawing.Size(75, 23);
            this.btnClearJourneyListBox.TabIndex = 24;
            this.btnClearJourneyListBox.Text = "Clear";
            this.btnClearJourneyListBox.UseVisualStyleBackColor = true;
            this.btnClearJourneyListBox.Click += new System.EventHandler(this.btnClearJourneyListBox_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(1727, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(43, 13);
            this.label4.TabIndex = 26;
            this.label4.Text = "DCC ID";
            // 
            // tbTrainDCCID
            // 
            this.tbTrainDCCID.Location = new System.Drawing.Point(1792, 61);
            this.tbTrainDCCID.Name = "tbTrainDCCID";
            this.tbTrainDCCID.Size = new System.Drawing.Size(100, 20);
            this.tbTrainDCCID.TabIndex = 27;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1727, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(35, 13);
            this.label5.TabIndex = 28;
            this.label5.Text = "Name";
            // 
            // tbTrainName
            // 
            this.tbTrainName.Location = new System.Drawing.Point(1792, 31);
            this.tbTrainName.Name = "tbTrainName";
            this.tbTrainName.Size = new System.Drawing.Size(100, 20);
            this.tbTrainName.TabIndex = 29;
            // 
            // btnUpdateTrainIDAndName
            // 
            this.btnUpdateTrainIDAndName.Location = new System.Drawing.Point(1817, 87);
            this.btnUpdateTrainIDAndName.Name = "btnUpdateTrainIDAndName";
            this.btnUpdateTrainIDAndName.Size = new System.Drawing.Size(75, 23);
            this.btnUpdateTrainIDAndName.TabIndex = 30;
            this.btnUpdateTrainIDAndName.Text = "Update";
            this.btnUpdateTrainIDAndName.UseVisualStyleBackColor = true;
            this.btnUpdateTrainIDAndName.Click += new System.EventHandler(this.btnUpdateTrainIDAndName_Click);
            // 
            // tbPrevDCCID
            // 
            this.tbPrevDCCID.Enabled = false;
            this.tbPrevDCCID.Location = new System.Drawing.Point(1730, 92);
            this.tbPrevDCCID.Name = "tbPrevDCCID";
            this.tbPrevDCCID.Size = new System.Drawing.Size(77, 20);
            this.tbPrevDCCID.TabIndex = 31;
            // 
            // lblSpeed
            // 
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Location = new System.Drawing.Point(1848, 122);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(0, 13);
            this.lblSpeed.TabIndex = 32;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(1804, 122);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(38, 13);
            this.label6.TabIndex = 33;
            this.label6.Text = "Speed";
            // 
            // LayoutMonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1904, 771);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lblSpeed);
            this.Controls.Add(this.tbPrevDCCID);
            this.Controls.Add(this.btnUpdateTrainIDAndName);
            this.Controls.Add(this.tbTrainName);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.tbTrainDCCID);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.btnClearJourneyListBox);
            this.Controls.Add(this.lbJourneyLog);
            this.Controls.Add(this.btnClearOutputLog);
            this.Controls.Add(this.lblTrainName);
            this.Controls.Add(this.btnRosterTest);
            this.Controls.Add(this.btnCancelAllocations);
            this.Controls.Add(this.btnTrainInfo);
            this.Controls.Add(this.btnTerminateTrain);
            this.Controls.Add(this.ddlTrainSelector);
            this.Controls.Add(this.lblBlockContainingDanger);
            this.Controls.Add(this.lvUpdates);
            this.Controls.Add(this.lblLikelyIssue);
            this.Controls.Add(this.btnAcknowledgeAlert);
            this.Controls.Add(this.btnStopMonitoring);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.tbConfigLocation);
            this.Controls.Add(this.lblBlockWarning);
            this.Controls.Add(this.btnStartMonitoring);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.tbServerPort);
            this.Controls.Add(this.tbServerIP);
            this.Controls.Add(this.lbOutput);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LayoutMonitorForm";
            this.Text = "JMRI Danger Early Warning System v0.2";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox lbOutput;
        private System.Windows.Forms.TextBox tbServerIP;
        private System.Windows.Forms.TextBox tbServerPort;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnStartMonitoring;
        private System.Windows.Forms.Label lblBlockWarning;
        private System.Windows.Forms.TextBox tbConfigLocation;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnStopMonitoring;
        private System.Windows.Forms.Button btnAcknowledgeAlert;
        private System.Windows.Forms.Label lblLikelyIssue;
        private System.Windows.Forms.ListView lvUpdates;
        private System.Windows.Forms.Label lblBlockContainingDanger;
        private System.Windows.Forms.ComboBox ddlTrainSelector;
        private System.Windows.Forms.Button btnTerminateTrain;
        private System.Windows.Forms.Button btnTrainInfo;
        private System.Windows.Forms.Button btnCancelAllocations;
        private System.Windows.Forms.Button btnRosterTest;
        private System.Windows.Forms.Label lblTrainName;
        private System.Windows.Forms.Button btnClearOutputLog;
        private System.Windows.Forms.ListBox lbJourneyLog;
        private System.Windows.Forms.Button btnClearJourneyListBox;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox tbTrainDCCID;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox tbTrainName;
        private System.Windows.Forms.Button btnUpdateTrainIDAndName;
        private System.Windows.Forms.TextBox tbPrevDCCID;
        private System.Windows.Forms.Label lblSpeed;
        private System.Windows.Forms.Label label6;
    }
}

