using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GwCustom.Forms
{
    partial class FInfo
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
            PDescription = new Panel();
            LDescription = new Label();
            LAuthor = new Label();
            LVersion = new Label();
            ((System.ComponentModel.ISupportInitialize)PbIcon).BeginInit();
            PDescription.SuspendLayout();
            SuspendLayout();
            // 
            // PbIcon
            // 
            PbIcon.Image = Image.FromStream(new MemoryStream(Properties.Resources.info));
            PbIcon.Location = new System.Drawing.Point(12, 12);
            PbIcon.Name = "PbIcon";
            PbIcon.Size = new System.Drawing.Size(48, 48);
            PbIcon.TabIndex = 22;
            PbIcon.TabStop = false;
            // 
            // PDescription
            // 
            PDescription.BackColor = System.Drawing.Color.FromArgb(221, 221, 221);
            PDescription.Controls.Add(LDescription);
            PDescription.Location = new System.Drawing.Point(12, 71);
            PDescription.Name = "PDescription";
            PDescription.Size = new System.Drawing.Size(406, 177);
            PDescription.TabIndex = 21;
            // 
            // LDescription
            // 
            LDescription.BackColor = System.Drawing.Color.White;
            LDescription.Font = new System.Drawing.Font("Segoe UI", 9F);
            LDescription.ForeColor = System.Drawing.SystemColors.ControlText;
            LDescription.Location = new System.Drawing.Point(1, 1);
            LDescription.Name = "LDescription";
            LDescription.Size = new System.Drawing.Size(404, 175);
            LDescription.TabIndex = 9;
            LDescription.Text = "LDescription";
            // 
            // LAuthor
            // 
            LAuthor.AutoSize = true;
            LAuthor.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            LAuthor.ForeColor = System.Drawing.SystemColors.ControlText;
            LAuthor.Location = new System.Drawing.Point(67, 23);
            LAuthor.Name = "LAuthor";
            LAuthor.Size = new System.Drawing.Size(52, 15);
            LAuthor.TabIndex = 20;
            LAuthor.Text = "LAuthor";
            // 
            // LVersion
            // 
            LVersion.AutoSize = true;
            LVersion.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            LVersion.ForeColor = System.Drawing.SystemColors.ControlText;
            LVersion.Location = new System.Drawing.Point(66, 45);
            LVersion.Name = "LVersion";
            LVersion.Size = new System.Drawing.Size(53, 15);
            LVersion.TabIndex = 19;
            LVersion.Text = "LVersion";
            // 
            // FInfo
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(246, 246, 246);
            ClientSize = new System.Drawing.Size(429, 257);
            ControlBox = false;
            Controls.Add(PbIcon);
            Controls.Add(PDescription);
            Controls.Add(LAuthor);
            Controls.Add(LVersion);
            ForeColor = System.Drawing.SystemColors.Control;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FInfo";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "FInfo";
            Load += FInfo_Load;
            ((System.ComponentModel.ISupportInitialize)PbIcon).EndInit();
            PDescription.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox PbIcon;
        private Panel PDescription;
        private Label LDescription;
        private Label LAuthor;
        private Label LVersion;
    }
}