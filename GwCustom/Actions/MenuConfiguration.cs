using GwCustom.Forms;
using System.Windows.Forms;

namespace GwCustom.Actions
{
    public class MenuConfiguration : Execute
    {
        public override bool RunBefore()
        {
            // Einstellungsformular anzeigen (mehrere Instanzen verhindern)
            foreach (Form form in Application.OpenForms)
            {
                if (form is FConfig)
                {
                    form.Focus();
                    return false;
                }
            }
            FConfig config = new();
            config.Show();

            return false;
        }

        public override void Run()
        {
            RunBefore();
        }
    }
}