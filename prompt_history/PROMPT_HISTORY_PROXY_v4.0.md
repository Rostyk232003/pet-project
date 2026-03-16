// PROMPT_HISTORY_PROXY_v4.0.md


## PROMPT v4.1.1 [Активна] — Конфігурація TTL через appsettings.json

**Дата:** 14.03.2026

### Опис змін (v4.1.1)
- TTL кешу для ProductRepositoryProxy тепер конфігурується через appsettings.json (секція CacheSettings:ProductCacheTTL).
- Додано файл Helpers/Constants.cs з константами для ключів конфігурації та дефолтного TTL.
- DI-реєстрація ProductRepositoryProxy та JsonProductRepository змінена на AddSingleton (кеш зберігається між запитами).
- TTL зчитується у Program.cs напряму через IConfiguration, fallback — дефолтне значення з Helpers/Constants.cs.
- Всі зміни позначені inline-міткою // PROMPT v4.1.1: Configurable TTL.

### Які помилки виникали та як виправляли
- **CS1529**: using LiteWebApp.Helpers; був розміщений не на початку файлу Program.cs. Виправлено — перенесено всі using-и на верхівку файлу, зайві using-и видалено.
- **CS0122**: "Constants" недоступен из-за его уровня защиты. Причина — зайвий using LiteWebApp.Helpers; у середині файлу, який спричиняв конфлікт. Виправлено — залишено лише один using на початку.

### Змінені/створені файли
- LiteWebApp/appsettings.json — додано секцію CacheSettings.
- LiteWebApp/Helpers/Constants.cs — створено файл з константами.
- LiteWebApp/Program.cs — DI, зчитування TTL, Singleton, виправлення using.
- LiteWebApp/Infrastructure/Data/ProductRepositoryProxy.cs — оновлено конструктор, inline-мітка.

### Edge-кейси
- Якщо в appsettings.json відсутній або некоректний CacheSettings:ProductCacheTTL (null, 0, від’ємне, не число) — використовується дефолтне значення з Helpers/Constants.cs (5 хвилин).

### Статус: [Активна]
---

[Активна]
---

## PROMPT v4.1 [Активна] — Фінальний рефакторинг Proxy

**Дата:** 10.03.2026

### Підсумки реалізації патерну Proxy (v4.0 → v4.1)

**Що виконує патерн:**
- Додає шар кешування для продуктів у пам’яті (RAM) із TTL (час життя кешу).
- Зменшує кількість звернень до файлової системи (JSON), підвищує швидкодію при частих запитах.
- Прозоро для контролерів та UI: клієнт працює через інтерфейс, не знаючи про кешування.
- Кеш автоматично скидається при зміні даних (додавання, оновлення, видалення продукту).

**Які файли створено та змінено:**
- Створено: Infrastructure/Data/ProductRepositoryProxy.cs — реалізація Proxy з TTL-кешем.
- Змінено: Program.cs — DI-реєстрація Proxy для IProductRepository.
- Перевірено/залучено: Core/Interfaces/IProductRepository.cs, Infrastructure/Data/JsonProductRepository.cs, Controllers (без змін).

**Які файли беруть участь у патерні:**
- IProductRepository — єдиний інтерфейс для Proxy та реального репозиторію.
- JsonProductRepository — сервіс, що працює з JSON-файлом.
- ProductRepositoryProxy — замісник, що додає кешування.
- Program.cs — налаштовує DI, щоб усі клієнти працювали через Proxy.
- Контролери — клієнти, які працюють через інтерфейс (не змінювались).

**Фінальний рефакторинг:**
- Весь код приведено до явних типів замість var для прозорості та підтримки стандартів стилю.
- Логіка кешування, TTL, потокобезпечність — реалізовано згідно best practices.

---

---

**Дата:** 10.03.2026
**Фіча:** Реалізація патерну Proxy для кешування продуктів

## Опис

Proxy обгортає ProductRepository, зберігає дані у RAM, при першому запиті читає з JSON, далі повертає з кешу. Додано TTL для актуальності даних.

## PROMPT на UML-діаграму класів (PlantUML)

> PROMPT v4.0: UML Proxy кешування продуктів
> Створити UML-діаграму класів для патерну Proxy кешування продуктів:
> - ProductRepositoryProxy : реалізує IProductRepository, містить посилання на JsonProductRepository, має поле cache та cacheTTL.
> - IProductRepository : інтерфейс.
> - JsonProductRepository : реалізація IProductRepository.
> - cache : in-memory список продуктів.
> - cacheTTL : час життя кешу.
> - Client : використовує IProductRepository.

## ToT-аналіз

- V1: Простий кеш
- V2: Кеш з TTL (обрано)
- V3: Observable Cache

**Критика:**
- V1 — простий, але не актуальний
- V2 — баланс швидкості й актуальності
- V3 — складний, але гнучкий

---

**Фінальний промпт:**

> PROMPT v4.0: Реалізувати Proxy для кешування продуктів з TTL, створити UML-діаграму класів (PlantUML) із Client, Proxy, Service, інтерфейсом та кешем.

[Активна]
---

## Деталізований ToT-аналіз варіантів (10.03.2026)

**V1. Простий Proxy з кешем (без TTL)**
- ProductRepositoryProxy з in-memory кешем, кеш оновлюється лише при перезапуску застосунку.
- Складність: 3/10
- Ризики: Дані можуть бути неактуальні при зміні JSON-файлу поза межами програми.

**V2. Proxy з кешем та TTL (АКТИВНИЙ ВАРІАНТ)**
- ProductRepositoryProxy з in-memory кешем і часом життя (TTL), після закінчення TTL кеш оновлюється з репозиторію.
- Складність: 5/10
- Ризики: Потрібно правильно обрати TTL, можливі короткі періоди неактуальних даних.

**V3. Proxy з інвалідацією кешу при зміні**
- Proxy реагує на зміну продуктів (додавання/видалення/редагування) — очищає кеш при кожній зміні.
- Складність: 6/10
- Ризики: Потрібно контролювати всі точки зміни, складніше підтримувати.

---

## Які файли будуть створені при реалізації (v4.0)

- Core/Interfaces/IProductRepository.cs (оновлення, якщо потрібно)
- Infrastructure/Data/JsonProductRepository.cs (без змін, або мінімальні)
- Infrastructure/Data/ProductRepositoryProxy.cs (НОВИЙ ФАЙЛ — реалізація Proxy)
- Оновлення DI у Program.cs (додається реєстрація Proxy)
- Оновлення unit-тестів (за потреби)

---

