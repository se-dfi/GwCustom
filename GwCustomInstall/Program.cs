using System.Diagnostics;

class Program {
    const string APPNAME = "Groupwi(se) Integration";
    public static readonly string APPINSTALLPATH = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "seele GmbH", APPNAME);

    static void Main(string[] args) {
        if (args.Length == 1) {
            if (args[0].ToLower().Equals("/i")) {
                // Eventlog Source erstellen
                if (EventLog.SourceExists(APPNAME)) {
                    Console.WriteLine($"Eventlog Source already exists: \"{APPNAME}\"");
                } else {
                    EventLog.CreateEventSource(APPNAME, $"{APPNAME} Log");
                    Console.WriteLine($"Eventlog Source and Log created: \"{APPNAME}\", \"{APPNAME} Log\"");
                }

                // Bereits registrierte Comhost-DLL erneut registrieren (nötig für Updateinstallation)
                string dllFile = Path.Combine(APPINSTALLPATH, "GwCustom.comhost.dll");
                if (File.Exists(dllFile)) {
                    // Deregistrieren
                    Process process = new();
                    process.StartInfo.FileName = "regsvr32.exe";
                    process.StartInfo.Arguments = "/u /s \"" + dllFile + "\"";
                    process.StartInfo.UseShellExecute = false;
                    process.StartInfo.CreateNoWindow = true;
                    process.StartInfo.RedirectStandardOutput = true;
                    process.Start();
                    process.WaitForExit();
                    process.Close();

                    // Registrieren
                    process.StartInfo.Arguments = "/s \"" + dllFile + "\"";
                    process.Start();
                    process.WaitForExit();
                    process.Close();
                }
            } else if (args[0].ToLower().Equals("/u")) {
                // Eventlog Source entfernen
                if (EventLog.SourceExists(APPNAME)) {
                    EventLog.DeleteEventSource(APPNAME);
                    Console.WriteLine($"Eventlog Source removed: \"{APPNAME}\"");
                } else {
                    Console.WriteLine($"Eventlog Source not present: \"{APPNAME}\"");
                }

                // Eventlog entfernen
                if (EventLog.Exists($"{APPNAME} Log")) {
                    EventLog.Delete($"{APPNAME} Log");
                    Console.WriteLine($"Eventlog removed: \"{APPNAME} Log\"");
                } else {
                    Console.WriteLine($"Eventlog Log not present: \"{APPNAME} Log\"");
                }
            } else {
                Console.WriteLine("Unbekannter Parameter.");
            }
        } else {
            Console.WriteLine("Falsche Parameteranzahl.");
        }
    }
}