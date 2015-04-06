using ProductMonitor.ProgramCode.Triggers;

namespace ProductMonitor.ProgramCode.Actions
{
    abstract class Action
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
