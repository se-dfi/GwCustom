using GwCustom.Custom;
using GwCustom.Forms;
using GwCustom.Models;
using GwCustomUI.Classes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.Actions {
    public class MenuStorage : Execute {
        /// <summary>
        /// Flag gegen parallele Ablaufläufe
        /// </summary>
        private static int isRunning;

        public override bool RunBefore() {
            return false;
        }

        /// <summary>
        /// Ausführung, Vorabprüfungen und Groupwise-COM-Zugriffe synchron auf dem Hauptthread, die Ablage selbst asynchron
        /// </summary>
        public override void Run() {
            // Erneuten Aufruf während laufender Ablage ignorieren
            if (Interlocked.CompareExchange(ref isRunning, 1, 0) != 0) {
                return;
            }

            try {
                // Betroffene Nachrichten aus Auswahl oder aktivem Fenster ermitteln
                List<GroupwareTypeLibrary.Message> msgList = GroupwiseUtil.GetCurrentMessages();

                // Keine Objekte gefunden?
                if (msgList.Count < 1) {
                    throw new Exception(MultiLanguage.GetText("nomessage"));
                }

                // Zuviele Objekte gefunden
                if (msgList.Count > CommonUtil.GetConfig().StorageLimit) {
                    throw new Exception(String.Format(MultiLanguage.GetText("maxmessages"), CommonUtil.GetConfig().StorageLimit, msgList.Count));
                }

                // Verarbeitungsdaten vorab ermitteln, Groupwise-COM-Zugriffe müssen auf dem Hauptthread erfolgen
                List<StorageItem> items = [];
                foreach (GroupwareTypeLibrary.Message msg in msgList) {
                    items.Add(new StorageItem(msg, new GwDocument(msg), GroupwiseUtil.GetMessageShortForm(msg)));
                }

                // Fortschrittsdialog anzeigen
                FUpload FormProgress = new(this);
                FormProgress.Initialize();
                FormProgress.Show();
                Application.DoEvents();

                // Ablage asynchron ohne den GroupWise-Hauptthread zu blockieren
                _ = ProcessStorage(items, FormProgress);
            }
            catch {
                Interlocked.Exchange(ref isRunning, 0);
                throw;
            }
        }

        /// <summary>
        /// Verarbeitung, Durchführung der Ablage im DMS und Auslösen der Weiterverarbeitung im Ablageclient
        /// </summary>
        private async Task ProcessStorage(List<StorageItem> items, FUpload FormProgress) {
            try {
                // enaio AppConnector Verfügbarkeitsprüfung
                if (!await AppConnector.CheckReachable()) {
                    throw new Exception(MultiLanguage.GetText("noappconnector"));
                }

                // Liste an Dms Dokument-Ids für Aufruf des Ablageclient
                List<ulong> ids = [];
                int i = 0;

                // Reguläre Verarbeitung
                foreach (StorageItem item in items) {
                    // Verarbeitungsfortschritt aktualisieren
                    DmsProgressArgs args = new();
                    i++;
                    args.Current = i;
                    args.Maximum = items.Count;
                    args.Information = item.ShortForm;
                    args.Cancel = false;
                    OnDmsProgress(args);

                    // Dms-Dokumentenobjektliste erstellen und mit eventuell vorhandenen Dokumenten befüllen
                    List<DmsDocument> documents = await item.Document.GetDmsDocumentsAsync();
                    if (documents.Count > 0) {
                        // Dokument ist bereits vorhanden, kein Upload erforderlich
                        foreach (DmsDocument dmsDocument in documents) {
                            ids.Add(dmsDocument.Id);

                            // GUID im DMS setzen falls möglich und erforderlich
                            if (string.IsNullOrEmpty(dmsDocument.SentGuid) && !string.IsNullOrEmpty(item.Document.SentGuid)) {
                                await AppConnector.SetGuidByDocumentId(dmsDocument.Id, item.Document.SentGuid);
                            }
                        }
                    } else {
                        // E-Mail als temporäre Datei exportieren, Groupwise-Zugriff wechselt dafür zurück auf den Hauptthread
                        string exportFile = Path.Combine(Path.GetTempPath(), $"{APPSHORT}_{DateTime.Now.ToFileTime()}.ima");
                        try {
                            // Upload, falls Standort nicht vorhanden und ermittelbar, wird die Outbox des Benutzers verwendet und gegebenenfalls erstellt
                            GroupwareTypeLibrary.Message msg = item.Message;
                            FormProgress.Invoke((Action)(() => GroupwiseUtil.ExportMessage(msg, exportFile)));
                            ulong dmsId = await AppConnector.CreateDocumentAsync(item.Document, await item.Document.GetDmsTargetLocationAsync(), exportFile);
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
                GwCustomUI.Classes.CommonUtil.ShowDmsClient(ids);

                // Formular ausblenden
                FormProgress.Invoke((Action)(() => FormProgress.CloseFade()));
            } catch (Exception e) {
                // Formular schließen
                try {
                    FormProgress.Invoke((Action)(() => FormProgress.Close()));
                } catch { }

                CommonUtil.GlobalErrorHandler(e);
            } finally {
                Interlocked.Exchange(ref isRunning, 0);
            }
        }

        /// <summary>
        /// Vorab ermittelte Verarbeitungsdaten einer Nachricht, alle Groupwise-COM-Zugriffe erfolgen beim Anlegen
        /// </summary>
        private sealed class StorageItem(GroupwareTypeLibrary.Message message, GwDocument document, string shortForm) {
            public GroupwareTypeLibrary.Message Message { get; } = message;

            public GwDocument Document { get; } = document;

            public string ShortForm { get; } = shortForm;
        }
    }
}
