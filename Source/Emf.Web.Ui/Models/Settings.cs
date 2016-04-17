using System;
using System.Collections.Generic;
using EqualityComparers;

namespace Emf.Web.Ui.Models
{
    public class Settings
    {
        public static readonly IEqualityComparer<Settings> ChangedComparer = EqualityCompare<Settings>.EquateBy(b => b.TfsCollectionUrl).ThenEquateBy(b => b.TfsCollectionUrl);

        public Settings(string tfsCollectionUrl, string tfsProject)
        {
            TfsCollectionUrl = tfsCollectionUrl;
            TfsProject = tfsProject;
        }

        public string TfsCollectionUrl { get;}
        public string TfsProject { get; }
    }
}