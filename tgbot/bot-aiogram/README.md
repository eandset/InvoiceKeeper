# Invoice Keeper — Telegram-бот (aiogram 3)

Микросервис уведомлений и быстрых команд. Взаимодействует с REST API бэкенда по /api/v1.

## Запуск
`ash
pip install -r requirements.txt
copy .env.example .env  # заполнить BOT_TOKEN, API_URL, BOT_SERVICE_TOKEN, TARGET_CHAT_ID
python main.py
`

## Переменные (.env)
BOT_TOKEN            — токен Telegram-бота (обязательно)
API_URL              — базовый URL API, например http://localhost:5080/api/v1
BOT_SERVICE_TOKEN    — сервисный токен для API (заголовок X-Service-Token)
TARGET_CHAT_ID       — чат/канал для напоминаний (узнать: /start в боте)
CRON_SCHEDULE_HOUR/MINUTE — время ежедневной проверки (по умолчанию 09:00)
REMINDER_LEAD_DAYS   — за сколько дней напоминать (по умолчанию 3)

## Команды
/start      — приветствие, chat_id и статус API
/occurrences — список всех периодов оплаты
/payments    — список регулярных платежей
/summary     — итоги по валютам (RUB/USD/EUR)
/overdue     — просроченные периоды
/reminders   — получить актуальные напоминания сейчас
/help        — справка

## Логика напоминаний
1. Ежедневно в CRON_SCHEDULE_HOUR бот запрашивает
   GET /api/v1/payment-occurrences/reminders?withinDays=3
2. Для каждого периода отправляет сообщение в TARGET_CHAT_ID.
3. После успешной отправки вызывает
   PATCH /api/v1/payment-occurrences/{id}/reminder-sent
   — дублей нет, оплаченные периоды не тревожатся.

## Структура
  main.py                — Bot + Dispatcher + APScheduler
  config.py              — pydantic-settings, .env
  services/api_client.py — aiohttp-клиент к /api/v1 (X-Service-Token)
  services/formatting.py — HTML-сообщения
  handlers/commands.py   — /start /occurrences /payments /summary /overdue /reminders /help
  handlers/errors.py     — глобальные ошибки
  scheduler/jobs.py      — ежедневная рассылка
