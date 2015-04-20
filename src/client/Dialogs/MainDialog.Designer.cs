namespace Sdm.Client
{
    partial class MainDialog
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
            this.lvUsers = new System.Windows.Forms.ListView();
            this.scRoot = new System.Windows.Forms.SplitContainer();
            this.tbHost = new System.Windows.Forms.TextBox();
            this.scChat = new System.Windows.Forms.SplitContainer();
            this.tabConversations = new System.Windows.Forms.TabControl();
            this.tpDummy = new System.Windows.Forms.TabPage();
            this.btnSendFile = new System.Windows.Forms.Button();
            this.btnSend = new System.Windows.Forms.Button();
            this.tbNewMsg = new System.Windows.Forms.TextBox();
            this.btnSrv = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.scRoot)).BeginInit();
            this.scRoot.Panel1.SuspendLayout();
            this.scRoot.Panel2.SuspendLayout();
            this.scRoot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scChat)).BeginInit();
            this.scChat.Panel1.SuspendLayout();
            this.scChat.Panel2.SuspendLayout();
            this.scChat.SuspendLayout();
            this.tabConversations.SuspendLayout();
            this.SuspendLayout();
            // 
            // lvUsers
            // 
            this.lvUsers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvUsers.Location = new System.Drawing.Point(3, 26);
            this.lvUsers.Name = "lvUsers";
            this.lvUsers.Size = new System.Drawing.Size(174, 322);
            this.lvUsers.TabIndex = 0;
            this.lvUsers.UseCompatibleStateImageBehavior = false;
            // 
            // scRoot
            // 
            this.scRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.scRoot.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.scRoot.Location = new System.Drawing.Point(0, 0);
            this.scRoot.Name = "scRoot";
            // 
            // scRoot.Panel1
            // 
            this.scRoot.Panel1.Controls.Add(this.btnSrv);
            this.scRoot.Panel1.Controls.Add(this.tbHost);
            this.scRoot.Panel1.Controls.Add(this.lvUsers);
            this.scRoot.Panel1MinSize = 160;
            // 
            // scRoot.Panel2
            // 
            this.scRoot.Panel2.Controls.Add(this.scChat);
            this.scRoot.Panel2MinSize = 240;
            this.scRoot.Size = new System.Drawing.Size(640, 351);
            this.scRoot.SplitterDistance = 180;
            this.scRoot.TabIndex = 1;
            // 
            // tbHost
            // 
            this.tbHost.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbHost.Location = new System.Drawing.Point(3, 3);
            this.tbHost.Name = "tbHost";
            this.tbHost.ReadOnly = true;
            this.tbHost.Size = new System.Drawing.Size(149, 20);
            this.tbHost.TabIndex = 1;
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
            this.scChat.Panel1.Controls.Add(this.tabConversations);
            this.scChat.Panel1MinSize = 64;
            // 
            // scChat.Panel2
            // 
            this.scChat.Panel2.Controls.Add(this.btnSendFile);
            this.scChat.Panel2.Controls.Add(this.btnSend);
            this.scChat.Panel2.Controls.Add(this.tbNewMsg);
            this.scChat.Panel2MinSize = 64;
            this.scChat.Size = new System.Drawing.Size(456, 351);
            this.scChat.SplitterDistance = 241;
            this.scChat.TabIndex = 0;
            // 
            // tabConversations
            // 
            this.tabConversations.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabConversations.Controls.Add(this.tpDummy);
            this.tabConversations.Location = new System.Drawing.Point(4, 4);
            this.tabConversations.Name = "tabConversations";
            this.tabConversations.SelectedIndex = 0;
            this.tabConversations.Size = new System.Drawing.Size(450, 235);
            this.tabConversations.TabIndex = 0;
            // 
            // tpDummy
            // 
            this.tpDummy.Location = new System.Drawing.Point(4, 22);
            this.tpDummy.Name = "tpDummy";
            this.tpDummy.Padding = new System.Windows.Forms.Padding(3);
            this.tpDummy.Size = new System.Drawing.Size(442, 209);
            this.tpDummy.TabIndex = 0;
            this.tpDummy.Text = "username";
            this.tpDummy.UseVisualStyleBackColor = true;
            // 
            // btnSendFile
            // 
            this.btnSendFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSendFile.Location = new System.Drawing.Point(3, 80);
            this.btnSendFile.Name = "btnSendFile";
            this.btnSendFile.Size = new System.Drawing.Size(75, 23);
            this.btnSendFile.TabIndex = 2;
            this.btnSendFile.Text = "Send file...";
            this.btnSendFile.UseVisualStyleBackColor = true;
            // 
            // btnSend
            // 
            this.btnSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSend.Location = new System.Drawing.Point(378, 80);
            this.btnSend.Name = "btnSend";
            this.btnSend.Size = new System.Drawing.Size(75, 23);
            this.btnSend.TabIndex = 1;
            this.btnSend.Text = "Send";
            this.btnSend.UseVisualStyleBackColor = true;
            // 
            // tbNewMsg
            // 
            this.tbNewMsg.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tbNewMsg.Location = new System.Drawing.Point(4, 4);
            this.tbNewMsg.Margin = new System.Windows.Forms.Padding(4);
            this.tbNewMsg.Multiline = true;
            this.tbNewMsg.Name = "tbNewMsg";
            this.tbNewMsg.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.tbNewMsg.Size = new System.Drawing.Size(448, 73);
            this.tbNewMsg.TabIndex = 0;
            // 
            // btnSrv
            // 
            this.btnSrv.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSrv.Location = new System.Drawing.Point(153, 2);
            this.btnSrv.Name = "btnSrv";
            this.btnSrv.Size = new System.Drawing.Size(25, 22);
            this.btnSrv.TabIndex = 3;
            this.btnSrv.Text = "...";
            this.btnSrv.UseVisualStyleBackColor = true;
            // 
            // MainDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 351);
            this.Controls.Add(this.scRoot);
            this.MinimumSize = new System.Drawing.Size(480, 320);
            this.Name = "MainDialog";
            this.Text = "SdmClient - username";
            this.scRoot.Panel1.ResumeLayout(false);
            this.scRoot.Panel1.PerformLayout();
            this.scRoot.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scRoot)).EndInit();
            this.scRoot.ResumeLayout(false);
            this.scChat.Panel1.ResumeLayout(false);
            this.scChat.Panel2.ResumeLayout(false);
            this.scChat.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scChat)).EndInit();
            this.scChat.ResumeLayout(false);
            this.tabConversations.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView lvUsers;
        private System.Windows.Forms.SplitContainer scRoot;
        private System.Windows.Forms.TextBox tbHost;
        private System.Windows.Forms.SplitContainer scChat;
        private System.Windows.Forms.Button btnSend;
        private System.Windows.Forms.TextBox tbNewMsg;
        private System.Windows.Forms.Button btnSendFile;
        private System.Windows.Forms.TabControl tabConversations;
        private System.Windows.Forms.TabPage tpDummy;
        private System.Windows.Forms.Button btnSrv;
    }
}