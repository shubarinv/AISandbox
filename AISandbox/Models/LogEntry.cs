namespace AISandbox.Models;

public class LogEntry
{
    public string Timestamp { get; set; }
    public string Provider { get; set; }
    public string Model { get; set; }
    public string Prompt { get; set; }
    public int TokensPrompt { get; set; }
    public int TokensCompletion { get; set; }
    public int TokensTotal { get; set; }
    public long Latency { get; set; }
    public decimal Cost { get; set; }
    public string ShortResponse { get; set; }
}