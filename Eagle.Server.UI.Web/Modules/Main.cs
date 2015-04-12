using Nancy;

namespace Eagle.Server.UI.Web.Modules
{
	public class Main : NancyModule
	{
		public Main()
		{
			Get["/"] = _ => View["dashboard.html"];
		}
	}
}