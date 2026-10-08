namespace GwCustom.Actions
{
    public class EvtDelivery : Execute
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