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
- MySQL 8.0+ (для локальной разработки) или Docker  
- Entity Framework Core Tools (для работы с миграциями)

### Установка EF Core Tools

Если у вас ещё не установлены инструменты Entity Framework Core:

```bash
dotnet tool install --global dotnet-ef
```

Проверка установки:

```bash
dotnet ef --version
```

---

## Способы запуска

### Вариант 1: Локальный запуск (без Docker)

#### 1. Настройка базы данных MySQL

Убедитесь, что MySQL запущен локально. Создайте базу данных:

```sql
CREATE DATABASE aquastore CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
```

#### 2. Настройка строки подключения

Отредактируйте `src/backend/AquaStore.Api/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=aquastore;User=root;Password=ваш_пароль;Port=3306;"
}
```

Или используйте переменные окружения:

```bash
# Windows (PowerShell)
$env:ConnectionStrings__DefaultConnection="Server=localhost;Database=aquastore;User=root;Password=ваш_пароль;Port=3306;"

# Linux/macOS
export ConnectionStrings__DefaultConnection="Server=localhost;Database=aquastore;User=root;Password=ваш_пароль;Port=3306;"
```

#### 3. Применение миграций

Перейдите в каталог бэкенда и примените миграции:

```bash
cd src/backend
dotnet ef database update --project AquaStore.Infrastructure --startup-project AquaStore.Api
```

Или миграции применятся автоматически при первом запуске API (в `Program.cs` настроено `Database.Migrate()`).

#### 4. Запуск бэкенда

```bash
cd src/backend
dotnet run --project AquaStore.Api
```

API будет доступен по адресу:
- HTTP: http://localhost:5251
- HTTPS: https://localhost:7040
- Swagger UI: http://localhost:5251/swagger (в режиме Development)

#### 5. Настройка фронтенда

Убедитесь, что в `src/frontend/appsettings.json` указан правильный URL API:

```json
"ApiSettings": {
  "BaseUrl": "http://localhost:5251",
  "TimeoutSeconds": 30
}
```

#### 6. Запуск фронтенда

```bash
cd src/frontend
dotnet run
```

Фронтенд будет доступен по адресу:
- HTTP: http://localhost:5001
- HTTPS: https://localhost:7186

---

### Вариант 2: Запуск с использованием Docker

Docker Compose автоматически настроит MySQL, применит миграции и запустит API и фронтенд.

#### 1. Настройка переменных окружения (опционально)

Создайте файл `.env` в каталоге `src/backend/`:

```env
# База данных
DB_NAME=aquastore
DB_USER=aquastore_user
DB_PASSWORD=sqlPassword123
DB_PORT=3307

# JWT
JWT_SECRET=your-super-secret-key-that-should-be-at-least-32-characters-long-change-in-production
JWT_ISSUER=AquaStore
JWT_AUDIENCE=AquaStore.Client

# Порты
API_PORT=5000
FRONTEND_PORT=3000

# Email (опционально)
EMAIL_HOST=smtp.gmail.com
EMAIL_PORT=587
EMAIL_ENABLE_SSL=true
EMAIL_USERNAME=your-email@gmail.com
EMAIL_PASSWORD=your-app-password
EMAIL_FROM_EMAIL=your-email@gmail.com
EMAIL_FROM_NAME=AquaStore

# Окружение
ASPNETCORE_ENVIRONMENT=Development
```

#### 2. Запуск контейнеров

```bash
cd src/backend
docker compose up -d
```

Это запустит:
- **MySQL** на порту 3307 (внешний) / 3306 (внутренний)
- **API** на порту 5000
- **Frontend** на порту 3000

#### 3. Проверка статуса

```bash
docker compose ps
```

#### 4. Просмотр логов

```bash
# Все сервисы
docker compose logs -f

# Только API
docker compose logs -f api

# Только Frontend
docker compose logs -f frontend
```

#### 5. Остановка контейнеров

```bash
docker compose down
```

Для удаления данных (включая БД):

```bash
docker compose down -v
```

---

## Работа с миграциями

