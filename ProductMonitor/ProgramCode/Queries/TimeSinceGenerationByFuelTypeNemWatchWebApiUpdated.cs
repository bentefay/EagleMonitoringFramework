using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Xml;
using ProductMonitor.Generic;
using Product_Monitor.Generic;

namespace ProductMonitor.ProgramCode.Queries
{
    public class TimeSinceGenerationByFuelTypeNemWatchWebApiUpdated : Query
    {
        private static readonly string[] _regions = { "QLD", "NSW", "VIC", "SA", "TAS" };
        private const string _aemoDataSentryFuelType = "Black Coal";
        private const string _apviSolarFuelType = "Apvi Small Solar";

        private Exception _lastException;
        private readonly string _baseUrl;
        DateTimeOffset? _mostRecentCheck;
        private DateTimeOffset? _newestAemoInterval;
        private readonly Dictionary<string, DateTimeOffset?> _newestApviSolarIntervalByRegion = _regions.ToDictionary(r => r, r => (DateTimeOffset?)null);
        private readonly bool _checkApviSolar;
        private readonly bool _checkAemo;
        private readonly bool _useOldestSolar;
        private readonly int _resolutionToCheck;

        public TimeSinceGenerationByFuelTypeNemWatchWebApiUpdated(XmlNode input)
        {
            _baseUrl = input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "LOCATION").Select(c => c.FirstChild.Value).First();
            
            var checkApviSolar = input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "CHECKAPVISOLARDATA").Select(c => c.FirstChild.Value).FirstOrDefault();
            var useOldestSolar = input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "USEOLDESTSOLAR").Select(c => c.FirstChild.Value).FirstOrDefault();
            var checkAemo = input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "CHECKAEMODATA").Select(c => c.FirstChild.Value).FirstOrDefault();
            var resolutionToCheck = input.ChildNodes.Cast<XmlNode>().Where(childNode => childNode.Name.ToUpper() == "RESOLUTION").Select(c => c.FirstChild.Value).FirstOrDefault();

            _checkApviSolar = bool.Parse(checkApviSolar ?? "True");
            _useOldestSolar = bool.Parse(useOldestSolar ?? "True");
            _checkAemo = bool.Parse(checkAemo ?? "True");
            _resolutionToCheck = int.Parse(resolutionToCheck ?? "5");
        }

        public override string GetLocation()
        {
            var location = new Uri(_baseUrl);
            return location.Host;
        }

        public override string GetLongLocation()
        {
            return _baseUrl;
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

                var uri = new Uri(_baseUrl);

                var request = ConstructRequest(_newestAemoInterval, _newestApviSolarIntervalByRegion, _resolutionToCheck);
                var result = GetGenerationByFuelTypeData(uri, request);

                _newestAemoInterval = GetMostRecentAemoInterval(result, _newestAemoInterval);
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
                Logger.getInstance().Log(e);
                return TimeSpan.MaxValue;
            }
        }

        private static RequestGenerationByFuelTypeParams ConstructRequest(DateTimeOffset? newestAemoInterval, Dictionary<string, DateTimeOffset?> newestApviSolarIntervalByRegion, int resolution)
        {
            // The APVI web API expects its parameter region ids to be postfixed with 1 but does not return them postfixed with 1.
            var request5Min = new RequestGenerationByFuelTypeParams
            {
                NewestAemoInterval = newestAemoInterval,
                NewestSolarIntervals = newestApviSolarIntervalByRegion
                    .ToLookup(r => r.Value)
                    .Where(g => g.Key.HasValue)
                    .Select(g => new IntervalRegions { NewestInterval = g.Key.Value, RegionIds = g.Select(r => r + "1").ToArray() })
                    .ToArray(),
                Resolution = resolution
            };
            return request5Min;
        }

        private static RegionGenerationByFuelTypeResult GetGenerationByFuelTypeData(Uri uri, RequestGenerationByFuelTypeParams request)
        {
            using (var client = new HttpClient())
            {
                client.BaseAddress = uri;
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = client.PostAsJsonAsync("", request).Result;
                if (response.IsSuccessStatusCode)
                {
                    var result = response.Content.ReadAsAsync<RegionGenerationByFuelTypeResult>().Result;
                    return result;
                }
                else
                {
                    throw new Exception(String.Format("Failed to contact web api {0} with error code {1} ({2})", uri, response.StatusCode, response.Content));
                }
            }
        }

        private static DateTimeOffset? GetMostRecentAemoInterval(RegionGenerationByFuelTypeResult result5Min, DateTimeOffset? previousAemoInterval)
        {
            // All of the AEMO fuel types should have the same data available for all interval/region combinations, so just check one.
            var mostRecentAemoInterval = result5Min.Records
                .Where(r => String.Equals(r.FuelType, _aemoDataSentryFuelType, StringComparison.CurrentCultureIgnoreCase))
                .MaxOrDefault(r => (DateTimeOffset?) r.NemInterval, null)
                ?? previousAemoInterval;

            return mostRecentAemoInterval;
        }

        private static void UpdateMostRecentApviIntervals(RegionGenerationByFuelTypeResult result, Dictionary<string, DateTimeOffset?> currentApviSolarIntervalByRegion)
        {
            // All of the regions for the APVI solar fuel type could potentially be out of date by different amounts, so check them all separately.
            var newestApviSolarIntervalByRegion = result.Records
                .Where(r => String.Equals(r.FuelType, _apviSolarFuelType, StringComparison.CurrentCultureIgnoreCase))
                .GroupBy(r => r.RegionId)
                .ToDictionary(g => g.Key, g => g.Select(r => r.NemInterval).Max());

            foreach (var region in _regions)
            {
                DateTimeOffset mostRecentInterval;
                if (newestApviSolarIntervalByRegion.TryGetValue(region, out mostRecentInterval))
                {
                    currentApviSolarIntervalByRegion[region] = mostRecentInterval;
                }
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
                { "Base Url", _baseUrl },
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

        public class RequestGenerationByFuelTypeParams
        {
            public RequestGenerationByFuelTypeParams()
            {
                NewestSolarIntervals = new IntervalRegions[0];
            }

            public int Resolution { get; set; }

            public DateTimeOffset? NewestAemoInterval { get; set; }
            public IntervalRegions[] NewestSolarIntervals { get; set; }
        }

        public class IntervalRegions
        {
            public IntervalRegions()
            {
                RegionIds = new string[0];
            }

            public string[] RegionIds { get; set; }
            public DateTimeOffset NewestInterval { get; set; }
        }

        public class RegionGenerationByFuelTypeResult
        {
            public RegionGenerationByFuelTypeResult()
            {
                Records = new List<RegionGenerationByFuelTypeRecord>();
            }

            public List<RegionGenerationByFuelTypeRecord> Records { get; set; }
        }

        public class RegionGenerationByFuelTypeRecord
        {
            public DateTimeOffset NemInterval { get; set; }
            public string RegionId { get; set; }
            public string FuelType { get; set; }
            public decimal TotalGeneration { get; set; }
        }
    }
}
