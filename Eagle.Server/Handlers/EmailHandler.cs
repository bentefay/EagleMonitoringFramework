using Eagle.Server.Handlers;

namespace Eagle.Server.Notifications
{
    public class EmailHandler : Handler
    {
        public string EmailAddress { get; set; }

        public override void Execute()
        {
            throw new System.NotImplementedException();
        }
    }
}