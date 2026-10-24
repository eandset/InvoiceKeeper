"""Глобальный обработчик ошибок aiogram."""
import logging

from aiogram import Router
from aiogram.types import ErrorEvent

logger = logging.getLogger(__name__)
router = Router(name="errors")


@router.error()
async def on_error(event: ErrorEvent) -> None:
    logger.exception("Telegram handler error: %s", event.exception)
