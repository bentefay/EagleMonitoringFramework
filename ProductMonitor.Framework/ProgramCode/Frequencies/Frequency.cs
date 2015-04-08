namespace ProductMonitor.Framework.ProgramCode.Frequencies
{
    public abstract class  Frequency
    {
        protected Check check;
        protected bool paused;
        protected object[] input;
        protected System.Timers.Timer timer;

        abstract public void Pause(bool paused);
        abstract public bool isPaused();

        virtual public bool ActivationForceable()
        {
            return true;
        }

        virtual protected void activate()
        {
            System.Threading.Thread.CurrentThread.IsBackground = true;
            check.Activate();
            System.Threading.Thread.CurrentThread.Abort();
        }

    }
}
