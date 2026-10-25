# Invoice Keeper — система учёта и контроля регулярных платежей

**Invoice Keeper** — платформа для отслеживания регулярных платежей, периодов оплаты и автоматической рассылки уведомлений. Состоит из трёх компонентов:

- **Backend** — .NET 10 (ASP.NET Core) + PostgreSQL
- **Frontend** — React + Vite
- **Telegram-бот** — aiogram 3

---

## 🚀 Быстрый запуск

Для запуска всей системы достаточно **Docker** и **одного файла `.env` в корне проекта**.

### Что нужно установить

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (Windows / macOS) или Docker Engine 20+ и Docker Compose v2+ (Linux)

Проверка:

```bash
docker --version          # 20+
docker compose version    # v2+
```

### Шаг 1. Создайте `.env` в корне проекта

```bash
cp .env.example .env
```

### Шаг 2. Заполните `.env`

Обязательно задайте:

| Переменная | Описание |
|---|---|
| `BOT_TOKEN` | Токен Telegram-бота, выданный [@BotFather](https://t.me/BotFather) (вида `1234567890:ABC...`) |
| `BOT_SERVICE_TOKEN` | Любая ваша секретная строка. Бот использует её для авторизации в бэкенде (заголовок `X-Service-Token`) |
| `ADMIN_USERNAME` / `ADMIN_PASSWORD` | Логин и пароль первого администратора (создаётся автоматически при старте) |
| Пароль PostgreSQL и JWT-секрет | См. комментарии в `.env.example` |

> ⚠️ `BOT_TOKEN` и `BOT_SERVICE_TOKEN` — **разные** токены. Первый выдаёт Telegram, второй вы придумываете сами. Подробнее — в [документации для разработки](docs/development.md#различие-токенов-бота).

### Шаг 3. Запустите

```bash
docker compose up -d --build
```

### Готово

| Сервис | Адрес |
|---|---|
| Веб-интерфейс | http://localhost |
| Swagger UI (API) | http://localhost/swagger |
| PostgreSQL | `localhost:5432` |

Миграции БД применяются автоматически при старте, администратор создаётся из `ADMIN_USERNAME` / `ADMIN_PASSWORD`.

Остановить систему:

```bash
docker compose down
```

---

## 🔑 Ключевые особенности

### Роли и авторизация

- **Admin** — полный CRUD платежей и пользователей, ручной пересчёт просрочек (JWT).
- **User** — просмотр данных, подтверждение оплаты периодов (JWT).
- **Service (бот)** — получение напоминаний и аналитики по заголовку `X-Service-Token`.

### Защита от повторных уведомлений

Бот запрашивает незакрытые напоминания (`GET /api/v1/payment-occurrences/reminders`) и после успешной отправки сразу помечает их (`PATCH /api/v1/payment-occurrences/{id}/reminder-sent`), поэтому дубли исключены. Новые регулярные платежи бот проверяет каждые 3 минуты.

---

## 📚 Документация

- 🧑‍💻 **[Разработка и тестирование](docs/development.md)** — локальный запуск компонентов без Docker, `.env` для разработки, тесты, миграции
- 🐳 [Запуск в Docker Compose](docs/docker-setup.md)
- ⚙️ [Backend API: настройка и запуск](docs/backend_setup.md)
- 📖 [Спецификация REST API](docs/backend_api.md)
- 💻 [Frontend: настройка и запуск](docs/frontend-setup.md)
- 🤖 [Telegram-бот: настройка и запуск](docs/tgbot-setup.md)