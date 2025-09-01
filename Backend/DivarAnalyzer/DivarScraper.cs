using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DivarAnalyzer
{
    public class DivarScraper
    {
        private readonly Random _random = new();

        private static readonly Dictionary<string, (double lat, double lng)> LocationMap = new()
        {
            ["1"] = (35.7797, 51.4183),
            ["2"] = (35.7797, 51.4026),
            ["3"] = (35.7869, 51.4399),
            ["6"] = (35.7219, 51.3892),
            ["15"] = (35.6669, 51.3753)
        };

        public async Task<List<RealEstateData>> ScrapeDataAsync()
        {
            await Task.Delay(1000);
            return GenerateSampleData();
        }

        public List<RealEstateData> GenerateSampleData(int count = 100)
        {
            var districts = new[] { "1", "2", "3", "6", "15" };
            var data = new List<RealEstateData>();

            for (int i = 0; i < count; i++)
            {
                var district = districts[_random.Next(districts.Length)];
                var size = _random.Next(50, 200);
                var totalPrice = _random.NextInt64(5_000_000_000L, 50_000_000_000L);
                long? pricePerSqm = size > 0 ? (long?)(totalPrice / size) : null;

                var location = GetApproximateLocation(district);

                data.Add(new RealEstateData
                {
                    ScrapedDate = DateTime.Now.AddDays(-_random.Next(0, 14)),
                    District = district,
                    Size = size,
                    TotalPrice = totalPrice,
                    PricePerSqm = pricePerSqm,
                    Latitude = location.lat,
                    Longitude = location.lng,
                    Title = $"Apartment {size}sqm in District {district}",
                    Token = $"sample_{i}",
                    Age = _random.Next(1, 30)
                });
            }

            return data;
        }

        private (double lat, double lng) GetApproximateLocation(string district)
        {
            var baseLocation = LocationMap.GetValueOrDefault(district, (35.7219, 51.3347));
            var lat = baseLocation.Item1 + (_random.NextDouble() - 0.5) * 0.04;
            var lng = baseLocation.Item2 + (_random.NextDouble() - 0.5) * 0.04;

            return (lat, lng);
        }
    }
}