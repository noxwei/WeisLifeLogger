using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using LifeLoggerApp.Services;
using LifeLoggerApp.Models;

namespace LifeLoggerApp.Pages
{
    public class EntriesModel : PageModel
    {
        private readonly IJournalService _journalService;
        private readonly ILogger<EntriesModel> _logger;

        public EntriesModel(IJournalService journalService, ILogger<EntriesModel> logger)
        {
            _journalService = journalService;
            _logger = logger;
        }

        public IEnumerable<JournalEntry> Entries { get; set; } = new List<JournalEntry>();
        public IEnumerable<string> AvailableLocations { get; set; } = new List<string>();
        public IEnumerable<string> AvailableActions { get; set; } = new List<string>();

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedLocation { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? SelectedAction { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; } = 1;

        public int PageSize { get; set; } = 20;
        public int TotalEntries { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalEntries / PageSize);

        public async Task OnGetAsync()
        {
            try
            {
                // Load filter options
                AvailableLocations = await _journalService.GetUniqueLocationsAsync();
                AvailableActions = await _journalService.GetUniqueActionsAsync();

                // Get total count for pagination
                TotalEntries = await _journalService.GetTotalEntriesCountAsync();

                // Load entries based on filters
                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    Entries = await _journalService.SearchEntriesAsync(SearchQuery);
                    TotalEntries = Entries.Count();
                    
                    // Apply pagination to search results
                    Entries = Entries.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
                }
                else
                {
                    // Get paginated entries
                    var allEntries = await _journalService.GetEntriesAsync(CurrentPage, PageSize);
                    
                    // Apply location and action filters if specified
                    if (!string.IsNullOrWhiteSpace(SelectedLocation) || !string.IsNullOrWhiteSpace(SelectedAction))
                    {
                        allEntries = allEntries.Where(e => 
                            (string.IsNullOrWhiteSpace(SelectedLocation) || e.Location == SelectedLocation) &&
                            (string.IsNullOrWhiteSpace(SelectedAction) || e.Action == SelectedAction));
                    }
                    
                    Entries = allEntries;
                }

                // Ensure current page is valid
                if (CurrentPage < 1) CurrentPage = 1;
                if (CurrentPage > TotalPages && TotalPages > 0) CurrentPage = TotalPages;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading entries page");
                Entries = new List<JournalEntry>();
            }
        }

        public string GetPageUrl(int page)
        {
            var queryParams = new List<string>();
            
            if (!string.IsNullOrWhiteSpace(SearchQuery))
                queryParams.Add($"search={Uri.EscapeDataString(SearchQuery)}");
            
            if (!string.IsNullOrWhiteSpace(SelectedLocation))
                queryParams.Add($"location={Uri.EscapeDataString(SelectedLocation)}");
            
            if (!string.IsNullOrWhiteSpace(SelectedAction))
                queryParams.Add($"action={Uri.EscapeDataString(SelectedAction)}");
            
            queryParams.Add($"currentPage={page}");
            
            return $"/Entries?{string.Join("&", queryParams)}";
        }
    }
}