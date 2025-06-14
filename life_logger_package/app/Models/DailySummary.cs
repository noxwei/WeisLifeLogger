namespace LifeLoggerApp.Models
{
    public class DailySummary
    {
        public string Date { get; set; } = string.Empty;
        public int EntryCount { get; set; }
        public List<string> Locations { get; set; } = new List<string>();
        public List<string> Activities { get; set; } = new List<string>();
        public int? TotalSteps { get; set; }
        public string? MostFrequentLocation { get; set; }
        public string? Weather { get; set; }
        public List<string> UniqueActions { get; set; } = new List<string>();
    }
}