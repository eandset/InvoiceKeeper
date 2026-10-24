"""Форматирование сообщений для Telegram (HTML parse_mode)."""
from __future__ import annotations

from html import escape
from typing import Any


def _fmt_amount(amount: Any, currency: str) -> str:
    try:
        return f"{float(amount):0.2f} {currency}"
    except (TypeError, ValueError):
        return f"{amount} {currency}"


def _due_text(due_date: str, days: int) -> str:
    date = due_date[:10] if due_date else "-"
    if days < 0:
        return f"{date} (просрочено {abs(days)} дн.)"
    if days == 0:
        return f"{date} (сегодня)"
    if days == 1:
        return f"{date} (завтра)"
    return f"{date} (осталось {days} дн.)"


def reminder_message(item: dict) -> str:
    """Сообщение о напоминании: название, сумма, валюта, дата."""
    title = escape(str(item.get("title", "")))
    amount = _fmt_amount(item.get("amount", 0), item.get("currency", ""))
    days = int(item.get("daysUntilDue", 0))
    due = _due_text(str(item.get("dueDate", "")), days)

    if item.get("isOverdue") or days < 0:
        return (
            "<b>Просрочка оплаты!</b>\n"
            f"Платеж: <b>{title}</b>\n"
            f"Сумма: <b>{amount}</b>\n"
            f"Срок оплаты: {due}\n"
            "Статус: Просрочка"
        )
    return (
        "<b>Напоминание об оплате!</b>\n"
        f"Платеж: <b>{title}</b>\n"
        f"Сумма: <b>{amount}</b>\n"
        f"Срок оплаты: {due}\n"
        "Статус: Не оплачено"
    )
