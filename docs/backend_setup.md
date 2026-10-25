# Backend Setup & Deployment (ASP.NET Core .NET 10)

Инструкция по настройке, конфигурированию, запуску и тестированию REST API приложения Invoice Keeper.

## Требования

- **.NET 10 SDK** (для локальной разработки и тестирования)
- **Docker** и **Docker Compose**
- **PostgreSQL 15+** (если используется локальный экземпляр БД без Docker)

## Быстрый запуск

### 1. Запуск через Docker Compose (Рекомендуемый)

1. Скопируйте файл конфигурации окружения и заполните секреты:

```powershell
Copy-Item .env.example .env
```

2. Соберите и запустите контейнеры:

```powershell
docker compose up -d --build
```

Документация Swagger UI будет доступна по адресу:
- `http://localhost:8080/swagger`
- `http://localhost:8080/api/v1/docs` (автоматический редирект)

### 2. Локальный запуск без Docker (для разработки)

1. Запустите СУБД PostgreSQL в Docker:

```powershell
docker compose up -d db
```

2. Выполните запуск приложения через .NET CLI:

```powershell
dotnet run --project backend
```

---

## Конфигурация и Переменные Окружения

Сервис конфигурируется с помощью переменных окружения или файла `.env`.

| Переменная | Обязательна | Назначение |
|---|---|---|
| `POSTGRES_PASSWORD` | Да | Пароль администратора PostgreSQL |
| `ADMIN_USERNAME` | Да | Логин начального пользователя-администратора |
| `ADMIN_PASSWORD` | Да | Пароль начального пользователя-администратора |
| `JWT_SECRET` | Да (вне `Development`) | Ключ HMAC-SHA256 для подписи JWT (минимум 32 символа) |
| `BOT_SERVICE_TOKEN` | Да | Токен авторизации сервисов (Telegram-бота) через заголовок `X-Service-Token` |
| `CORS_ALLOWED_ORIGINS` | Нет | Список разрешённых CORS-источников через запятую |
| `HttpsRedirection__Enabled` | Нет | Установите в `false`, если TLS завершается на reverse proxy (Nginx/Traefik) |

При старте приложения администратор автоматически инициализируется в базе данных, если заданы `ADMIN_USERNAME` и `ADMIN_PASSWORD` (или секция `AdminSeed:*` в `appsettings.json`). Повторные запуски не создают дубликатов.

---

## Запуск Тестов

### Выполнение тестов

```powershell
# Запуск всех тестов (параметры подключения берутся из .env)
dotnet test backend/tests/InvoiceKeeper.Tests.csproj

# Явное переопределение параметров через переменные окружения
$env:POSTGRES_PASSWORD = "<пароль>"
dotnet test backend/tests/InvoiceKeeper.Tests.csproj
```

### Архитектура и особенности интеграционных тестов

- Интеграционные тесты автоматически создают изолированную временную базу данных вида `invoice_keeper_test_<guid>`, применяют Entity Framework Core миграции и удаляют её после завершения тестового прогона.
- Параметры подключения к СУБД считываются из переменных `POSTGRES_*` или автоматически загружаются из корневого `.env` файла.
- В рамках набора тестов `backend/tests/Api` поднимается тестовый хост (`WebApplicationFactory`) поверх временной БД и тестируется матрица прав и авторизации:
  - Выполняются реальные HTTP-запросы от лиц ролей `Admin`, `User`, `Service` и без авторизации.
  - Проверяются корректные статусы ответов (`401 Unauthorized`, `403 Forbidden`, `404 Not Found`) для всех защищенных операций.