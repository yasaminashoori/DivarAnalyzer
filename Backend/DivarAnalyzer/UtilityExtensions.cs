using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using CsvHelper;
using CsvHelper.Configuration;

namespace DivarAnalyzer.Extensions
{
    public static class UtilityExtensions
    {
        private static readonly Dictionary<char, char> PersianToEnglishNumbers = new()
        {
            ['۰'] = '0',
            ['۱'] = '1',
            ['۲'] = '2',
            ['۳'] = '3',
            ['۴'] = '4',
            ['۵'] = '5',
            ['۶'] = '6',
            ['۷'] = '7',
            ['۸'] = '8',
            ['۹'] = '9'
        };
        public static string ConvertPersianToEnglish(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            var result = input;
            foreach (var (persian, english) in PersianToEnglishNumbers)
            {
                result = result.Replace(persian, english);
            }
            return result;
        }
        public static long? ParsePrice(this string? priceText)
        {
            if (string.IsNullOrWhiteSpace(priceText))
                return null;
            var text = priceText.ConvertPersianToEnglish();
            var patterns = new[]
            {
                @"(\d{1,3}(?:,\d{3})*)\s*(?:تومان|ریال|toman|rial)",
                @"(\d{1,3}(?:,\d{3})*)\s*(?:میلیون|میلیارد|million|billion)",
                @"(\d+)\s*(?:میلیون|million)",
                @"(\d+)\s*(?:میلیارد|billion)",
                @"(\d{4,})"
            };

            foreach (var pattern in patterns)
            {
                var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    var priceStr = match.Groups[1].Value.Replace(",", "");
                    if (long.TryParse(priceStr, out var price))
                    {
                        if (text.Contains("میلیون", StringComparison.OrdinalIgnoreCase) || text.Contains("million", StringComparison.OrdinalIgnoreCase))
                        {
                            price *= 1_000_000;
                        }
                        else if (text.Contains("میلیارد", StringComparison.OrdinalIgnoreCase) ||
                                text.Contains("billion", StringComparison.OrdinalIgnoreCase))
                        {
                            price *= 1_000_000_000;
                        }
                        else if (price < 1000)
                        {
                            price *= 1_000_000;
                        }

                        return price;
                    }
                }
            }

            return null;
        }
        public static string FormatLargeNumber(this long number)
        {
            return number switch
            {
                >= 1_000_000_000_000 => $"{number / 1_000_000_000_000.0:F1}T",
                >= 1_000_000_000 => $"{number / 1_000_000_000.0:F1}B",
                >= 1_000_000 => $"{number / 1_000_000.0:F1}M",
                >= 1_000 => $"{number / 1_000.0:F1}K",
                _ => number.ToString("N0")
            };
        }
        public static string? GetNestedProperty(this JsonElement element, string path)
        {
            if (string.IsNullOrEmpty(path))
                return null;

            var parts = path.Split('.');
            var current = element;

            foreach (var part in parts)
            {
                if (current.ValueKind == JsonValueKind.Object &&
                    current.TryGetProperty(part, out var next))
                {
                    current = next;
                }
                else
                {
                    return null;
                }
            }

            return current.ValueKind == JsonValueKind.String ? current.GetString() : null;
        }
        public static double CalculatePercentageChange(this double newValue, double oldValue)
        {
            if (Math.Abs(oldValue) < double.Epsilon)
                return newValue > 0 ? 100 : 0;

            return ((newValue - oldValue) / Math.Abs(oldValue)) * 100;
        }
        public static IEnumerable<IGrouping<DateTime, T>> GroupByTimePeriod<T>(
            this IEnumerable<T> source,
            Func<T, DateTime> dateSelector,
            TimePeriod period)
        {
            return period switch
            {
                TimePeriod.Daily => source.GroupBy(x => dateSelector(x).Date),
                TimePeriod.Weekly => source.GroupBy(x => GetWeekStart(dateSelector(x))),
                TimePeriod.Monthly => source.GroupBy(x => new DateTime(dateSelector(x).Year, dateSelector(x).Month, 1)),
                _ => source.GroupBy(x => dateSelector(x).Date)
            };
        }
        public static DateTime GetWeekStart(DateTime date)
        {
            var dayOfWeek = (int)date.DayOfWeek;
            dayOfWeek = dayOfWeek == 0 ? 7 : dayOfWeek;
            return date.AddDays(1 - dayOfWeek).Date;
        }
        public static IEnumerable<double> MovingAverage(this IEnumerable<double> source, int windowSize)
        {
            var values = source.ToList();
            for (int i = windowSize - 1; i < values.Count; i++)
            {
                yield return values.Skip(i - windowSize + 1).Take(windowSize).Average();
            }
        }

