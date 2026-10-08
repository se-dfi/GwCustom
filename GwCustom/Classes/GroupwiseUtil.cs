using C3POTypeLibrary;
using GroupwareTypeLibrary;
using GroupWiseCommander;
using GwCustom.C3PO;
using GwCustomUI.Classes;
using HtmlAgilityPack;
using mshtml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.RegularExpressions;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.Custom
{
    /// <summary>
    /// Hilfsfunktionen für Groupwise
    /// </summary>
    public static partial class GroupwiseUtil
    {
        [GeneratedRegex(OSFILEIDPATTERN, RegexOptions.IgnoreCase, "de-DE")]
        private static partial Regex OsAttachmentRegex();

        /// <summary>
        /// Groupwise Clientstate zurückgeben
        /// </summary>
        /// <returns>ClientState</returns>
        public static IGWClientState8 GetGwClientState()
        {
            return (IGWClientState8)GetGwClientStateDynamic();
        }

        /// <summary>
        /// Groupwise Clientstate zurückgeben (Late Binding)
        /// </summary>
        /// <returns>ClientState</returns>
        public static dynamic GetGwClientStateDynamic()
        {
            if (Implementation.GwC3POManager == null)
            {
                throw new NullReferenceException();
            }

            return Implementation.GwC3POManager.ClientState;
        }

        /// <summary>
        /// Groupwise Account zurückgeben
        /// </summary>
        /// <returns>CurrentAccount</returns>
        public static IGWAccount11 GetGwAccount()
        {
            return (IGWAccount11)GetGwClientState().CurrentAccount;
        }

        /// <summary>
        /// Groupwise Account zurückgeben (Late Binding)
        /// </summary>
        /// <returns>CurrentAccount</returns>
        public static dynamic GetGwAccountDynamic()
        {
            return GetGwClientState().CurrentAccount;
        }

        /// <summary>
        /// Groupwise Commander zurückgeben
        /// </summary>
        /// <returns>GroupwiseCommander</returns>
        public static GWCommander GetGwCommander()
        {
            return Implementation.GwCommander ?? new GWCommander();
        }

        /// <summary>
        /// Groupwise Spracheinstellung ermitteln
        /// </summary>
        /// <returns>Sprache, Fallback "de"</returns>
        public static string GetClientLanguage()
        {
            const string langugage_def = "en";
            string? language = langugage_def;

            // Sprache anhand des Kalenderordners ermitteln
            switch (GroupwiseUtil.GetGwAccountDynamic().Calendar.Name)
            {
                case "Kalender":
                    language = "de";
                    break;

                case "Calendar":
                    language = "en";
                    break;

                case "Calendario":
                    language = "it";
                    break;

                case "Agenda":
                    language = "fr";
                    break;

                case "Kalendář":
                    language = "cs";
                    break;
            }

            return language ?? langugage_def;
        }

        /// <summary>
        /// Ausgewählte Message-Objekte ermitteln
        /// </summary>
        /// <returns>Liste mit Message-Objekten</returns>
        public static List<GroupwareTypeLibrary.Message> GetSelectedMessages()
        {
            // Nachrichtenliste initialisieren (Standard-Liste verwenden, GroupwareTypeLibrary.MessageList reagiert fehlerhaft)
            List<GroupwareTypeLibrary.Message> gwMessages = [];
            foreach (GroupwareTypeLibrary.Message gwMessage in GetGwClientState().SelectedMessages)
            {
                gwMessages.Add(gwMessage);
            }

            return gwMessages;
        }

        /// <summary>
        /// Ausgewählte oder alternativ geöffnete Nachricht ermitteln
        /// </summary>
        /// <returns>Liste mit Message-Objekten</returns>
        public static List<GroupwareTypeLibrary.Message> GetCurrentMessages()
        {
            List<GroupwareTypeLibrary.Message> gwMessages = [];

            // Selektierte Nachrichten auf Mails filtern und hinzufügen
            foreach (GroupwareTypeLibrary.Message gwMessage in GetSelectedMessages())
            {
                if (gwMessage.ClassName.StartsWith(eGW_MAIL, StringComparison.CurrentCultureIgnoreCase))
                {
                    gwMessages.Add(gwMessage);
                }
            }

            // Keine Nachricht ausgewählt -> Id des aktuellen Fensters ermitteln
            if (gwMessages.Count == 0)
            {
                if (GetGwCommander().Execute("ItemMessageIdFromView()", out string msgId) && !msgId.Equals("X00", StringComparison.CurrentCultureIgnoreCase))
                {
                    GroupwareTypeLibrary.Message gwMessage = GetGwAccount().GetMessage(msgId);
                    if (gwMessage.ClassName.StartsWith(eGW_MAIL, StringComparison.CurrentCultureIgnoreCase))
                    {
                        gwMessages.Add(gwMessage);
                    }
                }
            }

            return gwMessages;
        }

        /// <summary>
        /// Die versendete Nachricht anhand Events und GUID ermitteln
        /// </summary>
        /// <param name="guid"></param>
        /// <returns>Objekt der versandten Nachricht</returns>
        public static GroupwareTypeLibrary.Message GetSentMessage(string guid)
        {
            // Events abholen und filtern, letzte Minute, rückwärts durchlaufen (aktuellste zuerst)
            EventCollection eCollection = GetGwAccount().GetEvents(APPSHORT, DateTime.Now.AddSeconds(-60), DateTime.Now, 0, false, false);
            for (int i = eCollection.Count; i > 0; i--)
            {
                // Nachrichtenobjekt ermitteln, Fehler sind zu erwarten und irrelevant wenn Objekt bereits entfernt, wie z.B. gesendete Work-in-Progress Nachricht
                GroupwareTypeLibrary.Message message;
                try
                {
                    message = GetGwAccount().GetMessage(eCollection.Item(i).MessageID);
                }
                catch
                {
                    continue;
                }

                // Filtern auf Ausgangsnachrichten
                if (message.BoxType.Equals(MessageBoxTypeConstants.egwOutgoing))
                {
                    // Filtern auf Typ Mail
                    if (message.ClassName.StartsWith(eGW_MAIL, StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Ermitteln und vergleichen der GUID, Ermittlung aus X-Field oder Mailbody
                        if (GetMessageSentGuid(message).Equals(guid))
                        {
                            return message;
                        }
                    }
                }
            }

            // Fallback: Herkömmliche Suche nach X-Field
            MessageList? messages;
            try
            {
                messages = GetGwAccount().AllMessages.Find($"(MAIL) AND (BOX_TYPE = OUTGOING AND CREATE_DATE = TODAY AND <NGW_HSZ_MIME_X_FIELD,STRING> MATCHES \"{SENTGUIDCLASS}={guid}\")");
            }
            catch
            {
                messages = GetGwAccount().AllMessages.Find($"(MAIL) AND (BOX_TYPE = OUTGOING AND CREATE_DATE = TODAY AND <F1574,STRING> MATCHES \"{SENTGUIDCLASS}={guid}\")");
            }
            foreach (GroupwareTypeLibrary.Message message in messages)
            {
                if (message != null && !string.IsNullOrEmpty(message.MessageID))
                {
                    if (GetXFieldValue(message.MessageID, SENTGUIDCLASS).Equals(guid))
                    {
                        return message;
                    }
                }
            }

            throw new Exception(MultiLanguage.GetText("nostoragemessage"));
        }

        /// <summary>
        /// Textinterprätation der Metadaten einer E-Mail
        /// </summary>
        /// <param name="message"></param>
        /// <returns>Kurzform der E-Mail Metadaten</returns>
        public static string GetMessageShortForm(GroupwareTypeLibrary.Message? message)
        {
            if (message == null)
            {
                return string.Empty;
            }

            return $"{message.CreationDate:g} {MultiLanguage.GetText("from")} {message.Sender.EMailAddress}\r\n{CommonUtil.TruncateWord(message.Subject.PlainText, 50)}";
        }

        /// <summary>
        /// Groupwise Benutzer (aktiv und eventuell Vertretung) ermitteln
        /// </summary>
        /// <returns>Liste aus aktivem Benutzer und eventuellem Vertretungsbenutzer</returns>
        public static List<string> GetGwUser()
        {
            List<string> user = [];

            string temp;

            // Abweichender Benutzer
            temp = CommonUtil.GetConfig().AlternativeUser;
            if (!string.IsNullOrEmpty(temp))
            {
                user.Add(temp.ToLower());
            }
            else
            {
                // Aktuellen Benutzer
                Implementation.GwCommander?.Execute("EnvUserID()", out temp);
                user.Add(temp.ToLower());

                // Vertretungsbenutzer
                IGWClientState8? GwClientState = Implementation.GwC3POManager?.ClientState;
                Account11? GwAccount = (Account11?)GwClientState?.CurrentAccount;
                if (GwAccount != null && GwAccount.Proxied)
                {
                    Implementation.GwCommander?.Execute("EnvUserID(1; 0)", out temp);
                    if (!user.Contains(temp))
                    {
                        user.Add(temp.ToLower());
                    }
                }
            }

            return user;
        }

        /// <summary>
        /// Mime Field Wert ermitteln, rückwärts um bei Mehrfacheinträgen den aktuellsten zu erhalten
        /// </summary>
        public static string GetXFieldValue(string msgId, string field)
        {
            string value = string.Empty;

            GetGwCommander().Execute($"ItemGetMimeXFieldCount(\"{msgId}\")", out string fieldcount);
            for (int i = int.Parse(fieldcount) - 1; i >= 0; i--)
            {
                GetGwCommander().Execute($"ItemGetMimeXField(\"{msgId}\"; {i})", out string fieldvalue);
                if (fieldvalue != null && fieldvalue.StartsWith(field))
                {
                    value = Regex.Match(fieldvalue, $"{field}:\\s*(.*)").Groups[1].Value;
                    break;
                }
            }

            return value;
        }

        /// <summary>
        /// Mime Field mit Wert hinzufügen
        /// </summary>
        public static void AddXField(string msgId, string field, string value)
        {
            GetGwCommander().Execute($"ItemAddMimeXField(\"{msgId}\"; \"{field}\"; \"{value}\")", out _);
        }

        /// <summary>
        /// Nachricht exportieren als MIME
        /// </summary>
        /// <param name="message"></param>
        /// <param name="path"></param>
        /// <exception cref="Exception"></exception>
        public static void ExportMessage(GroupwareTypeLibrary.Message message, string path)
        {
            string command = $"ItemSaveMessage(\"{message.MessageID}\"; \"{path}\"; 900)"; // 900 = MIME
            if (!GetGwCommander().Execute(command, out _) || !File.Exists(path))
            {
                throw new Exception(MultiLanguage.GetText("exportfailed"));
            }
        }

        /// <summary>
        /// enaio Id aus .os Anhang ermitteln
        /// </summary>
        /// <param name="message"></param>
        /// <returns>enaio Id des Dokuments</returns>
        public static ulong GetDocumentIdByAttachment(GroupwareTypeLibrary.Message message)
        {
            // Anhänge durchsuchen
            if (message.Attachments != null)
            {
                for (int i = 1; i <= message.Attachments.Count; i++)
                {
                    Attachment3 attachment = (Attachment3)message.Attachments.Item(i);
                    if (attachment.fileName.EndsWith(".os", StringComparison.CurrentCultureIgnoreCase))
                    {
                        GWStream stream = attachment.Stream;
                        byte[] byteArray = (byte[])stream.Read(10240); // Maximal 10 KB laden
                        string content = Encoding.UTF8.GetString(byteArray);
                        Match match = OsAttachmentRegex().Match(content);
                        if (match.Success)
                        {
                            Group group = match.Groups[^1];
                            Capture capture = group.Captures[^1];
                            return ulong.Parse(capture.Value);
                        }
                    }
                }
            }

            return 0;
        }

        /// <summary>
        /// E-Mail Html-Quellcode zurückgeben (late binding)
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetMessageHtmlDynamic(dynamic message)
        {
            // Anhänge nach Html-Bereich durchsuchen
            if (message.Attachments != null)
            {
                for (int i = 1; i <= message.Attachments.Count; i++)
                {
                    dynamic attachment = message.Attachments.Item(i);
                    if (attachment.fileName.Equals("TEXT.htm", StringComparison.CurrentCultureIgnoreCase))
                    {
                        GWStream stream = attachment.Stream;
                        byte[] byteArray = (byte[])stream.Read(1024 * 1000 * 10); // Maximal 10 Mb
                        return Encoding.UTF8.GetString(byteArray);
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// E-Mail Quellcode zurückgeben (late binding)
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetMessageSourceDynamic(dynamic message)
        {
            // Anhänge nach Html-Bereich durchsuchen
            if (message.Attachments != null)
            {
                for (int i = 1; i <= message.Attachments.Count; i++)
                {
                    dynamic attachment = message.Attachments.Item(i);
                    if (attachment.fileName.Equals("Mime.822", StringComparison.CurrentCultureIgnoreCase))
                    {
                        GWStream stream = attachment.Stream;
                        byte[] byteArray = (byte[])stream.Read(1024 * 10); // Maximal 10 Kb, nur Headerinfos
                        return Encoding.UTF8.GetString(byteArray);
                    }
                }
            }

            return string.Empty;
        }

        /// <summary>
        /// Statustext des Hauptfensters setzen
        /// </summary>
        /// <param name="text"></param>
        public static void SetStatusBarText(string text)
        {
            GetGwCommander().Execute($"StatusBarSetText(1; \"{text}\")", out _);
        }

        /// <summary>
        /// Entfernen von .os Anhängen aus dem aktuellen Nachrichtenfenster
        /// </summary>
        public static void RemoveDmsAttachmentsFromView()
        {
            // Anzahl Anhänge der aktuellen Nachricht ermitteln
            GetGwCommander().Execute($"ItemAttachmentGetCount(\"X00\")", out string sAttCount);
            if (int.TryParse(sAttCount, out int iAttCount) && iAttCount > 0)
            {
                // Anhänge absteigend nach Index verarbeiten
                for (int i = iAttCount - 1; i >= 0; i--)
                {
                    // Nur Dateianhänge weiter betrachten
                    if (GetGwCommander().Execute($"ItemAttachmentGetClass(\"X00\"; {i})", out string sAttClass) && sAttClass.Equals("1"))
                    {
                        // Dateiname des Anhangs ermitteln
                        GetGwCommander().Execute($"ItemAttachmentGetName(\"X00\"; {i})", out string sAttName);
                        if (sAttName.EndsWith(".os", StringComparison.CurrentCultureIgnoreCase))
                        {
                            // .os Anhang entfernen
                            GetGwCommander().Execute($"ItemAttachmentDelete(\"X00\"; {i})", out _);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Sent-Guid erzeugen und in X-Field der Nachricht einfügen
        /// </summary>
        public static string CreateSentGuid(string msgId)
        {
            string guid = string.Empty;

            if (!string.IsNullOrEmpty(msgId))
            {
                guid = Guid.NewGuid().ToString();
                AddXField(msgId, SENTGUIDCLASS, guid);
            }

            return guid;
        }

        /// <summary>
        /// Umstrukturierung des HTML-Code für das aktuelle Nachrichtenfenster
        /// </summary>
        public static string RestructureHtmlFromView(string? sentGuid = null, bool bodyOnly = true)
        {
            // Mehrfachverwendete Variablen
            HtmlNodeCollection? nodes;

            // Html Dokumentobjekt der aktuellen Nachricht erzeugen
            HtmlAgilityPack.HtmlDocument document = new();

            // Dokument sicher ermitteln
            document.LoadHtml(GetMessageHtml());

            // Body ermitteln
            HtmlNode? body = document.DocumentNode.SelectSingleNode("//body");

            // Body für Bearbeitung in der Voransicht sperren und für Neuaufbau leer duplizieren
            body?.SetAttributeValue("contentEditable", "false");
            HtmlNode? bodyNew = body?.CloneNode(false); // ohne Inhalt


            // Alle Verläufe in den neuen Body verschieben
            nodes = body?.SelectNodes($".//div[contains(@class,'{CLASS_GW_MAIN}')]/span[@class=\"{CLASS_GW_HISTORY}\"]");
            if (nodes != null)
            {
                int quantity = nodes.Count;
                int i = 0;
                while (nodes != null && i < quantity)
                {
                    // Vollständiges Verlaufsobjekt finden (parent wegen Hierarchie div->span)
                    HtmlNode divHistory = nodes.First().ParentNode;

                    // Sonderfallbereinigung: Aktiv-Klasse austauschen falls noch vorhanden
                    divHistory.ReplaceClass(CLASS_GW_MAIN, CLASS_GW_ACTIVE);

                    // Verlaufsobjekt mit Inhalt in den neuen Hauptknoten einfügen
                    bodyNew?.AppendChild(divHistory.CloneNode(true));

                    // Verlaufsobjekt mit Inhalt aus ursprünglichem Body entfernen
                    divHistory.ParentNode.RemoveChild(divHistory, false);

                    // Verbleibende Verläufe im ursprünglichen Body ermitteln
                    nodes = body?.SelectNodes($".//div[contains(@class,'{CLASS_GW_MAIN}')]/span[@class=\"{CLASS_GW_HISTORY}\"]");

                    // Überlaufschutz-Zähler aktualisieren
                    i++;
                }
            }

            // Groupwise Hauptcontainer finden und nach außen verlegen, alternativ erzeugen und einfügen
            HtmlNode? divRoot = body?.SelectSingleNode($".//div[contains(@class,'{CLASS_GW_MAIN}') and contains(@id, '{ID_GW_SECTION}')]");
            HtmlNode? divRootNew;
            if (divRoot != null)
            {
                divRootNew = divRoot.CloneNode(false); // ohne Inhalt

                // Class entfernen aus dem alten Container, damit ein bereits verarbeiteter Berich nicht womöglich erneut berücksichtigt wird
                divRoot.Attributes.Remove("class");

                // Id entfernen aus dem alten Container, da sie nicht mehrfach im Dokument vorkommen darf
                divRoot.Attributes.Remove("id");
            }
            else
            {
                divRootNew = HtmlNode.CreateNode($"<div class=\"{CLASS_GW_ACTIVE}\"></div>");
            }
            bodyNew?.PrependChild(divRootNew);

            // Layoutcontainer einfügen
            HtmlNode divLayout = HtmlNode.CreateNode($"<div class=\"{CLASS_LAYOUT}\"></div>");
            divRootNew.AppendChild(divLayout);

            // Contentcontainer einfügen
            HtmlNode divContent = HtmlNode.CreateNode($"<div class=\"{CLASS_CONTENT}\"></div>");
            divLayout.AppendChild(divContent);

            // Verbleibenden Inhalt der ursprünglichen Mail in den Contentcontainer innerhalb des Layouts übernehmen
            divContent.InnerHtml = body?.InnerHtml ?? string.Empty;

            // Entfernen von überflüssigen Umbrüchen am Ende des Contentcontainer, erkennt und entfernt rückwärts ganze Div-Container, die nur Umbrüche darstellen
            bool proceed;
            do
            {
                proceed = false;
                HtmlNodeCollection? divColl = divContent.SelectNodes(".//div");
                if (divColl != null)
                {
                    HtmlNode divLast = divColl.Last();
                    if (Regex.IsMatch(divLast.InnerHtml, "^[\\s,&nbsp;,<br>,<br\\/>]*$", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1)))
                    {
                        divLast.ParentNode.RemoveChild(divLast);
                        proceed = true;
                    }
                }
            } while (proceed);

            // Umbrüche am Ende des letzten Div-Containers entfernen
            HtmlNodeCollection? divColl2 = divContent.SelectNodes(".//div");
            HtmlNode? divEnd = divColl2?.Last();
            if (divEnd != null) {
                divEnd.InnerHtml = Regex.Replace(divEnd.InnerHtml, "(<br>)*$", string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
            }

            // Einen einfachen Umbruch vor der Signatur wieder einfügen
            divContent.InnerHtml += "<div><br></div>";

            // Container für Signatur und Barcode am Ende des Contentcontainer einfügen
            HtmlNode divSignature = HtmlNode.CreateNode($"<div class=\"{CLASS_SIGNATURE}\"></div>");
            divContent.AppendChild(divSignature);
            HtmlNode divBarcode = HtmlNode.CreateNode($"<div class=\"{CLASS_BARCODE}\"></div>");
            divBarcode.Attributes.Add(document.CreateAttribute("style", "display: none"));
            if (!string.IsNullOrEmpty(sentGuid))
            {
                divBarcode.Attributes.Add(document.CreateAttribute("id", sentGuid));
            }
            divContent.AppendChild(divBarcode);

            // Ergebnisrückgabe, vollständiger Htmlcode oder nur Inhalt des Body
            if (bodyOnly)
            {
                return bodyNew?.InnerHtml ?? string.Empty;
            }
            else
            {
                if (body != null) {
                    body.InnerHtml = bodyNew?.InnerHtml ?? string.Empty;
                }
                return document.DocumentNode.OuterHtml;
            }
        }

        /// <summary>
        /// Rückabwicklung von eventuell vorhandenem Layout, Barcode und Signatur beim erneuten Senden
        /// </summary>
        public static void RefactorHtmlFromView()
        {
            // Html Dokumentobjekt erzeugen
            HtmlAgilityPack.HtmlDocument document = new();

            // Dokument ermitteln
            document.LoadHtml(GetMessageHtml());

            // Body ermitteln
            HtmlNode? body = document.DocumentNode.SelectSingleNode("//body");
            if (body != null)
            {
                // Rootcontainer der Nachricht finden (Verläufe sind separat weiter unten auf der selben Ebene)
                HtmlNode? divRoot = body.SelectSingleNode($".//div[contains(@class,'{CLASS_GW_MAIN}') and contains(@id, '{ID_GW_SECTION}')]");
                if (divRoot != null)
                {
                    // Layoutcontainer lokalisieren
                    string content = string.Empty;
                    HtmlNode? divLayout = divRoot.SelectSingleNode($".//div[@class=\"{CLASS_LAYOUT}\"]");
                    if (divLayout != null)
                    {
                        // Contentcontainer innerhalb des Layoutcontainer lokalisieren
                        HtmlNode? divContent = divLayout.SelectSingleNode($".//div[@class=\"{CLASS_CONTENT}\"]");
                        if (divContent != null)
                        {
                            // Signaturcontainer innerhalb des Contentcontainer lokalisieren und entfernen
                            HtmlNode? divSignature = divContent.SelectSingleNode($".//div[@class=\"{CLASS_SIGNATURE}\"]");
                            divSignature?.ParentNode.RemoveChild(divSignature);

                            // Barcodecontainer innerhalb des Contentcontainer lokalisieren und entfernen
                            HtmlNode? divBarcode = divContent.SelectSingleNode($".//div[@class=\"{CLASS_BARCODE}\"]");
                            divBarcode?.ParentNode.RemoveChild(divBarcode);

                            // Verbleibenden Inhalt des Contentcontainer zwischenspeichern
                            content = divContent.InnerHtml;
                        }
                    }

                    // Zwischengespeicherten Content wieder einfügen
                    if (!string.IsNullOrEmpty(content))
                    {
                        divRoot.InnerHtml = content;
                    }
                }

                // Rückschreiben des Body ins Mailobjekt
                SetMessageHtml(body.InnerHtml);
            }
        }

        /// <summary>
        /// Sent-Guid der übergebenen Nachricht ermitteln, aus X-Field und Fallback aus Body
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetMessageSentGuid(dynamic message)
        {
            // Schnelle Variante: SentGuid aus vorhandenem X-Field ermitteln
            string xfield = GetXFieldValue(((GroupwareTypeLibrary.Message)message).MessageID, SENTGUIDCLASS);
            if (!string.IsNullOrEmpty(xfield))
            {
                return xfield;
            }

            // Aufwändigere Variante: SentGuid aus Html-Body ermitteln
            else
            {
                // Html Dokumentobjekt erzeugen
                HtmlAgilityPack.HtmlDocument document = new();
                document.LoadHtml(GroupwiseUtil.GetMessageHtmlDynamic(message));

                // Suchbereich auf Body eingrenzen
                HtmlNode? body = document.DocumentNode.SelectSingleNode("//body");
                if (body != null && body.InnerHtml != null)
                {
                    // Inhaltsbereich der Hauptnachricht finden
                    HtmlNode? content = body.SelectSingleNode($".//div[contains(@class,'{CLASS_GW_MAIN}')]/div[@class=\"{CLASS_LAYOUT}\"]/div[@class=\"{CLASS_CONTENT}\"]");
                    if (content != null)
                    {
                        HtmlNode? barcode = content.SelectSingleNode($"./div[@class=\"{CLASS_BARCODE}\"]");
                        if (barcode != null)
                        {
                            if (!string.IsNullOrEmpty(barcode.Id))
                            {
                                // X-Field versuchen zu setzen vor Rückgabe
                                AddXField(((GroupwareTypeLibrary.Message)message).MessageID, SENTGUIDCLASS, barcode.Id);

                                return barcode.Id;
                            }
                        }
                    }
                }
            }

            // Leerstring-Ergebnis falls nichts ermittelt werden konnte
            return string.Empty;
        }

        /// <summary>
        /// Sent-Guids aus dem Verlauf der übergegenen Nachricht ermitteln
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static List<string> GetMessageHistorySentGuids(dynamic message)
        {
            List<string> value = [];

            // Html Dokumentobjekt erzeugen
            HtmlAgilityPack.HtmlDocument document = new();
            document.LoadHtml(GroupwiseUtil.GetMessageHtmlDynamic(message));

            // Suchbereich auf Body eingrenzen
            HtmlNode? body = document.DocumentNode.SelectSingleNode("//body");
            if (body != null && body.InnerHtml != null)
            {
                // Historienblöcke Nachricht finden
                HtmlNodeCollection? histories = body.SelectNodes($".//div[contains(@class,'{CLASS_GW_MAIN}')]/span[@class=\"{CLASS_GW_HISTORY}\"]");
                if (histories != null)
                {
                    foreach (HtmlNode history in histories)
                    {
                        HtmlNode? barcode = history.SelectSingleNode($"../div[@class=\"{CLASS_LAYOUT}\"]/div[@class=\"{CLASS_CONTENT}\"]/div[@class=\"{CLASS_BARCODE}\"]");
                        if (barcode != null && barcode.Id != null && !value.Contains(barcode.Id))
                        {
                            value.Add(barcode.Id);
                        }
                    }
                }
            }

            return value;
        }

        /// <summary>
        /// Barcode der übergegenen Nachricht ermitteln
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static string GetMessageBarcode(dynamic message)
        {
            // Html Dokumentobjekt erzeugen
            HtmlAgilityPack.HtmlDocument document = new();
            document.LoadHtml(GroupwiseUtil.GetMessageHtmlDynamic(message));

            // Suchbereich auf Body eingrenzen
            HtmlNode? body = document.DocumentNode.SelectSingleNode("//body");
            if (body != null && body.InnerHtml != null)
            {
                return CommonUtil.FindBarcode(body.InnerHtml);
            }

            return string.Empty;
        }

        /// <summary>
        /// HTML Code der aktuellen Nachricht auslesen (vollständig)
        /// </summary>
        /// <param name="html"></param>
        public static string GetMessageHtml()
        {
            IHTMLDocument3? doc = GetGwClientState().HTMLEditor;
            return doc.documentElement.outerHTML ?? string.Empty;
        }

        /// <summary>
        /// HTML Code in aktuelle Nachricht schreiben (nur Body)
        /// </summary>
        /// <param name="html"></param>
        public static void SetMessageHtml(string html)
        {
            IHTMLDocument2? doc = GetGwClientState().HTMLEditor;
            doc.body.innerHTML = html;
        }
    }
}