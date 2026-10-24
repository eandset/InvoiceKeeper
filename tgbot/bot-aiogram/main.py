"""Точка входа: Bot + Dispatcher + APScheduler (ежедневно в CRON_SCHEDULE_HOUR)."""
from __future__ import annotations

import asyncio
import logging
import sys

from aiogram import Bot, Dispatcher
from aiogram.client.default import DefaultBotProperties
from aiogram.enums import ParseMode
from apscheduler.schedulers.asyncio import AsyncIOScheduler
from apscheduler.triggers.cron import CronTrigger

from config import settings
from handlers import commands, errors
from scheduler.jobs import send_payment_reminders


async def main() -> None:
    logging.basicConfig(level=logging.INFO, stream=sys.stdout)
    logger = logging.getLogger("invoice-keeper-bot")

    if not settings.bot_token:
        raise RuntimeError("BOT_TOKEN не задан — заполните .env")

    bot = Bot(token=settings.bot_token, default=DefaultBotProperties(parse_mode=ParseMode.HTML))
    dp = Dispatcher()
    dp.include_router(commands.router)
    dp.include_router(errors.router)

    scheduler = AsyncIOScheduler()
    scheduler.add_job(
        send_payment_reminders,
        CronTrigger(hour=settings.cron_schedule_hour, minute=settings.cron_schedule_minute),
        args=[bot],
        id="daily_payment_reminders",
        replace_existing=True,
    )
    scheduler.start()
    logger.info(
        "Планировщик запущен: ежедневно в %02d:%02d, lead=%d дн.",
        settings.cron_schedule_hour, settings.cron_schedule_minute, settings.reminder_lead_days,
    )

    # Первый прогон сразу при старте (на случай, если бот был выключен в 09:00)
    await send_payment_reminders(bot)

    logger.info("Бот запущен (long polling)")
    try:
        await dp.start_polling(bot)
    finally:
        await bot.session.close()


if __name__ == "__main__":
    asyncio.run(main())
