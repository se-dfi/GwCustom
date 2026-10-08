using GwCustom.Custom;
using System;
using System.Windows.Forms;

namespace GwCustom.Actions
{
    public class CmdOpen : Execute
    {
        public override bool RunBefore()
        {
            return false;
        }

        public override void Run()
        {
        }
    }
}