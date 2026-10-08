using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GwCustomUI.Forms
{
    partial class FError
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
            PbIcon = new PictureBox();
            LInfo = new Label();
            LError = new Label();
            PDescription = new Panel();
            LDescription = new Label();
            ((System.ComponentModel.ISupportInitialize)PbIcon).BeginInit();
            PDescription.SuspendLayout();
            SuspendLayout();
            // 
            // PbIcon
            // 
            PbIcon.Image = Image.FromStream(new MemoryStream(Properties.Resources.warning));
            PbIcon.Location = new Point(9, 7);
            PbIcon.Name = "PbIcon";
            PbIcon.Size = new Size(48, 48);
            PbIcon.TabIndex = 21;
            PbIcon.TabStop = false;
            // 
            // LInfo
            // 
            LInfo.Font = new Font("Segoe UI", 9F);
            LInfo.Location = new Point(10, 156);
            LInfo.Name = "LInfo";
            LInfo.Size = new Size(404, 89);
            LInfo.TabIndex = 20;
            LInfo.Text = "LInfo";
            // 
            // LError
            // 
            LError.AutoSize = true;
            LError.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            LError.Location = new Point(63, 40);
            LError.Name = "LError";
            LError.Size = new Size(41, 15);
            LError.TabIndex = 19;
            LError.Text = "LError";
            // 
            // PDescription
            // 
            PDescription.BackColor = Color.FromArgb(221, 221, 221);
            PDescription.Controls.Add(LDescription);
            PDescription.Location = new Point(9, 66);
            PDescription.Name = "PDescription";
            PDescription.Size = new Size(406, 87);
            PDescription.TabIndex = 18;
            // 
            // LDescription
            // 
            LDescription.BackColor = Color.White;
            LDescription.Font = new Font("Segoe UI", 9F);
            LDescription.ForeColor = SystemColors.ControlText;
            LDescription.Location = new Point(1, 1);
            LDescription.Name = "LDescription";
            LDescription.Size = new Size(404, 85);
            LDescription.TabIndex = 14;
            LDescription.Text = "LDescription";
            // 
            // FError
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(246, 246, 246);
            ClientSize = new Size(425, 253);
            Controls.Add(PbIcon);
            Controls.Add(LInfo);
            Controls.Add(LError);
            Controls.Add(PDescription);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FError";
            ShowIcon = false;
            ShowInTaskbar = false;
            Text = "FError";
            TopMost = true;
            Load += FError_Load;
            ((System.ComponentModel.ISupportInitialize)PbIcon).EndInit();
            PDescription.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox PbIcon;
        private Label LInfo;
        private Label LError;
        private Panel PDescription;
        private Label LDescription;
    }
}