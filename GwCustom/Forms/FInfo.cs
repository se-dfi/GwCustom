using GwCustom.Custom;
using GwCustomUI.Classes;
using System;
using System.Drawing;
using System.Windows.Forms;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.Forms
{
    public partial class FInfo : Form
    {
        /// <summary>
        /// Form-Konstruktor
        /// </summary>
        public FInfo()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Formular laden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FInfo_Load(object sender, EventArgs e)
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
            Text = $"{APPNAME} - {MultiLanguage.GetText("information")}";
            LAuthor.Text = MultiLanguage.GetText("global.appauthor");
            LVersion.Text = $"Version {APPVERSION} - {(APPRELEASEDATE.ToShortDateString())}";
            LDescription.Text = MultiLanguage.GetText("global.appdescription");

            // Formular sichtbar machen
            Visible = true;
        }
    }
}