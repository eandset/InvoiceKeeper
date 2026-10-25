# Invoice Keeper — Frontend

Фронтенд на React + Vite с CSS-переменными (без UI-библиотек).

## Установка и запуск

```bash
cd frontend
npm install
npm run dev
```

Фронтенд работает на `http://localhost:5173` и проксирует `/api` на `http://localhost:5164`.

## Структура

- `src/lib/api.js` — HTTP-клиент к бэкенду и работа с сессией.
- `src/hooks/useSession.js` — состояние авторизации.
- `src/lib/theme.js` — темы и акценты.
- `src/pages/` — страницы приложения.
- `src/components/` — общие компоненты и шапка.

## Покрытие API

- Auth: `POST /auth/login`, `GET /auth/me`, `POST /auth/logout`
- Recurring payments: список, создание, удаление
- Payment occurrences: список, отметка оплаты (`PATCH` с `isPaid`)
- Summary: `GET /payments/summary`
- Users: список, создание, удаление (admin)

## Темы

Тема и акцент переключаются в шапке и сохраняются в `localStorage` (`ik-theme`, `ik-accent`).

## Переменные окружения

Создайте .env на основе .env.example:

```bash
cp .env.example .env
```
- VITE_BOT_URL — ссылка на Telegram-бот, отображается в футере.