### Создание новой миграции

После изменения моделей в `AquaStore.Domain`:

```bash
cd src/backend
dotnet ef migrations add ИмяМиграции --project AquaStore.Infrastructure --startup-project AquaStore.Api
```

Пример:

```bash
dotnet ef migrations add AddProductReviews --project AquaStore.Infrastructure --startup-project AquaStore.Api
```

### Применение миграций

```bash
cd src/backend
dotnet ef database update --project AquaStore.Infrastructure --startup-project AquaStore.Api
```

### Откат миграции

Откат к предыдущей миграции:

```bash
cd src/backend
dotnet ef database update ИмяПредыдущейМиграции --project AquaStore.Infrastructure --startup-project AquaStore.Api
```

Откат всех миграций:

```bash
cd src/backend
dotnet ef database update 0 --project AquaStore.Infrastructure --startup-project AquaStore.Api
```

### Удаление последней миграции

```bash
cd src/backend
dotnet ef migrations remove --project AquaStore.Infrastructure --startup-project AquaStore.Api
```

### Просмотр списка миграций

```bash
cd src/backend
dotnet ef migrations list --project AquaStore.Infrastructure --startup-project AquaStore.Api
```

### Генерация SQL-скрипта миграции

```bash
cd src/backend
dotnet ef migrations script --project AquaStore.Infrastructure --startup-project AquaStore.Api --output migration.sql
```

Для конкретного диапазона миграций:

```bash
dotnet ef migrations script ОтМиграции ДоМиграции --project AquaStore.Infrastructure --startup-project AquaStore.Api
```

---

## Сборка проекта

### Сборка бэкенда

```bash
cd src/backend
dotnet build AquaStore.Api
```

### Сборка фронтенда

```bash
cd src/frontend
dotnet build
```

### Публикация для Production

Бэкенд:

```bash
cd src/backend
dotnet publish AquaStore.Api -c Release -o ./publish
```

Фронтенд:

```bash
cd src/frontend
dotnet publish -c Release -o ./publish
```

---

## Тестирование API

### Swagger UI

В режиме Development Swagger доступен по адресу: http://localhost:5251/swagger

### Примеры запросов

#### Регистрация пользователя

```bash
curl -X POST http://localhost:5251/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "firstName": "Иван",
    "lastName": "Иванов"
  }'
```

#### Вход в систему

```bash
curl -X POST http://localhost:5251/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!"
  }'
```

#### Получение списка товаров

```bash
curl -X GET "http://localhost:5251/api/products?pageNumber=1&pageSize=10"
```

---

## Устранение неполадок

### Ошибка подключения к MySQL

Проверьте:
- MySQL запущен: `mysql -u root -p`
- Строка подключения в `appsettings.json` корректна
- Порт MySQL (по умолчанию 3306)

### Ошибки миграций

Если миграции не применяются:

```bash
# Удалите базу данных
DROP DATABASE aquastore;
CREATE DATABASE aquastore CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

# Примените миграции заново
cd src/backend
dotnet ef database update --project AquaStore.Infrastructure --startup-project AquaStore.Api
```

### Фронтенд не может подключиться к API

Проверьте:
- API запущен и доступен
- `ApiSettings:BaseUrl` в `src/frontend/appsettings.json` указывает на правильный адрес
- CORS настроен в `src/backend/AquaStore.Api/appsettings.json` (добавьте URL фронтенда в `Cors:AllowedOrigins`)

### Docker: контейнеры не запускаются

```bash
# Просмотр логов
docker compose logs

# Пересоздание контейнеров
docker compose down -v
docker compose up -d --build
```

---

## Полезные команды

### Очистка проекта

```bash
# Бэкенд
cd src/backend
dotnet clean

# Фронтенд
cd src/frontend
dotnet clean
```

### Восстановление зависимостей

```bash
# Бэкенд
cd src/backend
dotnet restore

# Фронтенд
cd src/frontend
dotnet restore
```

### Проверка версии .NET

```bash
dotnet --version
```

### Список установленных инструментов

```bash
dotnet tool list --global
```