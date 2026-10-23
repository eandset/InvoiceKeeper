# Запуск Invoice Keeper через Docker

Инструкция для локального запуска backend и PostgreSQL через Docker Compose.

## Требования

- Docker Desktop (Windows/macOS) или Docker Engine + Compose plugin (Linux)
- Свободные порты **5432** (PostgreSQL) и **8080** (backend) — при необходимости меняются в `.env`

Проверка:

```bash
docker --version
docker compose version
```

## 1. Инициализация секретов

Пароль базы данных и параметры подключения хранятся в `.env`, который **не коммитится**.
В репозитории лежит только шаблон `.env.example`.

Создайте `.env` из шаблона:

```bash
cp .env.example .env
```

PowerShell (Windows):

```powershell
Copy-Item .env.example .env
```

Затем откройте `.env` и задайте пароль:

```dotenv
POSTGRES_PASSWORD=change-me-please
```

### Переменные

| Переменная | Обязательна | По умолчанию | Назначение |
|---|---|---|---|
| `POSTGRES_PASSWORD` | да | — | Пароль пользователя PostgreSQL |
| `POSTGRES_USER` | нет | `postgres` | Имя пользователя PostgreSQL |
| `POSTGRES_DB` | нет | `invoice_keeper` | Имя базы данных |
| `POSTGRES_PORT` | нет | `5432` | Порт PostgreSQL на хосте |
| `BACKEND_PORT` | нет | `8080` | Порт backend на хосте |
| `ASPNETCORE_ENVIRONMENT` | нет | `Development` | Окружение ASP.NET Core |
| `JWT_SECRET` | да | — | Ключ подписи JWT, минимум 32 символа |
| `ADMIN_USERNAME` | нет | `admin` | Логин администратора при первом запуске |
| `ADMIN_PASSWORD` | да | — | Пароль администратора |
| `BOT_SERVICE_TOKEN` | да | — | Сервисный токен Telegram-бота (`X-Service-Token`) |

> Пароль из `.env` читается и `docker compose`, и самим приложением при локальном запуске
> (`dotnet run`): файл подхватывается из корня репозитория автоматически.

### Правила для пароля

- Не используйте значение из `.env.example` — это заглушка.
- Не коммитьте `.env` и не прикладывайте его к тикетам.
- В строке подключения спецсимволы (`;`, `'`, `"`) нужно экранировать, поэтому проще
  использовать пароль из букв, цифр и символов `- _ . ! @ # % ^ *`.
- Для генерации надёжного пароля:

  ```powershell
  -join ((48..57) + (65..90) + (97..122) | Get-Random -Count 32 | ForEach-Object { [char]$_ })
  ```

  ```bash
  openssl rand -base64 32
  ```

Если `.env` уже существовал, а вы сменили пароль, старый том PostgreSQL сохранит прежние
учётные данные. Пересоздайте базу:

```bash
docker compose down -v
```

## 2. Поднятие Docker

```bash
docker compose up
```

В фоне:

```bash
docker compose up -d
```

Что произойдёт:

1. Скачается образ `postgres:16-alpine` и поднимется контейнер `db`.
2. Compose дождётся healthcheck (`pg_isready`).
3. Соберётся образ backend из `backend/Dockerfile`.
4. Backend применит EF Core миграции (`Database__MigrateOnStartup=true`), создаст
   администратора из `ADMIN_USERNAME`/`ADMIN_PASSWORD` и запустится на порту 8080.

Приложение: http://localhost:8080
Swagger UI: http://localhost:8080/swagger

## 3. Полезные команды

```bash
# Статус контейнеров
docker compose ps

# Логи backend
docker compose logs -f backend

# Логи базы
docker compose logs -f db

# Пересборка backend после изменений в коде
docker compose up -d --build backend

# Остановка (данные БД сохраняются)
docker compose down

# Остановка с удалением данных БД
docker compose down -v
```

Подключение к базе из контейнера:

```bash
docker compose exec db psql -U postgres -d invoice_keeper
```

## 4. Локальный запуск без Docker

База в Docker, приложение — через `dotnet`:

```bash
docker compose up -d db
dotnet run --project backend
```

Приложение прочитает `.env` из корня репозитория и подключится к `localhost:5432`.

## 5. Миграции

`dotnet ef` использует `AppDbContextFactory`, который читает `appsettings.json` и `.env`:

```bash
dotnet ef database update --project backend
dotnet ef migrations add <Name> --project backend --output-dir Infrastructure/Data/Migrations
```

В контейнере миграции применяются автоматически при старте. Отключить:

```dotenv
Database__MigrateOnStartup=false
```

## 6. Тесты

```bash
dotnet test backend/tests/InvoiceKeeper.Tests.csproj
```

Интеграционные тесты используют PostgreSQL и пропускаются, если не задан
`POSTGRES_PASSWORD`. Подробнее: [backend-api.md](backend-api.md).

## 7. Диагностика

| Проблема | Причина | Решение |
|---|---|---|
| `Set POSTGRES_PASSWORD in .env` | Не создан `.env` или переменная пуста | Выполните шаг 1 |
| `bind: address already in use` | Порты 5432/8080 заняты | Измените `POSTGRES_PORT` / `BACKEND_PORT` в `.env` |
| `password authentication failed` | Пароль в `.env` не совпадает с данными в томе | `docker compose down -v` и повторите запуск |
| Backend перезапускается | База не готова или ошибка миграции | `docker compose logs backend` |
| `relation "..." does not exist` | Миграции не применились | Проверьте `Database__MigrateOnStartup` в логах |
