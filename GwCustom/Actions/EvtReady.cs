using GwCustom.Custom;
using GwCustomUI.Classes;
using GwCustomUI.Signatures;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.Actions
{
    public class EvtReady : Execute
    {
        public override bool RunBefore()
        {
            return false;
        }

        public override void Run()
        {
            // Hinweis: Eventuelle Fehler werden einzeln abgefangen, um die Ausführung essentieller Aktionen
            // sicherzustellen, auch wenn vorherige fehlschlagen sollten

            // Programmordner erzeugen falls nicht vorhanden
            try
            {
                CommonUtil.CreateFolder(APPDATAPATH);
                CommonUtil.CreateFolder(USERDATAPATH);

                // WebView2-Datenverzeichnis bei Versionswechsel bereinigen
                CommonUtil.CleanWebViewUserData();
            }
            catch (Exception e)
            {
                CommonUtil.GlobalErrorHandler(e, true);
            }

            // Konfiguration erneuern
            try
            {
                CommonUtil.GetConfig().Save();
            }
            catch (Exception e)
            {
                CommonUtil.GlobalErrorHandler(e, true);
            }

            // Signaturdatenbank im Hintergrund aktualisieren
            try
            {
                if (CommonUtil.GetConfig().SignatureEnabled)
                {
                    // Groupwise Signaturen deaktivieren wenn Signaturintegration aktiv ist
                    GroupwiseUtil.GetGwClientState().IsSignatureAllowed = C3POTypeLibrary.SignatureInsertionConstants.eGWSignatureNone;

                    // Beim Start Update der Signaturen erzwingen, jedoch als Hintergrundprozess ohne den Startvorgang zu unterbrechen
                    List<string> users = GroupwiseUtil.GetGwUser(); // wichtig, die Benutzerermittlung muss synchron erfolgen!
                    Task.Run(async () => await SignatureManagement.UpdateSignatures(users, true));
                }
            }
            catch (Exception e)
            {
                CommonUtil.GlobalErrorHandler(e, true);
            }

            // Eventkonfiguration erzeugen und vorhandene Events beim Start entfernen
            try
            {
                // Events entfernen
                GroupwiseUtil.GetGwAccount().RemoveEvents(APPSHORT, DateTime.MinValue, DateTime.Now);

                // Eventkonfiguration erstellen
                GroupwareTypeLibrary.EventConfigurations eConfigurations = GroupwiseUtil.GetGwAccount().GetEventConfiguration(APPSHORT);
                if (eConfigurations.Count == 0)
                {
                    // fehlende Konfiguration erstellen
                    GroupwareTypeLibrary.EventConfiguration2 eConfiguration = (GroupwareTypeLibrary.EventConfiguration2)eConfigurations.Add(APPSHORT);
                    eConfiguration.Definition.FolderItemAdd = true; // Ausgangsnachricht muss in einem Ordner erstellt werden um sie finden und exportieren zu können
                    eConfiguration.Definition.FolderItemMove = true; // womöglich wird eine bestehende Nachricht aus in Arbeit nur in den Ausgang verschoben
                    eConfiguration.Definition.ItemModify = true; // falls die vorherigen Events fehlschlagen sollte zumindest dieses greifen
                    eConfiguration.Persistence = 1;
                    eConfiguration.Enabled = true;
                }
                else if (eConfigurations.Count == 1) {
                    // bestehende Konfiguration anpassen
                    eConfigurations.Item(0).Enabled = false;
                    eConfigurations.Item(0).Definition.FolderItemAdd = true;
                    eConfigurations.Item(0).Definition.FolderItemMove = true;
                    eConfigurations.Item(0).Definition.ItemModify = true;
                    eConfigurations.Item(0).Persistence = 1;
                    eConfigurations.Item(0).Enabled = true;
                }
            }
            catch(Exception e)
            {
                CommonUtil.GlobalErrorHandler(e, true);
            }
        }
    }
}