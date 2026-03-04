# AquaStore

Веб-приложение интернет-магазина (AquaStore). Проект разделён на **бэкенд** (REST API) и **фронтенд** (MVC с Razor), работающие как отдельные приложения.

---

## Содержание

- [Общая архитектура](#общая-архитектура)
- [Бэкенд](#бэкенд)
- [Фронтенд](#фронтенд)
- [Интеграция бэкенда и фронтенда](#интеграция-бэкенда-и-фронтенда)
- [Запуск проекта](#запуск-проекта)

---

## Общая архитектура

```
┌─────────────────────────────────────────────────────────────────┐
│                        Фронтенд (MVC)                            │
│  src/frontend/  — ASP.NET Core MVC, Razor, Cookie Auth           │
│  Порт по умолчанию: 5001 (http) / 7186 (https)                   │
|  Порт при использовании Docker: 3000                             |
└───────────────────────────────┬─────────────────────────────────┘
                                 │ HTTP (REST API)
                                 │ Bearer JWT в заголовках
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                        Бэкенд (API)                             │
│  src/backend/   — ASP.NET Core Web API, JWT, Swagger            │
│  Порт по умолчанию: 5251 (http) / 7040 (https)                  │
|  Порт при использовании Docker: 5000                            |
└───────────────────────────────┬─────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│  MySQL (Pomelo.EntityFrameworkCore.MySql)                        │
└─────────────────────────────────────────────────────────────────┘
```

- **Бэкенд** — единственный источник правды: REST API, бизнес-логика, доступ к БД.
- **Фронтенд** — UI: страницы, формы, вызовы API через `HttpClient` и единый `ApiService`.

---

## Бэкенд

Расположение: **`src/backend/`**.  
Решение: **`AquaStore.slnx`**.

Архитектура бэкенда построена по принципам **Clean Architecture** и **CQRS** (MediatR).

### Слои и проекты

| Слой | Проект | Назначение |
|------|--------|------------|
| **API (хост)** | `AquaStore.Api` | Точка входа: контроллеры, JWT, Swagger, CORS, глобальная обработка ошибок. Не содержит бизнес-логики. |
| **Application** | `AquaStore.Application` | Сценарии использования: команды и запросы (CQRS), валидация (FluentValidation), оркестрация. |
| **Domain** | `AquaStore.Domain` | Ядро: сущности (Product, Order, Cart, User, Category, Brand, Review и др.), value-объекты (Email, Slug, Money, Address), интерфейсы репозиториев. |
| **Infrastructure** | `AquaStore.Infrastructure` | Реализация персистентности: EF Core, MySQL (Pomelo), репозитории, `AquaStoreDbContext`. |
| **Contracts** | `AquaStore.Contracts` | Контракты API: DTO запросов/ответов, `ApiResponse<T>`, `PagedResponse<T>`. Без зависимостей от других проектов. |
| **Common** | `Common.Application`, `Common.Domain`, `Common.Infrastructure` | Общая инфраструктура: MediatR, FluentValidation, `IQuery`/`ICommand`, Result/Error, JWT, кэш, доменные события. |

### Поток зависимостей (Clean Architecture)

- **Domain** (AquaStore.Domain + Common.Domain) — центр, не зависит от остальных проектов приложения.
- **Application** зависит от Domain и Contracts; оркестрирует сценарии.
- **Infrastructure** реализует интерфейсы репозиториев из Domain, зависит от Application (регистрация в DI).
- **API** зависит от Application, Infrastructure и Contracts; контроллеры только отправляют запросы через MediatR и возвращают `ApiResponse`/`Result`.

```
Api ──► Application ──► Domain
 │            ▲
 └──► Infrastructure ──┘
```

### Паттерны и технологии

- **CQRS**: команды и запросы через MediatR (`ICommand`/`IQuery`, обработчики в Application).
- **Вертикальные срезы**: фичи по доменам (Products, Cart, Orders, Auth, Categories, Brands, Users, Reviews) — свои Commands/Queries/Validators.
- **Стек**: .NET 10, ASP.NET Core Web API, Entity Framework Core 9, Pomelo (MySQL), MediatR, FluentValidation, AutoMapper, JWT (Microsoft.AspNetCore.Authentication.JwtBearer), Swagger (Swashbuckle).

### Конфигурация API

Основные настройки в `AquaStore.Api/appsettings.json` (и переменные окружения):

- **ConnectionStrings:DefaultConnection** — строка подключения к MySQL.
- **JwtSettings** — секрет, Issuer, Audience, время жизни Access/Refresh токенов.
- **Cors:AllowedOrigins** — разрешённые источники для CORS (при необходимости добавить URL фронтенда).
- **EmailSettings** — SMTP для писем (подтверждение регистрации и т.д.).

Миграции EF применяются при старте приложения (`Database.Migrate()` в `Program.cs`).

---

## Фронтенд

Расположение: **`src/frontend/`**.  
Решение: **`frontend.slnx`** (один проект).

Фронтенд — отдельное **ASP.NET Core MVC** приложение с **Razor Views** (не Blazor). Все запросы к данным идут через REST API бэкенда.

### Структура

| Каталог/элемент | Назначение |
|-----------------|------------|
| **Controllers** | `HomeController`, `AccountController`, `ProductsController`, `CartController`, `AdminController` — маршрутизация и вызов сервисов. |
| **Views** | Razor-разметка: Home (Index, Catalog), Account (Login, Profile, ConfirmRegistration), Products (Details), Cart (Index, Checkout, Receipt), Admin, Shared (_Layout, навигация, скрипты). |
| **ViewModels** | Модели представлений, в т.ч. отражение DTO API (например `ProductViewModel`, `CartViewModel`, `AuthResponseViewModel`). |
| **Services** | `IApiService` / `ApiService` — все HTTP-вызовы к бэкенду (один слой общения с API). |
| **ViewComponents** | Переиспользуемые блоки (например, меню навигации по категориям/брендам). |
| **wwwroot** | Статика: CSS, JS, библиотеки (jQuery, Bootstrap, jquery-validation), изображения. |

### Аутентификация

- **Фронтенд**: Cookie-аутентификация (`CookieAuthenticationDefaults`), путь входа `/account/login`, cookie `aquastore.auth`, HttpOnly, SameSite Lax.
- **Бэкенд**: ожидает JWT в заголовке `Authorization: Bearer <token>`.
- **Связка**: после успешного входа через API фронтенд сохраняет JWT в cookie `access_token`; `ApiService` при каждом запросе к API подставляет этот токен в заголовок (через `IHttpContextAccessor`).

Итого: сессия на фронте — cookie; API — без состояния, только JWT.

### Конфигурация фронтенда

В `appsettings.json` (и переменная окружения):

- **ApiSettings:BaseUrl** — базовый URL бэкенда (по умолчанию `http://localhost:5000`). Для локальной разработки должен совпадать с тем, на каком порту реально запущен API (например `http://localhost:5251`).
- **ApiSettings:TimeoutSeconds** — таймаут HTTP-клиента.

Переменная **API_BASE_URL** переопределяет `BaseUrl` (удобно для Docker/окружений).

### Стек

- ASP.NET Core MVC, Razor, .NET 10  
- jQuery, Bootstrap, jquery-validation-unobtrusive  
- Data Protection (ключи в `DataProtection-Keys`) для защиты cookie  

---

## Интеграция бэкенда и фронтенда

1. **Протокол**: REST API. Ответы в едином формате (например `ApiResponse<T>` с полями `Success`, `Data`, `Message`, `Errors`).
2. **Вызовы**: фронтенд использует один `HttpClient` с именем `"ApiClient"` (BaseAddress из `ApiSettings`), все вызовы — через `IApiService`/`ApiService`.
3. **Авторизация**: пользователь логинится через фронт → запрос на `/api/auth/login` → бэкенд возвращает JWT → фронт сохраняет в cookie и подставляет как Bearer в заголовки запросов к API.
4. **Порядок запуска**: сначала запускается API (бэкенд), затем фронтенд; в настройках фронтенда `ApiSettings:BaseUrl` должен указывать на работающий API.

---

## Запуск проекта

### Требования

- .NET 10 SDK  
- MySQL (для бэкенда)  
- Настроенная строка подключения в `src/backend/AquaStore.Api/appsettings.json` (или через переменные окружения)

### Бэкенд

```bash
cd src/backend
dotnet run --project AquaStore.Api
```

API по умолчанию: http://localhost:5251. Swagger UI в Development — по корневому пути (или `/swagger`), в Production — при `ENABLE_SWAGGER=true`.

### Фронтенд

Убедитесь, что в `src/frontend/appsettings.json` (или в `API_BASE_URL`) указан URL запущенного API, например:

```json
"ApiSettings": {
  "BaseUrl": "http://localhost:5251",
  "TimeoutSeconds": 30
}
```

Затем:

```bash
cd src/frontend
dotnet run
```

Фронтенд по умолчанию: http://localhost:5001.

### Запуск с использованием Docker

- В каталоге **src/backend** выполнить команду в консоли `docker compose up -d`
- Порт api: 5000; порт frontend: 3000