using LiteWebApp.Core.Entities;

namespace LiteWebApp.Infrastructure.Handlers
{
    public interface IHandler
    {
        IHandler SetNext(IHandler handler);
        bool Handle(User? user, string action);
    }
}
