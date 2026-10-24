# Backend API Invoice Keeper

REST API для учёта регулярных платежей: аутентификация, управление пользователями,
платежами и периодами оплаты, напоминания для Telegram-бота и аналитика.

## Запуск

```powershell
Copy-Item .env.example .env    # задайте POSTGRES_PASSWORD, JWT_SECRET, ADMIN_PASSWORD, BOT_SERVICE_TOKEN
docker compose up -d --build
```

Swagger UI: http://localhost:8080/swagger (или http://localhost:8080/api/v1/docs)

Локально без Docker:

```powershell
docker compose up -d db
dotnet run --project backend
```

## Роли и аутентификация

| Роль | Как получить токен | Возможности |
|---|---|---|
| `Admin` | `POST /api/v1/auth/login` | полный CRUD платежей, создание пользователей, мягкое удаление |
| `User` | учётная запись, созданная администратором | просмотр платежей и аналитики, отметка оплаты |
| `Service` (бот) | заголовок `X-Service-Token` | выборка напоминаний и отметка об отправке |

JWT передаётся в заголовке `Authorization: Bearer <token>`.
Срок жизни и подпись задаются секцией `JWT` (`JWT_SECRET` в окружении).

Администратор создаётся при старте, если заданы `ADMIN_USERNAME` и `ADMIN_PASSWORD`
(или `AdminSeed:*` в конфигурации). Повторный запуск не создаёт дубликат.

## Эндпоинты

### Аутентификация

`POST /api/v1/auth/login`

```json
{ "username": "admin", "password": "secret" }
```

Ответ `200`:

```json
{ "token": "<jwt>", "username": "admin", "role": "Admin" }
```

| Метод | Путь | Роль | Описание |
|---|---|---|---|
| `POST` | `/api/v1/auth/login` | анонимно | вход по логину и паролю |
| `GET` | `/api/v1/auth/me` | User/Admin | профиль текущего пользователя |
| `POST` | `/api/v1/auth/logout` | User/Admin | завершение сессии (`204`) |

JWT не хранится на сервере, поэтому `logout` не отзывает токен: клиент должен
удалить его у себя. До истечения срока действия токен остаётся валидным.

### Пользователи

| Метод | Путь | Роль | Описание |
|---|---|---|---|
| `GET` | `/api/v1/users` | Admin | список пользователей |
| `GET` | `/api/v1/users/{id}` | Admin | пользователь по идентификатору |
| `POST` | `/api/v1/users` | Admin | создание пользователя |
| `PATCH` | `/api/v1/users/{id}` | Admin | изменение роли и/или пароля |
| `DELETE` | `/api/v1/users/{id}` | Admin | удаление пользователя (`204`) |

```json
{ "username": "ivan", "password": "minimum-8-chars", "role": "User" }
```

```json
{ "password": "(unchanged)", "role": "User" }
```

Правила управления пользователями:

- `POST /api/v1/users` создаёт **только** пользователей с ролью `User`; попытка
  создать `Admin` возвращает `403`.
- `PATCH /api/v1/users/{id}` меняет только пароль. Смена роли запрещена (`403`);
  значение `"(unchanged)"` (или пустая строка) сохраняет текущий пароль.
- Изменять и удалять **другого** администратора нельзя (`403`). Себя администратор
  может изменить (например, сменить себе пароль).
- `DELETE /api/v1/users/{id}` удаляет только пользователей с ролью `User` (`204`).

Имя пользователя уникально без учёта регистра (`409 Conflict` при дубликате).

### Платежи

| Метод | Путь | Роль | Описание |
|---|---|---|---|
| `GET` | `/api/v1/recurring-payments` | User/Admin | активные платежи + ближайший неоплаченный период |
| `GET` | `/api/v1/recurring-payments/{id}` | User/Admin | один активный платёж |
| `POST` | `/api/v1/recurring-payments` | Admin | создание платежа и первого периода |
| `DELETE` | `/api/v1/recurring-payments/{id}` | Admin | мягкое удаление (`isDeleted = true`) |

```json
{
  "title": "Аренда офиса",
  "amount": 1250.50,
  "currency": "USD",
  "nextPaymentDate": "2026-11-01",
  "periodicity": "monthly"
}
```

`currency`: `RUB` | `USD` | `EUR`.
`periodicity`: `monthly` (по умолчанию) | `weekly` | `quarterly` | `yearly`.

Ответ `201 Created` содержит сам платёж и `nextOccurrence` — первый период с
`dueDate = nextPaymentDate`. Повторное удаление возвращает `204` (идемпотентно).
Удалённые платежи не попадают в список, не создают новых периодов и не участвуют в напоминаниях.

