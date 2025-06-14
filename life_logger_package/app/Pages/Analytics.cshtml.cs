using Microsoft.AspNetCore.Mvc.RazorPages;
using LifeLoggerApp.Services;

namespace LifeLoggerApp.Pages
{
    public class AnalyticsModel : PageModel
    {
        private readonly IAnalyticsService _analyticsService;
        private readonly ILogger<AnalyticsModel> _logger;

        public AnalyticsModel(IAnalyticsService analyticsService, ILogger<AnalyticsModel> logger)
        {
            _analyticsService = analyticsService;
            _logger = logger;
        }

        public Dictionary<string, object> OverallStats { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, int> LocationFrequency { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> ActionFrequency { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> WeatherFrequency { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, double> AverageStepsByLocation { get; set; } = new Dictionary<string, double>();
        public Dictionary<string, int> EntriesByDate { get; set; } = new Dictionary<string, int>();

        public async Task OnGetAsync()
        {
            try
            {
                // Load all analytics data in parallel for better performance
                var tasks = new List<Task>
                {
                    LoadOverallStatsAsync(),
                    LoadLocationAnalyticsAsync(),
                    LoadActionAnalyticsAsync(),
                    LoadWeatherAnalyticsAsync(),
                    LoadTimelineDataAsync()
                };

                await Task.WhenAll(tasks);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading analytics data");
            }
        }

        private async Task LoadOverallStatsAsync()
        {
            try
            {
                OverallStats = await _analyticsService.GetOverallStatisticsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading overall statistics");
                OverallStats = new Dictionary<string, object>();
            }
        }

        private async Task LoadLocationAnalyticsAsync()
        {
            try
            {
                var locationData = await _analyticsService.GetLocationFrequencyAsync();
                LocationFrequency = locationData.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

                var stepsByLocation = await _analyticsService.GetAverageStepsByLocationAsync();
                AverageStepsByLocation = stepsByLocation.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading location analytics");
                LocationFrequency = new Dictionary<string, int>();
                AverageStepsByLocation = new Dictionary<string, double>();
            }
        }

        private async Task LoadActionAnalyticsAsync()
        {
            try
            {
                var actionData = await _analyticsService.GetActionFrequencyAsync();
                ActionFrequency = actionData.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading action analytics");
                ActionFrequency = new Dictionary<string, int>();
            }
        }

        private async Task LoadWeatherAnalyticsAsync()
        {
            try
            {
                var weatherData = await _analyticsService.GetWeatherFrequencyAsync();
                WeatherFrequency = weatherData.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading weather analytics");
                WeatherFrequency = new Dictionary<string, int>();
            }
        }

        private async Task LoadTimelineDataAsync()
        {
            try
            {
                EntriesByDate = await _analyticsService.GetEntriesByDateAsync(30);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading timeline data");
                EntriesByDate = new Dictionary<string, int>();
            }
        }
    }
}