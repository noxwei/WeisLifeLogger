namespace LifeLoggerApp.Services
{
    public interface IAnalyticsService
    {
        Task<Dictionary<string, int>> GetLocationFrequencyAsync();
        Task<Dictionary<string, int>> GetActionFrequencyAsync();
        Task<Dictionary<string, int>> GetWeatherFrequencyAsync();
        Task<Dictionary<string, double>> GetAverageStepsByLocationAsync();
        Task<Dictionary<string, int>> GetEntriesByDateAsync(int days = 30);
        Task<Dictionary<string, object>> GetOverallStatisticsAsync();
    }
}