        public static async Task<string> ToCsvAsync<T>(this IEnumerable<T> data)
        {
            using var writer = new StringWriter();
            using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

            await csv.WriteRecordsAsync(data);
            return writer.ToString();
        }
        public static async Task<List<T>> FromCsvAsync<T>(this string csvData)
        {
            using var reader = new StringReader(csvData);
            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            var records = new List<T>();
            await foreach (var record in csv.GetRecordsAsync<T>())
            {
                records.Add(record);
            }
            return records;
        }

        public static (double lat, double lng) AddNoise(this (double lat, double lng) coordinate, double maxNoise = 0.01)
        {
            var random = new Random();
            return (
                coordinate.lat + (random.NextDouble() - 0.5) * maxNoise * 2,
                coordinate.lng + (random.NextDouble() - 0.5) * maxNoise * 2
            );
        }
        public static bool IsValidTehranCoordinate(this (double lat, double lng) coordinate)
        {
            const double minLat = 35.5;
            const double maxLat = 35.9;
            const double minLng = 51.2;
            const double maxLng = 51.6;

            return coordinate.lat >= minLat && coordinate.lat <= maxLat &&
                   coordinate.lng >= minLng && coordinate.lng <= maxLng;
        }
        public static string SanitizeFileName(this string fileName)
        {
            var invalid = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", fileName.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
            return sanitized.Length > 200 ? sanitized[..200] : sanitized;
        }
        public static string GetDistrictDisplayName(this string district)
        {
            return district switch
            {
                "1" => "District 1 (Shemiran)",
                "2" => "District 2 (Vanak)",
                "3" => "District 3 (Zaferaniyeh)",
                "6" => "District 6 (Yusefabad)",
                "15" => "District 15 (Shahrak)",
                _ => $"District {district}"
            };
        }
        public static IEnumerable<IEnumerable<T>> Chunk<T>(this IEnumerable<T> source, int chunkSize)
        {
            var enumerator = source.GetEnumerator();
            while (enumerator.MoveNext())
            {
                yield return ChunkIterator(enumerator, chunkSize);
            }
        }

        private static IEnumerable<T> ChunkIterator<T>(IEnumerator<T> enumerator, int chunkSize)
        {
            do
            {
                yield return enumerator.Current;
            } while (--chunkSize > 0 && enumerator.MoveNext());
        }
    }

    public enum TimePeriod
    {
        Daily,
        Weekly,
        Monthly
    }

    public static class DateTimeExtensions
    {
        public static DateTime FromPersianDate(int year, int month, int day)
        {
            var persianCalendar = new PersianCalendar();
            return persianCalendar.ToDateTime(year, month, day, 0, 0, 0, 0);
        }

        public static string ToPersianDateString(this DateTime dateTime)
        {
            var persianCalendar = new PersianCalendar();
            var year = persianCalendar.GetYear(dateTime);
            var month = persianCalendar.GetMonth(dateTime);
            var day = persianCalendar.GetDayOfMonth(dateTime);

            return $"{year:0000}/{month:00}/{day:00}";
        }

        public static bool IsBetween(this DateTime date, DateTime start, DateTime end)
        {
            return date >= start && date <= end;
        }
    }
}