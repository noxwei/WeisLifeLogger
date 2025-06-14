using System.ComponentModel.DataAnnotations;

namespace LifeLoggerApp.Models
{
    public class JournalRaw
    {
        public int Id { get; set; }
        
        [Required]
        public string RawJson { get; set; } = string.Empty;
        
        [Required]
        public DateTime IngestionDateTime { get; set; }
        
        public string? FileSource { get; set; }
        
        [Required]
        public string ProcessingStatus { get; set; } = "pending";
    }
}