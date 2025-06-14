using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LifeLoggerApp.Services;
using LifeLoggerApp.Models;

namespace LifeLoggerApp.Pages
{
    public class SearchModel : PageModel
    {
        private readonly IJournalService _journalService;
        private readonly ILogger<SearchModel> _logger;

        public SearchModel(IJournalService journalService, ILogger<SearchModel> logger)
        {
            _journalService = journalService;
            _logger = logger;
        }

        [BindProperty(SupportsGet = true)]
        public string? Query { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? DateTo { get; set; }

        public IEnumerable<JournalEntry> Results { get; set; } = new List<JournalEntry>();
        public IEnumerable<string> PopularLocations { get; set; } = new List<string>();
        public IEnumerable<string> CommonActions { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            try
            {
                // Load popular locations and actions for suggestions
                PopularLocations = await _journalService.GetUniqueLocationsAsync();
                CommonActions = await _journalService.GetUniqueActionsAsync();

                // Perform search if query is provided
                if (!string.IsNullOrWhiteSpace(Query))
                {
                    await PerformSearchAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in search page");
                Results = new List<JournalEntry>();
            }
        }

        private async Task PerformSearchAsync()
        {
            try
            {
                // Get search results
                var searchResults = await _journalService.SearchEntriesAsync(Query!);

                // Apply date filters if specified
                if (!string.IsNullOrWhiteSpace(DateFrom) || !string.IsNullOrWhiteSpace(DateTo))
                {
                    if (DateTime.TryParse(DateFrom, out DateTime fromDate) && DateTime.TryParse(DateTo, out DateTime toDate))
                    {
                        // Filter by date range
                        searchResults = searchResults.Where(e => 
                        {
                            if (DateTime.TryParse(e.Date, out DateTime entryDate))
                            {
                                return entryDate >= fromDate && entryDate <= toDate;
                            }
                            return false;
                        });
                    }
                    else if (DateTime.TryParse(DateFrom, out DateTime onlyFromDate))
                    {
                        // Filter from date only
                        searchResults = searchResults.Where(e => 
                        {
                            if (DateTime.TryParse(e.Date, out DateTime entryDate))
                            {
                                return entryDate >= onlyFromDate;
                            }
                            return false;
                        });
                    }
                    else if (DateTime.TryParse(DateTo, out DateTime onlyToDate))
                    {
                        // Filter to date only
                        searchResults = searchResults.Where(e => 
                        {
                            if (DateTime.TryParse(e.Date, out DateTime entryDate))
                            {
                                return entryDate <= onlyToDate;
                            }
                            return false;
                        });
                    }
                }

                Results = searchResults.ToList();

                _logger.LogInformation("Search completed for query '{Query}' with {ResultCount} results", 
                    Query, Results.Count());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error performing search for query '{Query}'", Query);
                Results = new List<JournalEntry>();
            }
        }
    }
}