"""Фоновые задачи (APScheduler): ежедневная рассылка напоминаний и проверка новых платежей."""
from __future__ import annotations

import json
import logging
import os
from typing import Set

from aiogram import Bot

from config import settings
from services.api_client import ApiClient, ApiError
from services.formatting import new_payment_message, reminder_message

logger = logging.getLogger(__name__)

_SEEN_PAYMENTS_FILE = os.path.join(os.path.dirname(__file__), "..", "seen_payments.json")


def _load_seen_ids() -> Set[str]:
    if not os.path.exists(_SEEN_PAYMENTS_FILE):
        return set()
    try:
        with open(_SEEN_PAYMENTS_FILE, "r", encoding="utf-8") as f:
            data = json.load(f)
            if isinstance(data, list):
                return set(str(item) for item in data if item)
    except Exception:
        logger.exception("Не удалось прочитать seen_payments.json")
    return set()


def _save_seen_ids(seen_ids: Set[str]) -> None:
    try:
        with open(_SEEN_PAYMENTS_FILE, "w", encoding="utf-8") as f:
            json.dump(sorted(seen_ids), f, ensure_ascii=False)
    except Exception:
        logger.exception("Не удалось сохранить seen_payments.json")


async def send_payment_reminders(bot: Bot) -> int:
    """Один прогон: получить напоминания из API и разослать. Возвращает число отправленных."""
    if not settings.target_chat_id:
        logger.warning("TARGET_CHAT_ID не задан — пропускаю рассылку")
        return 0

    api = ApiClient(settings.api_base, settings.bot_service_token)
    sent = 0
    try:
        items = await api.get_reminders(within_days=settings.reminder_lead_days)
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
                await api.mark_reminder_sent(str(item["id"]))
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


async def check_new_payments(bot: Bot) -> int:
    """Проверяет новые регулярные платежи и уведомляет о них. Возвращает число новых."""
    if not settings.target_chat_id:
        logger.warning("TARGET_CHAT_ID не задан — пропускаю проверку новых платежей")
        return 0

    api = ApiClient(settings.api_base, settings.bot_service_token)
    try:
        payments = await api.get_recurring_payments()
        if not payments:
            return 0

        current_ids = {str(p.get("id")) for p in payments if p.get("id")}
        seen_ids = _load_seen_ids()
        new_ids = current_ids - seen_ids

        if not new_ids:
            logger.debug("Новых платежей не обнаружено")
            return 0

        sent = 0
        for payment in payments:
            payment_id = str(payment.get("id"))
            if payment_id not in new_ids:
                continue
            try:
                await bot.send_message(
                    chat_id=settings.target_chat_id,
                    text=new_payment_message(payment),
                    parse_mode="HTML",
                )
                sent += 1
            except Exception:
                logger.exception("Не удалось отправить уведомление о новом платеже %s", payment_id)

        # Сохраняем все текущие ID, чтобы не уведомлять повторно
        _save_seen_ids(current_ids)
        logger.info("Обнаружено новых платежей: %d", sent)
        return sent
    except ApiError as e:
        logger.error("API недоступен при проверке новых платежей: %s", e)
        return 0
    finally:
        await api.close()
