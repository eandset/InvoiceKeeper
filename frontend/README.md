# Invoice Keeper — Frontend

Фронтенд на React + Vite + Tailwind CSS.

## Установка и запуск

```bash
cd frontend
npm install
npm run dev
```

Фронтенд работает на `http://localhost:5173` и проксирует `/api` на `http://localhost:5164`.

## Структура

- `src/lib/api.js` — HTTP-клиент к бэкенду.
- `src/hooks/` — React-контексты для авторизации и темы.
- `src/pages/` — страницы приложения.
- `src/components/` — общие компоненты.

## Покрытие API

- Auth: `/auth/login`, `/auth/me`, `/auth/logout`
- Health: `/health`
- Recurring payments: CRUD
- Payment occurrences: список, отметка оплаченным
- Summary: `/payments/summary`
- Analytics: `/analytics/overdue`
- Users: CRUD (admin)
- Overdue recalculation: `/payment-occurrences/recalculate-overdue` (admin)

## Темы

Тёмная/светлая тема переключается кнопкой в навбаре. Состояние сохраняется в `localStorage`.

## Переменные окружения

Создайте .env на основе .env.example:

```bash
cp .env.example .env
```
- VITE_BOT_URL — ссылка на Telegram-бот, отображается в боковом меню.

