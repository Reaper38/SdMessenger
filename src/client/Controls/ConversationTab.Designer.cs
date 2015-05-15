namespace Sdm.Client.Controls
{
    partial class ConversationTab
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
            this.scChat = new System.Windows.Forms.SplitContainer();
            this.rtbHistory = new Sdm.Client.Controls.RichTextBoxEx();
            this.btnSendFile = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.tbNewMsg = new Sdm.Client.Controls.TextBoxEx();
            this.btnToggleFileTransfer = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.scChat)).BeginInit();
            this.scChat.Panel1.SuspendLayout();
            this.scChat.Panel2.SuspendLayout();
            this.scChat.SuspendLayout();
            this.SuspendLayout();
            // 
            // scChat
            // 
            this.scChat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scChat.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
            this.scChat.Location = new System.Drawing.Point(0, 0);
            this.scChat.Name = "scChat";
            this.scChat.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // scChat.Panel1
            // 
            this.scChat.Panel1.Controls.Add(this.rtbHistory);
            this.scChat.Panel1MinSize = 64;
            // 
            // scChat.Panel2
            // 
            this.scChat.Panel2.Controls.Add(this.btnToggleFileTransfer);
            this.scChat.Panel2.Controls.Add(this.btnSendFile);
            this.scChat.Panel2.Controls.Add(this.btnSend);
            this.scChat.Panel2.Controls.Add(this.tbNewMsg);
            this.scChat.Panel2MinSize = 64;
            this.scChat.Size = new System.Drawing.Size(349, 171);
            this.scChat.SplitterDistance = 64;
            this.scChat.TabIndex = 1;
            // 
            // rtbHistory
            // 
            this.rtbHistory.BackColor = System.Drawing.SystemColors.Window;
            this.rtbHistory.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbHistory.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbHistory.Location = new System.Drawing.Point(0, 0);
            this.rtbHistory.Name = "rtbHistory";
            this.rtbHistory.ReadOnly = true;
            this.rtbHistory.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.Vertical;
            this.rtbHistory.Size = new System.Drawing.Size(349, 64);
            this.rtbHistory.TabIndex = 0;
            this.rtbHistory.Text = "";
            // 
            // btnSendFile
            // 
            this.btnSendFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSendFile.Location = new System.Drawing.Point(3, 77);
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(75, 23);
            this.btnSendFile.TabIndex = 2;
            this.btnSendFile.Text = "Send file...";
            this.btnSendFile.UseVisualStyleBackColor = true;
            this.btnSendFile.Click += new System.EventHandler(this.btnSendFile_Click);
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(271, 77);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            this.btnSend.Click += new System.EventHandler(this.btnSend_Click);
            // 
            // tbNewMsg
            // 
            this.tbNewMsg.AllowDrop = true;
            this.tbNewMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbNewMsg.Location = new System.Drawing.Point(4, 4);
            this.tbNewMsg.Margin = new System.Windows.Forms.Padding(4);
            this.tbNewMsg.Multiline = true;
            this.tbNewMsg.Name = "tbNewMsg";
            this.tbNewMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbNewMsg.Size = new System.Drawing.Size(341, 70);
            this.tbNewMsg.TabIndex = 0;
            this.tbNewMsg.DragDrop += new System.Windows.Forms.DragEventHandler(this.tbNewMsg_DragDrop);
            this.tbNewMsg.DragEnter += new System.Windows.Forms.DragEventHandler(this.tbNewMsg_DragEnter);
            this.tbNewMsg.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbNewMsg_KeyDown);
            // 
            // btnToggleFileTransfer
            // 
            this.btnToggleFileTransfer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnToggleFileTransfer.BackgroundImage = global::Sdm.Client.Properties.Resources.IconFileTransfer;
            this.btnToggleFileTransfer.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Center;
            this.btnToggleFileTransfer.Location = new System.Drawing.Point(80, 77);
            this.btnToggleFileTransfer.Name = "btnShowFileTransfer";
            this.btnToggleFileTransfer.Size = new System.Drawing.Size(24, 23);
            this.btnToggleFileTransfer.TabIndex = 3;
            this.btnToggleFileTransfer.UseVisualStyleBackColor = true;
            this.btnToggleFileTransfer.Click += new System.EventHandler(this.btnToggleFileTransfer_Click);
            // 
            // ConversationTab
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.scChat);
            this.Name = "ConversationTab";
            this.Size = new System.Drawing.Size(349, 171);
            this.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scChat.Panel1.ResumeLayout(false);
            this.scChat.Panel2.ResumeLayout(false);
            this.scChat.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scChat)).EndInit();
            this.scChat.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        
        #endregion

        private System.Windows.Forms.SplitContainer scChat;
        private System.Windows.Forms.Button btnSendFile;
        private System.Windows.Forms.Button btnSend;
        private TextBoxEx tbNewMsg;
        private RichTextBoxEx rtbHistory;
        private System.Windows.Forms.Button btnToggleFileTransfer;

    }
}
