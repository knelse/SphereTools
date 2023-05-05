using System;

namespace PacketLogViewer.Models;

public class LogRecord
{
    public string Origin { get; set; }
    public DateTime Date { get; set; }
    public string Content { get; set; }

    public LogRecord(string origin, DateTime date, string content)
    {
        Origin = origin;
        Date = date;
        Content = content;
    }
}