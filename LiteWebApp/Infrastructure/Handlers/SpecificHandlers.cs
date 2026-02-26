using LiteWebApp.Core.Entities;

namespace LiteWebApp.Infrastructure.Handlers
{
    // 1. Перевірка на авторизацію (для гостей)
    public class AuthenticationCheckHandler : BaseHandler
    {
        public override bool Handle(User? user, string action)
        {
            if (user == null && action == "CHECKOUT")
            {
                return false; // Гість не може купувати
            }
            return base.Handle(user, action);
        }
    }

    // 2. Перевірка прав на замовлення (User/Admin)
    public class OrderPermissionHandler : BaseHandler
    {
        public override bool Handle(User? user, string action)
        {
            if (action == "CHECKOUT")
            {
                if (user?.Role == "User" || user?.Role == "Admin")
                {
                    return base.Handle(user, action);
                }
                return false;
            }
            return base.Handle(user, action);
        }
    }

    // 3. Перевірка прав на CRUD (Тільки Admin)
    public class AdminCRUDHandler : BaseHandler
    {
        public override bool Handle(User? user, string action)
        {
            string[] adminActions = { "PRODUCT_CREATE", "PRODUCT_EDIT", "PRODUCT_DELETE" };

            if (adminActions.Contains(action))
            {
                if (user?.Role == "Admin")
                {
                    return base.Handle(user, action);
                }
                return false;
            }
            return base.Handle(user, action);
        }
    }
}
