using GwCustom.Forms;
using System.Windows.Forms;

namespace GwCustom.Actions
{
    public class MenuInformation : Execute
    {
        public override bool RunBefore()
        {
            // Infoformular anzeigen (mehrere Instanzen verhindern)
            foreach (Form form in Application.OpenForms)
            {
                if (form is FInfo)
                {
                    form.Focus();
                    return false;
                }
            }
            FInfo info = new();
            info.Show();

            return false;
        }

        public override void Run()
        {
            RunBefore();
        }
    }
}