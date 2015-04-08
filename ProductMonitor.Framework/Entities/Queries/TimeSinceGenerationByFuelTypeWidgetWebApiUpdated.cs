using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web;
using System.Xml;
using ProductMonitor.Framework.Generic;
using Serilog;

namespace ProductMonitor.Framework.Entities.Queries
{
    public class TimeSinceGenerationByFuelTypeWidgetWebApiUpdated : Query
    {
        private static readonly string[] _regions = { "QLD", "NSW", "VIC", "SA", "TAS" };

        private Exception _lastException;
        private readonly Uri _baseUri;
        DateTimeOffset? _mostRecentCheck;
        private DateTimeOffset? _newestAemoInterval;
        private readonly Dictionary<string, DateTimeOffset?> _newestApviSolarIntervalByRegion = _regions.ToDictionary(r => r, r => (DateTimeOffset?)null);
        private readonly bool _checkApviSolar;
        private readonly bool _checkAemo;
        private readonly bool _useOldestSolar;
        private readonly string _apiToken;
        private readonly string _sessionId;

        public TimeSinceGenerationByFuelTypeWidgetWebApiUpdated(XmlNode input)
        {
            _baseUri = new Uri(input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "LOCATION").Select(c => c.FirstChild.Value).First());
            _apiToken = input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "APITOKEN").Select(c => c.FirstChild.Value).First();
            _sessionId = input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "SESSIONID").Select(c => c.FirstChild.Value).First();
            
            var checkApviSolar = input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "CHECKAPVISOLARDATA").Select(c => c.FirstChild.Value).FirstOrDefault();
            var useOldestSolar = input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "USEOLDESTSOLAR").Select(c => c.FirstChild.Value).FirstOrDefault();
            var checkAemo = input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "CHECKAEMODATA").Select(c => c.FirstChild.Value).FirstOrDefault();

            _checkApviSolar = bool.Parse(checkApviSolar ?? "True");
            _useOldestSolar = bool.Parse(useOldestSolar ?? "True");
            _checkAemo = bool.Parse(checkAemo ?? "True");
        }

        public override string GetLocation()
        {
            return _baseUri.Host;
        }

        public override string GetLongLocation()
        {
            return _baseUri.ToString();
        }

        public override string GetDescription()
        {
            return GetLocation();
        }

        public override object Test()
        {
            try
            {
                var mostRecentCheck = DateTimeOffset.Now.ToNemOffset();
                _mostRecentCheck = mostRecentCheck;

                var result = GetGenerationByFuelTypeData(_baseUri, _apiToken, _sessionId, 1);

                _newestAemoInterval = new DateTimeOffset(result.Settlementdate, NemInterval.NemTimeOffset);
                UpdateMostRecentApviIntervals(result, _newestApviSolarIntervalByRegion);

                var intervals = new List<DateTimeOffset?>();

                if (_checkAemo)
                {
                    intervals.Add(_newestAemoInterval);
                    if (_newestAemoInterval == null)
                        throw new Exception("The aemo data is missing");
                }

                if (_checkApviSolar)
                {
                    intervals.Add(_useOldestSolar ? _newestApviSolarIntervalByRegion.Select(r => r.Value).Min() : _newestApviSolarIntervalByRegion.Select(r => r.Value).Max());
                    if (_useOldestSolar && _newestApviSolarIntervalByRegion.Any(r => r.Value == null))
                        throw new Exception("One or more of the regions is missing solar data");
                    if (!_useOldestSolar && _newestApviSolarIntervalByRegion.All(r => r.Value == null))
                        throw new Exception("All of the regions are missing solar data");
                }

                if (intervals.Count == 0)
                    throw new Exception("No records were returned from the service");

                _lastException = null;
                return DateTimeOffset.Now.ToNemOffset() - intervals.Min();
            }
            catch (Exception e)
            {
                _lastException = e;
                Log.Error(e, "Failed to check Generation By Fuel Type Widget Web Api");
                return TimeSpan.MaxValue;
            }
        }

        private static void UpdateMostRecentApviIntervals(Result result, Dictionary<string, DateTimeOffset?> currentApviSolarIntervalByRegion)
        {
            // All of the regions for the APVI solar fuel type could potentially be out of date by different amounts, so check them all separately.
            var newestApviSolarIntervalByRegion = result.Data
                .ToDictionary(g => g.State, g => g.SmallSolarInterval.HasValue ? g.SmallSolarInterval.Value.ToNemOffset() : (DateTimeOffset?)null);

            foreach (var region in _regions)
            {
                DateTimeOffset? mostRecentInterval;
                if (newestApviSolarIntervalByRegion.TryGetValue(region, out mostRecentInterval) && mostRecentInterval != null)
                    currentApviSolarIntervalByRegion[region] = mostRecentInterval;
            }
        }

        private Result GetGenerationByFuelTypeData(Uri baseUri, string apiToken, string sessionId, int requestCount)
        {
            using (var client = new HttpClient())
            {
                var uri = new UriBuilder(baseUri);

                var queryParameters = HttpUtility.ParseQueryString(string.Empty);
                queryParameters["requestCount"] = requestCount.ToString();
                uri.Query = queryParameters.ToString();

                client.BaseAddress = uri.Uri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("ApiToken", apiToken);
                client.DefaultRequestHeaders.Add("SessionId", sessionId);

                var response = client.GetAsync("", 0).Result;
                if (!response.IsSuccessStatusCode)
                    throw new Exception(String.Format("Failed to contact web api {0} with error code {1} ({2})", uri, response.StatusCode, response.Content));
                
                var result = response.Content.ReadAsAsync<Result>().Result;
                return result;
            }
        }

        public override Type GetReturnType()
        {
            return typeof(TimeSpan);
        }

        public override Dictionary<string, string> GetAdditionalValues()
        {
            var values = new Dictionary<string, string>
            {
                { "Base Url", _baseUri.ToString() },
                { "Most Recent Check", Format(_mostRecentCheck) },
                { "Most Recent Aemo Interval", Format(_newestAemoInterval) },
            };

            if (_lastException != null)
                values.Add("Last Exception Message", _lastException.Message);

            foreach (var regionNewestInterval in _newestApviSolarIntervalByRegion)
                values.Add("Most Recent APVI Solar Interval for " + regionNewestInterval.Key, Format(regionNewestInterval.Value));

            return values;
        }

        private static string Format(DateTimeOffset? value)
        {
            return String.Format("{0}", value != null ? value.ToString() : "Unknown");
        }

        public class Result
        {
            public string Version;
            public int SuggestedRequerySeconds;
            public DateTime Settlementdate;
            public string DisplaySettlementDate;
            public RegionFuelTypeData[] Data;
            public Configuration[] Configuration;
        }

        public class RegionFuelTypeData
        {
            public string State;
            public decimal BlackCoal;
            public decimal BrownCoal;
            public decimal Hydro;
            public decimal LiquidFuel;
            public decimal Gas;
            public decimal Other;
            public decimal Wind;
            public decimal LargeSolar;
            public int? SmallSolar;
            public string DisplaySmallSolarInterval;
            public DateTime? SmallSolarInterval;
        }

        public class Configuration
        {
            public string Field;
            public string Name;
            public string Color;
        }
    }
}