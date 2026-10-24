"""Фоновые задачи (APScheduler): ежедневная рассылка напоминаний.

Алгоритм по ТЗ:
1. GET /api/v1/occurrences/reminders?leadDays=3 — периоды до срока ≤ 3 дней,
   без дублей (reminder_sent), оплаченные не возвращаются бэкендом.
2. Для каждого — сообщение в целевой чат TARGET_CHAT_ID.
3. После успешной отправки — PATCH .../reminder-sent (защита от дублей).
"""
from __future__ import annotations

import logging

from aiogram import Bot

from config import settings
from services.api_client import ApiClient, ApiError
from services.formatting import reminder_message

logger = logging.getLogger(__name__)


async def send_payment_reminders(bot: Bot) -> int:
    """Один прогон: получить напоминания из API и разослать. Возвращает число отправленных."""
    if not settings.target_chat_id:
        logger.warning("TARGET_CHAT_ID не задан — пропускаю рассылку")
        return 0

    api = ApiClient(settings.api_base, settings.bot_service_token)
    sent = 0
    try:
        if not settings.bot_service_token:
            await api.login(settings.admin_username, settings.admin_password)

        items = await api.get_reminders(lead_days=settings.reminder_lead_days)
        if not items:
            logger.info("Напоминаний нет")
            return 0

        logger.info("Напоминаний к отправке: %d", len(items))
        for item in items:
            try:
                await bot.send_message(
                    chat_id=settings.target_chat_id,
                    text=reminder_message(item),
                    parse_mode="HTML",
                )
                await api.mark_reminder_sent(str(item["id"]), overdue=bool(item.get("isOverdue")))
                sent += 1
            except ApiError as e:
                logger.warning("Не удалось отметить напоминание %s: %s", item.get("id"), e)
            except Exception:
                logger.exception("Не удалось отправить напоминание %s", item.get("id"))
        return sent
    except ApiError as e:
        logger.error("API недоступен: %s", e)
        return 0
    finally:
        await api.close()
