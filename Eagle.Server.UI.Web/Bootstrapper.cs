using Nancy;
using Nancy.Bootstrapper;
using Nancy.TinyIoc;

namespace Eagle.Server.UI.Web
{
	public class Bootstrapper : DefaultNancyBootstrapper
	{
		protected override void ApplicationStartup(TinyIoCContainer container, IPipelines pipelines)
		{
			base.ApplicationStartup(container, pipelines);
            Conventions.ViewLocationConventions.Add((viewName, model, context) => "../../Views/" + viewName);
		}
	}
}