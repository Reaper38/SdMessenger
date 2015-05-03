namespace Sdm.Client.Controls
{
    partial class FileTransferView
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
            this.pnRoot = new System.Windows.Forms.Panel();
            this.tlp = new System.Windows.Forms.TableLayoutPanel();
            this.pnRoot.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnRoot
            // 
            this.pnRoot.AutoScroll = true;
            this.pnRoot.BackColor = System.Drawing.SystemColors.Window;
            this.pnRoot.Controls.Add(this.tlp);
            this.pnRoot.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnRoot.Location = new System.Drawing.Point(0, 0);
            this.pnRoot.Name = "pnRoot";
            this.pnRoot.Size = new System.Drawing.Size(439, 150);
            this.pnRoot.TabIndex = 0;
            // 
            // tlp
            // 
            this.tlp.AutoSize = true;
            this.tlp.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tlp.BackColor = System.Drawing.SystemColors.Window;
            this.tlp.ColumnCount = 1;
            this.tlp.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tlp.Dock = System.Windows.Forms.DockStyle.Top;
            this.tlp.Location = new System.Drawing.Point(0, 0);
            this.tlp.Margin = new System.Windows.Forms.Padding(1);
            this.tlp.Name = "tlp";
            this.tlp.RowCount = 1;
            this.tlp.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tlp.Size = new System.Drawing.Size(439, 0);
            this.tlp.TabIndex = 1;
            // 
            // FileTransferView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pnRoot);
            this.Name = "FileTransferView";
            this.Size = new System.Drawing.Size(439, 150);
            this.pnRoot.ResumeLayout(false);
            this.pnRoot.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnRoot;
        private System.Windows.Forms.TableLayoutPanel tlp;
    }
}
