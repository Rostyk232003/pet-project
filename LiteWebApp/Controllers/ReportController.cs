// PROMPT v2.0: Створення файлу ReportController
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;
using LiteWebApp.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace LiteWebApp.Controllers
{
  // PROMPT v2.0: Контролер для аналітики та звітів
  public class ReportController : Controller
  {
    private readonly IOrderRepository _orderRepository;

    public ReportController(IOrderRepository orderRepository)
    {
      _orderRepository = orderRepository;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
      // Показати форму для вибору фільтрів та шаблону
      return View();
    }

    [HttpPost]
    public async Task<IActionResult> Generate(ReportTemplate template, DateTime? from, DateTime? to, List<string> statuses)
    {
      List<Order> orders = await _orderRepository.GetAllOrdersAsync();
      ConcreteReportBuilder builder = new ConcreteReportBuilder(orders);
      if (from.HasValue && to.HasValue)
        builder.WithPeriod(from.Value, to.Value);
      if (statuses != null && statuses.Any())
        builder.WithStatus(statuses);
      builder.SetTemplate(template); // PROMPT v2.3.1.3: Передаємо шаблон у Builder
      ReportDirector director = new ReportDirector();
      ReportResult report = director.BuildReport(builder, template);
      return View("Result", report);
    }
  }
}
