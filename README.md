# InvoiceKeeper
Хакатон проект

## Backend API

ASP.NET Core REST API для регулярных платежей: JWT-аутентификация, управление
пользователями и платежами, напоминания для Telegram-бота, аналитика.

Описание эндпоинтов и контрактов: [docs/backend-api.md](docs/backend-api.md)
Swagger UI после запуска: http://localhost:8080/swagger

## Запуск

Инструкция по настройке секретов и запуску через Docker Compose:
[docs/docker-setup.md](docs/docker-setup.md)

Кратко:

```bash
cp .env.example .env   # затем задайте POSTGRES_PASSWORD, JWT_SECRET, ADMIN_PASSWORD, BOT_SERVICE_TOKEN
docker compose up -d
```

Приложение: http://localhost:8080
