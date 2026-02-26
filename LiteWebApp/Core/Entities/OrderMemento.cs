using System;

namespace LiteWebApp.Core.Entities
{
  // PROMPT v1.0: OrderMemento для зберігання знімків статусу
  public class OrderMemento
  {
    public string? Status { get; set; }
    public DateTime ChangedAt { get; set; }
    public string? ChangedBy { get; set; }
    public string? Comment { get; set; }

    public OrderMemento() { }

    public OrderMemento(string status, string admin, string comment)
    {
      Status = status;
      ChangedAt = DateTime.Now;
      ChangedBy = admin;
      Comment = comment;
    }
  }
}
