# API для Telegram-бота

Документ описывает эндпоинты, которые используются Telegram-ботом (	gbot) для
интеграции с backend Invoice Keeper.

## Аутентификация сервиса

Все запросы от бота должны содержать заголовок:

`
X-Service-Token: <BOT_SERVICE_TOKEN>
`

<BOT_SERVICE_TOKEN> задаётся в .env (см. .env.example).

## Эндпоинты

### Получение напоминаний

GET /api/v1/payment-occurrences/reminders?withinDays=3&telegramChatId=<chat_id>

Возвращает неоплаченные периоды, для которых ещё не отправлено напоминание, со
сроком не позже 	oday + withinDays.

Если передан 	elegramChatId, возвращаются периоды пользователя с этим
	elegram_chat_id (если пользователь привязал Telegram и есть chat_id).

Заголовки:

`
X-Service-Token: <BOT_SERVICE_TOKEN>
`

Ответ 200:

`json
[
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
]
`

### Отметить напоминание отправленным

PATCH /api/v1/payment-occurrences/{id}/reminder-sent

Помечает период как напомнённый, чтобы он не попадал в повторные напоминания.

Заголовки:

`
X-Service-Token: <BOT_SERVICE_TOKEN>
`

Ответ: 204 No Content.

### Поиск пользователя по Telegram ID

GET /api/v1/telegram/users/{telegramId}

Возвращает профиль пользователя, привязанного к указанному Telegram ID.

Заголовки:

`
X-Service-Token: <BOT_SERVICE_TOKEN>
`

Ответ 200:

`json
{
  "id": "6da5c63a-b236-48de-a070-c7e7fbe98887",
  "username": "ivan",
  "role": "User",
  "telegramId": 123456789,
  "telegramChatId": 987654321
}
`

Если пользователь не найден — 204 No Content.

### Обновить Telegram Chat ID

В текущей версии backend не предоставляет отдельного эндпоинта для записи
chat_id. Бот может сохранять chat_id на своей стороне, используя полученный
	elegramId из /api/v1/telegram/users/{telegramId} и сопоставляя его с
chat_id из входящего сообщения.

Для обновления chat_id на стороне backend используйте существующий эндпоинт
привязки Telegram от имени пользователя (см. ackend-api.md).

## Связка пользователя с Telegram

Для привязки Telegram ID к учётной записи пользователя используется
[Telegram Login Widget](https://core.telegram.org/widgets/login). Frontend
встраивает виджет, получает payload и отправляет его на backend.

### Привязка Telegram ID

POST /api/v1/telegram/link

Авторизация: JWT (Authorization: Bearer <token>).

Тело запроса (payload от Telegram Login Widget):

`json
{
  "id": 123456789,
  "firstName": "Ivan",
  "lastName": "Ivanov",
  "username": "ivan",
  "hash": "...",
  "authDate": 1712345678
}
`

Backend проверяет подпись hash с помощью BOT_SERVICE_TOKEN (SHA-256 HMAC)
и сохраняет 	elegram_id для текущего пользователя.

Ответ 200:

`json
{
  "id": "6da5c63a-b236-48de-a070-c7e7fbe98887",
  "username": "ivan",
  "role": "User",
  "telegramId": 123456789,
  "telegramChatId": null
}
`

Возможные ошибки:

- 401 Unauthorized — неверная подпись Telegram Login Widget или срок действия
  JWT истёк.
- 409 Conflict — Telegram ID уже привязан к другой учётной записи, либо учётная
  запись уже привязана к другому Telegram ID.

## Порядок работы бота

1. Пользователь авторизуется в Telegram и нажимает кнопку «Start» или получает
   сообщение от бота.
2. Бот получает 	elegramId из сообщения.
3. Бот запрашивает backend:
   GET /api/v1/telegram/users/{telegramId}.
4. Если пользователь найден, бот запрашвает напоминания:
   GET /api/v1/payment-occurrences/reminders?telegramChatId=<chat_id>.
5. Бот отправляет напоминания и отмечает их отправленными:
   PATCH /api/v1/payment-occurrences/{id}/reminder-sent.

## Переменные окружения

| Переменная | Назначение |
|---|---|
| BOT_SERVICE_TOKEN | Секретный токен для X-Service-Token и верификации Telegram Login Widget |

## Расширение на других ботов

Для добавления бота в другом мессенджере (Max-Bot, VK-Bot) потребуется:

1. Создать отдельный backend-сервис или адаптер, который будет преобразовывать
   события от нового мессенджера в те же API-запросы к backend.
2. Использовать существующий X-Service-Token для аутентификации запросов.
3. Для идентификации пользователя по внешнему ID добавить отдельные поля в
   сущность User или вспомогательную таблицу связок.
