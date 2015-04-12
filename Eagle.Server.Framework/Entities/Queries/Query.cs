using System;
using System.Collections.Generic;

namespace Eagle.Server.Framework.Entities.Queries
{
    public abstract class Query
    {
        protected object[] Input { get; set; }

        /// <summary>
        /// Run the query and get the result.
        /// </summary>
        /// <returns>An object of the same type as GetReturnType which is the 
        /// result of the query</returns>
        public abstract object Test();

        public abstract Type GetReturnType();

        public abstract string GetLocation();
        public abstract string GetDescription();
        public abstract string GetLongLocation();
        public abstract Dictionary<String, String> GetAdditionalValues();

    }
}
