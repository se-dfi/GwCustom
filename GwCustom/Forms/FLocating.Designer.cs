using System.Drawing;
using System.Windows.Forms;

namespace GwCustom.Forms
{
    partial class FLocating
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
            TvStandorte = new TreeView();
            TbInfo = new TextBox();
            LZeit = new Label();
            LBetreff = new Label();
            TbBetreff = new TextBox();
            LVon = new Label();
            TbVon = new TextBox();
            LStandorte = new Label();
            LGuid = new Label();
            LbVerteilung = new ListBox();
            LVerteilung = new Label();
            TbZeit = new TextBox();
            TbGuid = new TextBox();
            SuspendLayout();
            // 
            // TvStandorte
            // 
            TvStandorte.Location = new Point(12, 178);
            TvStandorte.Name = "TvStandorte";
            TvStandorte.Size = new Size(478, 304);
            TvStandorte.TabIndex = 0;
            // 
            // TbInfo
            // 
            TbInfo.BackColor = Color.FromArgb(246, 246, 246);
            TbInfo.BorderStyle = BorderStyle.None;
            TbInfo.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            TbInfo.Location = new Point(12, 19);
            TbInfo.Margin = new Padding(10);
            TbInfo.Name = "TbInfo";
            TbInfo.Size = new Size(704, 16);
            TbInfo.TabIndex = 3;
            // 
            // LZeit
            // 
            LZeit.AutoSize = true;
            LZeit.BackColor = Color.FromArgb(246, 246, 246);
            LZeit.ForeColor = SystemColors.InfoText;
            LZeit.Location = new Point(12, 53);
            LZeit.Name = "LZeit";
            LZeit.Size = new Size(91, 15);
            LZeit.TabIndex = 5;
            LZeit.Text = "Datum / Uhrzeit";
            // 
            // LBetreff
            // 
            LBetreff.AutoSize = true;
            LBetreff.BackColor = Color.FromArgb(246, 246, 246);
            LBetreff.ForeColor = SystemColors.InfoText;
            LBetreff.Location = new Point(12, 107);
            LBetreff.Name = "LBetreff";
            LBetreff.Size = new Size(42, 15);
            LBetreff.TabIndex = 7;
            LBetreff.Text = "Betreff";
            // 
            // TbBetreff
            // 
            TbBetreff.HideSelection = false;
            TbBetreff.Location = new Point(12, 125);
            TbBetreff.MaxLength = 19;
            TbBetreff.Name = "TbBetreff";
            TbBetreff.ReadOnly = true;
            TbBetreff.Size = new Size(704, 23);
            TbBetreff.TabIndex = 6;
            // 
            // LVon
            // 
            LVon.AutoSize = true;
            LVon.BackColor = Color.FromArgb(246, 246, 246);
            LVon.ForeColor = SystemColors.InfoText;
            LVon.Location = new Point(128, 53);
            LVon.Name = "LVon";
            LVon.Size = new Size(27, 15);
            LVon.TabIndex = 9;
            LVon.Text = "Von";
            // 
            // TbVon
            // 
            TbVon.HideSelection = false;
            TbVon.Location = new Point(128, 71);
            TbVon.MaxLength = 19;
            TbVon.Name = "TbVon";
            TbVon.ReadOnly = true;
            TbVon.Size = new Size(352, 23);
            TbVon.TabIndex = 8;
            // 
            // LStandorte
            // 
            LStandorte.AutoSize = true;
            LStandorte.ForeColor = SystemColors.InfoText;
            LStandorte.Location = new Point(12, 160);
            LStandorte.Name = "LStandorte";
            LStandorte.Size = new Size(58, 15);
            LStandorte.TabIndex = 10;
            LStandorte.Text = "Standorte";
            // 
            // LGuid
            // 
            LGuid.AutoSize = true;
            LGuid.BackColor = Color.FromArgb(246, 246, 246);
            LGuid.ForeColor = SystemColors.InfoText;
            LGuid.Location = new Point(486, 53);
            LGuid.Name = "LGuid";
            LGuid.Size = new Size(34, 15);
            LGuid.TabIndex = 12;
            LGuid.Text = "GUID";
            // 
            // LbVerteilung
            // 
            LbVerteilung.ColumnWidth = 215;
            LbVerteilung.FormattingEnabled = true;
            LbVerteilung.ItemHeight = 15;
            LbVerteilung.Location = new Point(496, 178);
            LbVerteilung.MultiColumn = true;
            LbVerteilung.Name = "LbVerteilung";
            LbVerteilung.SelectionMode = SelectionMode.None;
            LbVerteilung.Size = new Size(220, 304);
            LbVerteilung.TabIndex = 13;
            // 
            // LVerteilung
            // 
            LVerteilung.AutoSize = true;
            LVerteilung.BackColor = Color.FromArgb(246, 246, 246);
            LVerteilung.ForeColor = SystemColors.InfoText;
            LVerteilung.Location = new Point(496, 160);
            LVerteilung.Name = "LVerteilung";
            LVerteilung.Size = new Size(78, 15);
            LVerteilung.TabIndex = 14;
            LVerteilung.Text = "Verteilhistorie";
            // 
            // TbZeit
            // 
            TbZeit.HideSelection = false;
            TbZeit.Location = new Point(12, 71);
            TbZeit.MaxLength = 19;
            TbZeit.Name = "TbZeit";
            TbZeit.ReadOnly = true;
            TbZeit.Size = new Size(110, 23);
            TbZeit.TabIndex = 4;
            // 
            // TbGuid
            // 
            TbGuid.HideSelection = false;
            TbGuid.Location = new Point(486, 71);
            TbGuid.MaxLength = 12;
            TbGuid.Name = "TbGuid";
            TbGuid.ReadOnly = true;
            TbGuid.Size = new Size(230, 23);
            TbGuid.TabIndex = 11;
            TbGuid.TextAlign = HorizontalAlignment.Center;
            // 
            // FLocating
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(246, 246, 246);
            ClientSize = new Size(728, 495);
            ControlBox = false;
            Controls.Add(LVerteilung);
            Controls.Add(LbVerteilung);
            Controls.Add(LGuid);
            Controls.Add(TbGuid);
            Controls.Add(LStandorte);
            Controls.Add(LVon);
            Controls.Add(TbVon);
            Controls.Add(LBetreff);
            Controls.Add(TbBetreff);
            Controls.Add(LZeit);
            Controls.Add(TbZeit);
            Controls.Add(TbInfo);
            Controls.Add(TvStandorte);
            ForeColor = SystemColors.Control;
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "FLocating";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "FLocation";
            Load += FLocation_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TreeView TvStandorte;
        private TextBox TbInfo;
        private Label LZeit;
        private Label LBetreff;
        private TextBox TbBetreff;
        private Label LVon;
        private TextBox TbVon;
        private Label LStandorte;
        private Label LGuid;
        private ListBox LbVerteilung;
        private Label LVerteilung;
        private TextBox TbZeit;
        private TextBox TbGuid;
    }
}