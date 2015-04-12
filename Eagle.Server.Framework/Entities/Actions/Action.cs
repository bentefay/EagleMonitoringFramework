using Eagle.Server.Framework.Entities.Triggers;

namespace Eagle.Server.Framework.Entities.Actions
{
    public abstract class Action
    {
        protected object[] Input { get; set; }
        protected Trigger Trigger { get; private set; }

        public abstract void Execute();

        public void SetTrigger(Trigger trigger)
        {
            Trigger = trigger;
        }
    }
}
