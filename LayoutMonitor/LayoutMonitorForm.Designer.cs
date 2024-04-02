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
            this.SuspendLayout();
            // 
            // lbOutput
            // 
            this.lbOutput.FormattingEnabled = true;
            this.lbOutput.Location = new System.Drawing.Point(13, 272);
            this.lbOutput.Name = "lbOutput";
            this.lbOutput.Size = new System.Drawing.Size(976, 160);
            this.lbOutput.TabIndex = 0;
            // 
            // tbServerIP
            // 
            this.tbServerIP.Location = new System.Drawing.Point(97, 13);
            this.tbServerIP.Name = "tbServerIP";
            this.tbServerIP.Size = new System.Drawing.Size(125, 20);
            this.tbServerIP.TabIndex = 1;
            // 
            // tbServerPort
            // 
            this.tbServerPort.Location = new System.Drawing.Point(266, 13);
            this.tbServerPort.Name = "tbServerPort";
            this.tbServerPort.Size = new System.Drawing.Size(100, 20);
            this.tbServerPort.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(40, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(51, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Server IP";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(234, 16);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Port";
            // 
            // btnStartMonitoring
            // 
            this.btnStartMonitoring.Location = new System.Drawing.Point(713, 12);
            this.btnStartMonitoring.Name = "btnStartMonitoring";
            this.btnStartMonitoring.Size = new System.Drawing.Size(75, 23);
            this.btnStartMonitoring.TabIndex = 5;
            this.btnStartMonitoring.Text = "Start";
            this.btnStartMonitoring.UseVisualStyleBackColor = true;
            this.btnStartMonitoring.Click += new System.EventHandler(this.btnStartMonitoring_Click);
            // 
            // lblBlockWarning
            // 
            this.lblBlockWarning.AutoSize = true;
            this.lblBlockWarning.Font = new System.Drawing.Font("Microsoft Sans Serif", 50F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBlockWarning.Location = new System.Drawing.Point(23, 159);
            this.lblBlockWarning.Name = "lblBlockWarning";
            this.lblBlockWarning.Size = new System.Drawing.Size(230, 76);
            this.lblBlockWarning.TabIndex = 6;
            this.lblBlockWarning.Text = "Ready";
            // 
            // tbConfigLocation
            // 
            this.tbConfigLocation.Location = new System.Drawing.Point(427, 12);
            this.tbConfigLocation.Name = "tbConfigLocation";
            this.tbConfigLocation.Size = new System.Drawing.Size(268, 20);
            this.tbConfigLocation.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(384, 15);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(37, 13);
            this.label3.TabIndex = 8;
            this.label3.Text = "Config";
            // 
            // btnStopMonitoring
            // 
            this.btnStopMonitoring.Location = new System.Drawing.Point(794, 13);
            this.btnStopMonitoring.Name = "btnStopMonitoring";
            this.btnStopMonitoring.Size = new System.Drawing.Size(75, 23);
            this.btnStopMonitoring.TabIndex = 9;
            this.btnStopMonitoring.Text = "Stop";
            this.btnStopMonitoring.UseVisualStyleBackColor = true;
            this.btnStopMonitoring.Click += new System.EventHandler(this.btnStopMonitoring_Click);
            // 
            // btnAcknowledgeAlert
            // 
            this.btnAcknowledgeAlert.BackColor = System.Drawing.Color.LimeGreen;
            this.btnAcknowledgeAlert.Font = new System.Drawing.Font("Microsoft Sans Serif", 50F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAcknowledgeAlert.Location = new System.Drawing.Point(1009, 272);
            this.btnAcknowledgeAlert.Name = "btnAcknowledgeAlert";
            this.btnAcknowledgeAlert.Size = new System.Drawing.Size(487, 160);
            this.btnAcknowledgeAlert.TabIndex = 10;
            this.btnAcknowledgeAlert.Text = "Acknowledge";
            this.btnAcknowledgeAlert.UseVisualStyleBackColor = false;
            this.btnAcknowledgeAlert.Click += new System.EventHandler(this.btnAcknowledgeAlert_Click);
            // 
            // lblLikelyIssue
            // 
            this.lblLikelyIssue.AutoSize = true;
            this.lblLikelyIssue.Font = new System.Drawing.Font("Microsoft Sans Serif", 50F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLikelyIssue.ImageAlign = System.Drawing.ContentAlignment.BottomRight;
            this.lblLikelyIssue.Location = new System.Drawing.Point(23, 72);
            this.lblLikelyIssue.Name = "lblLikelyIssue";
            this.lblLikelyIssue.Size = new System.Drawing.Size(230, 76);
            this.lblLikelyIssue.TabIndex = 11;
            this.lblLikelyIssue.Text = "Ready";
            // 
            // LayoutMonitorForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1508, 450);
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
            this.Text = "Form1";
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
    }
}

