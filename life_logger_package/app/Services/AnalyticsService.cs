using Microsoft.EntityFrameworkCore;
using LifeLoggerApp.Data;

namespace LifeLoggerApp.Services
{
    public class AnalyticsService : IAnalyticsService
    {
        private readonly LifeLoggerDbContext _context;
        private readonly ILogger<AnalyticsService> _logger;

        public AnalyticsService(LifeLoggerDbContext context, ILogger<AnalyticsService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Dictionary<string, int>> GetLocationFrequencyAsync()
        {
            return await _context.JournalEntries
                .Where(e => !string.IsNullOrEmpty(e.Location))
                .GroupBy(e => e.Location)
                .Select(g => new { Location = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Location!, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetActionFrequencyAsync()
        {
            return await _context.JournalEntries
                .Where(e => !string.IsNullOrEmpty(e.Action))
                .GroupBy(e => e.Action)
                .Select(g => new { Action = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Action!, x => x.Count);
        }

        public async Task<Dictionary<string, int>> GetWeatherFrequencyAsync()
        {
            return await _context.JournalEntries
                .Where(e => !string.IsNullOrEmpty(e.Weather))
                .GroupBy(e => e.Weather)
                .Select(g => new { Weather = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.Weather!, x => x.Count);
        }

        public async Task<Dictionary<string, double>> GetAverageStepsByLocationAsync()
        {
            return await _context.JournalEntries
                .Where(e => !string.IsNullOrEmpty(e.Location) && e.Steps.HasValue)
                .GroupBy(e => e.Location)
                .Select(g => new { Location = g.Key, AverageSteps = g.Average(e => e.Steps!.Value) })
                .ToDictionaryAsync(x => x.Location!, x => Math.Round(x.AverageSteps, 2));
        }

        public async Task<Dictionary<string, int>> GetEntriesByDateAsync(int days = 30)
        {
            var cutoffDate = DateTime.Now.AddDays(-days).ToString("yyyy-MM-dd");
            
            return await _context.JournalEntries
                .Where(e => string.Compare(e.Date, cutoffDate) >= 0)
                .GroupBy(e => e.Date)
                .Select(g => new { Date = g.Key, Count = g.Count() })
                .OrderBy(x => x.Date)
                .ToDictionaryAsync(x => x.Date, x => x.Count);
        }

        public async Task<Dictionary<string, object>> GetOverallStatisticsAsync()
        {
            var totalEntries = await _context.JournalEntries.CountAsync();
            var uniqueLocations = await _context.JournalEntries
                .Where(e => !string.IsNullOrEmpty(e.Location))
                .Select(e => e.Location)
                .Distinct()
                .CountAsync();
            
            var uniqueActions = await _context.JournalEntries
                .Where(e => !string.IsNullOrEmpty(e.Action))
                .Select(e => e.Action)
                .Distinct()
                .CountAsync();

            var dateRange = await _context.JournalEntries
                .GroupBy(e => 1)
                .Select(g => new { 
                    FirstEntry = g.Min(e => e.Date), 
                    LastEntry = g.Max(e => e.Date) 
                })
                .FirstOrDefaultAsync();

            var totalSteps = await _context.JournalEntries
                .Where(e => e.Steps.HasValue)
                .SumAsync(e => e.Steps!.Value);

            var averageEntriesPerDay = 0.0;
            if (dateRange != null && totalEntries > 0)
            {
                if (DateTime.TryParse(dateRange.FirstEntry, out var firstDate) && 
                    DateTime.TryParse(dateRange.LastEntry, out var lastDate))
                {
                    var daysDiff = (lastDate - firstDate).Days + 1;
                    averageEntriesPerDay = Math.Round((double)totalEntries / daysDiff, 2);
                }
            }

            return new Dictionary<string, object>
            {
                { "TotalEntries", totalEntries },
                { "UniqueLocations", uniqueLocations },
                { "UniqueActions", uniqueActions },
                { "FirstEntryDate", dateRange?.FirstEntry ?? "N/A" },
                { "LastEntryDate", dateRange?.LastEntry ?? "N/A" },
                { "TotalSteps", totalSteps },
                { "AverageEntriesPerDay", averageEntriesPerDay }
            };
        }
    }
}