namespace ProductMonitor.Framework.Entities.Frequencies
{
    public abstract class Frequency
    {
        protected Check Check;
        protected bool Paused;
        protected object[] Input;
        protected System.Timers.Timer Timer;

        abstract public void Pause(bool paused);
        abstract public bool IsPaused();

        virtual public bool ActivationForceable()
        {
            return true;
        }

        virtual protected void Activate()
        {
            System.Threading.Thread.CurrentThread.IsBackground = true;
            Check.Activate();
            System.Threading.Thread.CurrentThread.Abort();
        }

    }
}
