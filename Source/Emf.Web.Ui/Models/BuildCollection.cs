using System;
using System.Collections.Generic;

namespace Emf.Web.Ui.Models
{
    public class BuildCollection
    {
        public DateTime? LatestBuildFinishTime { get; set; }
        public Dictionary<int, Build> BuildsByDefinitionId { get; } = new Dictionary<int, Build>();
    }
}