using LifeLoggerApp.Models;

namespace LifeLoggerApp.Services
{
    public interface IJournalService
    {
        Task<IEnumerable<JournalEntry>> GetEntriesAsync(int page = 1, int pageSize = 50);
        Task<JournalEntry?> GetEntryByIdAsync(int id);
        Task<DailySummary> GetDailySummaryAsync(string date);
        Task<IEnumerable<JournalEntry>> SearchEntriesAsync(string query);
        Task<IEnumerable<JournalEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> GetTotalEntriesCountAsync();
        Task<IEnumerable<string>> GetUniqueLocationsAsync();
        Task<IEnumerable<string>> GetUniqueActionsAsync();
    }
}