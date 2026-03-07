# Copilot Instructions for LiteWebApp
---
name: copilot-instructions
description: Інструкції для AI Copilot щодо архітектури, патернів, протоколів ToT та стандартів LiteWebApp.
version: 1.2
last_updated: 2026-03-07
---

## Архітектура та основні компоненти
- Проект побудований на ASP.NET Core MVC.
- Основні компоненти:
  - Controllers: логіка обробки HTTP-запитів ([Controllers/]).
  - Core/Entities: сутності домену (User, Product, Order, Category,OrderMemento).
  - Core/Interfaces: інтерфейсидля роботи з даними.
  - Infrastructure/Data: реалізації репозиторіїв (зберігання в JSON).
  - Core/Discountss: патерн factory method.
  - Infrastructure/Handlers: патерн "ланцюжок обов'язків" для перевірки прав.
  - ViewModels: моделі для передачі даних у View.
  - Views: Razor-сторінки для UI.
  - Папка Helpers/: Усі readonly змінні, константи, методи валідації (IsValid) та допоміжні статичні класи мають бути винесені сюди. Основні класи (Builder, Controller) не повинні містити логіку перевірок або статичні списки.
    
    **Приклад для Helpers:**
    
    ```csharp
    // PROMPT v1.0: Створення класу валідації
    namespace Helpers {
        public static class ProductValidator {
            public static bool IsValidName(string name) => !string.IsNullOrWhiteSpace(name) && name.Length < 100;
        }
        public static class Constants {
            public const int MaxProductNameLength = 100;
        }
    }
    ```
  ## Оновлення
  - 2026-03-07: Додано YAML-метадані, секцію оновлення, приклад для Helpers.
  
## Inline‑мітки у коді (формат)
- Маркування: Кожен метод/блок позначати коментарем: // PROMPT vX.Y.Z: Назва фічі.

**Нові файли:** перший рядок файлу:

```csharp
// PROMPT v1.1: Створення файлу [Назва]
```

**Нові методи/класи:** перед оголошенням:

```csharp
// PROMPT v1.1: SetAccountActive
public async Task SetAccountActiveAsync(Guid accountId, bool isActive, CancellationToken cancellationToken) { ... }
```

**Виправлення/патчі:** позначати фрагмент з підверсією:

```csharp
// PROMPT v1.1.1: Fix Order Lookup
// змінений фрагмент коду...
```

## Патерни та конвенції
- Chain of Responsibility: для перевірки прав доступу (див. Infrastructure/Handlers).
- Memento: для збереження та відновлення станів (історія статусів замовлень).
- Репозиторії: всі дані зберігаються у JSON-файлах (Infrastructure/Storage).
- ViewModels використовуються для передачі даних між контролерами та View.
- Дії адміністратора відокремлені в AdminController та Admin Views.
- Builder: для побудови аналітичних звітів за шаблонами ReportTemplate.cs файлу.
- Factory Method: для створення об'єктів з різними розрахунками знижок (Core/Discounts).

## Протокол роботи з фічами (Major/Minor/ToT)
  Перед написанням будь-якого коду, ШІ повинен дотримуватися наступного алгоритму:
  - Генерація варіантів (Major Version): Запропонувати кілька концептуальних підходів до реалізації нової фічі.
  - Критика та Аналіз: Надати коротку критику кожного варіанту (плюси/мінуси).
  - Узгодження запису: Надати текст (Draft), який планується записати в історію промптів.
  - Вибір та Промпт: Після вибору користувачем варіанту, згенерувати промпт на реалізацію.
  -Команда до дії: Реалізовувати код ТІЛЬКИ ПІСЛЯ явного підтвердження користувачем команди "Виконуємо".
  - **Кожна відповідь**, що вносить зміни у код, повинна містити секцію **## Зміни у файлах** з переліком файлів і версією PROMPT.
## Версіонування та Документування (ToT)
  - Мажорні версії (v1.0, v2.0...): Створення нової фічі. Кожна мажорна фіча має ОКРЕМИЙ ФАЙЛ історії промптів зберігати їх у папці prompt_history(наприклад, PROMPT_HISTORY_v2.0.md для нового файлу мінятиметься версія у назві, щоб всі файли мали різні назви).
  - Мінорні версії (v1.1, v1.2...): Виправлення помилок, логічні правки відносно мажорної версії зберігаються разом із мажорною версією.
  - Глибина (v1.1.1...): Вирішення локальних проблем всередині мінорної правки.
  - Статуси: Кожна версія у файлі історії маркується як [Активна] або [Архів].
  - Фінальний Рефакторинг: Після завершення фічі провести рефакторинг та записати Підсумковий Фінальний Промпт, який резюмує весь шлях вирішення проблеми.
## Робота з даними
- Дані зберігаються у JSON-файлах: categories.json, products.json, orders.json, users.json, order_status_history.json.
- Репозиторії реалізують інтерфейси з Core/Interfaces.

## Запуск, збірка та налагодження
- Для запуску: використовуйте стандартні команди .NET (dotnet build, dotnet run).
- Налаштування середовища: appsettings.json, appsettings.Development.json.
- Для налагодження використовуйте launchSettings.json.

## Що AI не повинна робити автоматично

- **Не об'єднувати і не коммітити** згенерований код без:
  - додавання заголовків PROMPT у файлах/фрагментах;
  - оновлення відповідного `prompt_history` файлу;
  - отримання явного людського схвалення для вибраного варіанту ToT.

## Важливі файли
- Program.cs: точка входу.
- LiteWebApp.csproj: конфігурація проекту.
- Infrastructure/Handlers: реалізація Chain of Responsibility.
- Infrastructure/Data: репозиторії для роботи з JSON.
- Views/Shared/_Layout.cshtml: основний шаблон UI.

## Інтеграції та залежності
- Зовнішніх API чи баз даних немає — все локально через JSON.
- Використовується Bootstrap, jQuery для UI (wwwroot/lib).

## Приклади
- Для перевірки прав на дію:
  ```csharp
  var handler = new AuthenticationCheckHandler();
  handler.SetNext(new OrderPermissionHandler())
         .SetNext(new AdminCRUDHandler());
  handler.Handle(user, action);
  ```

- Для відновлення стану (Memento):
  ```csharp
  // PROMPT v1.0: Restore State
  public void Restore(OrderMemento memento)
  {
      this.Status = memento.Status;
  }
  ```
---

> Якщо щось незрозуміло або потрібно деталізувати — уточніть, і я доопрацюю інструкцію.
