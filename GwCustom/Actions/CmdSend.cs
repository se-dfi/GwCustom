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
using System.Threading.Tasks;
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
        /// Ausführung nach Groupwise Event / Ablage nach Mailversand, Vorabprüfungen synchron, die Ablage selbst asynchron
        /// </summary>
        public override void Run() {
            // Hinweis: Sofern eine E-Mailablage stattfinden soll, handelt es sich beim Versand immer um ein neues, hochzuladendes Objekt.
            // Eine Suche nach vorhandenen DMS Objekten kann man sich sparen, lediglich die Standortermittlung aus Historie oder Barcode kommt in Frage.

            // Ablage nur bei entsprechendem Sendebutton und vorhandener GUID
            if (SignatureStatic.Storage && !string.IsNullOrEmpty(SignatureStatic.SentGuid)) {
                // Statische Werte zurücksetzen, die asynchrone Verarbeitung arbeitet mit der lokalen Kopie
                string sentGuid = SignatureStatic.SentGuid!;
                SignatureStatic.SentGuid = string.Empty;
                SignatureStatic.Storage = false;

                // Fortschrittsdialog anzeigen
                FUpload FormProgress = new(this);
                FormProgress.Initialize();
                FormProgress.Show();

                // Ablage asynchron ohne den Groupwise-Hauptthread zu blockieren
                _ = ProcessStorage(sentGuid, FormProgress);
            }
        }

        /// <summary>
        /// Durchführung der Ablage im DMS und Auslösen der Weiterverarbeitung im Ablageclient
        /// </summary>
        private async Task ProcessStorage(string sentGuid, FUpload FormProgress) {
            try {
                // enaio AppConnector Verfügbarkeitsprüfung
                if (!await AppConnector.CheckReachable()) {
                    throw new Exception(MultiLanguage.GetText("noappconnector"));
                }

                // Verarbeitungsfortschritt aktualisieren
                DmsProgressArgs args = new() {
                    Current = 1,
                    Maximum = 1,
                    Information = MultiLanguage.GetText("getsentmessage"),
                    Cancel = false
                };
                OnDmsProgress(args);

                // Versendete Nachricht ermitteln und Ablage vorbereiten, Groupwise-Zugriff wechselt dafür zurück auf den Hauptthread
                GroupwareTypeLibrary.Message? msg = null;
                GwDocument? document = null;
                FormProgress.Invoke((Action)(() => {
                    msg = GroupwiseUtil.GetSentMessage(sentGuid);
                    document = new GwDocument(msg);
                }));

                // Standortermittlung aus History-Einträgen
                args.Information = MultiLanguage.GetText("gettargetlocation");
                OnDmsProgress(args);
                DmsLocation? location = await document!.GetDmsTargetLocationAsync();

                // Upload ins Dms mit Indexdaten und Ermittlung der Id
                args.Information = MultiLanguage.GetText("dmsupload");
                OnDmsProgress(args);
                string exportFile = Path.Combine(Path.GetTempPath(), $"{Constants.APPSHORT}_{DateTime.Now.ToFileTime()}.ima");
                ulong dmsId;
                try {
                    // Upload, falls Standort(e) nicht vorhanden, wird die Outbox des Benutzers verwendet und gegebenenfalls erstellt
                    GroupwareTypeLibrary.Message message = msg!;
                    FormProgress.Invoke((Action)(() => GroupwiseUtil.ExportMessage(message, exportFile)));
                    dmsId = await AppConnector.CreateDocumentAsync(document!, location, exportFile);
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

                // Formular ausblenden
                FormProgress.Invoke((Action)(() => FormProgress.CloseFade()));
            } catch (Exception e) {
                // Formular schließen
                try {
                    FormProgress.Invoke((Action)(() => FormProgress.Close()));
                } catch { }

                CommonUtil.GlobalErrorHandler(e);
            }
        }
    }
}