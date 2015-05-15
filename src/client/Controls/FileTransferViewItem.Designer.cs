namespace Sdm.Client.Controls
{
    partial class FileTransferViewItem
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pbProgress = new System.Windows.Forms.ProgressBar();
            this.btn2 = new System.Windows.Forms.Button();
            this.tbFileName = new System.Windows.Forms.TextBox();
            this.btn1 = new System.Windows.Forms.Button();
            this.flpDetails = new System.Windows.Forms.FlowLayoutPanel();
            this.lStatus = new System.Windows.Forms.Label();
            this.lDone = new System.Windows.Forms.Label();
            this.lSlash = new System.Windows.Forms.Label();
            this.lTotal = new System.Windows.Forms.Label();
            this.lSizeUnit = new System.Windows.Forms.Label();
            this.btnConceal = new System.Windows.Forms.Button();
            this.flpDetails.SuspendLayout();
            this.SuspendLayout();
            // 
            // pbProgress
            // 
            this.pbProgress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pbProgress.Location = new System.Drawing.Point(3, 19);
            this.pbProgress.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.pbProgress.Maximum = 1000;
            this.pbProgress.Name = "pbProgress";
            this.pbProgress.Size = new System.Drawing.Size(312, 8);
            this.pbProgress.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.pbProgress.TabIndex = 0;
            // 
            // btn2
            // 
            this.btn2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn2.Location = new System.Drawing.Point(241, 29);
            this.btn2.Margin = new System.Windows.Forms.Padding(1, 3, 3, 3);
            this.btn2.Name = "btn2";
            this.btn2.Size = new System.Drawing.Size(75, 23);
            this.btn2.TabIndex = 1;
            this.btn2.Text = "Accept";
            this.btn2.UseVisualStyleBackColor = true;
            this.btn2.Click += new System.EventHandler(this.btn2_Click);
            // 
            // tbFileName
            // 
            this.tbFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbFileName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbFileName.Location = new System.Drawing.Point(3, 3);
            this.tbFileName.Margin = new System.Windows.Forms.Padding(3, 3, 3, 0);
            this.tbFileName.MaxLength = 2048;
            this.tbFileName.Name = "tbFileName";
            this.tbFileName.ReadOnly = true;
            this.tbFileName.Size = new System.Drawing.Size(296, 13);
            this.tbFileName.TabIndex = 2;
            this.tbFileName.Text = "if_you_know_what_i_mean.png";
            // 
            // btn1
            // 
            this.btn1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btn1.Location = new System.Drawing.Point(165, 29);
            this.btn1.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.btn1.Name = "btn1";
            this.btn1.Size = new System.Drawing.Size(75, 23);
            this.btn1.TabIndex = 3;
            this.btn1.Text = "Decline";
            this.btn1.UseVisualStyleBackColor = true;
            this.btn1.Click += new System.EventHandler(this.btn1_Click);
            // 
            // flpDetails
            // 
            this.flpDetails.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.flpDetails.Controls.Add(this.lStatus);
            this.flpDetails.Controls.Add(this.lDone);
            this.flpDetails.Controls.Add(this.lSlash);
            this.flpDetails.Controls.Add(this.lTotal);
            this.flpDetails.Controls.Add(this.lSizeUnit);
            this.flpDetails.Location = new System.Drawing.Point(3, 30);
            this.flpDetails.Margin = new System.Windows.Forms.Padding(3, 3, 2, 3);
            this.flpDetails.Name = "flpDetails";
            this.flpDetails.Size = new System.Drawing.Size(160, 21);
            this.flpDetails.TabIndex = 6;
            this.flpDetails.WrapContents = false;
            // 
            // lStatus
            // 
            this.lStatus.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lStatus.AutoSize = true;
            this.lStatus.Location = new System.Drawing.Point(0, 4);
            this.lStatus.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lStatus.Name = "lStatus";
            this.lStatus.Size = new System.Drawing.Size(37, 13);
            this.lStatus.TabIndex = 12;
            this.lStatus.Text = "Status";
            // 
            // lDone
            // 
            this.lDone.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lDone.AutoSize = true;
            this.lDone.Location = new System.Drawing.Point(37, 4);
            this.lDone.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lDone.Name = "lDone";
            this.lDone.Size = new System.Drawing.Size(31, 13);
            this.lDone.TabIndex = 5;
            this.lDone.Text = "1024";
            // 
            // lSlash
            // 
            this.lSlash.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lSlash.AutoSize = true;
            this.lSlash.Location = new System.Drawing.Point(68, 4);
            this.lSlash.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lSlash.Name = "lSlash";
            this.lSlash.Size = new System.Drawing.Size(12, 13);
            this.lSlash.TabIndex = 9;
            this.lSlash.Text = "/";
            // 
            // lTotal
            // 
            this.lTotal.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lTotal.AutoSize = true;
            this.lTotal.Location = new System.Drawing.Point(80, 4);
            this.lTotal.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lTotal.Name = "lTotal";
            this.lTotal.Size = new System.Drawing.Size(31, 13);
            this.lTotal.TabIndex = 10;
            this.lTotal.Text = "1024";
            // 
            // lSizeUnit
            // 
            this.lSizeUnit.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lSizeUnit.AutoSize = true;
            this.lSizeUnit.Location = new System.Drawing.Point(111, 4);
            this.lSizeUnit.Margin = new System.Windows.Forms.Padding(0, 4, 0, 0);
            this.lSizeUnit.Name = "lSizeUnit";
            this.lSizeUnit.Size = new System.Drawing.Size(21, 13);
            this.lSizeUnit.TabIndex = 11;
            this.lSizeUnit.Text = "KB";
            // 
            // btnConceal
            // 
            this.btnConceal.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnConceal.BackColor = System.Drawing.SystemColors.Control;
            this.btnConceal.BackgroundImage = global::Sdm.Client.Properties.Resources.IconClose;
            this.btnConceal.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnConceal.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnConceal.Location = new System.Drawing.Point(302, 4);
            this.btnConceal.Margin = new System.Windows.Forms.Padding(0);
            this.btnConceal.Name = "btnConceal";
            this.btnConceal.Size = new System.Drawing.Size(13, 12);
            this.btnConceal.TabIndex = 7;
            this.btnConceal.UseVisualStyleBackColor = false;
            this.btnConceal.Click += new System.EventHandler(this.btnConceal_Click);
            // 
            // FileTransferViewItem
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.Controls.Add(this.btnConceal);
            this.Controls.Add(this.flpDetails);
            this.Controls.Add(this.btn1);
            this.Controls.Add(this.tbFileName);
            this.Controls.Add(this.btn2);
            this.Controls.Add(this.pbProgress);
            this.MaximumSize = new System.Drawing.Size(1000, 56);
            this.MinimumSize = new System.Drawing.Size(320, 56);
            this.Name = "FileTransferViewItem";
            this.Size = new System.Drawing.Size(320, 54);
            this.flpDetails.ResumeLayout(false);
            this.flpDetails.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar pbProgress;
        private System.Windows.Forms.Button btn1;
        private System.Windows.Forms.Button btn2;
        private System.Windows.Forms.TextBox tbFileName;
        private System.Windows.Forms.FlowLayoutPanel flpDetails;
        private System.Windows.Forms.Label lDone;
        private System.Windows.Forms.Label lSlash;
        private System.Windows.Forms.Label lTotal;
        private System.Windows.Forms.Label lSizeUnit;
        private System.Windows.Forms.Label lStatus;
        private System.Windows.Forms.Button btnConceal;
    }
}
