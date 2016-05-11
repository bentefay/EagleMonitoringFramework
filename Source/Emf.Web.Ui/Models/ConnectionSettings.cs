using System;
using System.Collections.Generic;
using EqualityComparers;

namespace Emf.Web.Ui.Models
{
    public class ConnectionSettings
    {
        public static readonly IEqualityComparer<ConnectionSettings> ChangedComparer = EqualityCompare<ConnectionSettings>.EquateBy(b => b.TfsCollectionUrl).ThenEquateBy(b => b.TfsCollectionUrl);

        public ConnectionSettings(string tfsCollectionUrl, string tfsProject)
        {
            TfsCollectionUrl = tfsCollectionUrl;
            TfsProject = tfsProject;
        }

        public string TfsCollectionUrl { get;}
        public string TfsProject { get; }
    }
}