using C3POTypeLibrary;
using GwCustom.Custom;
using GwCustomUI.Classes;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.C3PO {
    /// <summary>
    /// Diese Klasse erzeugt Menüeinträge und steuert Befehlsausführungen
    /// </summary>
    [ComVisible(true)]
    [Guid("F7636A94-4A5C-4210-8E89-9A2E58B28E34")]
    public class CommandFactory {
        /// <summary>
        /// Groupwisebefehl initialisieren
        /// </summary>
        /// <param name="sGWContext">Context</param>
        /// <param name="sGWPersistentID">PersistentId</param>
        /// <param name="objGWBaseCommand">Basecommand</param>
        /// <param name="objGWParameter">Parameter</param>
        /// <returns></returns>
        public object? BuildCommand(string sGWContext, string sGWPersistentID, ref object objGWBaseCommand, ref object objGWParameter) {
            Command? GWCmd = null;

            try {
                // Ausführung nur im Kontext von E-Mails
                if (sGWContext.StartsWith(eGW_MAIL, StringComparison.CurrentCultureIgnoreCase)) {
                    // Befehl erzeugen
                    if (CmdMapping.TryGetValue(sGWPersistentID, out int value)) {
                        GWCmd = new Command(value) {
                            BaseCmd = objGWBaseCommand
                        };
                    }
                }
            } catch (Exception e) {
                CommonUtil.GlobalErrorHandler(e);
            }

            return (IGWCommand?)GWCmd;
        }


        /// <summary>
        /// Kontextmenü erweitern
        /// </summary>
        /// <param name="sGWContext">Context</param>
        /// <param name="objGWMenu">Menu</param>
        public void CustomizeContextMenu(ref string sGWContext, ref object objGWMenu) {
            try {
                // Kontextmenüs erweitern
                if (sGWContext.Length > 0 && sGWContext.StartsWith(eGW_MAIL, StringComparison.CurrentCultureIgnoreCase)) {
                    GWMenu Menu;
                    GWMenuItems MenuItems;

                    // Menüeinträge vorbelegen
                    Implementation.InitializeMessageContextMenu();

                    // Menütrenner vorab einfügen
                    Menu = (GWMenu)objGWMenu;
                    MenuItems = (GWMenuItems)Menu.MenuItems;
                    MenuItems.AddSeparator();

                    // Menüeinträge verarbeiten
                    foreach (KeyValuePair<string, int> entry in Implementation.ContextMenu) {
                        MenuItems = (GWMenuItems)Menu.MenuItems;
                        Object MenuItem = MenuItems.Item(entry.Key);
                        if (MenuItem == null) {
                            Command GWCmd = new(entry.Value) {
                                LongPrompt = entry.Key
                            };
                            MenuItems.Add(entry.Key, GWCmd, null);
                        }
                    }
                }
            } catch (Exception e) {
                CommonUtil.GlobalErrorHandler(e);
            }
        }


        /// <summary>
        /// Hauptmenü erweitern
        /// </summary>
        /// <param name="sGWContext">Context</param>
        /// <param name="objGWMenu">Menu</param>
        /// <returns></returns>
        public bool CustomizeMenu(ref string sGWContext, ref object objGWMenu) {
            try {
                GWMenu MainMenu;
                GWMenu Menu;
                GWMenuItems MenuItems;

                // Groupwise Hauptmenü erweitern
                if (sGWContext.Length > 0 && sGWContext.StartsWith(eGW_CLIENT, StringComparison.CurrentCultureIgnoreCase) &&
                    !sGWContext.Equals(eGW_BROWSER, StringComparison.CurrentCultureIgnoreCase)) {
                    MainMenu = (GWMenu)objGWMenu;

                    // Menüeinträge vorbelegen
                    Implementation.InitializeMainMenu();

                    // Menüpunkt "Werkzeuge" bzw. "Tools" ermitteln
                    MenuItems = (GWMenuItems)MainMenu.MenuItems;
                    Menu = (GWMenu)MenuItems.Item(MultiLanguage.GetText("groupwise.tools"));

                    // Eigenen Menüeintrag als Überpunkt am Ende einfügen
                    MenuItems = (GWMenuItems)Menu.MenuItems;
                    MenuItems.AddMenu(APPNAME, null, null);

                    // Referenz für eigenen Menüpunkt ermitteln
                    MenuItems = (GWMenuItems)Menu.MenuItems;
                    Menu = (GWMenu)MenuItems.Item(APPNAME);

                    // Menü-Unterpunkte erstellen
                    foreach (KeyValuePair<string, int> entry in Implementation.MainMenu) {
                        MenuItems = (GWMenuItems)Menu.MenuItems;
                        Object MenuItem = MenuItems.Item(entry.Key);
                        if (MenuItem == null) {
                            Command GWCmd = new(entry.Value) {
                                LongPrompt = entry.Key
                            };
                            MenuItems.Add(entry.Key, GWCmd, null);
                        }
                    }
                }

                // Hauptmenü geöffneter Nachrichten erweitern
                if (sGWContext.Length > 0 && sGWContext.StartsWith(eGW_MAIL, StringComparison.CurrentCultureIgnoreCase)) {
                    Menu = (GWMenu)objGWMenu;

                    // Menüeinträge vorbelegen
                    Implementation.InitializeMessageContextMenu();

                    // Menüpunkt "Datei" bzw. "File" ermitteln
                    MenuItems = (GWMenuItems)Menu.MenuItems;
                    Menu = (GWMenu)MenuItems.Item(MultiLanguage.GetText("groupwise.file"));

                    // Menütrenner vorab einfügen
                    MenuItems = (GWMenuItems)Menu.MenuItems;
                    MenuItems.AddSeparator();

                    // Menüeinträge verarbeiten
                    foreach (KeyValuePair<string, int> entry in Implementation.ContextMenu) {
                        MenuItems = (GWMenuItems)Menu.MenuItems;
                        Object MenuItem = MenuItems.Item(entry.Key);
                        if (MenuItem == null) {
                            Command GWCmd = new(entry.Value) {
                                LongPrompt = entry.Key
                            };
                            MenuItems.Add(entry.Key, GWCmd, null);
                        }
                    }
                }
            } catch (Exception e) {
                CommonUtil.GlobalErrorHandler(e);
            }

            return false; // Menüanpassung wiederholt ausführen (true) oder nur einmalig (false)
        }


        /// <summary>
        /// This method is the first method of CommandFactory called. It is used to tell GroupWise what UI items will be modified.
        /// </summary>
        /// <param name="lGWLCID">This is the locale ID of the application driving the C3po.</param>
        /// <returns>The C3po server returns any combination of bits for eGW_CMDINIT_MENUS, eGW_CMDINIT_TOOLBARS, eGW_CMDINIT_CONTEXT_MENUS, eGW_CMDINIT_NO_PREDEFINED</returns>
        public int Init(ref int lGWLCID) {
            // Nur Menü und Kontextmenü berücksichtigen
            return ((int)GwCmdInit.eGW_CMDINIT_MENUS + (int)GwCmdInit.eGW_CMDINIT_CONTEXT_MENUS);
        }


        /// <summary>
        /// This method is used to query the C3po for it's intention to support a predefined command.
        /// </summary>
        /// <param name="sGWContext">This is the actual object class associates with the command.</param>
        /// <param name="sGWPersistentID">This can be either a "pre-defined" ID or is an ID provided in a previous run of GW.</param>
        /// <returns>true: support this predefined command; false: don´t support this predefined command</returns>
        public bool WantCommand(string sGWContext, string sGWPersistentID) {
            bool bWantCommand = false;

            try {
                // Befehle nur eingeschränkt berücksichtigen
                if (sGWContext.StartsWith(eGW_MAIL, StringComparison.CurrentCultureIgnoreCase)) {
                    switch (sGWPersistentID) {
                        case eGW_CMDID_COMPOSE:
                        case eGW_CMDID_OPEN:
                        case eGW_CMDID_REPLY:
                        case eGW_CMDID_FORWARD:
                        case eGW_CMDID_RESEND:
                        case eGW_CMDID_SEND:
                            bWantCommand = true;
                            break;
                    }
                }
            } catch (Exception e) {
                CommonUtil.GlobalErrorHandler(e);
            }

            return bWantCommand;
        }
    }
}