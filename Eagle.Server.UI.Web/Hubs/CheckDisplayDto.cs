using Eagle.Server.Framework;

namespace Eagle.Server.UI.Web.Hubs
{
    public class CheckDisplayDto
    {
        public CheckDisplayDto(ICheckDisplay check)
        {
            IsPaused = check.IsPaused();
            HasError = check.HasError();
            IsTriggered = check.IsTriggered();
            Result = check.GetResult();
            Status = check.GetStatus();
            Error = check.GetError();
            Location = check.GetLocation();
            CheckType = check.GetCheckType();
            TabName = check.GetTab();
        }

        public bool IsPaused { get; set; }
        public bool HasError { get; set; }
        public bool IsTriggered { get; set; }
        public string Result { get; set; }
        public string Status { get; set; }
        public string Error { get; set; }
        public string Location { get; set; }
        public string CheckType { get; set; }
        public string TabName { get; set; }
    }
}
