using System.Globalization;
using AISandbox.Models;
using CsvHelper;
using CsvHelper.Configuration;

namespace AISandbox.Services;

public static class LoggerService
{
    public static void AddEntry(LogEntry entry)
    {
        var fileName = $"{DateTime.Today.ToString("dd_MM_yyyy")}.csv";
        if (File.Exists(fileName))
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                // Don't write the header again.
                HasHeaderRecord = false,
            };
            using (var stream = File.Open(fileName, FileMode.Append))
            using (var writer = new StreamWriter(stream))
            using (var csv = new CsvWriter(writer, config))
            {
                csv.WriteRecord(entry);
            }
        }
        else
        {
            using (var writer = new StreamWriter(fileName))
            using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
            {
                csv.WriteRecord(entry);
            }
        }
    }
}