
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
            this.components = new System.ComponentModel.Container();
            this.lvUsers = new Sdm.Client.Controls.ListViewEx();
            this.scRoot = new System.Windows.Forms.SplitContainer();
            this.tbHost = new System.Windows.Forms.TextBox();
            this.tabConvs = new System.Windows.Forms.TabControl();
            this.vmMenu = new wyDay.Controls.VistaMenu(this.components);
            this.mmMenu = new System.Windows.Forms.MainMenu(this.components);
            this.miSdm = new System.Windows.Forms.MenuItem();
            this.miLogin = new System.Windows.Forms.MenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.scRoot)).BeginInit();
            this.scRoot.Panel1.SuspendLayout();
            this.scRoot.Panel2.SuspendLayout();
            this.scRoot.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.vmMenu)).BeginInit();
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
            this.lvUsers.View = System.Windows.Forms.View.List;
            this.lvUsers.DoubleClick += new System.EventHandler(this.lvUsers_DoubleClick);
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
            this.scRoot.Panel1.Controls.Add(this.tbHost);
            this.scRoot.Panel1.Controls.Add(this.lvUsers);
            this.scRoot.Panel1MinSize = 160;
            // 
            // scRoot.Panel2
            // 
            this.scRoot.Panel2.Controls.Add(this.tabConvs);
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
            this.tbHost.Size = new System.Drawing.Size(174, 20);
            this.tbHost.TabIndex = 1;
            // 
            // tabConvs
            // 
            this.tabConvs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabConvs.Location = new System.Drawing.Point(0, 0);
            this.tabConvs.Name = "tabConvs";
            this.tabConvs.SelectedIndex = 0;
            this.tabConvs.Size = new System.Drawing.Size(456, 351);
            this.tabConvs.TabIndex = 0;
            // 
            // vmMenu
            // 
            this.vmMenu.ContainerControl = this;
            // 
            // mmMenu
            // 
            this.mmMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miSdm});
            // 
            // miSdm
            // 
            this.miSdm.Index = 0;
            this.miSdm.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.miLogin});
            this.miSdm.Text = "SdMessenger";
            // 
            // miLogin
            // 
            this.miLogin.Index = 0;
            this.miLogin.Text = "Login";
            this.miLogin.Click += new System.EventHandler(this.miLogin_Click);
            // 
            // MainDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(640, 351);
            this.Controls.Add(this.scRoot);
            this.Menu = this.mmMenu;
            this.MinimumSize = new System.Drawing.Size(480, 320);
            this.Name = "MainDialog";
            this.Text = "SdmClient - username";
            this.scRoot.Panel1.ResumeLayout(false);
            this.scRoot.Panel1.PerformLayout();
            this.scRoot.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scRoot)).EndInit();
            this.scRoot.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.vmMenu)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private Controls.ListViewEx lvUsers;
        private System.Windows.Forms.SplitContainer scRoot;
        private System.Windows.Forms.TextBox tbHost;
        private System.Windows.Forms.TabControl tabConvs;
        private wyDay.Controls.VistaMenu vmMenu;
        private System.Windows.Forms.MainMenu mmMenu;
        private System.Windows.Forms.MenuItem miSdm;
        private System.Windows.Forms.MenuItem miLogin;
    }
}