using GwCustomUI.Classes;
using GwCustomUI.Forms;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using System.Text.RegularExpressions;
using static GwCustomUI.Classes.Constants;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace GwCustomUI.Classes
{
    public static partial class CommonUtil
    {

        [GeneratedRegex(BARCODEPATTERN)]
        private static partial Regex BarcodeRegex();

        /// <summary>
        /// Globale Fehlerbehandlung
        /// </summary>
        /// <param name="e"></param>
        public static void GlobalErrorHandler(Exception e, bool silent = false) {
            // Fehlerformular anzeigen
            if (!silent) {
                FError error = new(e.Message);
                error.ShowDialog();
            }

            // Eventlog-Eintrag
            string logmessage = e.Message + Environment.NewLine + Environment.NewLine + e.StackTrace;
            EventLog logger = new() {
                Source = APPNAME,
                Log = $"{APPNAME} Log"
            };
            logger.WriteEntry(logmessage, EventLogEntryType.Error);
        }

        /// <summary>
        /// Konfiguration laden und zurückgeben
        /// </summary>
        /// <returns></returns>
        public static ConfigModel GetConfig()
        {
            if (!File.Exists(Path.Combine(APPDATAPATH, CONFIGFILE)))
            {
                // Keine Konfigurationsdatei vorhanden, erzeugen
                ConfigModel config = new();
                config.Save();
            }

            // Lokale Konfiguration vorhanden, einlesen und zurückgeben
            ConfigurationBuilder builder = new();
            builder.SetBasePath(APPDATAPATH);
            builder.AddJsonFile(CONFIGFILE, false, true);
            ConfigModel? result = builder.Build().Get<ConfigModel>();
            return result ?? throw new Exception(MultiLanguage.GetText("noconfig"));
        }

        /// <summary>
        /// Programmordner im Filesystem erstellen falls noch nicht vorhanden
        /// </summary>
        /// <param name="path">Pfad zum zu erstellenden Ordner</param>
        public static void CreateFolder(string path) {
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }
        }

        /// <summary>
        /// WebView2-Datenverzeichnis bei Versionswechsel vollständig zurücksetzen (Schutz vor defektem EBWebView-Ordner)
        /// </summary>
        public static void CleanWebViewUserData() {
            string markerfile = Path.Combine(USERDATAPATH, "webview.version");
            string current = File.Exists(markerfile) ? File.ReadAllText(markerfile) : string.Empty;

            if (current != APPVERSION) {
                string ebwebview = Path.Combine(USERDATAPATH, "EBWebView");
                if (Directory.Exists(ebwebview)) {
                    Directory.Delete(ebwebview, true);
                }
                File.WriteAllText(markerfile, APPVERSION);
            }
        }

        /// <summary>
        /// String kürzen nach abgeschlossenem Wort gemäß der gewünschten Länge
        /// </summary>
        /// <param name="text"></param>
        /// <param name="maxlen"></param>
        /// <returns></returns>
        public static string TruncateWord(string text, int maxlen) {
            string result = text ?? string.Empty;

            if (result.Length >= maxlen && result.IndexOf(' ', maxlen) != -1) {
                return string.Concat(result.AsSpan(0, result.IndexOf(' ', maxlen)), "...");
            }

            // Keine Kürzung möglich oder notwendig
            return result;
        }

        /// <summary>
        /// Änderungsdatum einer Datei ermitteln
        /// </summary>
        /// <param name="path">Dateipfad</param>
        /// <returns>Zeitpunkt</returns>
        public static DateTime GetFileDate(string path)
        {
            FileInfo fi = new(path);
            return fi.LastWriteTime;
        }

        /// <summary>
        /// Absoluter Zeitunterschied zweier Datumswerte in ganzen Stunden
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns>Zeitunterschied in ganzen Stunden</returns>
        public static int GetHoursBetween(DateTime t1, DateTime t2)
        {
            TimeSpan ts = t1 - t2;
            return Math.Abs(ts.Hours);
        }

        /// <summary>
        /// Temporäre Datei mit übergebener Endung erstellen
        /// </summary>
        /// <param name="extension">Endung der temporären Datei</param>
        /// <returns>Pfad zur temporären Datei</returns>
        public static string CreateTempFile(string extension)
        {
            string file;
            string temp = Path.GetTempFileName();

            _ = new FileInfo(temp)
            {
                Attributes = FileAttributes.Temporary
            };

            file = temp;
            if (!string.IsNullOrEmpty(extension) && !extension.EndsWith("tmp", StringComparison.CurrentCultureIgnoreCase))
            {
                File.Move(temp, Path.ChangeExtension(temp, extension));
                file = Path.ChangeExtension(temp, extension);
            }

            return file;
        }

        /// <summary>
        /// Temporäre Datei mit übergebenem Inhalt füllen, gegebenenfalls überschreiben
        /// </summary>
        /// <param name="file">Pfad zur temporären Datei</param>
        /// <param name="content">Dateiinhalt</param>
        public static void UpdateTempFile(string file, string content)
        {
            using StreamWriter sw = new(file, false);
            sw.Write(content);
        }

        /// Datei löschen sofern diese existiert
        /// </summary>
        /// <param name="file">Pfad zur zu löschenden Datei</param>
        public static void DeleteFileIfExists(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
        }

        /// <summary>
        /// Auflistung in trennzeichengetrennte Zeichenfolge umwandeln
        /// </summary>
        /// <param name="list">Liste</param>
        /// <param name="separator">Trennzeichen</param>
        /// <param name="surround">Maskeriungszeichen</param>
        /// <returns>Auflistung als Zeichenfolge mit Trennzeichen</returns>
        public static string GetSeparatedList(List<string> list, string separator, string before = "", string behind = "")
        {
            string result = string.Empty;
            foreach (object item in list)
            {
                if (!string.IsNullOrEmpty(result))
                {
                    result += separator;
                }
                result += before + item + behind;
            }
            return result;
        }

        /// <summary>
        /// Formatierung vorgegebener Begriffe aus übergebenem Htmlcode in fett
        /// </summary>
        /// <returns></returns>
        public static string FormatWordsBold(string input)
        {
            List<string> words = [.. GetConfig().BoldWords];

            /* alte Logik, Problem: URL´s gehen kaputt
            string pattern = @"(?<![a-zA-Z@.\/])(" + string.Join("|", words) + @")(?![a-zA-Z])";
            return Regex.Replace(input, pattern, m => $"<span style=\"font-weight: bold;\">{m.Value}</span>");
            */

            string result = input;
            string pattern = @"(?<![a-zA-Z@.\/])(myword)(?![A-Za-z])(?![^<]*>)";
            foreach (string word in words)
            {
                string p = pattern.Replace("myword", word);
                result = Regex.Replace(result, p, m => $"<span style=\"font-weight: bold;\">{m.Value}</span>", RegexOptions.Multiline);
            }
            return result;
        }

        /// <summary>
        /// Ablageclient mit Id-Liste aufrufen
        /// </summary>
        /// <param name="idList"></param>
        public static void ShowDmsClient(List<ulong> idList)
        {
            // Aufruf des Ablageclient mit Id-Liste
            if (idList.Count > 0)
            {
                ProcessStartInfo psi = new()
                {
                    FileName = Path.Combine(APPINSTALLPATH, CONTAINERAPP),
                    WorkingDirectory = APPINSTALLPATH,
                    Arguments = $"-prod {string.Join(",", idList)}"
                };
                Process.Start(psi);
            }
        }

        /// <summary>
        /// Barcode-Zeichenfolge innerhalb des übergebenen Inhalts finden und zurückgeben
        /// </summary>
        /// <returns>Gefundener Barcode oder leere Zeichenfolge</returns>
        public static string FindBarcode(string source) {
            string result = string.Empty;

            // Barcode-Zeichenfolge finden, Beispiel: SEEC#0026237518#
            if (BarcodeRegex().IsMatch(source)) {
                result = BarcodeRegex().Match(source).Groups[1].Value;
            }

            return result;
        }

        /// <summary>
        /// Emailadressen als Liste aus Empfängerzeichenfolge extrahieren
        /// </summary>
        /// <param name="input"></param>
        /// <returns>Liste erkannter Emailadressen</returns>
        public static List<string> GetMailaddressesFromString(string input) {
            List<string> result = [];

            // Komma als Separator annehmen
            foreach (string item in input.Split(',')) {
                MailAddress? address;
                try {
                    address = new(item.ToLower());
                } catch {
                    address = null;
                }
                if (address != null && !result.Contains(address.Address.ToString())) {
                    result.Add(address.Address.ToString());
                }
            }

            // Semikolon als Separator annehmen
            foreach (string item in input.Split(';')) {
                MailAddress? address;
                try {
                    address = new(item.ToLower());
                } catch {
                    address = null;
                }
                if (address != null && !result.Contains(address.Address.ToString())) {
                    result.Add(address.Address.ToString());
                }
            }

            return result;
        }
    }
}