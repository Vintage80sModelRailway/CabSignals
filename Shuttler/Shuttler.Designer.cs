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
            this.btnMoveTrain = new System.Windows.Forms.Button();
            this.lbRoute = new System.Windows.Forms.ListBox();
            this.lblRoute = new System.Windows.Forms.Label();
            this.btnRoutePrev = new System.Windows.Forms.Button();
            this.btnRouteNext = new System.Windows.Forms.Button();
            this.btnRouteAccept = new System.Windows.Forms.Button();
            this.lblSpeedStep = new System.Windows.Forms.Label();
            this.btnStopTransit = new System.Windows.Forms.Button();
            this.label9 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.lblBlockLength = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.lblMmCoveredThisBlock = new System.Windows.Forms.Label();
            this.lblSpeedMMS = new System.Windows.Forms.Label();
            this.lblmmCoveredPercentLabel = new System.Windows.Forms.Label();
            this.lblmmCoveredPercent = new System.Windows.Forms.Label();
            this.lblCurrentBlock = new System.Windows.Forms.Label();
            this.lblTrainStatus = new System.Windows.Forms.Label();
            this.pbBlockProgress = new System.Windows.Forms.ProgressBar();
            this.cbManageYard = new System.Windows.Forms.CheckBox();
            this.tbAdditionalTriggerDelay = new System.Windows.Forms.TextBox();
            this.lblAdditionalTriggerDelay = new System.Windows.Forms.Label();
            this.btnStartStationAutomation = new System.Windows.Forms.Button();
            this.btnStopStationAutomation = new System.Windows.Forms.Button();
            this.btnTest = new System.Windows.Forms.Button();
            this.btnCopyLogToClipboard = new System.Windows.Forms.Button();
            this.btnStartClockwiseSA = new System.Windows.Forms.Button();
            this.btnStopClockwiseSA = new System.Windows.Forms.Button();
            this.lbInclineQueue = new System.Windows.Forms.ListBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label13 = new System.Windows.Forms.Label();
            this.lblNextSectionInfo = new System.Windows.Forms.Label();
            this.cbRepeatTransitWhenComplete = new System.Windows.Forms.CheckBox();
            this.cbPassOnDirectionToTriggers = new System.Windows.Forms.CheckBox();
            this.cbEndToEndRepeating = new System.Windows.Forms.CheckBox();
            this.lblBlockAllocationIssue = new System.Windows.Forms.Label();
            this.pnlControl = new System.Windows.Forms.Panel();
            this.pnlTrigger = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.pnlControl.SuspendLayout();
            this.pnlTrigger.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.SuspendLayout();
            // 
            // lbRoster
            // 
            this.lbRoster.FormattingEnabled = true;
            this.lbRoster.Location = new System.Drawing.Point(1376, 44);
            this.lbRoster.Name = "lbRoster";
            this.lbRoster.Size = new System.Drawing.Size(202, 342);
            this.lbRoster.TabIndex = 1;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(9, 13);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(9, 44);
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
            this.lbStartBlocks.Location = new System.Drawing.Point(998, 44);
            this.lbStartBlocks.Name = "lbStartBlocks";
            this.lbStartBlocks.Size = new System.Drawing.Size(192, 342);
            this.lbStartBlocks.TabIndex = 4;
            this.lbStartBlocks.SelectedIndexChanged += new System.EventHandler(this.lbStartBlocks_SelectedIndexChanged);
            // 
            // lbDestinationBlocks
            // 
            this.lbDestinationBlocks.FormattingEnabled = true;
            this.lbDestinationBlocks.Location = new System.Drawing.Point(1196, 44);
            this.lbDestinationBlocks.Name = "lbDestinationBlocks";
            this.lbDestinationBlocks.Size = new System.Drawing.Size(174, 342);
            this.lbDestinationBlocks.TabIndex = 5;
            // 
            // cbAvailableTransits
            // 
            this.cbAvailableTransits.FormattingEnabled = true;
            this.cbAvailableTransits.Location = new System.Drawing.Point(403, 68);
            this.cbAvailableTransits.Name = "cbAvailableTransits";
            this.cbAvailableTransits.Size = new System.Drawing.Size(206, 21);
            this.cbAvailableTransits.TabIndex = 6;
            this.cbAvailableTransits.SelectedIndexChanged += new System.EventHandler(this.cbAvailableTransits_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(995, 19);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Start blocks";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1193, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Destination blocks";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1373, 19);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Roster";
            // 
            // btnReloadStartBlocks
            // 
            this.btnReloadStartBlocks.Location = new System.Drawing.Point(1112, 12);
            this.btnReloadStartBlocks.Name = "btnReloadStartBlocks";
            this.btnReloadStartBlocks.Size = new System.Drawing.Size(75, 23);
            this.btnReloadStartBlocks.TabIndex = 10;
            this.btnReloadStartBlocks.Text = "Reload";
            this.btnReloadStartBlocks.UseVisualStyleBackColor = true;
            this.btnReloadStartBlocks.Click += new System.EventHandler(this.btnReloadStartBlocks_Click);
            // 
            // btnReloadDestBlocks
            // 
            this.btnReloadDestBlocks.Location = new System.Drawing.Point(1293, 12);
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
            this.label4.Location = new System.Drawing.Point(401, 52);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Availeble transits";
            // 
            // btnTransitsReload
            // 
            this.btnTransitsReload.Location = new System.Drawing.Point(534, 38);
            this.btnTransitsReload.Name = "btnTransitsReload";
            this.btnTransitsReload.Size = new System.Drawing.Size(75, 23);
            this.btnTransitsReload.TabIndex = 13;
            this.btnTransitsReload.Text = "Reload";
            this.btnTransitsReload.UseVisualStyleBackColor = true;
            this.btnTransitsReload.Click += new System.EventHandler(this.btnTransitsReload_Click);
            // 
            // btnStartTransit
            // 
            this.btnStartTransit.Location = new System.Drawing.Point(16, 13);
            this.btnStartTransit.Name = "btnStartTransit";
            this.btnStartTransit.Size = new System.Drawing.Size(92, 23);
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
            this.cbTransitTrainDirection.Location = new System.Drawing.Point(404, 122);
            this.cbTransitTrainDirection.Name = "cbTransitTrainDirection";
            this.cbTransitTrainDirection.Size = new System.Drawing.Size(205, 21);
            this.cbTransitTrainDirection.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(401, 106);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Train Direction";
            // 
            // lbOutput
            // 
            this.lbOutput.FormattingEnabled = true;
            this.lbOutput.Location = new System.Drawing.Point(13, 793);
            this.lbOutput.Name = "lbOutput";
            this.lbOutput.Size = new System.Drawing.Size(1564, 147);
            this.lbOutput.TabIndex = 17;
            // 
            // lbRunningTransits
            // 
            this.lbRunningTransits.FormattingEnabled = true;
            this.lbRunningTransits.Location = new System.Drawing.Point(840, 44);
            this.lbRunningTransits.Name = "lbRunningTransits";
            this.lbRunningTransits.Size = new System.Drawing.Size(152, 199);
            this.lbRunningTransits.TabIndex = 18;
            this.lbRunningTransits.SelectedIndexChanged += new System.EventHandler(this.lbRunningTransits_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(837, 19);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Active transits";
            // 
            // lblActiveTransitID
            // 
            this.lblActiveTransitID.AutoSize = true;
            this.lblActiveTransitID.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActiveTransitID.Location = new System.Drawing.Point(16, 14);
            this.lblActiveTransitID.Name = "lblActiveTransitID";
            this.lblActiveTransitID.Size = new System.Drawing.Size(28, 20);
            this.lblActiveTransitID.TabIndex = 20;
            this.lblActiveTransitID.Text = "ID";
            // 
            // lblActiveTransitName
            // 
            this.lblActiveTransitName.AutoSize = true;
            this.lblActiveTransitName.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblActiveTransitName.Location = new System.Drawing.Point(59, 14);
            this.lblActiveTransitName.Name = "lblActiveTransitName";
            this.lblActiveTransitName.Size = new System.Drawing.Size(65, 20);
            this.lblActiveTransitName.TabIndex = 21;
            this.lblActiveTransitName.Text = "Journey";
            // 
            // lblSignalAspect
            // 
            this.lblSignalAspect.AutoSize = true;
            this.lblSignalAspect.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSignalAspect.Location = new System.Drawing.Point(31, 136);
            this.lblSignalAspect.Name = "lblSignalAspect";
            this.lblSignalAspect.Size = new System.Drawing.Size(53, 20);
            this.lblSignalAspect.TabIndex = 22;
            this.lblSignalAspect.Text = "Signal";
            // 
            // lblSignalReason
            // 
            this.lblSignalReason.AutoSize = true;
            this.lblSignalReason.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSignalReason.Location = new System.Drawing.Point(151, 136);
            this.lblSignalReason.Name = "lblSignalReason";
            this.lblSignalReason.Size = new System.Drawing.Size(106, 20);
            this.lblSignalReason.TabIndex = 23;
            this.lblSignalReason.Text = "Signal reason";
            // 
            // lblSpeed
            // 
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpeed.Location = new System.Drawing.Point(22, 218);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(56, 20);
            this.lblSpeed.TabIndex = 24;
            this.lblSpeed.Text = "Speed";
            // 
            // lblSpeedReason
            // 
            this.lblSpeedReason.AutoSize = true;
            this.lblSpeedReason.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpeedReason.Location = new System.Drawing.Point(282, 218);
            this.lblSpeedReason.Name = "lblSpeedReason";
            this.lblSpeedReason.Size = new System.Drawing.Size(109, 20);
            this.lblSpeedReason.TabIndex = 25;
            this.lblSpeedReason.Text = "Speed reason";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(16, 198);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(61, 20);
            this.label7.TabIndex = 26;
            this.label7.Text = "Speed";
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(19, 116);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(59, 20);
            this.label8.TabIndex = 27;
            this.label8.Text = "Signal";
            // 
            // btnMoveTrain
            // 
            this.btnMoveTrain.Location = new System.Drawing.Point(467, 3);
            this.btnMoveTrain.Name = "btnMoveTrain";
            this.btnMoveTrain.Size = new System.Drawing.Size(96, 23);
            this.btnMoveTrain.TabIndex = 28;
            this.btnMoveTrain.Text = "Generate route";
            this.btnMoveTrain.UseVisualStyleBackColor = true;
            this.btnMoveTrain.Click += new System.EventHandler(this.btnMoveTrain_Click);
            // 
            // lbRoute
            // 
            this.lbRoute.FormattingEnabled = true;
            this.lbRoute.Location = new System.Drawing.Point(684, 44);
            this.lbRoute.Name = "lbRoute";
            this.lbRoute.Size = new System.Drawing.Size(150, 342);
            this.lbRoute.TabIndex = 29;
            // 
            // lblRoute
            // 
            this.lblRoute.AutoSize = true;
            this.lblRoute.Location = new System.Drawing.Point(681, 19);
            this.lblRoute.Name = "lblRoute";
            this.lblRoute.Padding = new System.Windows.Forms.Padding(0, 0, 50, 0);
            this.lblRoute.Size = new System.Drawing.Size(86, 13);
            this.lblRoute.TabIndex = 30;
            this.lblRoute.Text = "Route";
            // 
            // btnRoutePrev
            // 
            this.btnRoutePrev.Enabled = false;
            this.btnRoutePrev.Location = new System.Drawing.Point(20, 3);
            this.btnRoutePrev.Name = "btnRoutePrev";
            this.btnRoutePrev.Size = new System.Drawing.Size(33, 23);
            this.btnRoutePrev.TabIndex = 31;
            this.btnRoutePrev.Text = "<<";
            this.btnRoutePrev.UseVisualStyleBackColor = true;
            this.btnRoutePrev.Click += new System.EventHandler(this.btnRoutePrev_Click);
            // 
            // btnRouteNext
            // 
            this.btnRouteNext.Enabled = false;
            this.btnRouteNext.Location = new System.Drawing.Point(140, 3);
            this.btnRouteNext.Name = "btnRouteNext";
            this.btnRouteNext.Size = new System.Drawing.Size(30, 23);
            this.btnRouteNext.TabIndex = 32;
            this.btnRouteNext.Text = ">>";
            this.btnRouteNext.UseVisualStyleBackColor = true;
            this.btnRouteNext.Click += new System.EventHandler(this.btnRouteNext_Click);
            // 
            // btnRouteAccept
            // 
            this.btnRouteAccept.Enabled = false;
            this.btnRouteAccept.Location = new System.Drawing.Point(59, 3);
            this.btnRouteAccept.Name = "btnRouteAccept";
            this.btnRouteAccept.Size = new System.Drawing.Size(75, 23);
            this.btnRouteAccept.TabIndex = 33;
            this.btnRouteAccept.Text = "Start";
            this.btnRouteAccept.UseVisualStyleBackColor = true;
            this.btnRouteAccept.Click += new System.EventHandler(this.btnRouteAccept_Click);
            // 
            // lblSpeedStep
            // 
            this.lblSpeedStep.AutoSize = true;
            this.lblSpeedStep.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpeedStep.Location = new System.Drawing.Point(89, 218);
            this.lblSpeedStep.Name = "lblSpeedStep";
            this.lblSpeedStep.Size = new System.Drawing.Size(76, 20);
            this.lblSpeedStep.TabIndex = 34;
            this.lblSpeedStep.Text = "Spd Step";
            this.lblSpeedStep.UseWaitCursor = true;
            // 
            // btnStopTransit
            // 
            this.btnStopTransit.Location = new System.Drawing.Point(9, 274);
            this.btnStopTransit.Name = "btnStopTransit";
            this.btnStopTransit.Size = new System.Drawing.Size(89, 23);
            this.btnStopTransit.TabIndex = 35;
            this.btnStopTransit.Text = "Stop transit";
            this.btnStopTransit.UseVisualStyleBackColor = true;
            this.btnStopTransit.Click += new System.EventHandler(this.btnStopTransit_Click);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(16, 37);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 20);
            this.label9.TabIndex = 36;
            this.label9.Text = "Block";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(19, 262);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(98, 20);
            this.label10.TabIndex = 38;
            this.label10.Text = "Length mm";
            // 
            // lblBlockLength
            // 
            this.lblBlockLength.AutoSize = true;
            this.lblBlockLength.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBlockLength.Location = new System.Drawing.Point(123, 262);
            this.lblBlockLength.Name = "lblBlockLength";
            this.lblBlockLength.Size = new System.Drawing.Size(59, 20);
            this.lblBlockLength.TabIndex = 39;
            this.lblBlockLength.Text = "Length";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(217, 262);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(108, 20);
            this.label11.TabIndex = 40;
            this.label11.Text = "Covered mm";
            // 
            // lblMmCoveredThisBlock
            // 
            this.lblMmCoveredThisBlock.AutoSize = true;
            this.lblMmCoveredThisBlock.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMmCoveredThisBlock.Location = new System.Drawing.Point(331, 262);
            this.lblMmCoveredThisBlock.Name = "lblMmCoveredThisBlock";
            this.lblMmCoveredThisBlock.Size = new System.Drawing.Size(35, 20);
            this.lblMmCoveredThisBlock.TabIndex = 41;
            this.lblMmCoveredThisBlock.Text = "mm";
            // 
            // lblSpeedMMS
            // 
            this.lblSpeedMMS.AutoSize = true;
            this.lblSpeedMMS.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblSpeedMMS.Location = new System.Drawing.Point(181, 218);
            this.lblSpeedMMS.Name = "lblSpeedMMS";
            this.lblSpeedMMS.Size = new System.Drawing.Size(97, 20);
            this.lblSpeedMMS.TabIndex = 42;
            this.lblSpeedMMS.Text = "Speed MMS";
            // 
            // lblmmCoveredPercentLabel
            // 
            this.lblmmCoveredPercentLabel.AutoSize = true;
            this.lblmmCoveredPercentLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblmmCoveredPercentLabel.Location = new System.Drawing.Point(442, 262);
            this.lblmmCoveredPercentLabel.Name = "lblmmCoveredPercentLabel";
            this.lblmmCoveredPercentLabel.Size = new System.Drawing.Size(71, 20);
            this.lblmmCoveredPercentLabel.TabIndex = 43;
            this.lblmmCoveredPercentLabel.Text = "Percent";
            // 
            // lblmmCoveredPercent
            // 
            this.lblmmCoveredPercent.AutoSize = true;
            this.lblmmCoveredPercent.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblmmCoveredPercent.Location = new System.Drawing.Point(545, 262);
            this.lblmmCoveredPercent.Name = "lblmmCoveredPercent";
            this.lblmmCoveredPercent.Size = new System.Drawing.Size(64, 20);
            this.lblmmCoveredPercent.TabIndex = 44;
            this.lblmmCoveredPercent.Text = "Percent";
            // 
            // lblCurrentBlock
            // 
            this.lblCurrentBlock.AutoSize = true;
            this.lblCurrentBlock.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentBlock.Location = new System.Drawing.Point(71, 37);
            this.lblCurrentBlock.Name = "lblCurrentBlock";
            this.lblCurrentBlock.Size = new System.Drawing.Size(48, 20);
            this.lblCurrentBlock.TabIndex = 46;
            this.lblCurrentBlock.Text = "Block";
            // 
            // lblTrainStatus
            // 
            this.lblTrainStatus.AutoSize = true;
            this.lblTrainStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrainStatus.Location = new System.Drawing.Point(175, 14);
            this.lblTrainStatus.Name = "lblTrainStatus";
            this.lblTrainStatus.Size = new System.Drawing.Size(56, 20);
            this.lblTrainStatus.TabIndex = 47;
            this.lblTrainStatus.Text = "Status";
            // 
            // pbBlockProgress
            // 
            this.pbBlockProgress.Location = new System.Drawing.Point(14, 735);
            this.pbBlockProgress.Name = "pbBlockProgress";
            this.pbBlockProgress.Size = new System.Drawing.Size(1564, 23);
            this.pbBlockProgress.TabIndex = 48;
            // 
            // cbManageYard
            // 
            this.cbManageYard.AutoSize = true;
            this.cbManageYard.Location = new System.Drawing.Point(553, 13);
            this.cbManageYard.Name = "cbManageYard";
            this.cbManageYard.Size = new System.Drawing.Size(90, 17);
            this.cbManageYard.TabIndex = 49;
            this.cbManageYard.Text = "Manage Yard";
            this.cbManageYard.UseVisualStyleBackColor = true;
            // 
            // tbAdditionalTriggerDelay
            // 
            this.tbAdditionalTriggerDelay.Location = new System.Drawing.Point(546, 195);
            this.tbAdditionalTriggerDelay.Name = "tbAdditionalTriggerDelay";
            this.tbAdditionalTriggerDelay.Size = new System.Drawing.Size(63, 20);
            this.tbAdditionalTriggerDelay.TabIndex = 50;
            // 
            // lblAdditionalTriggerDelay
            // 
            this.lblAdditionalTriggerDelay.AutoSize = true;
            this.lblAdditionalTriggerDelay.Location = new System.Drawing.Point(472, 197);
            this.lblAdditionalTriggerDelay.Name = "lblAdditionalTriggerDelay";
            this.lblAdditionalTriggerDelay.Size = new System.Drawing.Size(68, 13);
            this.lblAdditionalTriggerDelay.TabIndex = 51;
            this.lblAdditionalTriggerDelay.Text = "Trigger delay";
            // 
            // btnStartStationAutomation
            // 
            this.btnStartStationAutomation.Location = new System.Drawing.Point(127, 13);
            this.btnStartStationAutomation.Name = "btnStartStationAutomation";
            this.btnStartStationAutomation.Size = new System.Drawing.Size(75, 23);
            this.btnStartStationAutomation.TabIndex = 52;
            this.btnStartStationAutomation.Text = "Start AC SA";
            this.btnStartStationAutomation.UseVisualStyleBackColor = true;
            this.btnStartStationAutomation.Click += new System.EventHandler(this.btnStartStationAutomation_Click);
            // 
            // btnStopStationAutomation
            // 
            this.btnStopStationAutomation.Enabled = false;
            this.btnStopStationAutomation.Location = new System.Drawing.Point(127, 274);
            this.btnStopStationAutomation.Name = "btnStopStationAutomation";
            this.btnStopStationAutomation.Size = new System.Drawing.Size(75, 23);
            this.btnStopStationAutomation.TabIndex = 53;
            this.btnStopStationAutomation.Text = "Stop AC SA";
            this.btnStopStationAutomation.UseVisualStyleBackColor = true;
            this.btnStopStationAutomation.Click += new System.EventHandler(this.btnStopStationAutomation_Click);
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(1440, 764);
            this.btnTest.Name = "btnTest";
            this.btnTest.Size = new System.Drawing.Size(75, 23);
            this.btnTest.TabIndex = 54;
            this.btnTest.Text = "Test";
            this.btnTest.UseVisualStyleBackColor = true;
            this.btnTest.Click += new System.EventHandler(this.btnTest_Click);
            // 
            // btnCopyLogToClipboard
            // 
            this.btnCopyLogToClipboard.Location = new System.Drawing.Point(1533, 764);
            this.btnCopyLogToClipboard.Name = "btnCopyLogToClipboard";
            this.btnCopyLogToClipboard.Size = new System.Drawing.Size(44, 23);
            this.btnCopyLogToClipboard.TabIndex = 55;
            this.btnCopyLogToClipboard.Text = "Clip";
            this.btnCopyLogToClipboard.UseVisualStyleBackColor = true;
            this.btnCopyLogToClipboard.Click += new System.EventHandler(this.btnCopyLogToClipboard_Click);
            // 
            // btnStartClockwiseSA
            // 
            this.btnStartClockwiseSA.Location = new System.Drawing.Point(230, 13);
            this.btnStartClockwiseSA.Name = "btnStartClockwiseSA";
            this.btnStartClockwiseSA.Size = new System.Drawing.Size(75, 23);
            this.btnStartClockwiseSA.TabIndex = 56;
            this.btnStartClockwiseSA.Text = "Start CW SA";
            this.btnStartClockwiseSA.UseVisualStyleBackColor = true;
            this.btnStartClockwiseSA.Click += new System.EventHandler(this.btnStartClockwiseSA_Click);
            // 
            // btnStopClockwiseSA
            // 
            this.btnStopClockwiseSA.Enabled = false;
            this.btnStopClockwiseSA.Location = new System.Drawing.Point(230, 273);
            this.btnStopClockwiseSA.Name = "btnStopClockwiseSA";
            this.btnStopClockwiseSA.Size = new System.Drawing.Size(75, 23);
            this.btnStopClockwiseSA.TabIndex = 57;
            this.btnStopClockwiseSA.Text = "Stop CW SA";
            this.btnStopClockwiseSA.UseVisualStyleBackColor = true;
            this.btnStopClockwiseSA.Click += new System.EventHandler(this.btnStopClockwiseSA_Click);
            // 
            // lbInclineQueue
            // 
            this.lbInclineQueue.FormattingEnabled = true;
            this.lbInclineQueue.Location = new System.Drawing.Point(840, 278);
            this.lbInclineQueue.Name = "lbInclineQueue";
            this.lbInclineQueue.Size = new System.Drawing.Size(152, 108);
            this.lbInclineQueue.TabIndex = 58;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(837, 262);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(71, 13);
            this.label12.TabIndex = 59;
            this.label12.Text = "Incline queue";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label13.Location = new System.Drawing.Point(16, 58);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(108, 20);
            this.label13.TabIndex = 60;
            this.label13.Text = "Next section";
            // 
            // lblNextSectionInfo
            // 
            this.lblNextSectionInfo.AutoSize = true;
            this.lblNextSectionInfo.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNextSectionInfo.Location = new System.Drawing.Point(151, 58);
            this.lblNextSectionInfo.Name = "lblNextSectionInfo";
            this.lblNextSectionInfo.Size = new System.Drawing.Size(96, 20);
            this.lblNextSectionInfo.TabIndex = 61;
            this.lblNextSectionInfo.Text = "Next section";
            // 
            // cbRepeatTransitWhenComplete
            // 
            this.cbRepeatTransitWhenComplete.AutoSize = true;
            this.cbRepeatTransitWhenComplete.Location = new System.Drawing.Point(407, 149);
            this.cbRepeatTransitWhenComplete.Name = "cbRepeatTransitWhenComplete";
            this.cbRepeatTransitWhenComplete.Size = new System.Drawing.Size(61, 17);
            this.cbRepeatTransitWhenComplete.TabIndex = 62;
            this.cbRepeatTransitWhenComplete.Text = "Repeat";
            this.cbRepeatTransitWhenComplete.UseVisualStyleBackColor = true;
            // 
            // cbPassOnDirectionToTriggers
            // 
            this.cbPassOnDirectionToTriggers.AutoSize = true;
            this.cbPassOnDirectionToTriggers.Location = new System.Drawing.Point(531, 149);
            this.cbPassOnDirectionToTriggers.Name = "cbPassOnDirectionToTriggers";
            this.cbPassOnDirectionToTriggers.Size = new System.Drawing.Size(64, 17);
            this.cbPassOnDirectionToTriggers.TabIndex = 63;
            this.cbPassOnDirectionToTriggers.Text = "Pass on";
            this.cbPassOnDirectionToTriggers.UseVisualStyleBackColor = true;
            // 
            // cbEndToEndRepeating
            // 
            this.cbEndToEndRepeating.AutoSize = true;
            this.cbEndToEndRepeating.Location = new System.Drawing.Point(531, 172);
            this.cbEndToEndRepeating.Name = "cbEndToEndRepeating";
            this.cbEndToEndRepeating.Size = new System.Drawing.Size(78, 17);
            this.cbEndToEndRepeating.TabIndex = 64;
            this.cbEndToEndRepeating.Text = "End to end";
            this.cbEndToEndRepeating.UseVisualStyleBackColor = true;
            // 
            // lblBlockAllocationIssue
            // 
            this.lblBlockAllocationIssue.AutoSize = true;
            this.lblBlockAllocationIssue.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblBlockAllocationIssue.ForeColor = System.Drawing.Color.Red;
            this.lblBlockAllocationIssue.Location = new System.Drawing.Point(34, 84);
            this.lblBlockAllocationIssue.Name = "lblBlockAllocationIssue";
            this.lblBlockAllocationIssue.Size = new System.Drawing.Size(0, 20);
            this.lblBlockAllocationIssue.TabIndex = 65;
            // 
            // pnlControl
            // 
            this.pnlControl.BackColor = System.Drawing.Color.LightCoral;
            this.pnlControl.Controls.Add(this.btnStart);
            this.pnlControl.Controls.Add(this.btnStop);
            this.pnlControl.Controls.Add(this.cbManageYard);
            this.pnlControl.Location = new System.Drawing.Point(13, 13);
            this.pnlControl.Name = "pnlControl";
            this.pnlControl.Size = new System.Drawing.Size(654, 101);
            this.pnlControl.TabIndex = 66;
            // 
            // pnlTrigger
            // 
            this.pnlTrigger.BackColor = System.Drawing.Color.LightGreen;
            this.pnlTrigger.Controls.Add(this.btnStartStationAutomation);
            this.pnlTrigger.Controls.Add(this.panel3);
            this.pnlTrigger.Controls.Add(this.btnStopStationAutomation);
            this.pnlTrigger.Controls.Add(this.btnStopClockwiseSA);
            this.pnlTrigger.Controls.Add(this.panel2);
            this.pnlTrigger.Controls.Add(this.panel1);
            this.pnlTrigger.Controls.Add(this.btnStartClockwiseSA);
            this.pnlTrigger.Controls.Add(this.cbAvailableTransits);
            this.pnlTrigger.Controls.Add(this.label4);
            this.pnlTrigger.Controls.Add(this.btnTransitsReload);
            this.pnlTrigger.Controls.Add(this.cbEndToEndRepeating);
            this.pnlTrigger.Controls.Add(this.btnStartTransit);
            this.pnlTrigger.Controls.Add(this.cbPassOnDirectionToTriggers);
            this.pnlTrigger.Controls.Add(this.cbTransitTrainDirection);
            this.pnlTrigger.Controls.Add(this.cbRepeatTransitWhenComplete);
            this.pnlTrigger.Controls.Add(this.label5);
            this.pnlTrigger.Controls.Add(this.btnStopTransit);
            this.pnlTrigger.Controls.Add(this.tbAdditionalTriggerDelay);
            this.pnlTrigger.Controls.Add(this.lblAdditionalTriggerDelay);
            this.pnlTrigger.Location = new System.Drawing.Point(13, 121);
            this.pnlTrigger.Name = "pnlTrigger";
            this.pnlTrigger.Size = new System.Drawing.Size(654, 307);
            this.pnlTrigger.TabIndex = 67;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.LightGreen;
            this.panel3.Location = new System.Drawing.Point(652, 271);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(913, 35);
            this.panel3.TabIndex = 68;
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(652, 268);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(913, 39);
            this.panel2.TabIndex = 68;
            // 
            // panel1
            // 
            this.panel1.Location = new System.Drawing.Point(652, 268);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(912, 38);
            this.panel1.TabIndex = 65;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.LightGreen;
            this.panel4.Controls.Add(this.btnRouteAccept);
            this.panel4.Controls.Add(this.btnMoveTrain);
            this.panel4.Controls.Add(this.btnRoutePrev);
            this.panel4.Controls.Add(this.btnRouteNext);
            this.panel4.Location = new System.Drawing.Point(668, 392);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(910, 36);
            this.panel4.TabIndex = 68;
            // 
            // panel5
            // 
            this.panel5.BackColor = System.Drawing.Color.LightSkyBlue;
            this.panel5.Controls.Add(this.label10);
            this.panel5.Controls.Add(this.lblActiveTransitID);
            this.panel5.Controls.Add(this.lblActiveTransitName);
            this.panel5.Controls.Add(this.lblSignalAspect);
            this.panel5.Controls.Add(this.lblBlockAllocationIssue);
            this.panel5.Controls.Add(this.lblSignalReason);
            this.panel5.Controls.Add(this.lblNextSectionInfo);
            this.panel5.Controls.Add(this.lblSpeed);
            this.panel5.Controls.Add(this.label13);
            this.panel5.Controls.Add(this.lblSpeedReason);
            this.panel5.Controls.Add(this.label7);
            this.panel5.Controls.Add(this.label8);
            this.panel5.Controls.Add(this.lblSpeedStep);
            this.panel5.Controls.Add(this.lblTrainStatus);
            this.panel5.Controls.Add(this.label9);
            this.panel5.Controls.Add(this.lblCurrentBlock);
            this.panel5.Controls.Add(this.lblBlockLength);
            this.panel5.Controls.Add(this.lblmmCoveredPercent);
            this.panel5.Controls.Add(this.label11);
            this.panel5.Controls.Add(this.lblmmCoveredPercentLabel);
            this.panel5.Controls.Add(this.lblMmCoveredThisBlock);
            this.panel5.Controls.Add(this.lblSpeedMMS);
            this.panel5.Location = new System.Drawing.Point(13, 435);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1565, 294);
            this.panel5.TabIndex = 69;
            // 
            // Shuttler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1602, 982);
            this.Controls.Add(this.panel5);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.pnlTrigger);
            this.Controls.Add(this.pnlControl);
            this.Controls.Add(this.label12);
            this.Controls.Add(this.lbInclineQueue);
            this.Controls.Add(this.pbBlockProgress);
            this.Controls.Add(this.btnCopyLogToClipboard);
            this.Controls.Add(this.btnTest);
            this.Controls.Add(this.lblRoute);
            this.Controls.Add(this.lbRoute);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.lbRunningTransits);
            this.Controls.Add(this.lbOutput);
            this.Controls.Add(this.btnReloadDestBlocks);
            this.Controls.Add(this.btnReloadStartBlocks);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lbDestinationBlocks);
            this.Controls.Add(this.lbStartBlocks);
            this.Controls.Add(this.lbRoster);
            this.Name = "Shuttler";
            this.Text = "Shuttler";
            this.pnlControl.ResumeLayout(false);
            this.pnlControl.PerformLayout();
            this.pnlTrigger.ResumeLayout(false);
            this.pnlTrigger.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
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
        private System.Windows.Forms.Button btnMoveTrain;
        private System.Windows.Forms.ListBox lbRoute;
        private System.Windows.Forms.Label lblRoute;
        private System.Windows.Forms.Button btnRoutePrev;
        private System.Windows.Forms.Button btnRouteNext;
        private System.Windows.Forms.Button btnRouteAccept;
        private System.Windows.Forms.Label lblSpeedStep;
        private System.Windows.Forms.Button btnStopTransit;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Label lblBlockLength;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.Label lblMmCoveredThisBlock;
        private System.Windows.Forms.Label lblSpeedMMS;
        private System.Windows.Forms.Label lblmmCoveredPercentLabel;
        private System.Windows.Forms.Label lblmmCoveredPercent;
        private System.Windows.Forms.Label lblCurrentBlock;
        private System.Windows.Forms.Label lblTrainStatus;
        private System.Windows.Forms.ProgressBar pbBlockProgress;
        private System.Windows.Forms.CheckBox cbManageYard;
        private System.Windows.Forms.TextBox tbAdditionalTriggerDelay;
        private System.Windows.Forms.Label lblAdditionalTriggerDelay;
        private System.Windows.Forms.Button btnStartStationAutomation;
        private System.Windows.Forms.Button btnStopStationAutomation;
        private System.Windows.Forms.Button btnTest;
        private System.Windows.Forms.Button btnCopyLogToClipboard;
        private System.Windows.Forms.Button btnStartClockwiseSA;
        private System.Windows.Forms.Button btnStopClockwiseSA;
        private System.Windows.Forms.ListBox lbInclineQueue;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.Label lblNextSectionInfo;
        private System.Windows.Forms.CheckBox cbRepeatTransitWhenComplete;
        private System.Windows.Forms.CheckBox cbPassOnDirectionToTriggers;
        private System.Windows.Forms.CheckBox cbEndToEndRepeating;
        private System.Windows.Forms.Label lblBlockAllocationIssue;
        private System.Windows.Forms.Panel pnlControl;
        private System.Windows.Forms.Panel pnlTrigger;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.Panel panel5;
    }
}