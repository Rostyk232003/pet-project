using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LiteWebApp.Core.Interfaces
{
    // PROMPT v1.0: Інтерфейс для зберігання історії знімків
    public interface IOrderHistoryRepository
    {
        Task SaveSnapshotAsync(Guid orderId, Entities.OrderMemento memento);
        Task<IEnumerable<Entities.OrderMemento>> GetHistoryAsync(Guid orderId);
    }
}
