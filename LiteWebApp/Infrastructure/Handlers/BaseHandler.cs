using LiteWebApp.Core.Entities;

namespace LiteWebApp.Infrastructure.Handlers
{
    public abstract class BaseHandler : IHandler
    {
        private IHandler? _nextHandler;

        public IHandler SetNext(IHandler handler)
        {
            _nextHandler = handler;
            return handler;
        }

        public virtual bool Handle(User? user, string action)
        {
            if (_nextHandler != null)
            {
                return _nextHandler.Handle(user, action);
            }
            return true; // Якщо ланцюжок закінчився і ніхто не заперечив — доступ дозволено
        }
    }
}
