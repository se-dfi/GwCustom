using GwCustom.Classes;
using GwCustom.Custom;
using GwCustom.Models;
using GwCustomUI.Classes;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Reflection.Metadata;
using System.Windows.Forms;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.Forms
{
    public partial class FLocating : Form
    {
        readonly GwDocument message;

        public FLocating(GwDocument message)
        {
            this.message = message;
            InitializeComponent();
        }

        /// <summary>
        /// Formular laden
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void FLocation_Load(object sender, EventArgs e)
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
            Text = $"{APPNAME} - {MultiLanguage.GetText("dmslocating")}";
            TbInfo.Text = MultiLanguage.GetText("dmslocatingprogress");

            // Formular sichtbar machen
            Visible = true;

            // enaio AppConnector Verfügbarkeitsprüfung
            if (!await AppConnector.CheckReachable())
            {
                throw new Exception(MultiLanguage.GetText("noappconnector"));
            }

            // zur Groupwise-Nachricht zugehörige Dokumente aus dem DMS ermitteln
            List<DmsDocument> documents = await message.GetDmsDocumentsAsync();

            // Für die gefundenen Dokumente die Informationen inklusive Standorte asynchron ermitteln und in eine Gesamtliste zusammenfassen
            List<string> distribution = [];
            List<DmsLocation> locations = [];
            foreach (DmsDocument document in documents)
            {
                foreach (string distributor in document.Distribution)
                {
                    if (!distribution.Contains(distributor))
                    {
                        distribution.Add(distributor);
                    }
                }

                foreach (DmsLocation location in document.Locations)
                {
                    if (!locations.Contains(location))
                    {
                        locations.Add(location);
                    }
                }
            }

            // Ausgabe der Dokumentinformationen
            LZeit.Text = MultiLanguage.GetText("datetime");
            TbZeit.Text = $"{message.CreationDate} {message.CreationTime}";
            LVon.Text = MultiLanguage.GetText("sender");
            TbVon.Text = message.MailAddressFrom;
            LBetreff.Text = MultiLanguage.GetText("subject");
            TbBetreff.Text = message.Subject;
            TbGuid.Text = string.IsNullOrEmpty(message.SentGuid) ? MultiLanguage.GetText("empty") : message.SentGuid;

            // Verteilhistorie
            LVerteilung.Text = MultiLanguage.GetText("distributionhistory");
            LbVerteilung.Items.Clear();
            distribution.Sort();
            foreach (string entry in distribution)
            {
                LbVerteilung.Items.Add(entry);
            }

            // Dokumentenanzahl unterscheiden inklusive textuellem Hinweis auf dem Formular
            if (documents.Count == 0) // Kein Treffer
            {
                TbInfo.Text = MultiLanguage.GetText("dmsnomessage");
            }
            else if (documents.Count == 1) // Eindeutiger Treffer
            {
                TbInfo.Text = string.Format(MultiLanguage.GetText("dmssinglemessage"), documents[0].Id.ToString());
            }
            else if (documents.Count > 1) // Mehrere Vorkommen
            {
                string ids = string.Empty;
                foreach (DmsDocument document in documents)
                {
                    if (ids.Length > 0)
                    {
                        ids += ", ";
                    }
                    ids += document.Id.ToString();
                }
                TbInfo.Text = string.Format(MultiLanguage.GetText("dmsmultiplemessage"), documents.Count.ToString(), ids);
            }
            GetLocationsTree(locations);
        }

        /// <summary>
        /// Standortinformationen aufbauen
        /// </summary>
        /// <exception cref="Exception"></exception>
        private void GetLocationsTree(List<DmsLocation> locations)
        {
            // Treeview initialisieren
            ImageList ILIcons = new();
            ILIcons.Images.Clear();
            ILIcons.Images.Add(Image.FromStream(new MemoryStream(Properties.Resources.client)));
            ILIcons.Images.Add(Image.FromStream(new MemoryStream(Properties.Resources.type)));
            ILIcons.Images.Add(Image.FromStream(new MemoryStream(Properties.Resources.folder)));

            TvStandorte.ImageList = ILIcons;
            TvStandorte.Nodes.Clear();

            // Standortermittlung bzw. Informationsausgabe
            foreach (DmsLocation dmsLocation in locations)
            {
                string client = $"{MultiLanguage.GetText("client")} {(string.IsNullOrEmpty(dmsLocation.Client) ? MultiLanguage.GetText("empty") : dmsLocation.Client)}";
                string type = $"{MultiLanguage.GetText("type")} {(string.IsNullOrEmpty(dmsLocation.Type) ? MultiLanguage.GetText("empty") : dmsLocation.Type)}";

                // Clients
                TreeNode? clientNode = TvStandorte.Nodes.Find(client, false).Length > 0 ? TvStandorte.Nodes.Find(client, false)[0] : null;
                if (clientNode == null)
                {
                    clientNode = TvStandorte.Nodes.Add(client, client); // key und name, nötig sonst wird der Node bei weiteren Einträgen nicht gefunden
                    clientNode.ImageIndex = 0;
                    clientNode.SelectedImageIndex = 0;
                }

                // Types
                TreeNode? typeNode = clientNode != null ? (clientNode.Nodes.Find(type, false).Length > 0 ? clientNode.Nodes.Find(type, false)[0] : null) : null;
                if (typeNode == null && clientNode != null)
                {
                    typeNode = clientNode.Nodes.Add(type, type); // key und name, nötig sonst wird der Node bei weiteren Einträgen nicht gefunden
                    typeNode.ImageIndex = 1;
                    typeNode.SelectedImageIndex = 1;
                }

                // Folders
                TreeNode? folderNode;
                if (typeNode != null)
                {
                    folderNode = typeNode.Nodes.Add(dmsLocation.GetInfo());
                    folderNode.ImageIndex = 2;
                    folderNode.SelectedImageIndex = 2;
                }
            }

            // Treeview fertigstellen und aufklappen
            TvStandorte.ExpandAll();
        }
    }
}