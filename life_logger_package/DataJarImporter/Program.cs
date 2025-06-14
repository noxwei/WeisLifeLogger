using System.Text.Json;
using Microsoft.Data.Sqlite;

namespace DataJarImporter
{
    class Program
    {
        private static string connectionString = "Data Source=../journal_db/life_logger.db";
        private static string defaultInputPath = "../journal_exports/store.json";
        
        static async Task Main(string[] args)
        {
            Console.WriteLine("Wei Life Logger - DataJar Importer v1.0");
            
            try
            {
                string inputPath = GetInputPath(args);
                Console.WriteLine($"Processing JSON file: {inputPath}");
                
                await InitializeDatabase();
                await ProcessJsonFile(inputPath);
                
                Console.WriteLine("Import completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                Environment.Exit(1);
            }
        }
        
        private static string GetInputPath(string[] args)
        {
            if (args.Length > 0 && args[0] == "--input" && args.Length > 1)
            {
                return args[1];
            }
            
            return defaultInputPath;
        }
        
        private static async Task InitializeDatabase()
        {
            Console.WriteLine("Initializing database...");
            
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            
            await CreateTablesAsync(connection);
            await CreateIndexesAsync(connection);
            
            Console.WriteLine("Database initialized successfully.");
        }
        
        private static async Task CreateTablesAsync(SqliteConnection connection)
        {
            string createJournalRawTable = @"
                CREATE TABLE IF NOT EXISTS JournalRaw (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    raw_json TEXT NOT NULL,
                    ingestion_datetime TEXT NOT NULL,
                    file_source TEXT,
                    processing_status TEXT DEFAULT 'pending'
                );";
            
            string createJournalEntriesTable = @"
                CREATE TABLE IF NOT EXISTS JournalEntries (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    entry TEXT NOT NULL,
                    action TEXT,
                    datetime TEXT NOT NULL,
                    date TEXT NOT NULL,
                    time TEXT NOT NULL,
                    location TEXT,
                    weather TEXT,
                    phone_mode TEXT,
                    device TEXT,
                    steps INTEGER,
                    hrv INTEGER,
                    device_battery INTEGER,
                    created_at TEXT DEFAULT CURRENT_TIMESTAMP,
                    updated_at TEXT DEFAULT CURRENT_TIMESTAMP
                );";
            
            using var command1 = new SqliteCommand(createJournalRawTable, connection);
            await command1.ExecuteNonQueryAsync();
            
            using var command2 = new SqliteCommand(createJournalEntriesTable, connection);
            await command2.ExecuteNonQueryAsync();
        }
        
        private static async Task CreateIndexesAsync(SqliteConnection connection)
        {
            string[] indexes = {
                "CREATE INDEX IF NOT EXISTS idx_journal_date ON JournalEntries(date);",
                "CREATE INDEX IF NOT EXISTS idx_journal_datetime ON JournalEntries(datetime);",
                "CREATE INDEX IF NOT EXISTS idx_journal_location ON JournalEntries(location);",
                "CREATE INDEX IF NOT EXISTS idx_journal_action ON JournalEntries(action);"
            };
            
            foreach (string indexSql in indexes)
            {
                using var command = new SqliteCommand(indexSql, connection);
                await command.ExecuteNonQueryAsync();
            }
        }
        
        private static async Task ProcessJsonFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"JSON file not found: {filePath}");
            }
            
            Console.WriteLine("Reading file...");
            string fileContent = await File.ReadAllTextAsync(filePath);
            
            if (string.IsNullOrWhiteSpace(fileContent))
            {
                throw new InvalidOperationException("File is empty or contains only whitespace");
            }
            
            string jsonContent;
            
            // Check if file is already JSON format or needs conversion
            if (fileContent.TrimStart().StartsWith("{"))
            {
                Console.WriteLine("File is already in JSON format");
                jsonContent = fileContent;
            }
            else
            {
                Console.WriteLine("File appears to be CSV format, converting to JSON...");
                jsonContent = DataJarConverter.ConvertCsvToJson(fileContent);
                Console.WriteLine("Conversion completed successfully");
                
                // Optionally save the converted JSON for debugging
                var convertedPath = Path.ChangeExtension(filePath, ".converted.json");
                await File.WriteAllTextAsync(convertedPath, jsonContent);
                Console.WriteLine($"Converted JSON saved to: {convertedPath}");
            }
            
            Console.WriteLine("Storing raw JSON in database...");
            await StoreRawJson(jsonContent, filePath);
            
