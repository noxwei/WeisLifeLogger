using System.Text.Json;
using System.Text.RegularExpressions;
using System.Globalization;

namespace DataJarImporter
{
    public static class DataJarConverter
    {
        public static string ConvertCsvToJson(string csvContent)
        {
            var lines = csvContent.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            
            if (lines.Length < 2)
            {
                throw new InvalidOperationException("CSV file must have at least a header and one data row");
            }

            // Parse header
            var headers = lines[0].Split('~');
            var entries = new List<object>();

            // Process each data line
            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;

                try
                {
                    var entry = ParseDataLine(line, headers);
                    if (entry != null)
                    {
                        entries.Add(entry);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error parsing line {i + 1}: {ex.Message}");
                    Console.WriteLine($"Line content: {line}");
                }
            }

            var result = new { entries = entries };
            return JsonSerializer.Serialize(result, new JsonSerializerOptions { WriteIndented = true });
        }

        private static object? ParseDataLine(string line, string[] headers)
        {
            // Handle complex entries that start with "{ Entry:"
            if (line.StartsWith("{ Entry:") || line.StartsWith("{Entry:"))
            {
                return ParseComplexEntry(line);
            }

            // Handle simple tilde-delimited entries
            var parts = line.Split('~');
            
            // Ensure we have enough parts (some might be empty)
            while (parts.Length < headers.Length)
            {
                var newParts = new string[headers.Length];
                Array.Copy(parts, newParts, parts.Length);
                for (int j = parts.Length; j < headers.Length; j++)
                {
                    newParts[j] = "";
                }
                parts = newParts;
            }

            var entry = new Dictionary<string, object>();

            for (int i = 0; i < headers.Length && i < parts.Length; i++)
            {
                var header = headers[i].Trim();
                var value = parts[i].Trim();

                // Map headers to expected field names
                var fieldName = MapHeaderToField(header);
                
                if (!string.IsNullOrEmpty(fieldName))
                {
                    entry[fieldName] = ProcessFieldValue(fieldName, value);
                }
            }

            // Ensure required fields exist
            if (!entry.ContainsKey("entry") || string.IsNullOrEmpty(entry["entry"].ToString()))
            {
                return null;
            }

            return entry;
        }

        private static object ParseComplexEntry(string line)
        {
            var entry = new Dictionary<string, object>();

