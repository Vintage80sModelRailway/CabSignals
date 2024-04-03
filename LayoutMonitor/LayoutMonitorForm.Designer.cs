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
            this.SuspendLayout();
            // 
            // lbOutput
            // 
            this.lbOutput.FormattingEnabled = true;
            this.lbOutput.Location = new System.Drawing.Point(12, 498);
            this.lbOutput.Name = "lbOutput";
            this.lbOutput.Size = new System.Drawing.Size(1484, 17);
            this.lbOutput.TabIndex = 0;
            this.lbOutput.Visible = false;
            // 
            // tbServerIP
            // 
            this.tbServerIP.Location = new System.Drawing.Point(1039, 16);
            this.tbServerIP.Name = "tbServerIP";
            this.tbServerIP.Size = new System.Drawing.Size(125, 20);
            this.tbServerIP.TabIndex = 1;
            this.tbServerIP.Visible = false;
            // 
            // tbServerPort
            // 
            this.tbServerPort.Location = new System.Drawing.Point(1064, 42);
            this.tbServerPort.Name = "tbServerPort";
            this.tbServerPort.Size = new System.Drawing.Size(100, 20);
            this.tbServerPort.TabIndex = 2;
            this.tbServerPort.Visible = false;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(982, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Server IP";
            this.label1.Visible = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1032, 45);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Port";
            this.label2.Visible = false;
            // 
            // btnStartMonitoring
            // 
            this.btnStartMonitoring.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnStartMonitoring.Location = new System.Drawing.Point(1170, 15);
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
            this.lblBlockWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBlockWarning.Location = new System.Drawing.Point(23, 14);
            this.lblBlockWarning.Name = "lblBlockWarning";
            this.lblBlockWarning.Size = new System.Drawing.Size(0, 63);
            this.lblBlockWarning.TabIndex = 6;
            // 
            // tbConfigLocation
            // 
            this.tbConfigLocation.Location = new System.Drawing.Point(896, 68);
            this.tbConfigLocation.Name = "tbConfigLocation";
            this.tbConfigLocation.Size = new System.Drawing.Size(268, 20);
            this.tbConfigLocation.TabIndex = 7;
            this.tbConfigLocation.Visible = false;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(853, 71);
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
            this.btnStopMonitoring.Location = new System.Drawing.Point(1265, 12);
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
            this.btnAcknowledgeAlert.Location = new System.Drawing.Point(1375, 12);
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
            this.lblLikelyIssue.Font = new System.Drawing.Font("Microsoft Sans Serif", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLikelyIssue.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
            this.lblLikelyIssue.Location = new System.Drawing.Point(23, 86);
            this.lblLikelyIssue.Name = "lblLikelyIssue";
            this.lblLikelyIssue.Size = new System.Drawing.Size(0, 63);
            this.lblLikelyIssue.TabIndex = 11;
            // 
            // lvUpdates
            // 
            this.lvUpdates.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lvUpdates.ForeColor = System.Drawing.Color.White;
            this.lvUpdates.HideSelection = false;
            this.lvUpdates.Location = new System.Drawing.Point(12, 240);
            this.lvUpdates.MultiSelect = false;
            this.lvUpdates.Name = "lvUpdates";
            this.lvUpdates.Size = new System.Drawing.Size(1484, 252);
            this.lvUpdates.TabIndex = 12;
            this.lvUpdates.UseCompatibleStateImageBehavior = false;
            this.lvUpdates.View = System.Windows.Forms.View.Details;
            this.lvUpdates.SelectedIndexChanged += new System.EventHandler(this.lvUpdates_SelectedIndexChanged);
            // 
            // lblBlockContainingDanger
            // 
            this.lblBlockContainingDanger.AutoSize = true;
            this.lblBlockContainingDanger.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.lblBlockContainingDanger.Font = new System.Drawing.Font("Microsoft Sans Serif", 40F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBlockContainingDanger.ForeColor = System.Drawing.Color.Black;
            this.lblBlockContainingDanger.Location = new System.Drawing.Point(23, 158);
            this.lblBlockContainingDanger.Name = "lblBlockContainingDanger";
            this.lblBlockContainingDanger.Size = new System.Drawing.Size(0, 63);
            this.lblBlockContainingDanger.TabIndex = 13;
            // 
            // LayoutMonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1508, 527);
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
            this.Name = "LayoutMonitorForm";
            this.Text = "JMRI Danger Early Warning System";
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
    }
}