### Периоды оплаты

| Метод | Путь | Роль | Описание |
|---|---|---|---|
| `GET` | `/api/v1/payment-occurrences` | User/Admin | все периоды: оплаченные, ожидаемые и просроченные |
| `GET` | `/api/v1/payment-occurrences/{id}` | User/Admin | конкретный период |
| `PATCH` | `/api/v1/payment-occurrences/{id}` | User/Admin | подтверждение/отмена оплаты и фактическая дата |

`PATCH /api/v1/payment-occurrences/{id}`

```json
{ "isPaid": true, "actualPaymentDate": null }
```

Если `actualPaymentDate` не указан, используется текущая дата (UTC).
После отметки оплаты последнего неоплаченного периода система создаёт следующий период
на дату `dueDate + период` и обновляет `nextPaymentDate` платежа.
Повторная отметка оплаты не создаёт дубликат периода (защищено уникальным индексом
`recurring_payment_id + due_date`).

### Интеграция с Telegram-ботом

Заголовок: `X-Service-Token: <BOT_SERVICE_TOKEN>`.

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/api/v1/payment-occurrences/reminders?withinDays=3` | неоплаченные периоды без отправленного напоминания со сроком не позже `today + withinDays` |
| `PATCH` | `/api/v1/payment-occurrences/{id}/reminder-sent` | отметить напоминание отправленным (`204`) |

Элемент ответа:

```json
{
  "id": "6da5c63a-b236-48de-a070-c7e7fbe98887",
  "recurringPaymentId": "d66ac9b6-d682-4f7a-a744-1c404cbe03ce",
  "title": "Интернет",
  "amount": 35.00,
  "currency": "EUR",
  "dueDate": "2026-10-24T00:00:00",
  "daysUntilDue": 1,
  "isOverdue": false
}
```

`withinDays` ограничен диапазоном `0..30`; по умолчанию `3`.

### Аналитика

`GET /api/v1/payments/summary` (User/Admin)

```json
{
  "totalRub": 1500.00,
  "totalUsd": 1250.50,
  "totalEur": 35.00,
  "pendingCount": 3,
  "overdueCount": 1
}
```

Суммируются неоплаченные периоды активных платежей. `overdueCount` — количество
таких периодов с `dueDate` в прошлом.

### Документация

`GET /api/v1/docs` — редирект (`302`) на `/swagger/index.html`. Доступен без токена.

## Ошибки

Бизнес-ошибки возвращаются в формате `application/problem+json`:

```json
{
  "title": "Конфликт состояния",
  "status": 409,
  "detail": "Платёж удалён, изменение его периодов недоступно.",
  "instance": "/api/v1/payment-occurrences/..."
}
```

Ошибки валидации DTO возвращаются стандартным `ValidationProblemDetails` (`400`)
с полем `errors`.

## Переменные окружения

| Переменная | Обязательна | Назначение |
|---|---|---|
| `POSTGRES_PASSWORD` | да | пароль PostgreSQL |
| `ADMIN_USERNAME` | да | логин администратора при первом запуске |
| `ADMIN_PASSWORD` | да | пароль администратора |
| `JWT_SECRET` | да (вне Development) | ключ подписи JWT, минимум 32 символа |
| `BOT_SERVICE_TOKEN` | да | сервисный токен бота |
| `CORS_ALLOWED_ORIGINS` | нет | список источников через запятую для веб-клиента |
| `HttpsRedirection__Enabled` | нет | `false`, если TLS завершается на proxy |

## Тесты

```powershell
# все тесты; параметры PostgreSQL подхватываются из корневого .env
dotnet test backend/tests/InvoiceKeeper.Tests.csproj

# переопределить параметры подключения можно переменными окружения
$env:POSTGRES_PASSWORD = "<пароль>"
dotnet test backend/tests/InvoiceKeeper.Tests.csproj
```

Интеграционные тесты создают временную базу `invoice_keeper_test_<guid>`,
применяют миграции и удаляют её после завершения.
Параметры подключения берутся из переменных окружения `POSTGRES_*`, а при их
отсутствии — из корневого `.env` (он загружается автоматически, как и в приложении).

Отдельно проверяется матрица доступа к API (`backend/tests/Api`): реальный хост
поднимается поверх временной БД, и запросы выполняются от имени `Admin`, `User`,
сервисного клиента и без токена — ожидаются `401`/`403`/`404` для запрещённых операций.

