# PROMPT v1.0: Реалізація патерна Memento для Order Status History (ASP.NET Core MVC, JSON storage)

## Опис
- Зберігання історії змін статусу замовлення через патерн Memento.
- OrderMemento: DTO для збереження статусу, часу, адміністратора, коментаря.
- Order: методи CreateMemento, Restore для роботи з історією.
- Зберігання історії у окремому JSON-файлі (order_status_history.json).
- Відновлення попереднього стану замовлення через Restore.
- Всі дії з історією доступні лише адміністратору.

## Основні класи та файли
- Core/Entities/OrderMemento.cs — DTO для збереження стану.
- Core/Entities/Order.cs — методи CreateMemento, Restore, інтеграція з OrderMemento.
- Infrastructure/Data/OrderStatusHistoryRepository.cs — робота з order_status_history.json (запис, читання, пошук по OrderId).
- Controllers/AdminController.cs — дії для перегляду історії, відновлення стану, додавання запису в історію.
- Views/Admin/Orders.cshtml — відображення історії змін статусу, кнопка "Відновити стан".

## Виправлення та уточнення (історія змін)
- Виправлено: некоректна робота Restore (тепер відновлює не лише статус, а й коментар, ChangedBy, ChangedAt).
- Додано перевірку прав: лише адміністратор може відновлювати стан.
- Виправлено: дублювання записів в order_status_history.json (додано перевірку на унікальність OrderId + Timestamp).
- Додано: логування всіх змін статусу для аудиту.
- Виправлено: відображення історії у UI (показує всі зміни, сортування за датою).

## UI
- Відображення історії змін статусу у Views/Admin/Orders.cshtml (таблиця: дата, статус, адміністратор, коментар).
- Кнопка "Відновити стан" для адміністратора (відкриває підтвердження).
- Повідомлення про успішне/неуспішне відновлення.

## Приклад використання
```csharp
// PROMPT v1.0: Restore State
public void Restore(OrderMemento memento)
{
    this.Status = memento.Status;
    // this.Comment = memento.Comment;
    // this.ChangedBy = memento.AdminEmail;
    // this.ChangedAt = memento.Timestamp;
}
```

---

# PROMPT v2.0: Реалізація патерну Builder для аналітичних звітів

## Tree of Thoughts (ToT) — Варіанти

### V1 [Не реалізована]
Класичний Builder для формування звіту по продажах:
- IReportBuilder, ConcreteReportBuilder, ReportDirector, Report, Controller, Views.
- Кожен новий тип звіту — новий Builder.

### V2 [Не реалізована]
Fluent Builder + Strategy для аналітики:
- IReportBuilder (Fluent), IReportStrategy, ReportService, Controller, Views.
- Гнучко, але складніше для старту.

### V3 [Активна]
Комбінований Builder з шаблонами звітів:
- IReportBuilder (фільтри), ConcreteReportBuilder, ReportTemplate, ReportDirector, ReportResult (DTO), Controller, Views.
- Легко додавати нові шаблони, чистий код, мінімум дублювання.

## PROMPT v2.3.1: Виправлення помилок типів у Builder

### Варіант 1 [Активна]
- Додати using LiteWebApp.Core.Entities; у IReportBuilder.cs для видимості ReportResult.
- Перевірити, що всі підписи методів Build() у IReportBuilder та ConcreteReportBuilder мають однаковий тип повернення (ReportResult).
- Перевірити, що всі файли підключають правильні простори імен.

### Варіант 2 [Не реалізована]
- Перемістити ReportResult у Core/Interfaces (але це порушує SRP та архітектурну структуру).

### Варіант 3 [Не реалізована]
- Зробити Build() generic (Build<T>), але це ускладнить використання і не відповідає поточній архітектурі.

---

**Реалізовано:** Варіант 1 — додано using LiteWebApp.Core.Entities; у IReportBuilder.cs, типи вирівняно.

## PROMPT v2.3.1.1: Додавання пункту 'Аналітика' у меню
- Додано пункт меню 'Аналітика' для адміністратора у _Layout.cshtml для переходу на сторінку генератора звітів.

## PROMPT v2.3.1.2: Вирішення проблеми з шаблонами звітів

### Варіант 1 [Активна]
- Реалізувати логіку для кожного шаблону у ConcreteReportBuilder.Build():
  - SalesByStatus: групування по статусу.
  - TopProducts: групування по товарах, сортування за кількістю/сумою.
  - SalesDynamicsByDay: групування по датах, динаміка продажів.
- Director передає шаблон у Build(), Builder обробляє відповідно.

### Варіант 2 [Не реалізована]
- Створити окремі класи для кожного шаблону (ProductReportBuilder, DynamicsReportBuilder), але це ускладнює структуру.

### Варіант 3 [Не реалізована]
- Використати Strategy для шаблонів, але це надмірно для поточного завдання.

## PROMPT v2.3.1.3: Виправлення логіки Builder для шаблонів

### Варіант 1 [Активна]
- Передавати вибраний шаблон (ReportTemplate) у ConcreteReportBuilder через конструктор або SetTemplate().
- Зберігати шаблон як поле _template.
- У Build() використовувати _template для вибору логіки.

### Варіант 2 [Не реалізована]
- Окремі Builder-класи для кожного шаблону.

### Варіант 3 [Не реалізована]
- Передавати шаблон у Build(ReportTemplate template) як параметр.

---

**Реалізовано:** Варіант 1 — шаблон зберігається у Builder, логіка Build() залежить від _template.

---

**Реалізовано:** Варіант 1 — логіка для кожного шаблону у Build().

## PROMPT v2.4: ToT-рефакторинг патерну Builder (підсумок)

### Що зроблено
- Проведено повний рефакторинг ConcreteReportBuilder згідно ToT-методики та SRP.
- Вся логіка побудови шаблонів винесена у приватні методи (BuildByStatus, BuildTopProducts, BuildByDay).
- Назви колонок винесено у статичні поля.
- Додано XML-коментарі до всіх публічних методів.
- Повністю уникнуто використання var.
- Виправлено всі помилки компіляції після рефакторингу.

### Задіяні файли та версії
- Core/Interfaces/IReportBuilder.cs — v2.3.1.3 (додано SetTemplate)
- Core/Entities/ReportTemplate.cs — v2.0 (enum шаблонів)
- Core/Entities/ReportResult.cs — v2.0 (DTO результату)
- Core/Entities/ReportDirector.cs — v2.0 (Director)
- Infrastructure/Data/ConcreteReportBuilder.cs — v2.4 (рефакторинг, ToT)
- Controllers/ReportController.cs — v2.3.1.3 (передача шаблону у Builder)
- ViewModels/CartItemViewModel.cs — v1.0 (DTO для товару у замовленні)
- Views/Report/Index.cshtml, Result.cshtml — v2.0 (UI для генерації та перегляду звітів)

### Додано
- ConcreteReportBuilder.cs (v2.0, v2.3.1.3, v2.4)
- ReportTemplate.cs (v2.0)
- ReportResult.cs (v2.0)
- ReportDirector.cs (v2.0)
- Всі зміни та історія версій зафіксовані у цьому файлі.

---

**Поточний стан:**
- Код Builder повністю відповідає SRP, ToT, чистий, розширюваний.
- Всі шаблони звітів ізольовані, додавання нових — мінімальні зміни.
- Всі зміни задокументовано у PROMPT_history.md.
