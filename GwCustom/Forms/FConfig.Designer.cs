using System.Drawing;
using System.Windows.Forms;

namespace GwCustom.Forms
{
    partial class FConfig
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
            GbGeneral = new GroupBox();
            CbDmsFiling = new CheckBox();
            CbSignature = new CheckBox();
            GbGeneral.SuspendLayout();
            SuspendLayout();
            // 
            // GbGeneral
            // 
            GbGeneral.Controls.Add(CbDmsFiling);
            GbGeneral.Controls.Add(CbSignature);
            GbGeneral.Location = new Point(12, 12);
            GbGeneral.Name = "GbGeneral";
            GbGeneral.Size = new Size(401, 229);
            GbGeneral.TabIndex = 13;
            GbGeneral.TabStop = false;
            GbGeneral.Text = "GbGeneral";
            // 
            // CbDmsFiling
            // 
            CbDmsFiling.AutoSize = true;
            CbDmsFiling.Location = new Point(6, 47);
            CbDmsFiling.Name = "CbDmsFiling";
            CbDmsFiling.Size = new Size(94, 19);
            CbDmsFiling.TabIndex = 1;
            CbDmsFiling.Text = "CbDmsFiling";
            CbDmsFiling.UseVisualStyleBackColor = true;
            // 
            // CbSignature
            // 
            CbSignature.AutoSize = true;
            CbSignature.Location = new Point(6, 22);
            CbSignature.Name = "CbSignature";
            CbSignature.Size = new Size(91, 19);
            CbSignature.TabIndex = 0;
            CbSignature.Text = "CbSignature";
            CbSignature.UseVisualStyleBackColor = true;
            // 
            // FConfig
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(246, 246, 246);
            ClientSize = new Size(425, 253);
            Controls.Add(GbGeneral);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FConfig";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "FConfig";
            FormClosing += FConfig_FormClosing;
            Load += FConfig_Load;
            GbGeneral.ResumeLayout(false);
            GbGeneral.PerformLayout();
            ResumeLayout(false);
        }

        #endregion
        private GroupBox GbGeneral;
        private CheckBox CbSignature;
        private CheckBox CbDmsFiling;
    }
}