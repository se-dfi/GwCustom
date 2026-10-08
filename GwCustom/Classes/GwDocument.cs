using GroupwareTypeLibrary;
using GwCustom.Classes;
using GwCustom.Custom;
using GwCustomUI.Classes;
using System;
using System.Collections.Generic;
using System.Net.Mail;
using System.Text.RegularExpressions;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.Models
{
    /// <summary>
    /// Klasse für Groupwise Dokumente
    /// </summary>
    public partial class GwDocument
    {
        [GeneratedRegex(SENTGUIDPATTERN)]
        private static partial Regex SentGuidRegex();

        [GeneratedRegex(@"\s")]
        private static partial Regex SpaceRegex();

        private string? _SentGuid;

        private readonly Message3 _message;

        public GwDocument(dynamic message, ulong fileId = 0)
        {
            _message = message;
            Id = _message.MessageID;
            FileId = fileId; // nur wenn im Konstruktor übergeben werden Id´s aus eventuellem .os Anhang ausgewertet
            SentGuid = GroupwiseUtil.GetMessageSentGuid(_message);
            CreationDate = _message.CreationDate.ToString("dd.MM.yyyy");
            CreationTime = _message.CreationDate.ToString("HH:mm");
            Subject = _message.Subject.PlainText;
            MailAddressFrom = _message.Sender.EMailAddress.ToLower();
            MailAddressesTo = [];
            MailAddressesCc = [];
            GetMailAdressLists();
        }

        public string Id { get; set; }

        public ulong FileId { get; set; }

        public string? SentGuid
        {
            get { return _SentGuid; }
            set
            {
                _SentGuid = (!string.IsNullOrEmpty(value) && SentGuidRegex().IsMatch(value)) ? value : string.Empty;
            }
        }

        public string CreationDate { get; set; }

        public string CreationTime { get; set; }

        public string Subject { get; set; }

        public string MailAddressFrom { get; set; }

        public List<string> MailAddressesTo { get; set; }

        public List<string> MailAddressesCc { get; set; }

        /// <summary>
        /// E-Mailadressen für Direkt- und Kopieempfänger ermitteln
        /// </summary>
        private void GetMailAdressLists()
        {
            // Workaround, Reply-To Adresse ermitteln um diese nicht fälschlicherweise mit als Empfänger einzufügen
            string source = GroupwiseUtil.GetMessageSourceDynamic(_message);
            string replyTo = string.Empty;
            string pattern = "(?<=Reply-To:)(.*)$";
            if (Regex.IsMatch(source, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase))
            {
                replyTo = Regex.Match(source, pattern, RegexOptions.Multiline | RegexOptions.IgnoreCase).Groups[1].Value.Trim();
            }

            // Empfänger separieren
            for (int i = 1; i <= _message.ExpandedRecipients.Count; i++)
            {
                GroupwareTypeLibrary.Recipient2 gwRecipient = (GroupwareTypeLibrary.Recipient2)_message.ExpandedRecipients.Item(i);
                switch (gwRecipient.TargetType)
                {
                    case GroupwareTypeLibrary.AddressTargetTypeConstants.egwTo:
                        if (!gwRecipient.EMailAddress.ToLower().Equals(replyTo))
                        {
                            MailAddressesTo.Add(gwRecipient.EMailAddress.ToLower());
                        }
                        break;

                    case GroupwareTypeLibrary.AddressTargetTypeConstants.egwCC:
                        MailAddressesCc.Add(gwRecipient.EMailAddress.ToLower());
                        break;
                }
            }
        }

        /// <summary>
        /// DMS Dokument(e) zur Groupwise Nachricht ermitteln
        /// </summary>
        /// <returns>Liste der DMS Dokumente</returns>
        public List<DmsDocument> GetDmsDocuments()
        {
            List<DmsDocument> documents = [];
            List<DmsDocument> search;

            // Variante 1: DmsObjekt und Standorte per SentGuid
            search = AppConnector.GetDocumentsByGuidAsync(SentGuid, true, true, true).GetAwaiter().GetResult();
            if (search.Count > 0)
            {
                documents.AddRange(GetDmsDocumentsEqual(search));
            }

            // Variante 2: DmsObjekt und Standorte per Dokument-Id aus .os Anhang
            if (documents.Count == 0 && FileId > 0)
            {
                search = AppConnector.GetDocumentsByIdAsync(FileId, true, true, true).GetAwaiter().GetResult();
                if (search.Count > 0)
                {
                    documents.AddRange(GetDmsDocumentsEqual(search));
                }
            }

            // Variante 3: DmsObjekt und Standorte per Indexdatensuche, aber nicht wenn es einen .os Anhang gibt
            if (documents.Count == 0 && FileId == 0)
            {
                search = AppConnector.GetDocumentsByIndexdataAsync(this, true, true, true, true).GetAwaiter().GetResult();
                if (search.Count == 0)
                {
                    search = AppConnector.GetDocumentsByIndexdataAsync(this, false, true, true, true).GetAwaiter().GetResult();
                }
                if (search.Count > 0)
                {
                    documents.AddRange(GetDmsDocumentsEqual(search));
                }
            }

            return documents;
        }

        /// <summary>
        ///  Zielstandort für eine im DMS noch nicht vorhandene Groupwise Nachricht ermitteln
        /// </summary>
        /// <returns>DMS Standort</returns>
        public DmsLocation? GetDmsTargetLocation()
        {
            // Zielstandort aus abgelegten Verlaufs - Mails anhand derer SentGuid finden
            List<DmsDocument> documents;
            List<DmsLocation> targets = [];
            foreach (var guid in GroupwiseUtil.GetMessageHistorySentGuids(_message))
            {
                documents = AppConnector.GetDocumentsByGuidAsync(guid, false, false, true).GetAwaiter().GetResult();
                foreach (var document in documents)
                {
                    foreach (var location in document.Locations)
                    {
                        if (location.IsValid() && !targets.Contains(location))
                        {
                            targets.Add(location);
                        }
                    }
                }
            }
            if (targets.Count == 1)
            {
                return targets[0];
            }
            else if (targets.Count == 0)
            {
                // Zielstandort aus altem E-mailbarcode ermitteln falls möglich
                if (ulong.TryParse(GroupwiseUtil.GetMessageBarcode(_message), out ulong id) && id > 0)
                {
                    documents = AppConnector.GetDocumentsByIdAsync(id, false, false, true).GetAwaiter().GetResult();
                    foreach (var document in documents)
                    {
                        foreach (var location in document.Locations)
                        {
                            if (location.IsValid() && !targets.Contains(location))
                            {
                                targets.Add(location);
                            }
                        }
                    }
                    if (targets.Count == 1)
                    {
                        return targets[0];
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Nur die DMS Dokumente aus der übergebenen Liste zurückgeben, die mit dieser Groupwise-Nachricht übereinstimmen
        /// 
        /// Dies ist ein schwieriges Unterfangen, weil:
        /// Datum - entspricht in enaio dem Archivierungsdatum, kann bei späterer Zustellung vom Maildatum abweichen
        /// Uhrzeit - siehe Datum, weicht in enaio noch viel häufiger vom Mailzeitpunkt ab und ist dort zudem ein Textfeld
        /// Absender - in enaio steht hier seit längerem nur noch die Mailadresse, diese ist aus Groupwise eindeutig und muss im enaio enthalten sein
        /// Empfänger (To, Cc) - bei To kommen in Groupwise teilweise reply-to Adressen mit vor, die in enaio nicht aufgelistet sind.
        ///                      Außerdem Textfeld mit bis zu 4000 Zeichen in enaio vs. Array der Empfängeradressen in Groupwise
        ///                      Die 4000 Zeichen werde durchaus öfter überschritten in enaio, dann wird nach der letzten darstellbaren Adresse mit [...] beendet
        ///                      Teilweise kommen Benennung und Mailadresse in der Auflistung vor, dabei kann dann auch die Mailadresse in beidem vorkommen
        ///                      Als Trennzeichen für mehrere Adressen werden sowohl "," als auch ";" verwendet
        /// Empfänger (Cc) - siehe To, es entfällt lediglich das Problem mit der reply-to Adresse
        /// Inhaltsvergleich für Anlagen oder Bodytext - Praktisch ungeeignet, hier müsste aus beiden Systemen das gesamte Mailobjekt gelesen werden mit seiner gesamten Größe
        /// Auch ein Vergleich der Größe macht keinen Sinn. Ein Groupwise-Objekt wird nach Export durch Base64 ein gutes Stück größer.
        /// 
        /// Deshalb kommt eine Score-Berechnung aus den verfügbaren Optionen zur Anwendung.
        /// </summary>
        /// <param name="documents">Übergebene DMS Dokumente</param>
        /// <returns>Liste der übereinstimmenden DMS Dokumente</returns>
        private List<DmsDocument> GetDmsDocumentsEqual(List<DmsDocument> documents)
        {
            List<DmsDocument> equalDocuments = [];

            foreach (DmsDocument document in documents)
            {
                // Punkteermittlung für Gleichheit, bis 10 Punkte gilt Gleichheit
                int score = 0;

                // Datum, kann abweichen bei späterer Archivierung
                if (!document.CreationDate.Equals(CreationDate))
                {
                    score += 6;
                    if (score > 10) continue;
                }

                // Uhrzeit, kann abweichen bei späterer Archivierung
                if (!document.CreationTime.Equals(CreationTime))
                {
                    score += 4;
                    if (score > 10) continue;
                }

                // Groupwise Absender-Email muss im Absenderstring des DMS vorkommen
                if (!document.MailFrom.Contains(MailAddressFrom, StringComparison.CurrentCultureIgnoreCase))
                {
                    score += 10;
                    if (score > 10) continue;
                }

                // Betreff muss übereinstimmen, nicht sichtbare Zeichen sind zu ignorieren
                if (!SpaceRegex().Replace(document.Subject, string.Empty).Equals(SpaceRegex().Replace(this.Subject, string.Empty), System.StringComparison.CurrentCultureIgnoreCase))
                {
                    score += 6;
                    if (score > 10) continue;
                }

                // Alle direkten Empfängeradressen in enaio müssen immer in Groupwise vorkommen
                List<string> toaddress_os = CommonUtil.GetMailaddressesFromString(document.MailTo.Trim());
                foreach (string address in toaddress_os)
                {
                    if (!MailAddressesTo.Contains(address))
                    {
                        score += 6;
                    }
                }
                if (score > 10) continue;

                // Die Anzahl der Adressen in Groupwise um 1 abweichen (Reply-To/Sender = Empfänger) als in enaio, außer dort wurde abgeschnitten
                if (!document.MailTo.Trim().EndsWith("[...]"))
                {
                    // enaio Adresszeile ist vollständig
                    if (Math.Abs(toaddress_os.Count - MailAddressesTo.Count) > 1)
                    {
                        score += 10;
                    }
                }
                else
                {
                    // enaio Adresszeile wurde gekürzt, also dürfen dort nicht mehr Adressen enthalten sein als in Groupwise
                    if (toaddress_os.Count > MailAddressesTo.Count)
                    {
                        score += 10;
                    }
                }
                if (score > 10) continue;

                // Alle Kopieempfängeradressen in enaio müssen immer in Groupwise vorkommen
                List<string> ccaddress_os = CommonUtil.GetMailaddressesFromString(document.MailCc.Trim());
                foreach (string address in ccaddress_os)
                {
                    if (!MailAddressesCc.Contains(address))
                    {
                        score += 6;
                    }
                }
                if (score > 10) continue;

                // Die Anzahl der Adressen in Groupwise muss für Kopieempfänger übereinstimmen
                if (!document.MailCc.Trim().EndsWith("[...]"))
                {
                    // enaio Adresszeile ist vollständig
                    if (ccaddress_os.Count != MailAddressesCc.Count)
                    {
                        score += 10;
                    }
                }
                else
                {
                    // enaio Adresszeile wurde gekürzt, also dürfen dort nicht mehr Adressen enthalten sein als in Groupwise
                    if (ccaddress_os.Count > MailAddressesCc.Count)
                    {
                        score += 10;
                    }
                }
                if (score > 10) continue;

                // Dokument gilt als identisch und wird ins Ergebnis aufgenommen
                equalDocuments.Add(document);
            }

            return equalDocuments;
        }
    }
}