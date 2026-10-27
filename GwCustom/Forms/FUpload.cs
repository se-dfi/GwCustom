using GwCustom.Custom;
using GwCustom.Actions;
using GwCustomUI.Classes;
using System.Windows.Forms;
using System;
using System.Drawing;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.Forms
{
    public partial class FUpload : Form
    {
        private readonly Execute MenuDms;

        /// <summary>
        /// Form-Konstruktor
        /// </summary>
        public FUpload(Execute initiator)
        {
            InitializeComponent();
            MenuDms = initiator;
            MenuDms.DmsProgress += MenuDms_DmsProgress;
        }

        /// <summary>
        /// Schatten für randloses Fenster aktivieren
        /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        /// <summary>
        /// Timer für ein- und ausblenden des Formulars
        /// </summary>
        readonly System.Windows.Forms.Timer timer = new();

        /// <summary>
        /// Formular laden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FDmsStorage_Load(object sender, EventArgs e)
        {
            // Timer um Formular langsam ausblenden
            Opacity = 1;
            timer.Interval = 10;
            timer.Tick += new EventHandler(CloseFade);
        }

        /// <summary>
        /// Formular initialisieren, Aufruf manuell vor Load
        /// </summary>
        public void Initialize()
        {
            // Formular unsichtbar initialisieren
            Visible = false;

            // Startposition mittig auf Bildschirm mit Cursor
            StartPosition = FormStartPosition.Manual;
            Screen screen = Screen.FromPoint(Cursor.Position);
            Rectangle area = screen.WorkingArea;
            Location = new(area.Left + area.Width / 2 - Width / 2, area.Top + area.Height / 2 - Height / 2);

            // Formulareigenschaften
            ControlBox = false;
            HelpButton = false;
            MaximizeBox = false;
            MinimizeBox = false;
            ShowIcon = false;
            ShowInTaskbar = false;
            TopMost = true;
            FormBorderStyle = FormBorderStyle.None;

            // Textfelder initialisieren
            Text = MultiLanguage.GetText("dmsstorage");
            LTitle.Text = MultiLanguage.GetText("dmsstorage");
            LProgress.Text = String.Empty;
            LDetail.Text = MultiLanguage.GetText("startprocess");

            // Formular sichtbar machen
            Visible = true;
        }

        /// <summary>
        /// EventHandler für Fortschritt
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void MenuDms_DmsProgress(object? sender, DmsProgressArgs e)
        {
            // Fortschrittsmeldungen aus asynchroner Ablage auf den Formularthread holen
            if (InvokeRequired)
            {
                BeginInvoke((Action)(() => MenuDms_DmsProgress(sender, e)));
                return;
            }

            Refresh();
            Application.DoEvents();
            LProgress.Text = String.Format(MultiLanguage.GetText("dmsprogress"), e.Current.ToString(), e.Maximum.ToString());
            LDetail.Text = e.Information;
            Refresh();
            Application.DoEvents();
        }

        /// <summary>
        /// Langsames Ausblenden des Formulars initialisieren
        /// </summary>
        public void CloseFade()
        {
            CloseFade(null, new EventArgs());
        }

        /// <summary>
        /// Langsames Ausblenden des Formulars durchführen
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseFade(object? sender, EventArgs e)
        {
            if (Opacity >= 1)
            {
                timer.Start();
                Opacity -= 0.02;
            }
            else if (Opacity <= 0)
            {
                timer.Stop();
                Close();
            }
            else
            {
                Opacity -= 0.02;
            }
        }
    }
}