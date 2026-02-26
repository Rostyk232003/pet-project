// PROMPT v2.0: Створення файлу ReportDirector
using System;
using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;

namespace LiteWebApp.Core.Entities
{
    // PROMPT v2.0: Директор для побудови звітів
    public class ReportDirector
    {
        public ReportResult BuildReport(IReportBuilder builder, ReportTemplate template)
        {
            // PROMPT v2.0: Вибір шаблону та побудова
            switch (template)
            {
                case ReportTemplate.SalesByStatus:
                    // Можна додати специфічні фільтри для шаблону
                    return builder.Build();
                case ReportTemplate.TopProducts:
                    return builder.Build();
                case ReportTemplate.SalesDynamicsByDay:
                    return builder.Build();
                default:
                    throw new ArgumentException("Unknown report template");
            }
        }
    }
}
