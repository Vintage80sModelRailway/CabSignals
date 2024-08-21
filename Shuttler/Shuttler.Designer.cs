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
            this.lblSpeedName = new System.Windows.Forms.Label();
            this.lblCurrentBlock = new System.Windows.Forms.Label();
            this.lblTrainStatus = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // btnTest
            // 
            this.btnTest.Location = new System.Drawing.Point(12, 413);
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
            this.lbRoster.Location = new System.Drawing.Point(1393, 39);
            this.lbRoster.Name = "lbRoster";
            this.lbRoster.Size = new System.Drawing.Size(202, 394);
            this.lbRoster.TabIndex = 1;
            // 
            // btnStart
            // 
            this.btnStart.Location = new System.Drawing.Point(27, 15);
            this.btnStart.Name = "btnStart";
            this.btnStart.Size = new System.Drawing.Size(75, 23);
            this.btnStart.TabIndex = 2;
            this.btnStart.Text = "Start";
            this.btnStart.UseVisualStyleBackColor = true;
            this.btnStart.Click += new System.EventHandler(this.btnStart_Click);
            // 
            // btnStop
            // 
            this.btnStop.Location = new System.Drawing.Point(27, 46);
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
            this.lbStartBlocks.Location = new System.Drawing.Point(1015, 39);
            this.lbStartBlocks.Name = "lbStartBlocks";
            this.lbStartBlocks.Size = new System.Drawing.Size(192, 342);
            this.lbStartBlocks.TabIndex = 4;
            this.lbStartBlocks.SelectedIndexChanged += new System.EventHandler(this.lbStartBlocks_SelectedIndexChanged);
            // 
            // lbDestinationBlocks
            // 
            this.lbDestinationBlocks.FormattingEnabled = true;
            this.lbDestinationBlocks.Location = new System.Drawing.Point(1213, 39);
            this.lbDestinationBlocks.Name = "lbDestinationBlocks";
            this.lbDestinationBlocks.Size = new System.Drawing.Size(174, 342);
            this.lbDestinationBlocks.TabIndex = 5;
            // 
            // cbAvailableTransits
            // 
            this.cbAvailableTransits.FormattingEnabled = true;
            this.cbAvailableTransits.Location = new System.Drawing.Point(471, 67);
            this.cbAvailableTransits.Name = "cbAvailableTransits";
            this.cbAvailableTransits.Size = new System.Drawing.Size(206, 21);
            this.cbAvailableTransits.TabIndex = 6;
            this.cbAvailableTransits.SelectedIndexChanged += new System.EventHandler(this.cbAvailableTransits_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1015, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Start blocks";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(1213, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(94, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Destination blocks";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1393, 20);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(38, 13);
            this.label3.TabIndex = 9;
            this.label3.Text = "Roster";
            // 
            // btnReloadStartBlocks
            // 
            this.btnReloadStartBlocks.Location = new System.Drawing.Point(1132, 13);
            this.btnReloadStartBlocks.Name = "btnReloadStartBlocks";
            this.btnReloadStartBlocks.Size = new System.Drawing.Size(75, 23);
            this.btnReloadStartBlocks.TabIndex = 10;
            this.btnReloadStartBlocks.Text = "Reload";
            this.btnReloadStartBlocks.UseVisualStyleBackColor = true;
            this.btnReloadStartBlocks.Click += new System.EventHandler(this.btnReloadStartBlocks_Click);
            // 
            // btnReloadDestBlocks
            // 
            this.btnReloadDestBlocks.Location = new System.Drawing.Point(1313, 13);
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
            this.label4.Location = new System.Drawing.Point(468, 46);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(86, 13);
            this.label4.TabIndex = 12;
            this.label4.Text = "Availeble transits";
            // 
            // btnTransitsReload
            // 
            this.btnTransitsReload.Location = new System.Drawing.Point(602, 41);
            this.btnTransitsReload.Name = "btnTransitsReload";
            this.btnTransitsReload.Size = new System.Drawing.Size(75, 23);
            this.btnTransitsReload.TabIndex = 13;
            this.btnTransitsReload.Text = "Reload";
            this.btnTransitsReload.UseVisualStyleBackColor = true;
            this.btnTransitsReload.Click += new System.EventHandler(this.btnTransitsReload_Click);
            // 
            // btnStartTransit
            // 
            this.btnStartTransit.Location = new System.Drawing.Point(472, 148);
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
            this.cbTransitTrainDirection.Location = new System.Drawing.Point(472, 121);
            this.cbTransitTrainDirection.Name = "cbTransitTrainDirection";
            this.cbTransitTrainDirection.Size = new System.Drawing.Size(205, 21);
            this.cbTransitTrainDirection.TabIndex = 15;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(472, 99);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 16;
            this.label5.Text = "Train Direction";
            // 
            // lbOutput
            // 
            this.lbOutput.FormattingEnabled = true;
            this.lbOutput.Location = new System.Drawing.Point(12, 260);
            this.lbOutput.Name = "lbOutput";
            this.lbOutput.Size = new System.Drawing.Size(665, 147);
            this.lbOutput.TabIndex = 17;
            // 
            // lbRunningTransits
            // 
            this.lbRunningTransits.FormattingEnabled = true;
            this.lbRunningTransits.Location = new System.Drawing.Point(857, 39);
            this.lbRunningTransits.Name = "lbRunningTransits";
            this.lbRunningTransits.Size = new System.Drawing.Size(152, 342);
            this.lbRunningTransits.TabIndex = 18;
            this.lbRunningTransits.SelectedIndexChanged += new System.EventHandler(this.lbRunningTransits_SelectedIndexChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(857, 20);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(73, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Active transits";
            // 
            // lblActiveTransitID
            // 
            this.lblActiveTransitID.AutoSize = true;
            this.lblActiveTransitID.Location = new System.Drawing.Point(24, 85);
            this.lblActiveTransitID.Name = "lblActiveTransitID";
            this.lblActiveTransitID.Size = new System.Drawing.Size(18, 13);
            this.lblActiveTransitID.TabIndex = 20;
            this.lblActiveTransitID.Text = "ID";
            // 
            // lblActiveTransitName
            // 
            this.lblActiveTransitName.AutoSize = true;
            this.lblActiveTransitName.Location = new System.Drawing.Point(48, 85);
            this.lblActiveTransitName.Name = "lblActiveTransitName";
            this.lblActiveTransitName.Size = new System.Drawing.Size(44, 13);
            this.lblActiveTransitName.TabIndex = 21;
            this.lblActiveTransitName.Text = "Journey";
            // 
            // lblSignalAspect
            // 
            this.lblSignalAspect.AutoSize = true;
            this.lblSignalAspect.Location = new System.Drawing.Point(24, 172);
            this.lblSignalAspect.Name = "lblSignalAspect";
            this.lblSignalAspect.Size = new System.Drawing.Size(36, 13);
            this.lblSignalAspect.TabIndex = 22;
            this.lblSignalAspect.Text = "Signal";
            // 
            // lblSignalReason
            // 
            this.lblSignalReason.AutoSize = true;
            this.lblSignalReason.Location = new System.Drawing.Point(115, 172);
            this.lblSignalReason.Name = "lblSignalReason";
            this.lblSignalReason.Size = new System.Drawing.Size(71, 13);
            this.lblSignalReason.TabIndex = 23;
            this.lblSignalReason.Text = "Signal reason";
            // 
            // lblSpeed
            // 
            this.lblSpeed.AutoSize = true;
            this.lblSpeed.Location = new System.Drawing.Point(24, 212);
            this.lblSpeed.Name = "lblSpeed";
            this.lblSpeed.Size = new System.Drawing.Size(38, 13);
            this.lblSpeed.TabIndex = 24;
            this.lblSpeed.Text = "Speed";
            // 
            // lblSpeedReason
            // 
            this.lblSpeedReason.AutoSize = true;
            this.lblSpeedReason.Location = new System.Drawing.Point(284, 212);
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
            this.label8.Location = new System.Drawing.Point(24, 153);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(42, 13);
            this.label8.TabIndex = 27;
            this.label8.Text = "Signal";
            // 
            // btnMoveTrain
            // 
            this.btnMoveTrain.Location = new System.Drawing.Point(1157, 384);
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
            this.lbRoute.Location = new System.Drawing.Point(701, 39);
            this.lbRoute.Name = "lbRoute";
            this.lbRoute.Size = new System.Drawing.Size(150, 342);
            this.lbRoute.TabIndex = 29;
            // 
            // lblRoute
            // 
            this.lblRoute.AutoSize = true;
            this.lblRoute.Location = new System.Drawing.Point(701, 20);
            this.lblRoute.Name = "lblRoute";
            this.lblRoute.Padding = new System.Windows.Forms.Padding(0, 0, 50, 0);
            this.lblRoute.Size = new System.Drawing.Size(86, 13);
            this.lblRoute.TabIndex = 30;
            this.lblRoute.Text = "Route";
            // 
            // btnRoutePrev
            // 
            this.btnRoutePrev.Enabled = false;
            this.btnRoutePrev.Location = new System.Drawing.Point(700, 384);
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
            this.btnRouteNext.Location = new System.Drawing.Point(820, 384);
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
            this.btnRouteAccept.Location = new System.Drawing.Point(739, 384);
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
            this.lblSpeedStep.Location = new System.Drawing.Point(91, 212);
            this.lblSpeedStep.Name = "lblSpeedStep";
            this.lblSpeedStep.Size = new System.Drawing.Size(51, 13);
            this.lblSpeedStep.TabIndex = 34;
            this.lblSpeedStep.Text = "Spd Step";
            this.lblSpeedStep.UseWaitCursor = true;
            // 
            // btnStopTransit
            // 
            this.btnStopTransit.Location = new System.Drawing.Point(588, 148);
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
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(24, 108);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(39, 13);
            this.label9.TabIndex = 36;
            this.label9.Text = "Block";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(27, 128);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(68, 13);
            this.label10.TabIndex = 38;
            this.label10.Text = "Length mm";
            // 
            // lblBlockLength
            // 
            this.lblBlockLength.AutoSize = true;
            this.lblBlockLength.Location = new System.Drawing.Point(101, 128);
            this.lblBlockLength.Name = "lblBlockLength";
            this.lblBlockLength.Size = new System.Drawing.Size(41, 13);
            this.lblBlockLength.TabIndex = 39;
            this.lblBlockLength.Text = "label11";
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(154, 129);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(76, 13);
            this.label11.TabIndex = 40;
            this.label11.Text = "Covered mm";
            // 
            // lblMmCoveredThisBlock
            // 
            this.lblMmCoveredThisBlock.AutoSize = true;
            this.lblMmCoveredThisBlock.Location = new System.Drawing.Point(236, 129);
            this.lblMmCoveredThisBlock.Name = "lblMmCoveredThisBlock";
            this.lblMmCoveredThisBlock.Size = new System.Drawing.Size(41, 13);
            this.lblMmCoveredThisBlock.TabIndex = 41;
            this.lblMmCoveredThisBlock.Text = "label12";
            // 
            // lblSpeedMMS
            // 
            this.lblSpeedMMS.AutoSize = true;
            this.lblSpeedMMS.Location = new System.Drawing.Point(498, 212);
            this.lblSpeedMMS.Name = "lblSpeedMMS";
            this.lblSpeedMMS.Size = new System.Drawing.Size(66, 13);
            this.lblSpeedMMS.TabIndex = 42;
            this.lblSpeedMMS.Text = "Speed MMS";
            // 
            // lblmmCoveredPercentLabel
            // 
            this.lblmmCoveredPercentLabel.AutoSize = true;
            this.lblmmCoveredPercentLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblmmCoveredPercentLabel.Location = new System.Drawing.Point(322, 129);
            this.lblmmCoveredPercentLabel.Name = "lblmmCoveredPercentLabel";
            this.lblmmCoveredPercentLabel.Size = new System.Drawing.Size(51, 13);
            this.lblmmCoveredPercentLabel.TabIndex = 43;
            this.lblmmCoveredPercentLabel.Text = "Percent";
            // 
            // lblmmCoveredPercent
            // 
            this.lblmmCoveredPercent.AutoSize = true;
            this.lblmmCoveredPercent.Location = new System.Drawing.Point(392, 129);
            this.lblmmCoveredPercent.Name = "lblmmCoveredPercent";
            this.lblmmCoveredPercent.Size = new System.Drawing.Size(41, 13);
            this.lblmmCoveredPercent.TabIndex = 44;
            this.lblmmCoveredPercent.Text = "label12";
            // 
            // lblSpeedName
            // 
            this.lblSpeedName.AutoSize = true;
            this.lblSpeedName.Location = new System.Drawing.Point(376, 415);
            this.lblSpeedName.Name = "lblSpeedName";
            this.lblSpeedName.Size = new System.Drawing.Size(57, 13);
            this.lblSpeedName.TabIndex = 45;
            this.lblSpeedName.Text = "Spd Name";
            // 
            // lblCurrentBlock
            // 
            this.lblCurrentBlock.AutoSize = true;
            this.lblCurrentBlock.Location = new System.Drawing.Point(79, 108);
            this.lblCurrentBlock.Name = "lblCurrentBlock";
            this.lblCurrentBlock.Size = new System.Drawing.Size(34, 13);
            this.lblCurrentBlock.TabIndex = 46;
            this.lblCurrentBlock.Text = "Block";
            // 
            // lblTrainStatus
            // 
            this.lblTrainStatus.AutoSize = true;
            this.lblTrainStatus.Location = new System.Drawing.Point(145, 85);
            this.lblTrainStatus.Name = "lblTrainStatus";
            this.lblTrainStatus.Size = new System.Drawing.Size(37, 13);
            this.lblTrainStatus.TabIndex = 47;
            this.lblTrainStatus.Text = "Status";
            // 
            // Shuttler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1602, 450);
            this.Controls.Add(this.lblTrainStatus);
            this.Controls.Add(this.lblCurrentBlock);
            this.Controls.Add(this.lblSpeedName);
            this.Controls.Add(this.lblmmCoveredPercent);
            this.Controls.Add(this.lblmmCoveredPercentLabel);
            this.Controls.Add(this.lblSpeedMMS);
            this.Controls.Add(this.lblMmCoveredThisBlock);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.lblBlockLength);
            this.Controls.Add(this.label10);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.btnStopTransit);
            this.Controls.Add(this.lblSpeedStep);
            this.Controls.Add(this.btnRouteAccept);
            this.Controls.Add(this.btnRouteNext);
            this.Controls.Add(this.btnRoutePrev);
            this.Controls.Add(this.lblRoute);
            this.Controls.Add(this.lbRoute);
            this.Controls.Add(this.btnMoveTrain);
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
        private System.Windows.Forms.Label lblSpeedName;
        private System.Windows.Forms.Label lblCurrentBlock;
        private System.Windows.Forms.Label lblTrainStatus;
    }
}