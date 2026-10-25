# Разработка и тестирование

Этот документ — для тех, кто дорабатывает Invoice Keeper или запускает компоненты вручную (или же как микросервисы на разных серверах), без полного стека в Docker. Для обычного запуска достаточно [быстрого старта в README](../README.md).

---

## Содержание

- [Требования](#требования)
- [Проверка окружения](#проверка-окружения)
- [Роль `.env` файлов](#роль-env-файлов)
- [Различие токенов бота](#различие-токенов-бота)
- [Запуск по компонентам](#запуск-по-компонентам)
  - [PostgreSQL](#1-postgresql)
  - [Backend](#2-backend)
  - [Frontend](#3-frontend)
  - [Telegram-бот](#4-telegram-бот)
- [Тесты и миграции](#тесты-и-миграции)
- [Пересборка контейнеров при разработке](#пересборка-контейнеров-при-разработке)
- [Дополнительная документация](#дополнительная-документация)

---

## Требования

| Компонент | Что нужно |
|---|---|
| База данных | [PostgreSQL 15+](https://www.postgresql.org/download/) или Docker для запуска только БД |
| Backend (`backend/`) | [.NET 10 SDK](https://dotnet.microsoft.com/download); опционально `dotnet-ef` |
| Frontend (`frontend/`) | [Node.js 18+](https://nodejs.org/) (LTS) и npm / pnpm / yarn |
| Telegram-бот (`tgbot/`) | [Python 3.10+](https://www.python.org/downloads/) и pip |
| Docker (по желанию) | Docker 20+ и Docker Compose v2+ |

Инструмент миграций EF Core (необязательно):

```bash
dotnet tool install --global dotnet-ef
```

## Проверка окружения

```bash
docker --version             # 20+
docker compose version       # v2+
dotnet --version             # 10.x
node -v                      # 18+
python --version             # 3.10+
```

---

## Роль `.env` файлов

В проекте несколько конфигурационных файлов, и они служат разным целям:

1. **Корневой `.env`** (создаётся из `.env.example`)
   - Единственный файл, который нужен для запуска через `docker compose up`.
   - Содержит порты, секреты БД, JWT, `BOT_TOKEN`, `BOT_SERVICE_TOKEN`, параметры сборки (`BACKEND_PORT`, `VITE_BOT_URL`).

2. **`frontend/.env` и `tgbot/bot-aiogram/.env`** (создаются из своих `.env.example`)
   - **Не участвуют** в сборке Docker-контейнеров: Docker берёт конфигурацию из корневого `.env` или через `build args`.
   - Нужны **только** для локального запуска вне Docker: `npm run dev`, `python main.py`, отладка в IDE.

Создание локальных файлов:

```bash
cp .env.example .env                                    # корень (если ещё нет)
cp frontend/.env.example frontend/.env
cp tgbot/bot-aiogram/.env.example tgbot/bot-aiogram/.env
```

## Различие токенов бота

В конфигурации используются **два разных токена**, их нельзя путать:

### `BOT_TOKEN` — токен Telegram Bot API

- **Откуда:** выдаёт BotFather (начинается с цифр, например `1234567890:ABC...`).
- **Зачем:** только самому боту — подключение к Telegram API (long polling), отправка сообщений, чтение команд.
- **Где указывать:** в `tgbot/bot-aiogram/.env` (локальный запуск) или в корневом `.env` (Docker).

### `BOT_SERVICE_TOKEN` — сервисный токен backend API

- **Откуда:** вы генерируете сами (любая секретная строка, например `bot-vefqwk3ymlrodng48azij1x07bus25cp`).
- **Зачем:** авторизация бота в бэкенде. Бот передаёт его в заголовке `X-Service-Token` в каждом запросе к `/api/v1`, что позволяет выполнять служебные запросы без логина и пароля.
- **Где указывать:** **и в корневом `.env`, и в `.env` бота** — значения должны **строго совпадать**.

---

## Запуск по компонентам

Рекомендуемый порядок: БД → Backend → Frontend → Бот.

### 1. PostgreSQL

Поднять только базу данных через Docker:

```bash
docker compose up -d db
```

Либо используйте свой экземпляр PostgreSQL 15+ и укажите строку подключения в настройках бэкенда.

### 2. Backend

```bash
cd backend
dotnet restore
dotnet run
```

- По умолчанию миграции применяются автоматически при старте (`Database__MigrateOnStartup=true`).
- Первый администратор создаётся автоматически, если заданы `ADMIN_USERNAME` и `ADMIN_PASSWORD`.
- Swagger UI доступен по адресу, который выведет `dotnet run` (путь `/swagger`).

Подробнее: [backend_setup.md](backend_setup.md), [backend_api.md](backend_api.md).

### 3. Frontend

```bash
cd frontend
npm install
npm run dev
```

Интерфейс откроется на http://localhost:5173. Запросы к API проксируются согласно настройкам Vite (см. [frontend-setup.md](frontend-setup.md)).

### 4. Telegram-бот

```bash
cd tgbot/bot-aiogram
python -m venv .venv
source .venv/bin/activate        # Windows: .venv\Scripts\activate
pip install -r requirements.txt
python main.py
```

В `tgbot/bot-aiogram/.env` должны быть заданы `BOT_TOKEN`, `BOT_SERVICE_TOKEN` (такой же, как в корневом `.env`) и адрес backend API.

Особенности работы бота:

- Напоминания: `GET /api/v1/payment-occurrences/reminders`, после успешной отправки — `PATCH /api/v1/payment-occurrences/{id}/reminder-sent` (защита от дублей).
- Новые регулярные платежи проверяются каждые 3 минуты; обработанные идентификаторы хранятся в локальном файле `seen_payments.json`.

Подробнее: [tgbot-setup.md](tgbot-setup.md).

---

## Тесты и миграции

### Тесты backend

```bash
cd backend
dotnet test
```

### Миграции EF Core

Нужен установленный `dotnet-ef`. Команды выполняются из каталога backend-проекта:

```bash
dotnet ef migrations add <MigrationName>
dotnet ef database update
```

Подробности и параметры проектов — в [backend_setup.md](backend_setup.md).

---

## Пересборка контейнеров при разработке

Пересобрать и перезапустить весь стек после изменений:

```bash
docker compose up -d --build
```

Пересобрать один сервис (имя сервиса смотрите в `docker-compose.yml`):

```bash
docker compose up -d --build <service>
```

Логи:

```bash
docker compose logs -f <service>
```

---

## Дополнительная документация

- [Запуск в Docker Compose](docker-setup.md)
- [Backend: настройка и запуск](backend_setup.md)
- [Спецификация REST API](backend_api.md)
- [Frontend: настройка и запуск](frontend-setup.md)
- [Telegram-бот: настройка и запуск](tgbot-setup.md)
