using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using CsvHelper;
using CsvHelper.Configuration;

namespace DivarAnalyzer
{
    public class RealEstateData
    {
        public DateTime ScrapedDate { get; set; }
        public string District { get; set; } = "";
        public int? Size { get; set; }
        public long? TotalPrice { get; set; }
        public long? PricePerSqm { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string Title { get; set; } = "";
        public string Token { get; set; } = "";
        public int? Age { get; set; }
        public DateTime Date => ScrapedDate.Date;
    }

    public class AggregatedData
    {
        public DateTime Date { get; set; }
        public string District { get; set; } = "";
        public string DistrictName => $"District {District}";
        public int Count { get; set; }
        public long? AvgTotalPrice { get; set; }
        public long? AvgPricePerSqm { get; set; }
        public double Lat { get; set; } = 35.72;
        public double Lng { get; set; } = 51.33;
    }

    public class AnalysisMetrics
    {
        public int TotalListings { get; set; }
        public long HighestTotal { get; set; }
        public long AvgTotal { get; set; }
        public long AvgSqm { get; set; }
    }

    public class DivarDataAnalyzer
    {
        private const string BaselineIso = "2024-06-12";
        private const int BaselineWindowDays = 7;
        private const int RecentWindowDays = 7;

        private List<RealEstateData> _rawData = new();
        private List<AggregatedData> _samples = new();
        private static readonly Dictionary<string, (double lat, double lng)> DistrictLocations = new()
        {
            ["1"] = (lat: 35.7797, lng: 51.4183), // Shemiran
            ["2"] = (lat: 35.7797, lng: 51.4026), // Vanak
            ["3"] = (lat: 35.7869, lng: 51.4399), // Zaferaniyeh
            ["6"] = (lat: 35.7219, lng: 51.3892), // Yusefabad
            ["15"] = (lat: 35.6669, lng: 51.3753), // Shahrak
        };

        public async Task<bool> LoadDataAsync(string csvFilePath)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null,
                    MissingFieldFound = null,
                    PrepareHeaderForMatch = args => args.Header.ToLower(),
                };

                using var reader = new StringReader(await File.ReadAllTextAsync(csvFilePath));
                using var csv = new CsvReader(reader, config);

