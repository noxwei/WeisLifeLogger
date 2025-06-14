namespace DataJarImporter.Models
{
    public class JournalRaw
    {
        public int Id { get; set; }
        public string RawJson { get; set; } = string.Empty;
        public DateTime IngestionDateTime { get; set; }
        public string? FileSource { get; set; }
        public string ProcessingStatus { get; set; } = "pending";
    }
}