using C3POTypeLibrary;
using GwCustom.Actions;
using GwCustom.Custom;
using GwCustomUI.Classes;
using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.C3PO
{
    /// <summary>
    /// Diese Klasse bildet Groupwise-Befehle ab und führt diese aus
    /// </summary>
    /// <remarks>
    /// Command, COM-Voraussetzung
    /// </remarks>
    /// <param name="id">id</param>
    [ComVisible(true)]
    [Guid("565DF085-5C3F-4386-AB0F-39897323A601")]
    public class Command(int id) : IGWCommand
    {
        private object? m_objBaseCmd;
        private string? m_sLongPrompt;
        private string? m_sToolTip;
        private readonly int m_PersistentID = id;
        private readonly Lock syncLock = new();

        /// <summary>
        /// BaseCmd, COM-Voraussetzung 
        /// </summary>
        public object? BaseCmd
        {
            get
            {
                return m_objBaseCmd;
            }
            set
            {
                m_objBaseCmd = value;
            }
        }

        /// <summary>
        /// LongPrompt, COM-Voraussetzung
        /// </summary>
        public string? LongPrompt
        {
            get
            {
                return m_sLongPrompt;
            }
            set
            {
                m_sLongPrompt = value;
            }
        }

        /// <summary>
        /// Parameters, COM-Voraussetzung
        /// </summary>
        public object? Parameters
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// PersistentID, COM-Voraussetzung
        /// </summary>
        public string? PersistentID
        {
            get
            {
                return null;
            }
        }

        /// <summary>
        /// ToolTip, COM-Voraussetzung
        /// </summary>
        public string? ToolTip
        {
            get
            {
                return m_sToolTip;
            }
            set
            {
                m_sToolTip = value;
            }
        }

        /// <summary>
        /// Befehl ausführen, COM-Voraussetzung
        /// </summary>
        public void Execute()
        {
            lock (syncLock)
            {
                bool cancel = false;
                Execute? cmd = null;

                try
                {
                    // Befehle vor der eigentlichen Groupwise-Aktion
                    if (CmdMapping.ContainsValue(m_PersistentID))
                    {
                        switch (CmdMapping.FirstOrDefault(x => x.Value == m_PersistentID).Key)
                        {
                            case eGW_CMDID_COMPOSE:
                                cmd = new CmdCompose();
                                break;

                            case eGW_CMDID_OPEN:
                                cmd = new CmdOpen();
                                break;

                            case eGW_CMDID_REPLY:
                                cmd = new CmdReply();
                                break;

                            case eGW_CMDID_FORWARD:
                                cmd = new CmdForward();
                                break;

                            case eGW_CMDID_SEND:
                                cmd = new CmdSend();
                                break;
                        }
                    }
                    if (cmd != null)
                    {
                        cancel = cmd.RunBefore();
                    }

                    // Abbruchbedingung
                    if (cancel == true)
                    {
                        return;
                    }

                    // Ausführung der eigentlichen Groupwise-Aktion
                    if (m_objBaseCmd != null && Implementation.GwC3POManager != null)
                    {
                        Implementation.GwC3POManager.CreateGWCommand(m_objBaseCmd)?.Execute();
                    }

                    // Befehle nach der eigentlichen Groupwise-Aktion
                    if (CmdMapping.ContainsValue(m_PersistentID))
                    {
                        switch (CmdMapping.FirstOrDefault(x => x.Value == m_PersistentID).Key)
                        {
                            case eGW_CMDID_COMPOSE:
                                cmd ??= new CmdCompose();
                                break;

                            case eGW_CMDID_OPEN:
                                cmd ??= new CmdOpen();
                                break;

                            case eGW_CMDID_RESEND:
                                cmd ??= new CmdResend();
                                break;

                            case eGW_CMDID_SEND:
                                cmd ??= new CmdSend();
                                break;

                            case eGW_CMDID_FORWARD:
                                cmd ??= new CmdForward();
                                break;

                            default:
                                cmd = null;
                                break;
                        }
                    }
                    cmd?.Run();

                    // Menü- und Kontextmenüaufrufe
                    switch (m_PersistentID)
                    {
                        case 100:
                            new MenuStorage().Run();
                            break;
                        case 101:
                            new MenuLocating().Run();
                            break;
                        case 200:
                            new MenuConfiguration().Run();
                            break;
                        case 201:
                            new MenuInformation().Run();
                            break;
                    }
                }
                catch (Exception e)
                {
                    CommonUtil.GlobalErrorHandler(e);
                }
            }

            return;
        }

        /// <summary>
        /// Help, COM-Voraussetzung
        /// </summary>
        public void Help()
        {
        }

        /// <summary>
        /// Undo, COM-Voraussetzung
        /// </summary>
        public void Undo()
        {
        }

        /// <summary>
        /// Befehl validieren, gegebenenfalls in Abhängigkeit deaktivieren, COM-Voraussetzung
        /// </summary>
        /// <returns></returns>
        public int Validate()
        {
            int res = eGW_CMDVAL_ALWAYS;

            try
            {
                // Aufruf der Ablage ohne Nachricht oder Ablage generell deaktiviert?
                if (m_PersistentID.Equals(100) && (GroupwiseUtil.GetCurrentMessages().Count == 0 || !CommonUtil.GetConfig().DmsFilingEnabled))
                {
                    res = eGW_CMDVAL_DISABLED;
                }

                // Standortsuche ohne bestehende Nachricht unterbinden
                if (m_PersistentID.Equals(101) && GroupwiseUtil.GetCurrentMessages().Count == 0)
                {
                    res = eGW_CMDVAL_DISABLED;
                }
            }
            catch (Exception e)
            {
                CommonUtil.GlobalErrorHandler(e);
            }

            return res;
        }
    }
}