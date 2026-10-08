using GwCustom.Custom;
using GwCustom.Forms;
using GwCustom.Models;
using GwCustomUI.Classes;
using System;
using System.Collections.Generic;

namespace GwCustom.Actions
{
    public class MenuLocating : Execute
    {
        /// <summary>
        /// Verarbeitung, Standortermittlung durchführen
        /// </summary>
        /// <returns></returns>
        public override bool RunBefore()
        {
            // Betroffene Nachrichten aus Auswahl oder aktivem Fenster ermitteln und verarbeiten
            List<GroupwareTypeLibrary.Message> msgList = GroupwiseUtil.GetCurrentMessages();

            // Exakt ein ausgewähltes Objekt sicherstellen
            if (msgList.Count > 1)
            {
                throw new Exception(MultiLanguage.GetText("singlemessage"));
            }

            // Groupwise-Dokumentobjekt für Standortermittlung
            GwDocument gwDocument = new(msgList[0], GroupwiseUtil.GetDocumentIdByAttachment(msgList[0]));

            // Standortformular anzeigen
            FLocating locating = new(gwDocument);
            locating.Show();

            return false;
        }


        public override void Run()
        {
            RunBefore();
        }
    }
}