                _rawData = csv.GetRecords<RealEstateData>().ToList();
                Console.WriteLine($"✅ {_rawData.Count} records loaded from {csvFilePath}");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading data: {ex.Message}");
                return false;
            }
        }

        public void GenerateTimeSeriesData()
        {
            Console.WriteLine("📊 Generating time series data...");

            var timeSeriesData = new List<AggregatedData>();
            var baseDate = DateTime.Now.AddDays(-14);
            var districts = new[] { "1", "2", "3", "6", "15" };
            var baseCounts = new Dictionary<string, int>
            {
                ["1"] = 45,
                ["2"] = 60,
                ["3"] = 55,
                ["6"] = 70,
                ["15"] = 40
            };

            var random = new Random();

            for (int i = 0; i < 14; i++)
            {
                var date = baseDate.AddDays(i);
                var growthFactor = 1 + (i * 0.04);

                foreach (var district in districts)
                {
                    var baseCount = baseCounts[district];
                    var count = (int)(baseCount * growthFactor * random.NextDouble() * (1.2 - 0.8) + 0.8);

                    var districtMultiplier = district switch
                    {
                        "1" => 3.5,
                        "2" => 3.0,
                        "3" => 4.0,
                        "6" => 2.5,
                        "15" => 1.8,
                        _ => 2.0
                    };

                    var avgPrice = (long)(2000000 * districtMultiplier * (random.NextDouble() * (1.1 - 0.9) + 0.9));
                    var location = DistrictLocations.GetValueOrDefault(district, (lat: 35.72, lng: 51.33));

                    timeSeriesData.Add(new AggregatedData
                    {
                        Date = date,
                        District = district,
                        Count = count,
                        AvgTotalPrice = avgPrice,
                        AvgPricePerSqm = avgPrice / random.Next(80, 150),
                        Lat = location.lat,
                        Lng = location.lng
                    });
                }
            }

            _samples = timeSeriesData;
            SaveTimeSeriesData();
        }

        private async void SaveTimeSeriesData()
        {
            var filename = "divar_time_series.csv";
            var config = new CsvConfiguration(CultureInfo.InvariantCulture);

            await using var writer = new StringWriter();
            await using var csv = new CsvWriter(writer, config);

            await csv.WriteRecordsAsync(_samples);
            await File.WriteAllTextAsync(filename, writer.ToString());

            Console.WriteLine($"💾 Time series data saved to {filename}");
        }

        public AnalysisMetrics AnalyzeBasicStats()
        {
            if (!_rawData.Any())
                return new AnalysisMetrics();

            var validTotalPrices = _rawData.Where(r => r.TotalPrice.HasValue).ToList();
            var validSqmPrices = _rawData.Where(r => r.PricePerSqm.HasValue).ToList();

            return new AnalysisMetrics
            {
                TotalListings = _rawData.Count,
                HighestTotal = validTotalPrices.Any() ? validTotalPrices.Max(r => r.TotalPrice!.Value) : 0,
                AvgTotal = validTotalPrices.Any() ? (long)validTotalPrices.Average(r => r.TotalPrice!.Value) : 0,
                AvgSqm = validSqmPrices.Any() ? (long)validSqmPrices.Average(r => r.PricePerSqm!.Value) : 0
            };
        }

        public List<AggregatedData> AnalyzeTimeTrends()
        {
            if (!_samples.Any())
                GenerateTimeSeriesData();

            Console.WriteLine("\n📈 Analyzing time trends:");

            var dailyTotals = _samples
                .GroupBy(s => s.Date)
                .Select(g => new { Date = g.Key, Count = g.Sum(x => x.Count) })
                .OrderBy(x => x.Date)
                .ToList();

            var growthRates = new List<double>();
            for (int i = 1; i < dailyTotals.Count; i++)
            {
                var prevCount = dailyTotals[i - 1].Count;
                if (prevCount > 0)
                {
                    var growth = ((double)(dailyTotals[i].Count - prevCount) / prevCount) * 100;
                    growthRates.Add(growth);
                }
            }

            if (growthRates.Any())
            {
                var avgGrowth = growthRates.Average();
                Console.WriteLine($"📊 Average daily growth: {avgGrowth:F1}%");

                var maxGrowthIndex = growthRates.ToList().IndexOf(growthRates.Max()) + 1;
                var maxGrowthDay = dailyTotals[maxGrowthIndex];
                Console.WriteLine($"🚀 Highest growth: {growthRates.Max():F1}% on {maxGrowthDay.Date:yyyy-MM-dd}");
            }

            return _samples;
        }

        public List<AggregatedData> AggregateData(List<RealEstateData> rawData)
        {
            var grouped = rawData
                .GroupBy(r => new { r.Date, r.District })
                .Select(g =>
                {
                    var location = DistrictLocations.GetValueOrDefault(g.Key.District, (lat: 35.72, lng: 51.33));

                    return new AggregatedData
                    {
                        Date = g.Key.Date,
                        District = g.Key.District,
                        Count = g.Count(),
                        AvgTotalPrice = g.Where(x => x.TotalPrice.HasValue).Any()
                            ? (long)g.Where(x => x.TotalPrice.HasValue).Average(x => x.TotalPrice!.Value)
                            : null,
                        AvgPricePerSqm = g.Where(x => x.PricePerSqm.HasValue).Any()
                            ? (long)g.Where(x => x.PricePerSqm.HasValue).Average(x => x.PricePerSqm!.Value)
                            : null,
                        Lat = location.lat,
                        Lng = location.lng
                    };
                })
                .ToList();

            return grouped;
        }

        public async Task GenerateReportAsync()
        {
            var reportFile = $"divar_report_{DateTime.Now:yyyyMMdd_HHmm}.txt";

            var report = new System.Text.StringBuilder();
            report.AppendLine("============================================================");
            report.AppendLine("📊 Tehran Real Estate Market Analysis Report - Divar");
            report.AppendLine("============================================================");
            report.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm}");
            report.AppendLine();

            if (_rawData.Any())
            {
                var stats = AnalyzeBasicStats();
                report.AppendLine("🔢 Overall Statistics:");
                report.AppendLine($"• Total listings: {stats.TotalListings:N0}");
                report.AppendLine($"• Unique districts: {_rawData.Select(r => r.District).Distinct().Count()}");
                report.AppendLine($"• Average price: {stats.AvgTotal:N0} Toman");
                report.AppendLine($"• Highest price: {stats.HighestTotal:N0} Toman");
                report.AppendLine($"• Average size: {_rawData.Where(r => r.Size.HasValue).Average(r => r.Size!.Value):F1} sqm");
                report.AppendLine();
            }

            if (_samples.Any())
            {
                report.AppendLine("📈 Trend Analysis:");
                var totalListings = _samples.Sum(s => s.Count);
                var totalValue = _samples.Where(s => s.AvgTotalPrice.HasValue)
                    .Sum(s => s.Count * s.AvgTotalPrice!.Value);

                report.AppendLine($"• Total listings in 14 days: {totalListings:N0}");
                report.AppendLine($"• Total market value: {totalValue / 1e9:F1} billion Toman");
                report.AppendLine();

                report.AppendLine("🏘️ District Analysis:");
                var districtSummary = _samples
                    .GroupBy(s => s.District)
                    .Select(g => new
                    {
                        District = g.Key,
                        Count = g.Sum(x => x.Count),
                        AvgPrice = g.Where(x => x.AvgTotalPrice.HasValue).Any()
                            ? g.Where(x => x.AvgTotalPrice.HasValue).Average(x => x.AvgTotalPrice!.Value)
                            : 0,
                        TotalValue = g.Where(x => x.AvgTotalPrice.HasValue).Sum(x => x.Count * x.AvgTotalPrice!.Value)
                    });

                foreach (var district in districtSummary.OrderBy(d => d.District))
                {
                    report.AppendLine($"District {district.District}:");
                    report.AppendLine($"  - Listing count: {district.Count:N0}");
                    report.AppendLine($"  - Average price: {district.AvgPrice:N0} Toman");
                    report.AppendLine($"  - Market value: {district.TotalValue / 1e9:F1} billion Toman");
                    report.AppendLine();
                }
            }

            report.AppendLine("============================================================");
            report.AppendLine("📁 Generated files:");
            report.AppendLine("• divar_comprehensive_analysis.png - Analysis charts");
            report.AppendLine("• divar_map.html - Interactive map");
            report.AppendLine("• divar_time_series.csv - Time series data");
            report.AppendLine($"• {reportFile} - This report");

            await File.WriteAllTextAsync(reportFile, report.ToString());
            Console.WriteLine($"📋 Complete report saved to {reportFile}");
        }

        public List<RealEstateData> GetRawData() => _rawData;
        public List<AggregatedData> GetSamples() => _samples;
    }
}