using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GwCustom.Forms
{
    partial class FUpload
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
            PMain = new Panel();
            LProgress = new Label();
            PictureDmsStorage = new PictureBox();
            LTitle = new Label();
            LDetail = new Label();
            PMain.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)PictureDmsStorage).BeginInit();
            SuspendLayout();
            // 
            // PMain
            // 
            PMain.BackColor = Color.FromArgb(246, 246, 246);
            PMain.Controls.Add(LProgress);
            PMain.Controls.Add(PictureDmsStorage);
            PMain.Controls.Add(LTitle);
            PMain.Controls.Add(LDetail);
            PMain.Location = new Point(1, 1);
            PMain.Name = "PMain";
            PMain.Size = new Size(418, 198);
            PMain.TabIndex = 11;
            // 
            // LProgress
            // 
            LProgress.Font = new Font("Segoe UI", 8.25F, FontStyle.Bold);
            LProgress.ForeColor = SystemColors.ControlText;
            LProgress.Location = new Point(12, 90);
            LProgress.Name = "LProgress";
            LProgress.Size = new Size(395, 23);
            LProgress.TabIndex = 15;
            LProgress.Text = "LProgress";
            // 
            // PictureDmsStorage
            // 
            PictureDmsStorage.Image = Image.FromStream(new MemoryStream(Properties.Resources.storage));
            PictureDmsStorage.InitialImage = null;
            PictureDmsStorage.Location = new Point(157, 11);
            PictureDmsStorage.Name = "PictureDmsStorage";
            PictureDmsStorage.Size = new Size(250, 69);
            PictureDmsStorage.SizeMode = PictureBoxSizeMode.AutoSize;
            PictureDmsStorage.TabIndex = 12;
            PictureDmsStorage.TabStop = false;
            // 
            // LTitle
            // 
            LTitle.Font = new Font("Segoe UI", 14.25F, FontStyle.Bold);
            LTitle.ForeColor = SystemColors.ControlText;
            LTitle.Location = new Point(212, 160);
            LTitle.Name = "LTitle";
            LTitle.Size = new Size(195, 25);
            LTitle.TabIndex = 11;
            LTitle.Text = "LTitle";
            LTitle.TextAlign = ContentAlignment.TopRight;
            // 
            // LDetail
            // 
            LDetail.BackColor = Color.Transparent;
            LDetail.Font = new Font("Segoe UI", 8.25F);
            LDetail.ForeColor = SystemColors.ControlText;
            LDetail.Location = new Point(12, 114);
            LDetail.Name = "LDetail";
            LDetail.Size = new Size(395, 71);
            LDetail.TabIndex = 13;
            LDetail.Text = "LDetail";
            // 
            // FUpload
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(221, 221, 221);
            ClientSize = new Size(420, 200);
            ControlBox = false;
            Controls.Add(PMain);
            ForeColor = SystemColors.HighlightText;
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FUpload";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FDmsStorageProgress";
            Load += FDmsStorage_Load;
            PMain.ResumeLayout(false);
            PMain.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)PictureDmsStorage).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Panel PMain;
        private Label LProgress;
        private PictureBox PictureDmsStorage;
        private Label LTitle;
        private Label LDetail;
    }
}