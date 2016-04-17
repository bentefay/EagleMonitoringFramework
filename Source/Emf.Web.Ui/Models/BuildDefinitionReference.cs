using System;
using System.Collections.Generic;
using Comparers.Util;
using EqualityComparers;
using Microsoft.TeamFoundation.Build.WebApi;

namespace Emf.Web.Ui.Models
{
    public class BuildDefinitionReference
    {
        public static readonly IEqualityComparer<BuildDefinitionReference> ChangedComparer = EqualityCompare<BuildDefinitionReference>.EquateBy(b => b.Id).ThenEquateBy(b => b.Revision, allowNulls: true);

        public BuildDefinitionReference(int id, string name, int? revision, DefinitionType type)
        {
            Id = id;
            Name = name;
            Revision = revision;
            Type = type;
        }

        public int Id { get; }
        public int? Revision { get; }
        public string Name { get; }
        public DefinitionType Type { get; }
    }
}