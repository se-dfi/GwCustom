using GwCustomUI.Classes;
using HtmlAgilityPack;
using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Xml;
using System.Xml.Linq;
using static GwCustomUI.Classes.Constants;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace GwCustomUI.Signatures {
    /// <summary>
    /// Zentrale Verwaltung der Signaturdaten mit Aktualisierung der lokalen Datenbankdatei, 
    /// Überführung deren Inhalte in ein Repository im Speicher und Interaktion mit der Benutzeroberfläche
    /// </summary>
    public class SignatureManagement {
        private readonly SignatureTransport _transport;

        private readonly SignatureUI? _form;

        private static List<string>? _users;

        private static readonly string _file = Path.Combine(APPDATAPATH, "Signatures.sqlite");

        public string GetModifiedBody() {
            // Html Dokumentobjekt erzeugen
            HtmlAgilityPack.HtmlDocument document = new();
            document.LoadHtml(_transport.Modified);

            // Html Body ermitteln
            HtmlNode? body = document.DocumentNode.SelectSingleNode("//body");

            return body?.InnerHtml ?? string.Empty;
        }

        public bool GetStorage() {
            return _transport.Storage;
        }


        /// <summary>
        /// Signaturmanagement initialisieren, es muss eine gültige lokale Signaturdatei vorhanden sein
        /// </summary>
        /// <param name="users"></param>
        /// <param name="transport"></param>
        /// <exception cref="Exception"></exception>
        public SignatureManagement(List<string> users, SignatureTransport transport) {
            _users = users;
            _transport = transport;

            SignatureRepository repo = new();
            try {
                GetDbSignatureUsers(repo);
                GetDbSignatureItems(repo);
                GetDbSignatureLayouts(repo);
            } catch (Exception e) {
                CommonUtil.GlobalErrorHandler(e, true);
            }
            if (repo == null || repo.GetAllUsers().Count <= 0) {
                throw new Exception(MultiLanguage.GetText("nodatabase"));
            }

            _form = new(repo, _transport);
        }

        public bool Dialog() {
            return _form?.ShowDialog() ?? false; // falls kein Result gesetzt wurde mittels Sendebutton false annehmen und abbrechen
        }

        public static async Task<bool> UpdateSignatures(List<string> users, bool forcereload = false) {
            _users = users;

            // Keine Aktualisierung erforderlich
            if (!forcereload && CheckSignatureFile()) {
                return false;
            }

            await CreateSignatureFile();

            return true;
        }

        private static bool CheckSignatureFile() {
            // Signaturdatei existiert nicht
            if (!File.Exists(_file)) {
                return false;
            }

            // Signaturdatei hat 0 Kb
            FileInfo fi = new(_file);
            if (fi.Length == 0) {
                return false;
            }

            // Datenbankstand auf Alter prüfen
            if (CommonUtil.GetHoursBetween(DateTime.Now, GetCurrentness(_file)) >= CommonUtil.GetConfig().DbUpdateHours) {
                return false;
            }

            // Enthält die Datenbank mindestens alle Groupwise-Benutzer aus Vertretung oder alternativer Benutzerkonfiguration?
            if (_users == null || _users.Count == 0 || !_users.SequenceEqual(GetDbUserLogins())) {
                return false;
            }

            return true;
        }

        private static DateTime GetCurrentness(string dbfile) {
            try {
                return CommonUtil.GetFileDate(dbfile);
            } catch {
                return DateTime.MinValue;
            }
        }

        private static List<string> GetDbUserLogins() {
            List<string> result = [];

            if (_users == null || _users.Count == 0) {
                return result;
            }

            string query = @"select benutzer from benutzer where benutzer in (" + CommonUtil.GetSeparatedList(_users, ",", "'", "'") + ")";

            using (SQLiteConnection conn = new($"Data Source={_file};", true)) {
                conn.Open();
                using (SQLiteCommand cmd = new(query, conn)) {
                    using SQLiteDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows) {
                        while (dr.Read()) {
                            string benutzer = dr["benutzer"].ToString() ?? string.Empty;
                            if (benutzer.Length > 0) {
                                result.Add(benutzer);
                            }
                        }
                    }
                    dr.Close();
                }
                conn.Close();
            }
            return result;
        }

        private static async Task CreateSignatureFile() {
            if (_users == null || _users.Count == 0) {
                return;
            }

            // Temporären Dateinamen festlegen und vorab bereinigen
            string tempfile = _file + ".new";
            CommonUtil.DeleteFileIfExists(tempfile);

            // Temporäre Datenbank erzeugen
            if (await SeeleRestFramework.GetDatabase(tempfile, _users)) {
                if (File.Exists(tempfile)) {
                    CommonUtil.DeleteFileIfExists(_file);
                    File.Move(tempfile, _file);
                }
            }
        }

        private static void GetDbSignatureUsers(SignatureRepository repo) {
            if (_users == null || _users.Count == 0) {
                return;
            }

            repo.ClearUsers();
            string query = @"select benutzer, name from benutzer where benutzer in (" + CommonUtil.GetSeparatedList(_users, ",", "'", "'") + ")";
            using SQLiteConnection conn = new($"Data Source={_file};", true);
            conn.Open();
            using (SQLiteCommand cmd = new(query, conn)) {
                using SQLiteDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows) {
                    while (dr.Read()) {
                        string benutzer = dr["benutzer"].ToString() ?? string.Empty;
                        string name = dr["name"].ToString() ?? benutzer;
                        if (benutzer.Length > 0) {
                            repo.AddUser(new SignatureUser(benutzer, name));
                        }
                    }
                }
                dr.Close();
            }
            conn.Close();

            // Fehlende Benutzer (ohne Mitarbeiterkonfiguration) nur mit Login ergänzen
            foreach (string login in _users) {
                if (repo.GetUserByLogin(login) == null) {
                    repo.AddUser(new SignatureUser(login, login));
                }
            }
        }

        private static void GetDbSignatureItems(SignatureRepository repo) {
            repo.ClearSignatures();

            // Keine Signatur als Eintrag grundsätzlich für alle Benutzer einfügen
            SignatureItem nosig = new(0, MultiLanguage.GetText("nosignature"), string.Empty, string.Empty);
            repo.AddSignature(nosig);

            foreach (var user in repo.GetAllUsers()) {
                // Zuweisung zum Eintrag "Keine Signatur"
                nosig.AddUser(user);

                // Benutzersignaturen einfügen
                string query = @$"select s.signatur_id, s.bezeichnung || ' - ' || lower(substr(sp.bezeichnung, 1, 2)) as bezeichnung,
                              lower(substr(sp.bezeichnung, 1, 2)) as sprache, s.inhalt
                              from signatur s inner join zuordnung z on s.signatur_id = z.signatur_id
                              left join sprache sp on s.sprache = sp.sprache
                              where s.benutzer = '{user.Login}'
                              group by s.signatur_id, s.bezeichnung, s.sprache
                              order by s.bezeichnung, s.sprache";
                using SQLiteConnection conn = new($"Data Source={_file};", true);
                conn.Open();
                using (SQLiteCommand cmd = new(query, conn)) {
                    using SQLiteDataReader dr = cmd.ExecuteReader();
                    if (dr.HasRows) {
                        while (dr.Read()) {
                            int id = int.Parse(dr["signatur_id"].ToString() ?? "-1");
                            if (id >= 0) {
                                SignatureItem? signature = repo.GetSignatureByIdAndUser(id, user);
                                if (signature == null) {
                                    // Signaturinhalt ermitteln
                                    string name = dr["bezeichnung"].ToString() ?? string.Empty;
                                    string language = dr["sprache"].ToString() ?? string.Empty;
                                    string content = dr["inhalt"].ToString() ?? string.Empty;

                                    // Signatur hinzufügen
                                    signature = new SignatureItem(id, name, language, content);
                                    repo.AddSignature(signature);
                                }
                                signature.AddUser(user);
                            }
                        }
                    }
                    dr.Close();
                }
                conn.Close();
            }
        }

        private static void GetDbSignatureLayouts(SignatureRepository repo) {
            repo.ClearLayouts();

            // Plain-Text Layout für alle Signaturen
            SignatureLayout plain = new(0, MultiLanguage.GetText("plaintext"), string.Empty, string.Empty, string.Empty, string.Empty);
            repo.AddLayout(plain);
            foreach (SignatureItem signature in repo.GetAllSignatures()) {
                plain.AddSignature(signature);
            }

            // Layouts und Signaturzuordnung
            string query = @$"select l.bezeichnung, l.layout_id, z.signatur_id, l.inhalt, l.info_de, l.info_en, l.info_fr
                        from signatur s inner join zuordnung z on s.signatur_id = z.signatur_id
                        inner join layout l on z.layout_id = l.layout_id
                        order by l.layout_id, s.signatur_id";
            using SQLiteConnection conn = new($"Data Source={_file};", true);
            conn.Open();
            using (SQLiteCommand cmd = new(query, conn)) {
                using SQLiteDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows) {
                    while (dr.Read()) {
                        int id = int.Parse(dr["layout_id"].ToString() ?? "-1");
                        if (id >= 0) {
                            // Layout finden und ggf. einfügen
                            SignatureLayout? layout = repo.GetLayoutById(id);
                            if (layout == null) {
                                string name = dr["bezeichnung"].ToString() ?? string.Empty;
                                string content = dr["inhalt"].ToString() ?? string.Empty;
                                string info_de = dr["info_de"].ToString() ?? string.Empty;
                                string info_en = dr["info_en"].ToString() ?? string.Empty;
                                string info_fr = dr["info_fr"].ToString() ?? string.Empty;

                                // Bereinigung von obsoleten Tags
                                content = Regex.Replace(content, @"<\/{0,1}content_new>", string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
                                content = Regex.Replace(content, @"<sig>", string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
                                content = Regex.Replace(content, @"<barcode>", string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));
                                content = Regex.Replace(content, @"<old>", string.Empty, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

                                // Umwandlung von Tags in neues Format
                                content = Regex.Replace(content, @"<new>", $"<div class=\"{CLASS_CONTENT}\"></div>", RegexOptions.IgnoreCase, TimeSpan.FromSeconds(1));

                                // Layout hinzufügen
                                layout = new SignatureLayout(id, name, content, info_de, info_en, info_fr);
                                repo.AddLayout(layout);
                            }

                            foreach(SignatureUser user in repo.GetAllUsers()) {
                                // Keine Signatur Eintrag für alle Layouts hinzufügen
                                SignatureItem? nosignature = repo.GetSignatureByIdAndUser(0, user);
                                if (nosignature != null) {
                                    layout.AddSignature(nosignature);
                                }

                                // Spezifische Signatur aus Zuordnungstabelle hinzufügen
                                SignatureItem? signature = repo.GetSignatureByIdAndUser(int.Parse(dr["signatur_id"].ToString() ?? "-1"), user);
                                if (signature != null) {
                                    layout.AddSignature(signature);
                                }
                            }
                        }
                    }
                }
                dr.Close();
            }
            conn.Close();
        }
    }
}