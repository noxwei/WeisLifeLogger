using Microsoft.EntityFrameworkCore;
using LifeLoggerApp.Data;
using LifeLoggerApp.Models;

namespace LifeLoggerApp.Services
{
    public class JournalService : IJournalService
    {
        private readonly LifeLoggerDbContext _context;
        private readonly ILogger<JournalService> _logger;

        public JournalService(LifeLoggerDbContext context, ILogger<JournalService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IEnumerable<JournalEntry>> GetEntriesAsync(int page = 1, int pageSize = 50)
        {
            var skip = (page - 1) * pageSize;
            return await _context.JournalEntries
                .OrderByDescending(e => e.DateTime)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<JournalEntry?> GetEntryByIdAsync(int id)
        {
            return await _context.JournalEntries
                .FirstOrDefaultAsync(e => e.Id == id);
        }

        public async Task<DailySummary> GetDailySummaryAsync(string date)
        {
            var entries = await _context.JournalEntries
                .Where(e => e.Date == date)
                .ToListAsync();

            var summary = new DailySummary
            {
                Date = date,
                EntryCount = entries.Count,
                Locations = entries.Where(e => !string.IsNullOrEmpty(e.Location))
                                 .Select(e => e.Location!)
                                 .Distinct()
                                 .ToList(),
                Activities = entries.Where(e => !string.IsNullOrEmpty(e.Action))
                                  .Select(e => e.Action!)
                                  .Distinct()
                                  .ToList(),
                TotalSteps = entries.Where(e => e.Steps.HasValue)
                                  .Sum(e => e.Steps),
                MostFrequentLocation = entries.Where(e => !string.IsNullOrEmpty(e.Location))
                                            .GroupBy(e => e.Location)
                                            .OrderByDescending(g => g.Count())
                                            .FirstOrDefault()?.Key,
                Weather = entries.Where(e => !string.IsNullOrEmpty(e.Weather))
                               .Select(e => e.Weather)
                               .FirstOrDefault(),
                UniqueActions = entries.Where(e => !string.IsNullOrEmpty(e.Action))
                                     .Select(e => e.Action!)
                                     .Distinct()
                                     .ToList()
            };

            return summary;
        }

        public async Task<IEnumerable<JournalEntry>> SearchEntriesAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<JournalEntry>();

            var searchTerm = $"%{query}%";
            return await _context.JournalEntries
                .Where(e => EF.Functions.Like(e.Entry, searchTerm) ||
                           EF.Functions.Like(e.Location ?? "", searchTerm) ||
                           EF.Functions.Like(e.Action ?? "", searchTerm))
                .OrderByDescending(e => e.DateTime)
                .Take(1000)
                .ToListAsync();
        }

        public async Task<IEnumerable<JournalEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            var startDateStr = startDate.ToString("yyyy-MM-dd");
            var endDateStr = endDate.ToString("yyyy-MM-dd");

            return await _context.JournalEntries
                .Where(e => string.Compare(e.Date, startDateStr) >= 0 && 
                           string.Compare(e.Date, endDateStr) <= 0)
                .OrderByDescending(e => e.DateTime)
                .ToListAsync();
        }

        public async Task<int> GetTotalEntriesCountAsync()
        {
            return await _context.JournalEntries.CountAsync();
        }

        public async Task<IEnumerable<string>> GetUniqueLocationsAsync()
        {
            return await _context.JournalEntries
                .Where(e => !string.IsNullOrEmpty(e.Location))
                .Select(e => e.Location!)
                .Distinct()
                .OrderBy(l => l)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetUniqueActionsAsync()
        {
            return await _context.JournalEntries
                .Where(e => !string.IsNullOrEmpty(e.Action))
                .Select(e => e.Action!)
                .Distinct()
                .OrderBy(a => a)
                .ToListAsync();
        }
    }
}