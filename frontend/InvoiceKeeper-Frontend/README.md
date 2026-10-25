# Invoice Keeper — Frontend (React + Vite)

Отдельный SPA-клиент к API Invoice Keeper. Бэкенд живёт в `../backend` и отдаёт JSON
по `/api/v1/*`. В production-сборке (`dist/`) бэкенд сам раздаёт этот фронтенд.

## Стек

- React 18 + React Router 6
- Vite 5
- Чистый CSS с переменными (5 тем × 6 акцентов), без UI-библиотек
- Шрифты: **TD Mars** для заголовков, **TT Inters** для текста (лежат в `public/fonts`)

## Запуск в разработке

```bash
npm install
npm run dev
```

Открыть http://localhost:5173

Дев-сервер проксирует `/api` и `/swagger` на бэкенд (`http://localhost:5164` по умолчанию).
Если бэкенд на другом адресе, задайте переменную перед запуском:

```powershell
$env:VITE_API_TARGET='http://localhost:8080'
npm run dev
```

Бэкенд при этом должен быть уже запущен:

```powershell
cd ..\backend
$env:POSTGRES_PASSWORD='<ваш пароль из .env>'
dotnet run
```

## Production-сборка

```bash
npm run build
```

Результат — папка `dist/`. Бэкенд ищет её по пути `../frontend/dist` и автоматически
раздаёт как SPA (включая клиентские маршруты `/app`, `/login` и т.д.).

Альтернатива — любой статический хостинг с SPA-fallback на `index.html`.

## Демо-доступ

```
admin / admin123
```

Учётная запись создаётся бэкендом при первом старте (раздел `AdminSeed` в `appsettings.json`).

## Маршруты

| Путь | Доступ | Назначение |
| --- | --- | --- |
| `/` | все | Рекламный лендинг |
| `/login` | все | Вход (JWT) |
| `/app` | авторизованные | Обзор: суммы, ближайшие и просроченные периоды |
| `/app/occurrences` | авторизованные | Периоды: фильтры, отметка оплаты |
| `/app/payments` | авторизованные | Реестр регулярных платежей |
| `/app/payments/new` | только Admin | Создание платежа |
| `/app/analytics` | авторизованные | Аналитика по валютам и статусам |
| `/app/users` | только Admin | Пользователи и роли |

## Темы и акценты

Переключаются в шапке, сохраняются в `localStorage` (`ik-theme`, `ik-accent`).
Темы: `obsidian`, `noctra`, `emerald`, `amber`, `monolith`.
Акценты: `crimson`, `violet`, `teal`, `amber`, `lime`, `ice`.

Все цвета — CSS-переменные в `src/styles/tokens.css`, менять тему можно и вручную:

```js
document.documentElement.dataset.theme = 'emerald';
```

## Структура

```
frontend/
├── index.html
├── vite.config.js
├── public/
│   ├── favicon.svg
│   └── fonts/            # TD Mars, TT Inters
├── src/
│   ├── main.jsx
│   ├── App.jsx           # маршруты
│   ├── components/       # Layout, ThemeControls, PixelRing, ProtectedRoute
│   ├── hooks/            # useSession
│   ├── lib/              # api.js, format.js, theme.js
│   ├── pages/            # Landing, Login, Dashboard, …
│   └── styles/           # tokens, base, layout, components, landing, app, pixel
└── dist/                 # готовая production-сборка
```

## Авторизация

JWT хранится в `localStorage` (`ik-token`), пользователь — в `ik-user`.
Все запросы идут с `Authorization: Bearer <token>`. При `401` сессия очищается,
пользователь возвращается на `/login`.
