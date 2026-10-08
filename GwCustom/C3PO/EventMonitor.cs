using GwCustom.Actions;
using GwCustom.Custom;
using GwCustomUI.Classes;
using C3POTypeLibrary;
using System;
using System.Runtime.InteropServices;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.C3PO
{
    /// <summary>
    /// Diese Klasse bildet Groupwise Events ab
    /// </summary>
    [ComVisible(true)]
    [Guid("2B8CA321-A8FE-43EF-A525-B6E62722A2B1")]
    public class EventMonitor
    {
        /// <summary>
        /// Eventverarbeitung
        /// </summary>
        /// <param name="sGWContext">Context</param>
        /// <param name="objGWEvent">Event</param>
        public void Notify(ref string sGWContext, ref object objGWEvent)
        {
            try
            {
                IGWEvent gwEvent = (IGWEvent)objGWEvent;
                switch (gwEvent.PersistentID)
                {
                    case eGW_CMDEVTID_READY:
                        new EvtReady().Run();
                        break;

                    case eGW_CMDEVTID_DELIVERY:
                        new EvtDelivery().Run();
                        break;

                    case eGW_CMDEVTID_SHUTDOWN:
                        new EvtShutdown().Run();
                        break;
                }
            }
            catch (Exception e)
            {
                CommonUtil.GlobalErrorHandler(e);
            }
        }
    }
}