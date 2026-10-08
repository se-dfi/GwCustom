using GwCustom.Forms;
using GwCustom.Models;
using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using static GwCustomUI.Classes.Constants;

namespace GwCustom.Classes
{
    public static class SignatureStatic
    {
        /// <summary>
        /// GUID der zuletzt versandten E-Mail
        /// </summary>
        public static string? SentGuid {  get; set; }


        /// <summary>
        /// Versandte E-Mail soll abgelegt werden
        /// </summary>
        public static bool Storage { get; set; }
    }
}