using GwCustom.Custom;
using GwCustom.Models;
using GwCustomUI.Classes;
using System;
using System.Drawing;
using System.Windows.Forms;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.Forms
{
    public partial class FConfig : Form
    {
        private readonly ConfigModel config;

        /// <summary>
        /// Form-Konstruktor
        /// </summary>
        public FConfig()
        {
            InitializeComponent();
            config = CommonUtil.GetConfig();
        }
                
        /// <summary>
        /// Formular laden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FConfig_Load(object sender, EventArgs e)
        {
            // Formular unsichtbar initialisieren
            Visible = false;

            // Startposition mittig auf Bildschirm mit Cursor
            StartPosition = FormStartPosition.Manual;
            Screen screen = Screen.FromPoint(Cursor.Position);
            Rectangle area = screen.WorkingArea;
            Location = new(area.Left + area.Width / 2 - Width / 2, area.Top + area.Height / 2 - Height / 2);

            // Formulareigenschaften
            ControlBox = true;
            HelpButton = false;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            TopMost = true;
            FormBorderStyle = FormBorderStyle.FixedSingle;

            // Steuerelemente initialisieren
            Text = $"{APPNAME} - {MultiLanguage.GetText("settings")}";
            GbGeneral.Text = MultiLanguage.GetText("general");

            // Einstellungs-Steuerelemente vorbereiten
            CbSignature.Text = MultiLanguage.GetText("signaturelayout");
            CbSignature.Checked = config.SignatureEnabled;
            CbDmsFiling.Text = MultiLanguage.GetText("dmsstorage");
            CbDmsFiling.Checked = config.DmsFilingEnabled;

            // Ladevorgang abgeschlossen, Formular sichtbar machen
            DialogResult = DialogResult.None;

            // Formular sichtbar machen
            Visible = true;
        }

        /// <summary>
        /// Abschließendes Speichern der Konfiguration
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FConfig_FormClosing(object sender, FormClosingEventArgs e)
        {
            config.SignatureEnabled = CbSignature.Checked;
            config.DmsFilingEnabled = CbDmsFiling.Checked;
            config.Save();
        }
    }
}