using Microsoft.Web.WebView2.Core;
using System;
using System.Threading.Tasks;
using static GwCustomUI.Classes.Constants;

namespace GwCustomUI.Classes
{
    /// <summary>
    /// Prozessweit geteilte WebView2-Environment.
    /// Die Environment überlebt die Signaturdialoge, damit der Browserprozess nicht zwischenzeitlich beendet wird
    /// und beim nächsten Versand eine disconnected COM-Referenz (RPC_E_DISCONNECTED) aus dem SDK-Cache geliefert wird.
    /// Stirbt der Browserprozess (z.B. Crash), wird die Environment verworfen und beim nächsten Aufruf neu erzeugt.
    /// </summary>
    public static class WebViewEnvironment
    {
        private static CoreWebView2Environment? _environment;

        /// <summary>
        /// Gemeinsame Environment ermitteln, bei Bedarf erzeugen (muss im UI-Thread aufgerufen werden)
        /// </summary>
        public static async Task<CoreWebView2Environment> GetAsync()
        {
            if (_environment == null)
            {
                CoreWebView2Environment env = await CoreWebView2Environment.CreateAsync(null, USERDATAPATH);
                env.BrowserProcessExited += (s, e) =>
                {
                    // Referenz ablegen, SDK entfernt den Environment-Cacheeintrag beim Beenden des Browserprozesses selbst
                    _environment = null;
                };
                _environment = env;
            }
            return _environment;
        }
    }
}
