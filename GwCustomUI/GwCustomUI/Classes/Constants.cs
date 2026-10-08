using System;
using System.Collections.Generic;
using System.IO;

namespace GwCustomUI.Classes
{
    /// <summary>
    /// Diese Klasse enthält alle erforderlichen Konstanten
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Anwendungskürzel
        /// </summary>
        public static readonly string APPSHORT = "gwse";

        /// <summary>
        /// Anwendungsname
        /// </summary>
        public static readonly string APPNAME = "Groupwi(se) Integration";

        /// <summary>
        /// Versionsnummer
        /// </summary>
        public static readonly string APPVERSION = "1.4.1";

        /// <summary>
        /// Releasedate der Version
        /// </summary>
        public static readonly DateTime APPRELEASEDATE = new(2026, 4, 13);

        /// <summary>
        /// Applikationspfad Daten
        /// </summary>
        public static readonly string APPDATAPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), APPNAME);

        /// <summary>
        /// Benutzerdatenverzeichnis u.a. für WebView2
        /// </summary>
        public static readonly string USERDATAPATH = Path.Combine(APPDATAPATH, "WebViewUserData");

        /// <summary>
        /// Applikationspfad Installation
        /// </summary>
        public static readonly string APPINSTALLPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "seele GmbH", APPNAME);

        // Groupwise Command Konstanten
        public const string eGW_CMDID_ACCEPT = "GW#C#ACCEPT";
        public const string eGW_CMDID_ARCHIVE = "GW#C#ARCHIVE";
        public const string eGW_CMDID_COMPLETE = "GW#C#COMPLETE";
        public const string eGW_CMDID_COMPOSE = "GW#C#COMPOSE";
        public const string eGW_CMDID_DECLINE = "GW#C#DECLINE";
        public const string eGW_CMDID_DELEGATE = "GW#C#DELEGATE";
        public const string eGW_CMDID_DELETE = "GW#C#DELETE";
        public const string eGW_CMDID_DOC_CHECKIN = "GW#C#DOC_CHECKIN";
        public const string eGW_CMDID_DOC_CHECKOUT = "GW#C#DOC_CHECKOUT";
        public const string eGW_CMDID_DOC_RESETINUSE = "GW#C#DOC_RESETINUSE";
        public const string eGW_CMDID_FORWARD = "GW#C#FORWARD";
        public const string eGW_CMDID_INFO = "GW#C#INFO";
        public const string eGW_CMDID_OPEN = "GW#C#OPEN";
        public const string eGW_CMDID_OPENFILEATTACH = "GW#C#OPENFILEATTACH";
        public const string eGW_CMDID_PRINT = "GW#C#PRINT";
        public const string eGW_CMDID_PROPERTIES = "GW#C#PROPERTIES";
        public const string eGW_CMDID_REPLY = "GW#C#REPLY";
        public const string eGW_CMDID_RESEND = "GW#C#RESEND";
        public const string eGW_CMDID_SAVE = "GW#C#SAVE";
        public const string eGW_CMDID_SAVEAS = "GW#C#SAVEAS";
        public const string eGW_CMDID_SAVEATTACHAS = "GW#C#SAVEATTACHAS";
        public const string eGW_CMDID_SEND = "GW#C#SEND";
        public const string eGW_CMDID_SETALARMS = "GW#C#SETALARMS";
        public const string eGW_CMDID_UNDELETE = "GW#C#UNDELETE";
        public const string eGW_CMDID_VIEW = "GW#C#VIEW";
        public const string eGW_CMDID_VIEWATTACH = "GW#C#VIEWATTACH";

        // Groupwise Event Konstanten
        public const string eGW_CMDEVTID_READY = "GW#E#0";
        public const string eGW_CMDEVTID_DELIVERY = "GW#E#1";
        public const string eGW_CMDEVTID_SHUTDOWN = "GW#E#2";

        // Mapping für Commands zu eindeutigem Befehl
        public static readonly Dictionary<string, int> CmdMapping = new()
        {
            { eGW_CMDID_COMPOSE, 10 },
            { eGW_CMDID_OPEN, 11 },
            { eGW_CMDID_REPLY, 12 },
            { eGW_CMDID_FORWARD, 13 },
            { eGW_CMDID_RESEND, 14 },
            { eGW_CMDID_SEND, 15 },
        };

        // Mapping für anzulegende Kategorien
        public static readonly Dictionary<string, int> Categories = new()
        {
            { "enaio missing", 206 },
            { "enaio existing", 3115760 },
            { "enaio valid", 6723891 }
        };

        // Groupwise Commandvalidation Konstanten
        public const int eGW_CMDVAL_ALWAYS = 1;
        public const int eGW_CMDVAL_CHECKED = 2;
        public const int eGW_CMDVAL_DISABLED = 4;

        // Groupwise Klassenbezeichner
        public const string eGW_CLIENT = "GW.CLIENT";
        public const string eGW_BROWSER = "GW.CLIENT.WINDOW.BROWSER";
        public const string eGW_CALENDAR = "GW.CLIENT.WINDOW.CALENDAR";
        public const string eGW_APPOINTMENT = "GW.MESSAGE.APPOINTMENT";
        public const string eGW_MAIL = "GW.MESSAGE.MAIL";

        // HTML Klassenkonstanten
        public const string CLASS_LAYOUT = "GwIntegrationLayout";
        public const string CLASS_CONTENT = "GwIntegrationInhalt";
        public const string CLASS_SIGNATURE = "GwIntegrationSignatur";
        public const string CLASS_BARCODE = "GwIntegrationBarcode";
        public const string CLASS_MOBILE = "GwIntegrationMobil";
        public const string CLASS_INFO = "GwIntegrationInfo";
        public const string CLASS_HISTORY = "GwIntegrationVerlauf";
        public const string CLASS_GW_MAIN = "GroupWiseMessageBody";
        public const string CLASS_GW_ACTIVE = "GroupWiseMessageBody active";
        public const string CLASS_GW_HISTORY = "GroupwiseReplyHeader";
        public const string ID_GW_SECTION = "GroupWiseSection";

        // Allgemeine Anwendungskonstanten
        public const string NAMESPACE = "GwCustom";
        public const string ROOTCLASS = "Entry";
        public const string CONFIGFILE = "appsettings.json";
        public const string REGKEY_CONFIG = @"SOFTWARE\" + NAMESPACE;
        public const string REGKEY_C3PO = @"SOFTWARE\Novell\GroupWise\5.0\C3PO\DataTypes\";
        public const string BARCODEPATTERN = @"[A-Z]{2}EC#([0-9]{10})#";
        public const string OSFILEIDPATTERN = @"(\[Dokumente\][\r|\n]*#OSID#=)([0-9]*)";
        public const string SENTGUIDCLASS = "X-SentGuid";
        public const string SENTGUIDPATTERN = @"([A-Fa-f0-9]{8}-([A-Fa-f0-9]{4}-){3}[A-Fa-f0-9]{12})";
        public const string HASHFIELD = "HASH";
        public const string CONTAINERAPP = "WebAppContainer.exe";
        public const int CS_DROPSHADOW = 0x20000;

        // enaio Feldkonstanten
        public const string DMS_DUPLICATENAME = "DUBLETTEN";
        public const string DMSFIELD_SENTGUID = "MAIL_GUID";
        public const string DMSFIELD_MAILFROM = "MAIL_FROM";
        public const string DMSFIELD_MAILTO = "MAIL_TO";
        public const string DMSFIELD_MAILCC = "MAIL_CC";
        public const string DMSFIELD_MAILDATE = "MAIL_SUBMIT_TIME";
        public const string DMSFIELD_MAILTIME = "MAIL_SUBMIT_REALTIME";
        public const string DMSFIELD_MAILSUBJECT = "MAIL_SUBJECT";
        public const string DMSFIELD_COMMENT = "i_Kommentar";
        public const string DMSFIELD_CLIENT = "i_Mandant";
        public const string DMSFIELD_PROJECTTYPE = "i_Projekttyp";
        public const string DMSFIELD_PROJECTNUMBER = "i_ProjektNr.";
        public const string DMSFIELD_PROJECTNAME = "i_Projektbezeichnung";
        public const string DMSFIELD_INBOX = "i_Inbox";
        public const string DMSFIELD_OUTBOX = "i_projektunabhaengigerAusgang";
        public const string DMSFIELD_VISIBILITY = "i_sichtbarfuer";
        public const string DMSFIELD_DISTRIBUTION = "i_Verteilungshistorie";
        public const string DMSFIELD_CREATOR = "i_Erfasser";
    }
}