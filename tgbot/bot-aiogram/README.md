# Invoice Keeper — Telegram-бот (aiogram 3)

Микросервис уведомлений и быстрых команд. Взаимодействует с REST API бэкенда по /api/v1.

## Запуск
pip install -r requirements.txt
copy .env.example .env  # заполнить BOT_TOKEN, API_URL, TARGET_CHAT_ID
python main.py

## Переменные (.env)
BOT_TOKEN            — токен Telegram-бота (обязательно)
API_URL              — базовый URL API, например http://localhost:5080/api/v1
BOT_SERVICE_TOKEN    — JWT с ролью Bot (предпочтительно)
ADMIN_USERNAME/ADMIN_PASSWORD — автологин, если нет сервисного токена
TARGET_CHAT_ID       — чат/канал для напоминаний (узнать: /start в боте)
CRON_SCHEDULE_HOUR/MINUTE — время ежедневной проверки (по умолчанию 09:00)
REMINDER_LEAD_DAYS   — за сколько дней напоминать (по умолчанию 3)

## Команды
/start   — приветствие + chat_id + статус API
/status  — неоплаченные платежи
/overdue — просроченные платежи
/summary — итоги по валютам (RUB/USD/EUR, без конвертации)
/help    — справка

## Логика напоминаний
1. Ежедневно в CRON_SCHEDULE_HOUR (а также при старте) бот запрашивает
   GET /api/v1/occurrences/reminders?leadDays=3
2. Для каждого периода отправляет сообщение в TARGET_CHAT_ID.
3. После успешной отправки вызывает
   PATCH /api/v1/payment-occurrences/{id}/reminder-sent
   — дублей нет, оплаченные периоды не тревожатся.

## Структура
  main.py               — Bot + Dispatcher + APScheduler
  config.py             — pydantic-settings, .env
  services/api_client.py — aiohttp-клиент к /api/v1 (JWT)
  services/formatting.py — HTML-сообщения
  handlers/commands.py   — /start /status /overdue /summary /help
  handlers/errors.py     — глобальные ошибки
  scheduler/jobs.py      — ежедневная рассылка
