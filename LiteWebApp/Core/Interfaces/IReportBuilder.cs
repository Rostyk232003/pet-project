// PROMPT v2.0: Створення файлу IReportBuilder
using System;
using System.Collections.Generic;

namespace LiteWebApp.Core.Interfaces
{
  using LiteWebApp.Core.Entities;
  // PROMPT v2.0: Інтерфейс Builder для аналітичних звітів
  public interface IReportBuilder
  {
    IReportBuilder WithPeriod(DateTime from, DateTime to);
    IReportBuilder WithStatus(IEnumerable<string> statuses);
    IReportBuilder WithCategory(IEnumerable<Guid> categoryIds);
    IReportBuilder WithProduct(IEnumerable<Guid> productIds);
    IReportBuilder SetTemplate(ReportTemplate template); // PROMPT v2.3.1.3: Додаємо метод для шаблону
    ReportResult Build();
  }
}
