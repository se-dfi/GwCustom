using GwCustomUI.Classes;
using System;
using System.Drawing;
using System.Windows.Forms;
using static GwCustomUI.Classes.Constants;

namespace GwCustomUI.Forms
{
    public partial class FError : Form
    {
        /// <summary>
        /// Form-Konstruktor
        /// </summary>
        public FError(string message)
        {
            InitializeComponent();
            LDescription.Text = message;
        }

        /// <summary>
        /// Formular laden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FError_Load(object sender, EventArgs e)
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
            Text = $"{APPNAME} - {MultiLanguage.GetText("error")}";
            LError.Text = MultiLanguage.GetText("errorlabel");
            LInfo.Text = MultiLanguage.GetText("errorinfo");

            // Formular sichtbar machen
            Visible = true;
        }

        /// <summary>
        /// Formular schließen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BClose_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }

        /// <summary>
        /// Fehler zur Behebung melden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BReport_Click(object sender, EventArgs e)
        {
            // Funktion folgt, derzeit deaktiviert:
            // Per Mail den Benutzer, die Fehlermeldung und den Stacktrace versenden an DFI
        }
    }
}