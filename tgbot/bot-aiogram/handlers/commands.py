"""Обработчики команд бота."""
from __future__ import annotations

import logging

from aiogram import Router
from aiogram.filters import Command
from aiogram.types import Message

from config import settings
from services.api_client import ApiClient, ApiError
from services.formatting import occurrence_line, payment_line, reminder_message, summary_message

logger = logging.getLogger(__name__)
router = Router(name="commands")


def _get_api() -> ApiClient:
    """Клиент API: используется только сервисный токен."""
    return ApiClient(settings.api_base, settings.bot_service_token)


@router.message(Command("start"))
async def cmd_start(message: Message) -> None:
    api = _get_api()
    try:
        healthy = await api.health()
        status = "✅ API доступен" if healthy else "⚠️ API недоступен"
        await message.answer(
            "👋 <b>Invoice Keeper Bot</b>\n\n"
            "Слежу за регулярными платежами.\n\n"
            f"{status}\n"
            f"Ваш chat_id: <code>{message.chat.id}</code>\n\n"
            "Команды: /start /occurrences /payments /summary /overdue /reminders /help",
            parse_mode="HTML",
        )
    finally:
        await api.close()


@router.message(Command("help"))
async def cmd_help(message: Message) -> None:
    await message.answer(
        "<b>Команды</b>\n"
        "/start — приветствие и chat_id\n"
        "/occurrences — список всех периодов оплаты\n"
        "/payments — список регулярных платежей\n"
        "/summary — итоги по валютам (RUB/USD/EUR)\n"
        "/overdue — просроченные периоды\n"
        "/reminders — получить актуальные напоминания прямо сейчас\n"
        "/help — справка",
        parse_mode="HTML",
    )


@router.message(Command("occurrences"))
async def cmd_occurrences(message: Message) -> None:
    api = _get_api()
    try:
        items = await api.get_payment_occurrences()
        if not items:
            await message.answer("✅ Периодов оплаты нет.")
            return

        # Сортируем по дате от ранних к поздним (A->Z) и берем первые 10
        items = sorted(items, key=lambda i: i.get("dueDate", ""))
        limited_items = items[:10]

        lines = ["<b>Периоды оплаты</b>\n"]
        lines += [occurrence_line(i) for i in limited_items]

        # Добавляем многоточие в конец, если записей больше 10
        if len(items) > 10:
            lines.append("...")

        await message.answer("\n\n".join(lines), parse_mode="HTML")
    except ApiError as e:
        await message.answer(f"Ошибка API: {e.status}")
    except Exception:
        logger.exception("occurrences failed")
        await message.answer("Не удалось получить периоды оплаты.")
    finally:
        await api.close()


@router.message(Command("payments"))
async def cmd_payments(message: Message) -> None:
    api = _get_api()
    try:
        items = await api.get_recurring_payments()
        if not items:
            await message.answer("✅ Регулярных платежей нет.")
            return
        items = sorted(items, key=lambda i: i.get("title", ""))[:15]
        lines = ["<b>Регулярные платежи</b>\n"]
        lines += [payment_line(i) for i in items]
        await message.answer("\n\n".join(lines), parse_mode="HTML")
    except ApiError as e:
        await message.answer(f"Ошибка API: {e.status}")
    except Exception:
        logger.exception("payments failed")
        await message.answer("Не удалось получить регулярные платежи.")
    finally:
        await api.close()


@router.message(Command("summary"))
async def cmd_summary(message: Message) -> None:
    api = _get_api()
    try:
        summary = await api.get_payments_summary()
        await message.answer(summary_message(summary), parse_mode="HTML")
    except ApiError as e:
        await message.answer(f"Ошибка API: {e.status}")
    except Exception:
        logger.exception("summary failed")
        await message.answer("Не удалось получить итоги.")
    finally:
        await api.close()


@router.message(Command("overdue"))
async def cmd_overdue(message: Message) -> None:
    api = _get_api()
    try:
        items = await api.get_overdue()
        if not items:
            await message.answer("🎉 Просрочек нет.")
            return
        items = sorted(items, key=lambda i: i.get("dueDate", ""))
        lines = ["<b>Просроченные периоды</b>\n"]
        lines += [occurrence_line(i) for i in items]
        await message.answer("\n\n".join(lines), parse_mode="HTML")
    except ApiError as e:
        await message.answer(f"Ошибка API: {e.status}")
    except Exception:
        logger.exception("overdue failed")
        await message.answer("Не удалось получить просроченные периоды.")
    finally:
        await api.close()


@router.message(Command("reminders"))
async def cmd_reminders(message: Message) -> None:
    """Ручной запрос напоминаний: показывает и сразу отмечает отправленными."""
    api = _get_api()
    try:
        items = await api.get_reminders(within_days=settings.reminder_lead_days)
        if not items:
            await message.answer("✅ Напоминаний нет.")
            return

        sent_count = 0
        for item in items:
            try:
                await message.answer(reminder_message(item), parse_mode="HTML")
                await api.mark_reminder_sent(str(item["id"]))
                sent_count += 1
            except Exception:
                logger.exception("Не удалось отправить/отметить напоминание %s", item.get("id"))

        await message.answer(f"Отправлено напоминаний: {sent_count}.")
    except ApiError as e:
        await message.answer(f"Ошибка API: {e.status}")
    except Exception:
        logger.exception("reminders failed")
        await message.answer("Не удалось получить напоминания от API.")
    finally:
        await api.close()
