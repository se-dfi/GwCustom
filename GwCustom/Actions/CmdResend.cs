using GwCustom.Custom;
using GwCustomUI.Classes;

namespace GwCustom.Actions
{
    public class CmdResend : Execute
    {
        public override bool RunBefore()
        {
            // Signaturen laden falls aktiviert und nicht vorhanden oder abweichender Benutzer
            if (CommonUtil.GetConfig().SignatureEnabled)
            {
                // Groupwise-Signatur deaktivieren
                GroupwiseUtil.GetGwClientState().IsSignatureAllowed = C3POTypeLibrary.SignatureInsertionConstants.eGWSignatureNone;
            }

            return false;
        }

        public override void Run()
        {
            // Vorhandenes Layout, Signatur und Barcode entfernen
            GroupwiseUtil.RefactorHtmlFromView();
        }
    }
}