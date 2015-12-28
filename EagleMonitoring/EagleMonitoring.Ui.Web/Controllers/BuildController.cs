using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eagle.Server.Framework.Tests;
using Microsoft.AspNet.Mvc;

namespace EagleMonitoring.Ui.Web.Controllers
{
    public class CredentialsDto
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }

    public class BuildController : Controller
    {
        [HttpPost]
        public Task<IReadOnlyList<BuildDefinitionReferenceDto>> BuildDefinitionReferences([FromBody] CredentialsDto credentials)
        {
            var repository = new TfsBuildDefinitionRepository(credentials);
            var tokenSource = new CancellationTokenSource();

            return repository.GetTestRuns(tokenSource.Token);
        }
    }
}