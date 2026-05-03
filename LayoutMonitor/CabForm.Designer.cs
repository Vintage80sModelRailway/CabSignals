namespace LayoutMonitor
{
    partial class CabForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CabForm));
            this.pbSignalBox = new System.Windows.Forms.PictureBox();
            this.lblDCCID = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblNextBlock = new System.Windows.Forms.Label();
            this.lblTwoBlock = new System.Windows.Forms.Label();
            this.lblTrainName = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblCurrentBlock = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.pbSignalBox)).BeginInit();
            this.SuspendLayout();
            // 
            // pbSignalBox
            // 
            this.pbSignalBox.Image = ((System.Drawing.Image)(resources.GetObject("pbSignalBox.Image")));
            this.pbSignalBox.Location = new System.Drawing.Point(25, 12);
            this.pbSignalBox.Name = "pbSignalBox";
            this.pbSignalBox.Size = new System.Drawing.Size(55, 298);
            this.pbSignalBox.TabIndex = 0;
            this.pbSignalBox.TabStop = false;
            // 
            // lblDCCID
            // 
            this.lblDCCID.AutoSize = true;
            this.lblDCCID.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDCCID.Location = new System.Drawing.Point(738, 17);
            this.lblDCCID.Name = "lblDCCID";
            this.lblDCCID.Size = new System.Drawing.Size(115, 31);
            this.lblDCCID.TabIndex = 1;
            this.lblDCCID.Text = "DCC ID";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(145, 152);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(151, 31);
            this.label1.TabIndex = 2;
            this.label1.Text = "Next block";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(145, 224);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(146, 31);
            this.label2.TabIndex = 3;
            this.label2.Text = "Two block";
            // 
            // lblNextBlock
            // 
            this.lblNextBlock.AutoSize = true;
            this.lblNextBlock.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNextBlock.Location = new System.Drawing.Point(349, 152);
            this.lblNextBlock.Name = "lblNextBlock";
            this.lblNextBlock.Size = new System.Drawing.Size(141, 31);
            this.lblNextBlock.TabIndex = 5;
            this.lblNextBlock.Text = "Next block";
            // 
            // lblTwoBlock
            // 
            this.lblTwoBlock.AutoSize = true;
            this.lblTwoBlock.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTwoBlock.Location = new System.Drawing.Point(349, 224);
            this.lblTwoBlock.Name = "lblTwoBlock";
            this.lblTwoBlock.Size = new System.Drawing.Size(137, 31);
            this.lblTwoBlock.TabIndex = 6;
            this.lblTwoBlock.Text = "Two block";
            // 
            // lblTrainName
            // 
            this.lblTrainName.AutoSize = true;
            this.lblTrainName.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTrainName.Location = new System.Drawing.Point(145, 17);
            this.lblTrainName.Name = "lblTrainName";
            this.lblTrainName.Size = new System.Drawing.Size(160, 31);
            this.lblTrainName.TabIndex = 8;
            this.lblTrainName.Text = "Train name";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(145, 81);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(189, 31);
            this.label3.TabIndex = 9;
            this.label3.Text = "Current block";
            // 
            // lblCurrentBlock
            // 
            this.lblCurrentBlock.AutoSize = true;
            this.lblCurrentBlock.Font = new System.Drawing.Font("Microsoft Sans Serif", 20F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCurrentBlock.Location = new System.Drawing.Point(355, 81);
            this.lblCurrentBlock.Name = "lblCurrentBlock";
            this.lblCurrentBlock.Size = new System.Drawing.Size(176, 31);
            this.lblCurrentBlock.TabIndex = 10;
            this.lblCurrentBlock.Text = "Current block";
            // 
            // CabForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(873, 323);
            this.Controls.Add(this.lblCurrentBlock);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblTrainName);
            this.Controls.Add(this.lblTwoBlock);
            this.Controls.Add(this.lblNextBlock);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lblDCCID);
            this.Controls.Add(this.pbSignalBox);
            this.Name = "CabForm";
            this.Text = "CabForm";
            ((System.ComponentModel.ISupportInitialize)(this.pbSignalBox)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.PictureBox pbSignalBox;
        private System.Windows.Forms.Label lblDCCID;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblNextBlock;
        private System.Windows.Forms.Label lblTwoBlock;
        private System.Windows.Forms.Label lblTrainName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblCurrentBlock;
    }
}