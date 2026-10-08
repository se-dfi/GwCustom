using GwCustom.Custom;
using GwCustom.Forms;
using GwCustom.Models;
using GwCustomUI.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.Actions {
    public class MenuStorage : Execute {
        /// <summary>
        /// Verarbeitung, Durchführung der Ablage im DMS und Auslösen der Weiterverarbeitung im Ablageclient
        /// </summary>
        /// <returns></returns>
        public override bool RunBefore() {
            // enaio AppConnector Verfügbarkeitsprüfung
            if (!AppConnector.CheckReachable().GetAwaiter().GetResult()) {
                throw new Exception(MultiLanguage.GetText("noappconnector"));
            }

            // Fortschrittsdialog anzeigen
            FUpload FormProgress = new(this);
            FormProgress.Initialize();
            FormProgress.Show();
            Application.DoEvents();

            try {
                // Betroffene Nachrichten aus Auswahl oder aktivem Fenster ermitteln und verarbeiten
                List<GroupwareTypeLibrary.Message> msgList = GroupwiseUtil.GetCurrentMessages();
                int i = 0;

                // Keine Objekte gefunden?
                if (msgList.Count < 1) {
                    throw new Exception(MultiLanguage.GetText("nomessage"));
                }

                // Zuviele Objekte gefunden
                if (msgList.Count > CommonUtil.GetConfig().StorageLimit) {
                    throw new Exception(String.Format(MultiLanguage.GetText("maxmessages"), CommonUtil.GetConfig().StorageLimit, msgList.Count));
                }

                // Liste an Dms Dokument-Ids für Aufruf des Ablageclient
                List<ulong> ids = [];

                // Reguläre Verarbeitung
                foreach (GroupwareTypeLibrary.Message msg in msgList) {
                    // Verarbeitungsfortschritt aktualisieren
                    DmsProgressArgs args = new();
                    i++;
                    args.Current = i;
                    args.Maximum = msgList.Count;
                    args.Information = GroupwiseUtil.GetMessageShortForm(msg);
                    args.Cancel = false;
                    OnDmsProgress(args);

                    // Groupwise-Dokumentobjekt erstellen
                    GwDocument document = new(msg);

                    // Dms-Dokumentenobjektliste erstellen und mit eventuell vorhandenen Dokumenten befüllen
                    List<DmsDocument> documents = document.GetDmsDocuments();
                    if (documents.Count > 0) {
                        // Dokument ist bereits vorhanden, kein Upload erforderlich
                        foreach (DmsDocument item in documents) {
                            ids.Add(item.Id);
                        }

                        // GUID im DMS setzen falls möglich und erforderlich
                        foreach (var dmsDocument in documents) {
                            if (string.IsNullOrEmpty(dmsDocument.SentGuid) && !string.IsNullOrEmpty(document.SentGuid)) {
                                AppConnector.SetGuidByDocumentId(dmsDocument.Id, document.SentGuid).GetAwaiter().GetResult();
                            }
                        }
                    } else {
                        // E-Mail als temporäre Datei exportieren
                        string exportFile = Path.Combine(Path.GetTempPath(), $"{APPSHORT}_{DateTime.Now.ToFileTime()}.ima");
                        try {
                            // Upload, falls Standort nicht vorhanden und ermittelbar, wird die Outbox des Benutzers verwendet und gegebenenfalls erstellt
                            GroupwiseUtil.ExportMessage(msg, exportFile);
                            ulong dmsId = AppConnector.CreateDocumentAsync(document, document.GetDmsTargetLocation(), exportFile).GetAwaiter().GetResult();
                            if (dmsId > 0) {
                                ids.Add(dmsId);
                            }
                        } finally {
                            // Temporärdatei löschen
                            if (File.Exists(exportFile)) {
                                File.Delete(exportFile);
                            }
                        }
                    }

                    // (later) todo Ablagestatus in Groupwise per Kategorie und Feld mit Projektnummer setzen und auf Stand halten
                }

                // Ablageclient aufrufen
                GwCustomUI.Classes.CommonUtil.ShowDmsClient(ids);// Beim Start Update der Signaturen erzwingen
            } catch {
                // Formular schließen
                FormProgress.Close();

                throw;
            }

            // Formular ausblenden
            FormProgress.CloseFade();

            return false;
        }

        public override void Run() {
            RunBefore();
        }
    }
}