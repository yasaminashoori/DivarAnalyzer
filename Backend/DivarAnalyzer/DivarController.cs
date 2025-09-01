using Microsoft.AspNetCore.Mvc;
using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;

namespace DivarAnalyzer.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DivarController : ControllerBase
    {
        private readonly DivarDataAnalyzer _analyzer;
        private readonly DivarScraper _scraper;

        public DivarController()
        {
            _analyzer = new DivarDataAnalyzer();
            _scraper = new DivarScraper();
        }

        [HttpGet("sample-data")]
        public ActionResult<ApiResponse<List<RealEstateData>>> GenerateSampleData([FromQuery] int count = 100)
        {
            try
            {
                var data = _scraper.GenerateSampleData(count);

                Console.WriteLine($"Generated {data.Count} sample records");

                return Ok(new ApiResponse<List<RealEstateData>>
                {
                    Success = true,
                    Data = data,
                    Message = $"Generated {data.Count} sample records"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error generating sample data: {ex.Message}");
                return BadRequest(new ApiResponse<List<RealEstateData>>
                {
                    Success = false,
                    Message = $"Error generating sample data: {ex.Message}"
                });
            }
        }

        [HttpPost("analyze")]
        public ActionResult<ApiResponse<AnalysisResult>> AnalyzeData([FromBody] AnalyzeRequest request)
        {
            try
            {
                Console.WriteLine($"Analyzing {request.Data?.Count ?? 0} records");

                List<RealEstateData> rawData;

                if (request.Data?.Any() == true)
                {
                    rawData = request.Data;
                }
                else
                {
                    rawData = _scraper.GenerateSampleData(100);
                }
                var filteredData = ApplyFilters(rawData, request);
                Console.WriteLine($"Filtered to {filteredData.Count} records");
                var aggregatedData = _analyzer.AggregateData(filteredData);
                var metrics = CalculateMetrics(filteredData);
                var insights = GenerateInsights(filteredData);

                var result = new AnalysisResult
                {
                    RawData = filteredData,
                    AggregatedData = aggregatedData,
                    Metrics = metrics,
                    Insights = insights,
                    FilteredCount = filteredData.Count,
                    DateRange = new DateRange
                    {
                        From = filteredData.Any() ? filteredData.Min(d => d.ScrapedDate) : DateTime.Now,
                        To = filteredData.Any() ? filteredData.Max(d => d.ScrapedDate) : DateTime.Now
                    }
                };

                Console.WriteLine($"Analysis completed: {result.FilteredCount} records, {result.Insights.Count} insights");

                return Ok(new ApiResponse<AnalysisResult>
                {
                    Success = true,
                    Data = result,
                    Message = "Analysis completed successfully"
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during analysis: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");

                return BadRequest(new ApiResponse<AnalysisResult>
                {
                    Success = false,
                    Message = $"Error during analysis: {ex.Message}"
                });
            }
        }

        [HttpPost("export-csv")]
        public async Task<IActionResult> ExportToCsv([FromBody] List<RealEstateData> data)
        {
            try
            {
                var config = new CsvConfiguration(CultureInfo.InvariantCulture);
                using var writer = new StringWriter();
                using var csv = new CsvWriter(writer, config);

                await csv.WriteRecordsAsync(data);
                var csvContent = writer.ToString();

                var fileName = $"divar_export_{DateTime.Now:yyyyMMdd_HHmm}.csv";
                return File(System.Text.Encoding.UTF8.GetBytes(csvContent), "text/csv", fileName);
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = $"Error exporting CSV: {ex.Message}" });
            }
        }

        private List<RealEstateData> ApplyFilters(List<RealEstateData> data, AnalyzeRequest request)
        {
            var filtered = data.AsQueryable();

            if (request.FromDate.HasValue)
            {
                filtered = filtered.Where(d => d.ScrapedDate >= request.FromDate.Value);
            }

            if (request.ToDate.HasValue)
            {
                filtered = filtered.Where(d => d.ScrapedDate <= request.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(request.District) && request.District != "all")
            {
                filtered = filtered.Where(d => d.District == request.District);
            }

            return filtered.ToList();
        }

        private AnalysisMetrics CalculateMetrics(List<RealEstateData> data)
        {
            if (!data.Any())
                return new AnalysisMetrics();

            var validTotalPrices = data.Where(r => r.TotalPrice.HasValue).ToList();
            var validSqmPrices = data.Where(r => r.PricePerSqm.HasValue).ToList();

            return new AnalysisMetrics
            {
                TotalListings = data.Count,
                HighestTotal = validTotalPrices.Any() ? validTotalPrices.Max(r => r.TotalPrice!.Value) : 0,
                AvgTotal = validTotalPrices.Any() ? (long)validTotalPrices.Average(r => r.TotalPrice!.Value) : 0,
                AvgSqm = validSqmPrices.Any() ? (long)validSqmPrices.Average(r => r.PricePerSqm!.Value) : 0
            };
        }

        private List<string> GenerateInsights(List<RealEstateData> data)
        {
            var insights = new List<string>();

            if (!data.Any()) return insights;
            var validPrices = data.Where(d => d.TotalPrice.HasValue).ToList();
            if (validPrices.Any())
            {
                var avgPrice = validPrices.Average(d => d.TotalPrice!.Value);
                insights.Add($"Average property price: {avgPrice / 1e9:F2} billion Toman");
            }
            var districtCounts = data.GroupBy(d => d.District)
                .OrderByDescending(g => g.Count())
                .Take(3)
                .ToList();

            if (districtCounts.Any())
            {
                var topDistrict = districtCounts.First();
                insights.Add($"Most active district: District {topDistrict.Key} with {topDistrict.Count()} listings");
            }
            var validSizes = data.Where(d => d.Size.HasValue).ToList();
            if (validSizes.Any())
            {
                var avgSize = validSizes.Average(d => d.Size!.Value);
                insights.Add($"Average property size: {avgSize:F1} square meters");
            }
            var totalMarketValue = validPrices.Sum(d => d.TotalPrice!.Value);
            insights.Add($"Total market value: {totalMarketValue / 1e12:F1} trillion Toman");

            return insights;
        }
    }

    public class AnalyzeRequest
    {
        public List<RealEstateData>? Data { get; set; }
        public bool UseFile { get; set; }
        public string? FilePath { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? District { get; set; }
    }

    public class AnalysisResult
    {
        public List<RealEstateData> RawData { get; set; } = new();
        public List<AggregatedData> AggregatedData { get; set; } = new();
        public AnalysisMetrics Metrics { get; set; } = new();
        public List<AggregatedData> TimeTrends { get; set; } = new();
        public List<string> Insights { get; set; } = new();
        public int FilteredCount { get; set; }
        public DateRange DateRange { get; set; } = new();
    }

    public class DateRange
    {
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string Message { get; set; } = "";
    }
}