using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using static GwCustomUI.Classes.Constants;

namespace GwCustomUI.Classes
{
    public class ConfigModel
    {
        public string AlternativeUser { get; set; } = string.Empty;
        public string RestUrlCustom { get; set; } = "https://gerw2k12mail:8011";
        public string RestUrlEnaio { get; set; } = "https://enaio.ger.int.seele.com/osrest";
        public int RestTimeout { get; set; } = 15000;
        public long UniversalFolderId { get; set; } = 0;
        public long UniversalDocumentId { get; set; } = 262144;
        public int DbUpdateHours { get; set; } = 4;
        public int StorageLimit { get; set; } = 20;
        public bool SignatureEnabled { get; set; } = true;
        public bool DmsFilingEnabled { get; set; } = true;
        public string LastUser { get; set; } = string.Empty;
        public List<UserSettings> Settings { get; set; } = [];
        public HashSet<string> BoldWords { get; set; } = ["seele", "sedak", "se commerce", "se-austria", "se cover", "sunglass"];


        /// <summary>
        /// Konfiguration speichern
        /// </summary>
        public void Save()
        {
            // RestTimeout erhöhen (bisher 5000)
            if (RestTimeout < 15000)
            {
                RestTimeout = 15000;
            }

            // Storagelimit erhöhen (bisher 10)
            if (StorageLimit < 20)
            {
                StorageLimit = 20;
            }

            // BoldWords erweitern
            BoldWords.Add("sunglass"); // Duplikate sind im Hashset nicht möglich, Prüfung auf vorhanden ist unnötig

            JsonSerializerOptions jsonSerializerOptions = new() {
                WriteIndented = true
            };
            JsonSerializerOptions options = jsonSerializerOptions;
            options.Converters.Add(new JsonStringEnumConverter());
            string json = JsonSerializer.Serialize(this, options);
            File.WriteAllText(Path.Combine(APPDATAPATH, CONFIGFILE), json);
        }

        /// <summary>
        /// Gibt die letzten Einstellungen für den übergebenen Benutzer zurück oder legt diese neu an
        /// </summary>
        /// <param name="user">Benutzername</param>
        /// <returns>Benutzereinstellungsobjekt</returns>
        public UserOptions GetUserOptions(string user)
        {
            UserSettings? userconfig = null;
            List<UserSettings> userconfigs = [.. Settings.Where(item => item.User.Equals(user))];
            if (userconfigs.Count == 1)
            {
                userconfig = userconfigs.First();
            }
            if (userconfig == null)
            {
                userconfig = new()
                {
                    User = user,
                    UserOptions = new()
                };
                Settings.Add(userconfig);
            }
            return userconfig.UserOptions;
        }
    }


    public class UserSettings
    {
        public string User { get; set; } = string.Empty;
        public UserOptions UserOptions { get; set; } = new();
    }


    public class UserOptions
    {
        public int Layout { get; set; } = 0;
        public int Signature { get; set; } = 0;
        public bool Info { get; set; } = false;
        public bool Mobile { get; set; } = false;
    }
}