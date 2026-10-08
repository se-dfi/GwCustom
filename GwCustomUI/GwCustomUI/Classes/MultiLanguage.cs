using System;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Threading;

namespace GwCustomUI.Classes
{
    /// <summary>
    /// Diese Klasse bildet die Mehrsprachigkeit ab
    /// </summary>
    public static class MultiLanguage
    {
        /// <summary>
        /// Ressourcenmanager für Sprachdateien
        /// </summary>
        private static ResourceManager? resManager;

        /// <summary>
        /// Spracheinstellung für den Ressourcenmanager festlegen
        /// </summary>
        /// <param name="language"></param>
        public static void SetLanguage(string language)
        {
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(language);
            resManager = new ResourceManager("GwCustomUI.Files.Language", Assembly.GetExecutingAssembly());
        }

        /// <summary>
        /// Sprachübersetzung für einen Text abholen
        /// </summary>
        /// <param name="pattern"></param>
        /// <returns></returns>
        public static string GetText(string pattern)
        {
            string? text = null;

            try
            {
                if (resManager != null && pattern != null)
                {
                    text = resManager.GetString(pattern);
                }
            }
            catch
            {
                text = String.Concat("{", pattern, "}");
            }

            return text ?? pattern ?? String.Empty;
        }
    }
}