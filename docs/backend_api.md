# Backend API Invoice Keeper

REST API для учёта регулярных платежей: аутентификация, управление пользователями,
платежами и периодами оплаты, напоминания для Telegram-бота и аналитика.

## Роли и аутентификация

| Роль | Как получить токен | Возможности |
|---|---|---|
| `Admin` | `POST /api/v1/auth/login` | полный CRUD платежей, управление пользователями, ручной пересчёт просрочек |
| `User` | учётная запись, созданная администратором | просмотр платежей, периодов и аналитики, отметка оплаты |
| `Service` (бот) | заголовок `X-Service-Token` | выборка напоминаний и отметка об отправке |

JWT передаётся в заголовке `Authorization: Bearer <token>`.
Срок жизни и подпись задаются секцией `JWT` (`JWT_SECRET` в окружении).

Администратор создаётся при старте, если заданы `ADMIN_USERNAME` и `ADMIN_PASSWORD`
(или `AdminSeed:*` в конфигурации). Повторный запуск не создаёт дубликат.

Политики авторизации, используемые в контроллерах:

| Политика | Кто проходит |
|---|---|
| `UserOrAdmin` | `User`, `Admin` |
| `UserOrAdminOrService` | `User`, `Admin`, `Service` |
| `ServiceOrAdmin` | `Service`, `Admin` |
| `AdminOnly` | только `Admin` |

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

| Метод | Путь | Политика | Описание |
|---|---|---|---|
| `POST` | `/api/v1/auth/login` | анонимно | вход по логину и паролю |
| `GET` | `/api/v1/auth/me` | UserOrAdmin | профиль текущего пользователя |
| `POST` | `/api/v1/auth/logout` | UserOrAdmin | завершение сессии (`204`) |

JWT не хранится на сервере, поэтому `logout` не отзывает токен: клиент должен
удалить его у себя. До истечения срока действия токен остаётся валидным.

### Пользователи

Все эндпоинты контроллера защищены политикой `AdminOnly`.

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/api/v1/users` | список пользователей |
| `GET` | `/api/v1/users/{id}` | пользователь по идентификатору |
| `POST` | `/api/v1/users` | создание пользователя |
| `PATCH` | `/api/v1/users/{id}` | изменение пользователя |
| `DELETE` | `/api/v1/users/{id}` | удаление пользователя (`204`) |

```json
{ "username": "ivan", "password": "minimum-8-chars", "role": "User" }
```

```json
{ "password": "(unchanged)", "role": "User" }
```

Правила управления пользователями:

- `POST /api/v1/users` создаёт пользователей; при попытке создать `Admin`
  возвращается `403`.
- `PATCH /api/v1/users/{id}` меняет пароль. Смена роли запрещена (`403`);
  значение `"(unchanged)"` (или пустая строка) сохраняет текущий пароль.
- Изменять и удалять **другого** администратора нельзя (`403`). Себя администратор
  может изменить (например, сменить себе пароль).
- `DELETE /api/v1/users/{id}` удаляет только пользователей с ролью `User` (`204`).

Имя пользователя уникально без учёта регистра (`409 Conflict` при дубликате).

### Платежи

| Метод | Путь | Политика | Описание |
|---|---|---|---|
| `GET` | `/api/v1/recurring-payments` | UserOrAdminOrService | активные платежи + ближайший неоплаченный период |
| `GET` | `/api/v1/recurring-payments/{id}` | UserOrAdmin | один активный платёж |
| `POST` | `/api/v1/recurring-payments` | AdminOnly | создание платежа и первого периода |
| `DELETE` | `/api/v1/recurring-payments/{id}` | AdminOnly | мягкое удаление (`isDeleted = true`) |

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

| Метод | Путь | Политика | Описание |
|---|---|---|---|
| `GET` | `/api/v1/payment-occurrences` | UserOrAdminOrService | все периоды с фильтрами |
| `GET` | `/api/v1/payment-occurrences/{id}` | UserOrAdmin | конкретный период |
| `PATCH` | `/api/v1/payment-occurrences/{id}` | UserOrAdmin | подтверждение/отмена оплаты и фактическая дата |

`GET /api/v1/payment-occurrences` поддерживает query-параметры:

| Параметр | Тип | Описание |
|---|---|---|
| `isPaid` | `bool?` | фильтр по признаку оплаты |
| `isOverdue` | `bool?` | фильтр по признаку просрочки |
| `dueBefore` | `DateTime?` | срок не позже указанной даты |
| `dueAfter` | `DateTime?` | срок не раньше указанной даты |

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

Заголовок: `X-Service-Token: <BOT_SERVICE_TOKEN>` (политика `ServiceOrAdmin`).

| Метод | Путь | Описание |
|---|---|---|
| `GET` | `/api/v1/payment-occurrences/reminders?withinDays=3&telegramChatId=<id>` | неоплаченные периоды без отправленного напоминания со сроком не позже `today + withinDays` |
| `PATCH` | `/api/v1/payment-occurrences/{id}/reminder-sent` | отметить напоминание отправленным (`204`) |

`withinDays` ограничен диапазоном `0..30`; по умолчанию `3`.
Необязательный `telegramChatId` ограничивает выборку периодами конкретного чата.

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

### Аналитика

| Метод | Путь | Политика | Описание |
|---|---|---|---|
| `GET` | `/api/v1/payments/summary` | UserOrAdminOrService | итоговые суммы неоплаченных платежей по валютам |
| `GET` | `/api/v1/analytics/overdue` | UserOrAdminOrService | список просроченных периодов |

`GET /api/v1/payments/summary`

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

`GET /api/v1/analytics/overdue` возвращает `IReadOnlyList<PaymentOccurrenceResponseDto>`
— те же DTO периодов, что и в `/api/v1/payment-occurrences`.

### Пересчёт просрочек

| Метод | Путь | Политика | Описание |
|---|---|---|---|
| `POST` | `/api/v1/payment-occurrences/recalculate-overdue` | AdminOnly | ручной пересчёт просроченных периодов, возвращает число обновлённых записей |

### Служебные эндпоинты

| Метод | Путь | Политика | Описание |
|---|---|---|---|
| `GET` | `/api/v1/health` | анонимно | проверка доступности API (`{ "status": "ok" }`) |
| `GET` | `/api/v1/docs` | анонимно | редирект (`302`) на `/swagger/index.html` |

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