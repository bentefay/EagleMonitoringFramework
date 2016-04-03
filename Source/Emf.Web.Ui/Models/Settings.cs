using System;
using System.Collections.Generic;
using EqualityComparers;

namespace Emf.Web.Ui.Models
{
    public class Settings : IEquatable<Settings>
    {
        public static readonly IEqualityComparer<Settings> DefaultComparer = EqualityCompare<Settings>.EquateBy(b => b.TfsCollectionUrl).ThenEquateBy(b => b.TfsCollectionUrl);

        public Settings(string tfsCollectionUrl, string tfsProject)
        {
            TfsCollectionUrl = tfsCollectionUrl;
            TfsProject = tfsProject;
        }

        public string TfsCollectionUrl { get;}
        public string TfsProject { get; }

        public bool Equals(Settings other) => DefaultComparer.Equals(this, other);
        public override bool Equals(object other) => Equals(other as BuildDefinitionReference);
        public override int GetHashCode() => DefaultComparer.GetHashCode(this);
    }
}