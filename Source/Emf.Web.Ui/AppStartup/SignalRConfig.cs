using System;
using System.Reflection;
using Emf.Web.Ui.Hubs.Core;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using Microsoft.AspNet.SignalR.Infrastructure;
using Microsoft.Owin.Cors;
using Microsoft.Practices.Unity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Owin;

namespace Emf.Web.Ui.AppStartup
{
    public static class SignalRConfig
    {
        public static void Initialize(IAppBuilder app)
        {
            GlobalHost.DependencyResolver.Register(typeof(IHubActivator), () => new UnityHubActivator(UnityConfig.GetConfiguredContainer()));

            var serializer = new JsonSerializer { ContractResolver = new SignalRCamelCaseResolver() };
            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);

            GlobalHost.HubPipeline.AddModule(new SerilogErrorPipelineModule());
#if DEBUG
            var hubConfiguration = new HubConfiguration { EnableDetailedErrors = true, EnableJSONP = false };
            GlobalHost.HubPipeline.AddModule(new ClientExceptionHandlerPipelineModule());
#else
            var hubConfiguration = new HubConfiguration { EnableDetailedErrors = false, EnableJSONP = false };
#endif
            app.UseCors(CorsOptions.AllowAll);
            app.MapSignalR(hubConfiguration);
        }
    }

    /// <summary>
    /// An activator that uses unity to resolve our hubs and their depedencies. 
    /// We do NOT use an IDependencyResolver as this would require unity to resolve all of the SignalR infrastucture.
    /// </summary>
    public class UnityHubActivator : IHubActivator
    {
        private readonly IUnityContainer _container;

        public UnityHubActivator(IUnityContainer container)
        {
            _container = container;
        }

        public IHub Create(HubDescriptor descriptor)
        {
            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            if (descriptor.HubType == null)
                return null;

            var hub = _container.Resolve(descriptor.HubType);
            return hub as IHub;
        }
    }

    /// <summary>
    /// A resolver that uses PascalCase for SignalR types, and camelCase for all other types
    /// </summary>
    public class SignalRCamelCaseResolver : IContractResolver
    {

        private readonly Assembly _signalRAssembly;
        private readonly IContractResolver _camelCaseContractResolver;
        private readonly IContractResolver _defaultContractSerializer;

        public SignalRCamelCaseResolver()
        {
            _defaultContractSerializer = new DefaultContractResolver();
            _camelCaseContractResolver = new CamelCasePropertyNamesContractResolver();
            _signalRAssembly = typeof(Connection).Assembly;
        }

        public JsonContract ResolveContract(Type type)
        {
            return type.Assembly.Equals(_signalRAssembly) ? _defaultContractSerializer.ResolveContract(type) : _camelCaseContractResolver.ResolveContract(type);
        }
    }

}