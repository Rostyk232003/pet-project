// PROMPT v2.0: Створення файлу ReportResult
using System;
using System.Collections.Generic;

namespace LiteWebApp.Core.Entities
{
    // PROMPT v2.0: DTO для результату звіту
    public class ReportResult
    {
        public string Title { get; set; }
        public List<string> Columns { get; set; }
        public List<List<string>> Rows { get; set; }
        public Dictionary<string, object> Summary { get; set; }

        public ReportResult()
        {
            Columns = new List<string>();
            Rows = new List<List<string>>();
            Summary = new Dictionary<string, object>();
        }
    }
}
