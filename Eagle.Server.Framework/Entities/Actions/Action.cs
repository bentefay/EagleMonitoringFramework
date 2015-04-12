using Eagle.Server.Framework.Entities.Triggers;

namespace Eagle.Server.Framework.Entities.Actions
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
