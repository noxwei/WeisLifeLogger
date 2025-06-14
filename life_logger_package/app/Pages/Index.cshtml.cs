using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LifeLoggerApp.Services;
using LifeLoggerApp.Models;

namespace LifeLoggerApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly IJournalService _journalService;
    private readonly IAnalyticsService _analyticsService;

    public IndexModel(ILogger<IndexModel> logger, IJournalService journalService, IAnalyticsService analyticsService)
    {
        _logger = logger;
        _journalService = journalService;
        _analyticsService = analyticsService;
    }

    public IEnumerable<JournalEntry> RecentEntries { get; set; } = new List<JournalEntry>();
    public Dictionary<string, object> Statistics { get; set; } = new Dictionary<string, object>();
    public Dictionary<string, int> LocationFrequency { get; set; } = new Dictionary<string, int>();
    public DailySummary? TodaySummary { get; set; }

    public async Task OnGetAsync()
    {
        try
        {
            // Get recent entries (last 10)
            RecentEntries = await _journalService.GetEntriesAsync(1, 10);
            
            // Get overall statistics
            Statistics = await _analyticsService.GetOverallStatisticsAsync();
            
            // Get top locations
            var allLocations = await _analyticsService.GetLocationFrequencyAsync();
            LocationFrequency = allLocations.OrderByDescending(x => x.Value).Take(5).ToDictionary(x => x.Key, x => x.Value);
            
            // Get today's summary
            var today = DateTime.Now.ToString("yyyy-MM-dd");
            TodaySummary = await _journalService.GetDailySummaryAsync(today);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading dashboard data");
        }
    }
}
