using GwCustom.C3PO;
using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using static GwCustomUI.Classes.Constants;

namespace GwCustom
{
    /*
     * Das grundlegende Konstrukt der Anwendung stellt sich wie folgt dar:
     * 
     * 1.) Die aus dem Projekt erzeugte Klassenbibliothek GwCustom.comhost.dll wird mittels regsvr32 registriert
     * 
     * 2.) Bei Registrierung wird über [ComRegisterFunctionAttribute] die Interaktion in die Registry eingetragen
     *     und entsprechend auch bei Deregistrierung mittels [ComUnregisterFunctionAttribute] wieder entfernt.
     *     
     *     Basispfad: HKLM\SOFTWARE\Novell\GroupWise\5.0\C3PO\DataTypes\
     *     
     * 3.) Einträge für Menü und Kontextmenü werden folgendermaßen eingebunden und mit Funktion ausgestattet.
     *     Dies erfordert Einträge des Objekts CommandFactory für den gewünschten Kontext in der Registry
     *     
     *     Entry, new CommandFactory
     *       CommandFactory.Init
     *         CommandFactory.CustomizeMenu / CommandFactory.CustomizeContextMenu
     *           new Command -> Execute
     *           
     * 4.) Events werden folgendermaßen registriert und ausgelöst.
     *     Dies erfordert Einträge des Objekts EventMonitor und der von Groupwise unterstützen Events für den
     *     gewünschten Kontext in der Registry (siehe 2).
     *     
     *     Entry, new EventMonitor
     *       EventMonitor.Notify
     *         eGW_CMDEVTID_READY (GW#E#0)
     *         eGW_CMDEVTID_DELIVERY (GW#E#1)
     *         eGW_CMDEVTID_SHUTDOWN (GW#E#2)
     */

    /// <summary>
    /// Diese Klasse bildet den Einstieg und die direkte Interaktion mit Groupwise ab
    /// </summary>
    [ComVisible(true)]
    [Guid("80517EEA-EAC4-498A-AD0E-63CAA3130F9C")]
    public class Entry
    {
        /// <summary>
        /// Schließen der Anwendung erlauben
        /// </summary>
        private bool m_bCanShutdown;


        /// <summary>
        /// Konstruktor, COM-Voraussetzung
        /// </summary>
        public Entry()
        {
        }


        /// <summary>
        /// CommandFactory, COM-Voraussetzung
        /// </summary>
        public CommandFactory CommandFactory
        {
            get
            {
                return new CommandFactory();
            }
        }


        /// <summary>
        /// Description, COM-Voraussetzung
        /// </summary>
        public string Description
        {
            get
            {
                return string.Empty;
            }
        }


        /// <summary>
        /// EventMonitor, COM-Voraussetzung
        /// </summary>
        public EventMonitor EventMonitor
        {
            get
            {
                return new EventMonitor();
            }
        }


        /// <summary>
        /// Setter für m_bCanShutdown, COM-Voraussetzung
        /// </summary>
        public bool SetCanShutdown
        {
            set
            {
                m_bCanShutdown = value;
            }
        }


        /// <summary>
        /// Getter für m_bCanShutdown, COM-Voraussetzung
        /// </summary>
        public bool CanShutdown()
        {
            return m_bCanShutdown;
        }


        /// <summary>
        /// DeInit, COM-Voraussetzung
        /// </summary>
        public void DeInit()
        {
        }


        /// <summary>
        /// Init, COM-Voraussetzung
        /// </summary>
        public void Init(ref object objGWManager)
        {
            Implementation.Init(objGWManager);
            m_bCanShutdown = true;
        }


