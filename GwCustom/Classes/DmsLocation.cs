namespace GwCustom.Classes {
    /// <summary>
    /// Klasse für DMS Standorte
    /// </summary>
    public class DmsLocation
    {
        public DmsLocation()
        {
            Id = 0;
            Client = string.Empty;
            Type = string.Empty;
            Number = string.Empty;
            Name = string.Empty;
            Inbox = false;
            Outbox = false;
        }

        public ulong Id { get; set; }

        public string Client { get; set; }

        public string Type { get; set; }

        public string Number { get; set; }

        public string Name { get; set; }

        public bool Outbox { get; set; }

        public bool Inbox { get; set; }

        /// <summary>
        /// Ist das Projekt gültig
        /// </summary>
        /// <returns>bool</returns>
        public bool IsValid()
        {
            return Id > 0 && !string.IsNullOrEmpty(Client) && !string.IsNullOrEmpty(Type) && !string.IsNullOrEmpty(Number);
        }

        /// <summary>
        /// Textuelle Standortinformation
        /// </summary>
        /// <returns>string</returns>
        public string GetInfo()
        {
            string result = string.Empty;

            if (IsValid())
            {
                result += $"{Number}" + (string.IsNullOrEmpty(Name) ? string.Empty : $" - {Name}");
            }
            else if (Inbox == true)
            {
                result += $"Inbox {Name}";
            }
            else if (Outbox == true)
            {
                result += $"Outbox {Name}";
            }
            else
            {
                result += $"{Name}";
            }

            return result;
        }
    }
}