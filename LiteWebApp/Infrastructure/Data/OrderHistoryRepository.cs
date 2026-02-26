using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LiteWebApp.Core.Entities;
using LiteWebApp.Core.Interfaces;

namespace LiteWebApp.Infrastructure.Data
{
  // PROMPT v1.0: OrderHistoryRepository для зберігання знімків у JSON
  public class OrderHistoryRepository : IOrderHistoryRepository
  {
    private readonly string _filePath;

    public OrderHistoryRepository(string filePath)
    {
      _filePath = filePath;
    }

    public async Task SaveSnapshotAsync(Guid orderId, OrderMemento memento)
    {
      var histories = await LoadAllAsync();
      if (!histories.ContainsKey(orderId))
        histories[orderId] = new List<OrderMemento>();
      histories[orderId].Add(memento);
      await SaveAllAsync(histories);
    }

    public async Task<IEnumerable<OrderMemento>> GetHistoryAsync(Guid orderId)
    {
      var histories = await LoadAllAsync();
      return histories.ContainsKey(orderId) ? histories[orderId] : Enumerable.Empty<OrderMemento>();
    }

    private async Task<Dictionary<Guid, List<OrderMemento>>> LoadAllAsync()
    {
      if (!File.Exists(_filePath))
        return new Dictionary<Guid, List<OrderMemento>>();
      var json = await File.ReadAllTextAsync(_filePath);
      return JsonSerializer.Deserialize<Dictionary<Guid, List<OrderMemento>>>(json) ?? new();
    }

    private async Task SaveAllAsync(Dictionary<Guid, List<OrderMemento>> histories)
    {
      var json = JsonSerializer.Serialize(histories);
      await File.WriteAllTextAsync(_filePath, json);
    }
  }
}