        /// <summary>
        /// Erzeugen der Anwendungseinträge in der Registry
        /// </summary>
        /// <param name="t"></param>
        [ComRegisterFunction]
        public static void RegisterC3poServer(Type t)
        {
            RegistryKey rkLocalMachine = Registry.LocalMachine;
            RegistryKey rkRoot;

            // Erzeugen der Anwendungseinträge für die Anwendung
            rkRoot = rkLocalMachine.CreateSubKey(REGKEY_C3PO + eGW_CLIENT + "\\" + NAMESPACE + "." + ROOTCLASS);
            if (rkRoot != null)
            {
                using RegistryKey
                    rkEvents = rkRoot.CreateSubKey("Events"),
                    rkObjects = rkRoot.CreateSubKey("Objects");
                rkEvents.SetValue("OnReady", "");
                rkEvents.SetValue("OnShutdown", "");
                rkObjects.SetValue("CommandFactory", "");
                rkObjects.SetValue("EventMonitor", "");
            }

            // Erzeugen der Anwendungseinträge für E-Mailnachrichten
            rkRoot = rkLocalMachine.CreateSubKey(REGKEY_C3PO + eGW_MAIL + "\\" + NAMESPACE + "." + ROOTCLASS);
            if (rkRoot != null)
            {
                using RegistryKey
                    rkEvents = rkRoot.CreateSubKey("Events"),
                    rkObjects = rkRoot.CreateSubKey("Objects");
                rkEvents.SetValue(eGW_CMDID_COMPOSE, "");
                rkEvents.SetValue(eGW_CMDID_FORWARD, "");
                rkEvents.SetValue(eGW_CMDID_OPEN, "");
                rkEvents.SetValue(eGW_CMDID_REPLY, "");
                rkEvents.SetValue(eGW_CMDID_RESEND, "");
                rkEvents.SetValue(eGW_CMDID_SEND, "");
                rkEvents.SetValue("OnDelivery", "");
                rkObjects.SetValue("EventMonitor", "");
                rkObjects.SetValue("CommandFactory", "");
            }

            // Erzeugen der Anwendungseinträge für Terminnachrichten
            rkRoot = rkLocalMachine.CreateSubKey(REGKEY_C3PO + eGW_APPOINTMENT + "\\" + NAMESPACE + "." + ROOTCLASS);
            if (rkRoot != null)
            {
                using RegistryKey
                    rkEvents = rkRoot.CreateSubKey("Events"),
                    rkObjects = rkRoot.CreateSubKey("Objects");
                rkEvents.SetValue(eGW_CMDID_COMPOSE, "");
                rkEvents.SetValue(eGW_CMDID_FORWARD, "");
                rkEvents.SetValue(eGW_CMDID_OPEN, "");
                rkEvents.SetValue(eGW_CMDID_REPLY, "");
                rkEvents.SetValue(eGW_CMDID_RESEND, "");
                rkEvents.SetValue(eGW_CMDID_SEND, "");
                rkEvents.SetValue("OnDelivery", "");
                rkObjects.SetValue("EventMonitor", "");
                rkObjects.SetValue("CommandFactory", "");
            }
        }


        /// <summary>
        /// Entfernen der Anwendungseinträge aus der Registry
        /// </summary>
        /// <param name="t"></param>
        [ComUnregisterFunction]
        public static void UnregisterC3poServer(Type t)
        {
            RegistryKey rkLocalMachine = Registry.LocalMachine;
            RegistryKey? rkRoot;
            RegistryKey? rkArea;

            // Entfernen der Anwendungseinträge für die Anwendung
            rkRoot = rkLocalMachine.OpenSubKey(REGKEY_C3PO + eGW_CLIENT, true);
            if (rkRoot != null)
            {
                rkArea = rkRoot.OpenSubKey(NAMESPACE + "." + ROOTCLASS);
                if (rkArea != null)
                    rkRoot.DeleteSubKeyTree(NAMESPACE + "." + ROOTCLASS);
                rkRoot.Close();
            }

            // Entfernen der Anwendungseinträge für E-Mailnachrichten
            rkRoot = rkLocalMachine.OpenSubKey(REGKEY_C3PO + eGW_MAIL, true);
            if (rkRoot != null)
            {
                rkArea = rkRoot.OpenSubKey(NAMESPACE + "." + ROOTCLASS);
                if (rkArea != null)
                    rkRoot.DeleteSubKeyTree(NAMESPACE + "." + ROOTCLASS);
                rkRoot.Close();
            }

            // Entfernen der Anwendungseinträge für Terminnachrichten
            rkRoot = rkLocalMachine.OpenSubKey(REGKEY_C3PO + eGW_APPOINTMENT, true);
            if (rkRoot != null)
            {
                rkArea = rkRoot.OpenSubKey(NAMESPACE + "." + ROOTCLASS);
                if (rkArea != null)
                    rkRoot.DeleteSubKeyTree(NAMESPACE + "." + ROOTCLASS);
                rkRoot.Close();
            }
        }
    }
}