// PROMPT v2.0: Створення файлу ConcreteReportBuilder
using System;
using System.Collections.Generic;
using System.Linq;
using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;
using LiteWebApp.ViewModels;

namespace LiteWebApp.Infrastructure.Data
{
  // PROMPT v2.0: Concrete Builder для аналітичних звітів
  /// <summary>
  /// Concrete Builder для аналітичних звітів (PROMPT v2.4: ToT-рефакторинг)
  /// </summary>
  public class ConcreteReportBuilder : IReportBuilder
  {
    private DateTime? _from;
    private DateTime? _to;
    private List<string> _statuses = new List<string>();
    private List<Guid> _categoryIds = new List<Guid>();
    private List<Guid> _productIds = new List<Guid>();
    private List<Order> _orders;
    private ReportTemplate _template;

    // PROMPT v2.4: Статичні колонки для шаблонів
    private static readonly List<string> ColumnsByStatus = new List<string> { "Статус", "Кількість замовлень", "Сума" };
    private static readonly List<string> ColumnsTopProducts = new List<string> { "Назва товару", "Кількість продажів", "Сума" };
    private static readonly List<string> ColumnsByDay = new List<string> { "Дата", "Кількість замовлень", "Сума" };

    public ConcreteReportBuilder(List<Order> orders)
    {
      _orders = orders;
    }

    public IReportBuilder WithPeriod(DateTime from, DateTime to)
    /// <summary>
    /// Встановлює період для фільтрації замовлень.
    /// </summary>
    {
      _from = from;
      _to = to;
      return this;
    }

    public IReportBuilder WithStatus(IEnumerable<string> statuses)
    /// <summary>
    /// Встановлює список статусів для фільтрації замовлень.
    /// </summary>
    {
      _statuses = statuses.ToList();
      return this;
    }

    public IReportBuilder WithCategory(IEnumerable<Guid> categoryIds)
    /// <summary>
    /// Встановлює список категорій для фільтрації замовлень.
    /// </summary>
    {
      _categoryIds = categoryIds.ToList();
      return this;
    }

    public IReportBuilder WithProduct(IEnumerable<Guid> productIds)
    /// <summary>
    /// Встановлює список продуктів для фільтрації замовлень.
    /// </summary>
    {
      _productIds = productIds.ToList();
      return this;
    }

    // PROMPT v2.3.1.3: Встановлення шаблону
    public IReportBuilder SetTemplate(ReportTemplate template)
    /// <summary>
    /// Встановлює шаблон звіту.
    /// </summary>
    {
      _template = template;
      return this;
    }

    /// <summary>
    /// Будує звіт згідно вибраного шаблону.
    /// </summary>
    public ReportResult Build()
    {
      List<Order> filtered = GetFilteredOrders();
      switch (_template)
      {
        case ReportTemplate.SalesByStatus:
          return BuildByStatus(filtered);
        case ReportTemplate.TopProducts:
          return BuildTopProducts(filtered);
        case ReportTemplate.SalesDynamicsByDay:
          return BuildByDay(filtered);
        default:
          ReportResult unknown = new ReportResult();
          unknown.Title = "Невідомий шаблон";
          return unknown;
      }
    }

    /// <summary>
    /// Повертає відфільтрований список замовлень згідно встановлених фільтрів.
    /// </summary>
    private List<Order> GetFilteredOrders()
    {
      List<Order> filtered = _orders;
      if (_from.HasValue && _to.HasValue)
      {
        filtered = filtered.Where(o => o.OrderDate >= _from && o.OrderDate <= _to).ToList();
      }
      if (_statuses.Any())
      {
        filtered = filtered.Where(o => _statuses.Contains(o.Status)).ToList();
      }
      // Категорії та продукти можна додати за потреби
      return filtered;
    }

    /// <summary>
    /// Побудова звіту "Продажі по статусу".
    /// </summary>
    private ReportResult BuildByStatus(List<Order> filtered)
    {
      ReportResult result = new ReportResult();
      result.Title = "Продажі по статусу";
      result.Columns = new List<string>(ColumnsByStatus);
      List<IGrouping<string, Order>> groups = filtered.GroupBy(o => o.Status).ToList();
      List<List<string>> rows = new List<List<string>>();
      foreach (IGrouping<string, Order> g in groups)
      {
        List<string> row = new List<string>
        {
          g.Key,
          g.Count().ToString(),
          g.Sum(o => o.TotalAmount).ToString("F2")
        };
        rows.Add(row);
      }
      result.Rows = rows;
      result.Summary["Всього замовлень"] = filtered.Count;
      result.Summary["Всього сума"] = filtered.Sum(o => o.TotalAmount).ToString("F2");
      return result;
    }

    /// <summary>
    /// Побудова звіту "Топ-товари".
    /// </summary>
    private ReportResult BuildTopProducts(List<Order> filtered)
    {
      ReportResult result = new ReportResult();
      result.Title = "Топ-товари";
      result.Columns = new List<string>(ColumnsTopProducts);
      List<CartItemViewModel> allItems = new List<CartItemViewModel>();
      foreach (Order o in filtered)
      {
        allItems.AddRange(o.Items);
      }
      List<IGrouping<string, CartItemViewModel>> groups = allItems.GroupBy(i => i.ProductName).ToList();
      List<List<string>> rows = new List<List<string>>();
      foreach (IGrouping<string, CartItemViewModel> g in groups.OrderByDescending(g => g.Sum(i => i.Price * i.Quantity)).Take(10))
      {
        List<string> row = new List<string>
        {
          g.Key,
          g.Sum(i => i.Quantity).ToString(),
          g.Sum(i => i.Price * i.Quantity).ToString("F2")
        };
        rows.Add(row);
      }
      result.Rows = rows;
      result.Summary["Всього товарів"] = rows.Count;
      decimal total = 0M;
      foreach (List<string> r in rows)
      {
        total += decimal.Parse(r[2]);
      }
      result.Summary["Всього сума"] = total.ToString("F2");
      return result;
    }

    /// <summary>
    /// Побудова звіту "Динаміка продажів по днях".
    /// </summary>
    private ReportResult BuildByDay(List<Order> filtered)
    {
      ReportResult result = new ReportResult();
      result.Title = "Динаміка продажів по днях";
      result.Columns = new List<string>(ColumnsByDay);
      List<IGrouping<DateTime, Order>> groups = filtered.GroupBy(o => o.OrderDate.Date).OrderBy(g => g.Key).ToList();
      List<List<string>> rows = new List<List<string>>();
      foreach (IGrouping<DateTime, Order> g in groups)
      {
        List<string> row = new List<string>
        {
          g.Key.ToShortDateString(),
          g.Count().ToString(),
          g.Sum(o => o.TotalAmount).ToString("F2")
        };
        rows.Add(row);
      }
      result.Rows = rows;
      result.Summary["Всього днів"] = rows.Count;
      decimal total = 0M;
      foreach (List<string> r in rows)
      {
        total += decimal.Parse(r[2]);
      }
      result.Summary["Всього сума"] = total.ToString("F2");
      return result;
    }
  }
}
