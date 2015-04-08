using ProductMonitor.Framework.ProgramCode.Triggers;

namespace ProductMonitor.Framework.ProgramCode.Actions
{
    public abstract class Action
    {
        protected object[] input;
        protected Trigger trigger;

        public abstract void Execute();

        public void setTrigger(Trigger trigger)
        {
            this.trigger = trigger;
        }
    }
}
