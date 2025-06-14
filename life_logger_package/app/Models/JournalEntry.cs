using System.ComponentModel.DataAnnotations;

namespace LifeLoggerApp.Models
{
    public class JournalEntry
    {
        public int Id { get; set; }
        
        [Required]
        public string Entry { get; set; } = string.Empty;
        
        public string? Action { get; set; }
        
        [Required]
        public string DateTime { get; set; } = string.Empty;
        
        [Required]
        public string Date { get; set; } = string.Empty;
        
        [Required]
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