            try
            {
                // Extract the main entry text from "{ Entry: ... }"
                var entryMatch = Regex.Match(line, @"\{\s*Entry:\s*([^,}]+?)(?:\s*,|\s*Time:)", RegexOptions.IgnoreCase);
                if (entryMatch.Success)
                {
                    entry["entry"] = entryMatch.Groups[1].Value.Trim();
                }

                // Extract Time
                var timeMatch = Regex.Match(line, @"Time:\s*([^,}]+?)(?:\s*,|\s*Location:|\s*Weather:|\s*Phone Focus Mode:|\s*Device:|\s*Steps:|\s*HRV:|\s*Device Battery:|\s*})", RegexOptions.IgnoreCase);
                if (timeMatch.Success)
                {
                    var timeStr = timeMatch.Groups[1].Value.Trim();
                    entry["datetime"] = timeStr;
                    
                    // Parse date and time components
                    if (TryParseDateTime(timeStr, out var parsedDate))
                    {
                        entry["date"] = parsedDate.ToString("yyyy-MM-dd");
                        entry["time"] = parsedDate.ToString("HH:mm:ss");
                    }
                }

                // Extract Location
                var locationMatch = Regex.Match(line, @"Location:\s*([^,}]+?)(?:\s*,|\s*Weather:|\s*Phone Focus Mode:|\s*Device:|\s*Steps:|\s*HRV:|\s*Device Battery:|\s*})", RegexOptions.IgnoreCase);
                if (locationMatch.Success)
                {
                    entry["location"] = locationMatch.Groups[1].Value.Trim();
                }

                // Extract Weather
                var weatherMatch = Regex.Match(line, @"Weather:\s*([^,}]+?)(?:\s*,|\s*Phone Focus Mode:|\s*Device:|\s*Steps:|\s*HRV:|\s*Device Battery:|\s*})", RegexOptions.IgnoreCase);
                if (weatherMatch.Success)
                {
                    entry["weather"] = weatherMatch.Groups[1].Value.Trim();
                }

                // Extract Phone Focus Mode
                var phoneModeMatch = Regex.Match(line, @"Phone Focus Mode:\s*([^,}]+?)(?:\s*,|\s*Device:|\s*Steps:|\s*HRV:|\s*Device Battery:|\s*})", RegexOptions.IgnoreCase);
                if (phoneModeMatch.Success)
                {
                    entry["phone_mode"] = phoneModeMatch.Groups[1].Value.Trim();
                }

                // Extract Device
                var deviceMatch = Regex.Match(line, @"Device:\s*([^,}]+?)(?:\s*,|\s*Steps:|\s*HRV:|\s*Device Battery:|\s*})", RegexOptions.IgnoreCase);
                if (deviceMatch.Success)
                {
                    entry["device"] = deviceMatch.Groups[1].Value.Trim();
                }

                // Extract Steps
                var stepsMatch = Regex.Match(line, @"Steps:\s*(\d+)", RegexOptions.IgnoreCase);
                if (stepsMatch.Success && int.TryParse(stepsMatch.Groups[1].Value, out var steps))
                {
                    entry["steps"] = steps;
                }

                // Extract HRV
                var hrvMatch = Regex.Match(line, @"HRV:\s*(\d+)", RegexOptions.IgnoreCase);
                if (hrvMatch.Success && int.TryParse(hrvMatch.Groups[1].Value, out var hrv))
                {
                    entry["hrv"] = hrv;
                }

                // Extract Device Battery
                var batteryMatch = Regex.Match(line, @"Device Battery:\s*(\d+)", RegexOptions.IgnoreCase);
                if (batteryMatch.Success && int.TryParse(batteryMatch.Groups[1].Value, out var battery))
                {
                    entry["device_battery"] = battery;
                }

                return entry;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing complex entry: {ex.Message}");
                return null;
            }
        }

        private static string MapHeaderToField(string header)
        {
            return header.ToLower() switch
            {
                "entry" => "entry",
                "action" => "action",
                "time" => "time",
                "datetime" => "datetime",
                "location" => "location",
                "weather" => "weather",
                "phonefocusmode" => "phone_mode",
                "device" => "device",
                "steps" => "steps",
                "hrv" => "hrv",
                "devicebattery" => "device_battery",
                _ => ""
            };
        }

        private static object ProcessFieldValue(string fieldName, string value)
        {
            if (string.IsNullOrEmpty(value))
                return "";

            switch (fieldName)
            {
                case "steps":
                case "hrv":
                case "device_battery":
                    if (int.TryParse(value, out var intValue))
                        return intValue;
                    return null;

                case "datetime":
                    if (TryParseDateTime(value, out var parsedDate))
                    {
                        return parsedDate.ToString("yyyy-MM-dd HH:mm:ss");
                    }
                    return value;

                case "time":
                    if (TryParseDateTime(value, out var parsedTime))
                    {
                        return parsedTime.ToString("HH:mm:ss");
                    }
                    return value;

                default:
                    return value;
            }
        }

        private static bool TryParseDateTime(string dateTimeStr, out DateTime result)
        {
            result = default;

            // Common DataJar formats
            string[] formats = {
                "MMM d, yyyy 'at' h:mm tt",      // "Jun 12, 2025 at 7:29 PM"
                "MMM d, yyyy 'at' h:mm:ss tt",   // "Jun 12, 2025 at 7:29:30 PM"
                "MMM d, yyyy 'at' hh:mm tt",     // "Jun 12, 2025 at 07:29 PM"
                "yyyy-MM-dd HH:mm:ss",           // "2025-06-12 19:29:00"
                "yyyy-MM-dd'T'HH:mm:ss",         // "2025-06-12T19:29:00"
                "M/d/yyyy h:mm:ss tt",           // "6/12/2025 7:29:00 PM"
                "M/d/yyyy h:mm tt"               // "6/12/2025 7:29 PM"
            };

            foreach (var format in formats)
            {
                if (DateTime.TryParseExact(dateTimeStr, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
                {
                    return true;
                }
            }

            // Try general parsing as fallback
            return DateTime.TryParse(dateTimeStr, out result);
        }
    }
}