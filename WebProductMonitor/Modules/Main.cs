using Nancy;

namespace WebProductMonitor.Modules
{
	public class Main : NancyModule
	{
		public Main()
		{
			Get["/"] = _ => View["main.html"];
		}
	}
}