            Console.WriteLine("Parsing and normalizing data...");
            await ParseAndNormalizeData(jsonContent);
        }
        
        private static async Task StoreRawJson(string jsonContent, string filePath)
        {
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            
            string insertSql = @"
                INSERT INTO JournalRaw (raw_json, ingestion_datetime, file_source, processing_status)
                VALUES (@json, @datetime, @source, 'processing')";
            
            using var command = new SqliteCommand(insertSql, connection);
            command.Parameters.AddWithValue("@json", jsonContent);
            command.Parameters.AddWithValue("@datetime", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
            command.Parameters.AddWithValue("@source", Path.GetFileName(filePath));
            
            await command.ExecuteNonQueryAsync();
        }
        
        private static async Task ParseAndNormalizeData(string jsonContent)
        {
            JsonDocument document = JsonDocument.Parse(jsonContent);
            JsonElement root = document.RootElement;
            
            if (!root.TryGetProperty("entries", out JsonElement entriesElement))
            {
                Console.WriteLine("Warning: No 'entries' property found in JSON");
                return;
            }
            
            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync();
            
            int entryCount = 0;
            foreach (JsonElement entry in entriesElement.EnumerateArray())
            {
                await ProcessSingleEntry(connection, entry);
                entryCount++;
            }
            
            Console.WriteLine($"Processed {entryCount} journal entries");
            
            await UpdateProcessingStatus(connection, "completed");
        }
        
        private static async Task ProcessSingleEntry(SqliteConnection connection, JsonElement entry)
        {
            string insertSql = @"
                INSERT INTO JournalEntries (
                    entry, action, datetime, date, time, location, weather, 
                    phone_mode, device, steps, hrv, device_battery
                ) VALUES (
                    @entry, @action, @datetime, @date, @time, @location, @weather,
                    @phone_mode, @device, @steps, @hrv, @device_battery
                )";
            
            using var command = new SqliteCommand(insertSql, connection);
            
            // Get values with fallback logic
            var entryText = GetStringValue(entry, "entry");
            var action = GetStringValue(entry, "action");
            var dateTimeStr = GetStringValue(entry, "datetime");
            var dateStr = GetStringValue(entry, "date");
            var timeStr = GetStringValue(entry, "time");
            
            // If we don't have separate date/time, try to extract from datetime
            if (string.IsNullOrEmpty(dateStr) && !string.IsNullOrEmpty(dateTimeStr))
            {
                if (TryParseDateTime(dateTimeStr, out var parsedDateTime))
                {
                    dateStr = parsedDateTime.ToString("yyyy-MM-dd");
                    if (string.IsNullOrEmpty(timeStr))
                    {
                        timeStr = parsedDateTime.ToString("HH:mm:ss");
                    }
                }
            }
            
            // Ensure we have at least basic required fields
            if (string.IsNullOrEmpty(entryText))
            {
                entryText = $"{action} at {dateTimeStr}".Trim();
            }
            
            if (string.IsNullOrEmpty(dateTimeStr))
            {
                dateTimeStr = $"{dateStr} {timeStr}".Trim();
            }
            
            command.Parameters.AddWithValue("@entry", entryText);
            command.Parameters.AddWithValue("@action", action);
            command.Parameters.AddWithValue("@datetime", dateTimeStr);
            command.Parameters.AddWithValue("@date", dateStr);
            command.Parameters.AddWithValue("@time", timeStr);
            command.Parameters.AddWithValue("@location", GetStringValue(entry, "location"));
            command.Parameters.AddWithValue("@weather", GetStringValue(entry, "weather"));
            command.Parameters.AddWithValue("@phone_mode", GetStringValue(entry, "phone_mode"));
            command.Parameters.AddWithValue("@device", GetStringValue(entry, "device"));
            command.Parameters.AddWithValue("@steps", GetIntValue(entry, "steps") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@hrv", GetIntValue(entry, "hrv") ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@device_battery", GetIntValue(entry, "device_battery") ?? (object)DBNull.Value);
            
            await command.ExecuteNonQueryAsync();
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
                if (DateTime.TryParseExact(dateTimeStr, format, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out result))
                {
                    return true;
                }
            }

            // Try general parsing as fallback
            return DateTime.TryParse(dateTimeStr, out result);
        }
        
        private static async Task UpdateProcessingStatus(SqliteConnection connection, string status)
        {
            string updateSql = @"
                UPDATE JournalRaw 
                SET processing_status = @status 
                WHERE processing_status = 'processing'";
            
            using var command = new SqliteCommand(updateSql, connection);
            command.Parameters.AddWithValue("@status", status);
            
            await command.ExecuteNonQueryAsync();
        }
        
        private static string GetStringValue(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property))
            {
                return property.GetString() ?? string.Empty;
            }
            return string.Empty;
        }
        
        private static int? GetIntValue(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property))
            {
                // Handle null or undefined values
                if (property.ValueKind == JsonValueKind.Null || property.ValueKind == JsonValueKind.Undefined)
                {
                    return null;
                }
                
                // Handle string values first
                if (property.ValueKind == JsonValueKind.String)
                {
                    string stringValue = property.GetString() ?? string.Empty;
                    if (!string.IsNullOrWhiteSpace(stringValue) && int.TryParse(stringValue, out int parsedValue))
                    {
                        return parsedValue;
                    }
                    return null;
                }
                
                // Handle numeric values
                if (property.ValueKind == JsonValueKind.Number)
                {
                    // Try to get as integer directly
                    if (property.TryGetInt32(out int value))
                    {
                        return value;
                    }
                    
                    // Try to handle other numeric types (like double that represents an integer)
                    if (property.TryGetDouble(out double doubleValue))
                    {
                        if (doubleValue == Math.Floor(doubleValue) && doubleValue >= int.MinValue && doubleValue <= int.MaxValue)
                        {
                            return (int)doubleValue;
                        }
                    }
                }
            }
            return null;
        }
    }
}