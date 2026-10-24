"""Обработчики команд бота: /start, /status, /overdue, /summary."""
from __future__ import annotations

import logging

from aiogram import Router
from aiogram.filters import Command
from aiogram.types import Message

from config import settings
from services.api_client import ApiClient, ApiError
from services.formatting import occurrence_line, summary_message

logger = logging.getLogger(__name__)
router = Router(name="commands")


def _get_api() -> ApiClient:
    """Клиент API: сервисный токен либо автологин под admin."""
    api = ApiClient(settings.api_base, settings.bot_service_token)
    return api


async def _ensure_auth(api: ApiClient) -> None:
    if settings.bot_service_token:
        return
    await api.login(settings.admin_username, settings.admin_password)


@router.message(Command("start"))
async def cmd_start(message: Message) -> None:
    api = _get_api()
    try:
        healthy = await api.health()
        status = "✅ API доступен" if healthy else "⚠️ API недоступен"
        await message.answer(
            "👋 <b>Invoice Keeper Bot</b>\n\n"
            "Слежу за регулярными платежами и присылаю напоминания за 3 дня до срока.\n\n"
            f"{status}\n"
            f"Ваш chat_id: <code>{message.chat.id}</code>\n\n"
            "Команды: /status /overdue /summary /help",
            parse_mode="HTML",
        )
    finally:
        await api.close()


@router.message(Command("help"))
async def cmd_help(message: Message) -> None:
    await message.answer(
        "<b>Команды</b>\n"
        "/start — приветствие и chat_id\n"
        "/status — неоплаченные платежи и ближайшие сроки\n"
        "/overdue — просроченные платежи\n"
        "/summary — итоги по валютам (RUB/USD/EUR)",
        parse_mode="HTML",
    )


@router.message(Command("status"))
async def cmd_status(message: Message) -> None:
    api = _get_api()
    try:
        await _ensure_auth(api)
        items = await api.get_occurrences(status="unpaid")
        if not items:
            await message.answer("✅ Неоплаченных платежей нет.")
            return
        items = sorted(items, key=lambda i: i.get("dueDate", ""))[:15]
        lines = ["🟡 <b>Неоплаченные платежи</b>\n"]
        lines += [occurrence_line(i) for i in items]
        await message.answer("\n\n".join(lines), parse_mode="HTML")
    except ApiError as e:
        await message.answer(f"Ошибка API: {e.status}")
    except Exception:
        logger.exception("status failed")
        await message.answer("Не удалось получить данные от API.")
    finally:
        await api.close()


@router.message(Command("overdue"))
async def cmd_overdue(message: Message) -> None:
    api = _get_api()
    try:
        await _ensure_auth(api)
        items = await api.get_occurrences(status="overdue")
        if not items:
            await message.answer("🎉 Просрочек нет.")
            return
        items = sorted(items, key=lambda i: i.get("dueDate", ""))
        lines = ["🔴 <b>Просроченные платежи</b>\n"]
        lines += [occurrence_line(i) for i in items]
        await message.answer("\n\n".join(lines), parse_mode="HTML")
    except ApiError as e:
        await message.answer(f"Ошибка API: {e.status}")
    except Exception:
        logger.exception("overdue failed")
        await message.answer("Не удалось получить данные от API.")
    finally:
        await api.close()


@router.message(Command("summary"))
async def cmd_summary(message: Message) -> None:
    api = _get_api()
    try:
        await _ensure_auth(api)
        summary = await api.get_summary()
        await message.answer(summary_message(summary), parse_mode="HTML")
    except ApiError as e:
        await message.answer(f"Ошибка API: {e.status}")
    except Exception:
        logger.exception("summary failed")
        await message.answer("Не удалось получить данные от API.")
    finally:
        await api.close()
