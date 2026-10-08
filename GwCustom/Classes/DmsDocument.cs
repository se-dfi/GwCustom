using GwCustom.Classes;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.Models
{
    /// <summary>
    /// Klasse für DMS Dokumente
    /// </summary>
    public partial class DmsDocument
    {
        [GeneratedRegex(SENTGUIDPATTERN)]
        private static partial Regex SentGuidRegex();
        private string? _SentGuid;

        public DmsDocument()
        {
            Id = 0;
            SentGuid = string.Empty;
            CreationDate = string.Empty;
            CreationTime = string.Empty;
            Subject = string.Empty;
            MailFrom = string.Empty;
            MailTo = string.Empty;
            MailCc = string.Empty;
            Distribution = [];
            Locations = [];
        }

        public ulong Id { get; set; }

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

        public string MailFrom { get; set; }

        public string MailTo { get; set; }

        public string MailCc { get; set; }
        
        public List<string> Distribution { get; set; }

        public List<DmsLocation> Locations { get; set; }
    }
}