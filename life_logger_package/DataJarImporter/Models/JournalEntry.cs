namespace DataJarImporter.Models
{
    public class JournalEntry
    {
        public int Id { get; set; }
        public string Entry { get; set; } = string.Empty;
        public string? Action { get; set; }
        public string DateTime { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public string Time { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Weather { get; set; }
        public string? PhoneMode { get; set; }
        public string? Device { get; set; }
        public int? Steps { get; set; }
        public int? Hrv { get; set; }
        public int? DeviceBattery { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}