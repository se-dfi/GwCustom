using GwCustom.Classes;
using GwCustom.Custom;
using GwCustom.Forms;
using GwCustom.Models;
using GwCustomUI.Classes;
using GwCustomUI.Signatures;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Windows.Forms;
namespace GwCustom.Actions {
    public class CmdSend : Execute {
        /// <summary>
        /// Ausführung vor Groupwise Event = Dialog mit Vorschau
        /// </summary>
        /// <returns>Fortsetzung unterbrechen</returns>
        public override bool RunBefore() {
            // Prüfung auf leeren Empfängerkreis, Bugfix Layout-Dopplung
            GroupwiseUtil.GetGwCommander().Execute($"ItemGetText(\"X00\"; 0)", out string to);
            GroupwiseUtil.GetGwCommander().Execute($"ItemGetText(\"X00\"; 2)", out string cc);
            GroupwiseUtil.GetGwCommander().Execute($"ItemGetText(\"X00\"; 3)", out string bc);
            if (string.IsNullOrEmpty(to) && string.IsNullOrEmpty(cc) && string.IsNullOrEmpty(bc)) {
                throw new Exception(MultiLanguage.GetText("norecipient"));
            }

            // GUID zuweisen und in X-Field schreiben
            string sentguid = GroupwiseUtil.GetXFieldValue("X00", Constants.SENTGUIDCLASS);
            if (string.IsNullOrEmpty(sentguid)) {
                sentguid = GroupwiseUtil.CreateSentGuid("X00");
            }

            // .os Anhänge entfernen (erneut obwohl bereits bei Antwort und Weiterleitung, wegen Schnelloption "inklusive Anlagen des Senders") 
            GroupwiseUtil.RemoveDmsAttachmentsFromView();

            // Signaturen laden falls aktiviert und nicht vorhanden oder abweichender Benutzer, dann Dialog öffnen
            if (GwCustomUI.Classes.CommonUtil.GetConfig().SignatureEnabled) {
                // Groupwise-Signatur deaktivieren
                GroupwiseUtil.GetGwClientState().IsSignatureAllowed = C3POTypeLibrary.SignatureInsertionConstants.eGWSignatureNone;

                // Signaturen aktualisieren (erfolgt nur falls notwendig)
                SignatureManagement.UpdateSignatures(GroupwiseUtil.GetGwUser()).GetAwaiter().GetResult();

                // Signaturverwaltung
                SignatureManagement sm = new(GroupwiseUtil.GetGwUser(), new SignatureTransport(GroupwiseUtil.RestructureHtmlFromView(sentguid, false)));
                bool cancel = !sm.Dialog();
                if (!cancel) {
                    GroupwiseUtil.SetMessageHtml(sm.GetModifiedBody());
                    SignatureStatic.SentGuid = sentguid;
                    SignatureStatic.Storage = sm.GetStorage();
                }
                return cancel;
            } else {
                // E-Mail Codestruktur auch ohne Layout und Signatur anpassen und SentGuid einfügen
                GroupwiseUtil.SetMessageHtml(GroupwiseUtil.RestructureHtmlFromView(sentguid));
            }

            return false;
        }

        /// <summary>
        /// Ausführung nach Groupwise Event / Ablage nach Mailversand
        /// </summary>
        public override void Run() {
            // Hinweis: Sofern eine E-Mailablage stattfinden soll, handelt es sich beim Versand immer um ein neues, hochzuladendes Objekt.
            // Eine Suche nach vorhandenen DMS Objekten kann man sich sparen, lediglich die Standortermittlung aus Historie oder Barcode kommt in Frage.

            // Ablage nur bei entsprechendem Sendebutton und vorhandener GUID
            if (SignatureStatic.Storage && !string.IsNullOrEmpty(SignatureStatic.SentGuid)) {
                // enaio AppConnector Verfügbarkeitsprüfung
                if (!AppConnector.CheckReachable().GetAwaiter().GetResult()) {
                    throw new Exception(MultiLanguage.GetText("noappconnector"));
                }

                // Fortschrittsdialog anzeigen
                FUpload FormProgress = new(this);
                FormProgress.Initialize();
                FormProgress.Show();

                try {
                    // Verarbeitungsfortschritt aktualisieren
                    DmsProgressArgs args = new() {
                        Current = 1,
                        Maximum = 1,
                        Information = MultiLanguage.GetText("getsentmessage"),
                        Cancel = false
                    };
                    OnDmsProgress(args);
                    Application.DoEvents();

                    // Versendete Nachricht ermitteln und Ablage vorbereiten
                    GroupwareTypeLibrary.Message msg = GroupwiseUtil.GetSentMessage(SignatureStatic.SentGuid);
                    GwDocument document = new(msg);
                    ulong dmsId;

                    // Statische Werte zurücksetzen
                    SignatureStatic.SentGuid = string.Empty;
                    SignatureStatic.Storage = false;

                    // Standortermittlung aus History-Einträgen
                    args.Information = MultiLanguage.GetText("gettargetlocation");
                    OnDmsProgress(args);
                    DmsLocation? location = document.GetDmsTargetLocation();

                    // Upload ins Dms mit Indexdaten und Ermittlung der Id
                    args.Information = MultiLanguage.GetText("dmsupload");
                    OnDmsProgress(args);
                    string exportFile = Path.Combine(Path.GetTempPath(), $"{Constants.APPSHORT}_{DateTime.Now.ToFileTime()}.ima");
                    try {
                        // Upload, falls Standort(e) nicht vorhanden, wird die Outbox des Benutzers verwendet und gegebenenfalls erstellt
                        GroupwiseUtil.ExportMessage(msg, exportFile);
                        dmsId = AppConnector.CreateDocumentAsync(document, location, exportFile).GetAwaiter().GetResult();
                    } finally {
                        // Temporärdatei löschen
                        if (File.Exists(exportFile)) {
                            File.Delete(exportFile);
                        }
                    }

                    // Ablageclient aufrufen
                    if (dmsId > 0) {
                        GwCustomUI.Classes.CommonUtil.ShowDmsClient([dmsId]);
                    }
                } catch {
                    // Formular schließen
                    FormProgress.Close();

                    throw;
                }

                // Formular ausblenden
                FormProgress.CloseFade();
            }
        }
    }
}