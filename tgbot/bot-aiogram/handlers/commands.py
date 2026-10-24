"""Обработчики команд бота: /start, /reminders, /help."""
from __future__ import annotations

import logging

from aiogram import Router
from aiogram.filters import Command
from aiogram.types import Message

from config import settings
from services.api_client import ApiClient

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
            "Слежу за регулярными платежами и присылаю напоминания.\n\n"
            f"{status}\n"
            f"Ваш chat_id: <code>{message.chat.id}</code>\n\n"
            "Команды: /start /reminders /help",
            parse_mode="HTML",
        )
    finally:
        await api.close()


@router.message(Command("help"))
async def cmd_help(message: Message) -> None:
    await message.answer(
        "<b>Команды</b>\n"
        "/start — приветствие и chat_id\n"
        "/reminders — получить актуальные напоминания прямо сейчас\n"
        "/help — справка",
        parse_mode="HTML",
    )


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
                from services.formatting import reminder_message
                await message.answer(reminder_message(item), parse_mode="HTML")
                await api.mark_reminder_sent(str(item["id"]))
                sent_count += 1
            except Exception:
                logger.exception("Не удалось отправить/отметить напоминание %s", item.get("id"))

        await message.answer(f"Отправлено напоминаний: {sent_count}.")
    except Exception:
        logger.exception("reminders failed")
        await message.answer("Не удалось получить напоминания от API.")
    finally:
        await api.close()
