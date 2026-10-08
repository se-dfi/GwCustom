namespace GwCustom.Actions
{
    public class EvtShutdown : Execute
    {
        public override bool RunBefore()
        {
            return false;
        }

        public override void Run()
        {
            RunBefore();
        }
    }
}