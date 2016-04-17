using System;
using System.Collections.Generic;
using System.Linq;
using EqualityComparers;
using Microsoft.TeamFoundation.Build.WebApi;
using Newtonsoft.Json;

namespace Emf.Web.Ui.Models
{
    public class Build
    {
        public static readonly IEqualityComparer<Build> ChangedComparer = EqualityCompare<Build>.EquateBy(b => b.Id).ThenEquateBy(b => b.FinishTime).ThenEquateBy(b => b.StartTime);

        [JsonConstructor]
        public Build(BuildDefinitionReference definition, int id, BuildStatus? status, DateTime? queueTime, DateTime? startTime, DateTime? finishTime, BuildResult? result, IReadOnlyList<TestRun> testRuns)
        {
            Definition = definition;
            Id = id;
            Status = status;
            QueueTime = queueTime;
            StartTime = startTime;
            FinishTime = finishTime;
            Result = result;
            TestRuns = testRuns ?? new List<TestRun>();
        }

        public Build(BuildDefinitionReference definition, Microsoft.TeamFoundation.Build.WebApi.Build build, IList<Microsoft.TeamFoundation.TestManagement.WebApi.TestRun> testRuns = null)
        {
            Definition = definition;

            Id = build.Id;
            Status = build.Status;
            QueueTime = build.QueueTime;
            StartTime = build.StartTime;
            FinishTime = build.FinishTime;
            Result = build.Result;
            TestRuns = testRuns?.Select(t => new TestRun(t)).ToList() ?? new List<TestRun>();
        }

        public BuildDefinitionReference Definition { get; }

        public int Id { get; }

        public BuildStatus? Status { get; }

        public DateTime? QueueTime { get; }
        public DateTime? StartTime { get; }
        public DateTime? FinishTime { get; }
        
        public BuildResult? Result { get; }

        public IReadOnlyList<TestRun> TestRuns { get; }
    }
}