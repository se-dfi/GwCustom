using C3POTypeLibrary;
using GroupWiseCommander;
using GwCustom.Custom;
using GwCustomUI.Classes;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace GwCustom.C3PO
{
    /// <summary>
    /// Implementierung der C3PO mit Initialisierungsschritten
    /// </summary>
    public class Implementation
    {
        /// <summary>
        /// Globaler C3PO-Manager
        /// </summary>
        public static C3POManager? GwC3POManager { get; set; }

        /// <summary>
        /// Globaler Groupwise-Commander
        /// </summary>
        public static GWCommander? GwCommander { get; set; }
        
        /// <summary>
        /// Kontextmenü-Einträge
        /// </summary>
        public static Dictionary<string, int> ContextMenu { get; set; } = [];

        /// <summary>
        /// Hauptmenü-Einträge
        /// </summary>
        public static Dictionary<string, int> MainMenu { get; set; } = [];

        /// <summary>
        /// Initiale Ausführungen bei Programmstart
        /// </summary>
        /// <param name="gwmanager"></param>
        public static void Init(object gwmanager)
        {
            // Globale Groupwiseobjekte initialisieren
            GwC3POManager = (C3POManager)gwmanager;
            GwCommander = new GWCommander();

            // Oberflächeninitialisierung
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Allgemeine Fehlerbehandlung für Formulare und unbehandelte Fehler konfigurieren
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            // Mehrsprachigkeit einrichten und Sprache vorbelegen
            // Todo vereinheitlichen
            MultiLanguage.SetLanguage(GroupwiseUtil.GetClientLanguage());
            GwCustomUI.Classes.MultiLanguage.SetLanguage(GroupwiseUtil.GetClientLanguage());
        }

        /// <summary>
        /// Handler für Threadexceptions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            CommonUtil.GlobalErrorHandler(e.Exception);
        }

        /// <summary>
        /// Handler für sonstige Exceptions
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            CommonUtil.GlobalErrorHandler((Exception)e.ExceptionObject);
        }

        /// <summary>
        /// Einfache Kontextmenüeinträge für Mails abhängig von Konfiguration vorbelegen
        /// </summary>
        public static void InitializeMessageContextMenu()
        {
            if (ContextMenu.Count == 0)
            {
                ContextMenu.Add(MultiLanguage.GetText("dmsstorage"), 100);
                ContextMenu.Add(MultiLanguage.GetText("dmslocating"), 101);
            }
        }

        /// <summary>
        /// Hauptmenüeinträge abhängig von Konfiguration vorbelegen
        /// </summary>
        public static void InitializeMainMenu()
        {
            if (MainMenu.Count == 0)
            {
                MainMenu.Add(MultiLanguage.GetText("configuration"), 200);
                MainMenu.Add(MultiLanguage.GetText("information"), 201);
            }
        }
    }
}