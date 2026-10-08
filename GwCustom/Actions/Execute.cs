using System;

namespace GwCustom.Actions
{
    /// <summary>
    /// Abstrakte Basisklasse für alle Ausführungen
    /// </summary>
    public abstract class Execute
    {
        public event EventHandler<DmsProgressArgs>? DmsProgress;

        /// <summary>
        /// Ausführung vor der Groupwise-Aktion mit optional Abbruch derselben
        /// </summary>
        /// <returns>Abbruch des Vorgangs</returns>
        public abstract bool RunBefore();


        /// <summary>
        /// Allgemeine Befehlsausführung bzw. nach der Groupwise-Aktion
        /// </summary>
        public abstract void Run();


        /// <summary>
        /// Event für Fortschritt zum nächsten Dokument
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnDmsProgress(DmsProgressArgs e)
        {
            DmsProgress?.Invoke(this, e);
        }
    }


    /// <summary>
    /// EventArgs für Fortschrittsevent
    /// </summary>
    public class DmsProgressArgs : EventArgs
    {
        public int Current { get; set; }
        public int Maximum { get; set; }
        public string? Information { get; set; }
        public bool Cancel { get; set; }
    }
}