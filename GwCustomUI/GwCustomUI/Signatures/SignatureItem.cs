using GwCustomUI.Classes;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GwCustomUI.Classes.Constants;

namespace GwCustomUI.Signatures {
    public class SignatureItem(int id, string name, string language, string content) {
        public int Id { get; set; } = id;

        public string Name { get; set; } = name;

        public string Language { get; set; } = language;

        private string _content = content;

        public string Content {
            get { return _content; }
            set {
                _content = value;
                _hasmobile = _content.Contains(CLASS_MOBILE);
                _hasinfo = _content.Contains(CLASS_INFO);
            }
        }

        private bool _hasmobile = content.Contains(CLASS_MOBILE);

        public bool HasMobile {
            get { return _hasmobile; }
        }

        private bool _hasinfo = content.Contains(CLASS_INFO);

        public bool HasInfo {
            get { return _hasinfo; }
        }

        public List<SignatureUser> Users { get; set; } = [];

        private readonly List<SignatureLayout> _layouts = [];

        internal void AddLayoutInternal(SignatureLayout layout) {
            if (!_layouts.Contains(layout)) {
                _layouts.Add(layout);
            }
        }

        internal void RemoveLayoutInternal(SignatureLayout layout) {
            _layouts.Remove(layout);
        }

        public IReadOnlyList<SignatureLayout> GetLayouts() => _layouts;

        public void AddUser(SignatureUser user) {
            if (!Users.Contains(user)) {
                Users.Add(user);
                user.AddSignature(this);
            }
        }

        public string GetContent(bool info, bool mobile, string infohtml) {
            // Html-Element für Signatur erstellen
            HtmlNode divSignatur = HtmlNode.CreateNode($"<div class=\"{CLASS_SIGNATURE}\"></div>");
            divSignatur.InnerHtml = _content + Environment.NewLine + "<div><br></div>";

            // Infografik bei Bedarf einfügen
            if (info && HasInfo && !string.IsNullOrEmpty(infohtml)) {
                HtmlNode? nodeInfo = divSignatur.SelectSingleNode($".//*[@class=\"{CLASS_INFO}\"]");
                if (nodeInfo != null) {
                    nodeInfo.InnerHtml = infohtml;
                }
            }

            // Mobilnummer bei Bedarf entfernen
            if (!mobile && HasMobile) {
                HtmlNode? nodeMobil = divSignatur.SelectSingleNode($".//*[@class=\"{CLASS_MOBILE}\"]");
                nodeMobil?.ParentNode.RemoveChild(nodeMobil);
            }

            // Deutsche Sonderzeichen ersetzen
            if (Language != "de") {
                string content = divSignatur.InnerHtml;
                content = content.Replace("ä", "ae", StringComparison.CurrentCulture);
                content = content.Replace("&auml;", "ae", StringComparison.CurrentCulture);
                content = content.Replace("ö", "oe", StringComparison.CurrentCulture);
                content = content.Replace("&ouml;", "oe", StringComparison.CurrentCulture);
                content = content.Replace("ü", "ue", StringComparison.CurrentCulture);
                content = content.Replace("&uuml;", "ue", StringComparison.CurrentCulture);
                content = content.Replace("Ä", "Ae", StringComparison.CurrentCulture);
                content = content.Replace("&Auml;", "Ae", StringComparison.CurrentCulture);
                content = content.Replace("Ö", "Oe", StringComparison.CurrentCulture);
                content = content.Replace("&Ouml;", "Oe", StringComparison.CurrentCulture);
                content = content.Replace("Ü", "Ue", StringComparison.CurrentCulture);
                content = content.Replace("&Uuml;", "Ue", StringComparison.CurrentCulture);
                content = content.Replace("ß", "ss", StringComparison.CurrentCulture);
                content = content.Replace("&szlig;", "ss", StringComparison.CurrentCulture);
                divSignatur.InnerHtml = content;
            }

            // Signatur zurückgeben
            return divSignatur.InnerHtml;
        }
    }
}