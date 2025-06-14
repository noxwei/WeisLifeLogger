using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LifeLoggerApp.Services;
using LifeLoggerApp.Models;

namespace LifeLoggerApp.Pages
{
    public class SummaryModel : PageModel
    {
        private readonly IJournalService _journalService;
        private readonly ILogger<SummaryModel> _logger;

        public SummaryModel(IJournalService journalService, ILogger<SummaryModel> logger)
        {
            _journalService = journalService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string SelectedDate { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");

        public DailySummary? Summary { get; set; }
        public IEnumerable<JournalEntry> DayEntries { get; set; } = new List<JournalEntry>();

        public async Task OnGetAsync()
        {
            try
            {
                // Validate and parse the selected date
                if (!DateTime.TryParse(SelectedDate, out DateTime parsedDate))
                {
                    SelectedDate = DateTime.Now.ToString("yyyy-MM-dd");
                    parsedDate = DateTime.Now;
                }

                // Get summary for the selected date
                Summary = await _journalService.GetDailySummaryAsync(SelectedDate);

                // Get all entries for the selected date
                var startDate = parsedDate.Date;
                var endDate = parsedDate.Date.AddDays(1).AddTicks(-1);
                DayEntries = await _journalService.GetEntriesByDateRangeAsync(startDate, endDate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading summary for date {Date}", SelectedDate);
                Summary = null;
                DayEntries = new List<JournalEntry>();
            }
        }

        public string GetDateUrl(string date)
        {
            return $"/Summary?selectedDate={date}";
        }
    }
}