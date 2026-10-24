"""Загрузка и валидация переменных окружения (.env)."""
from __future__ import annotations

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


class Settings(BaseSettings):
    model_config = SettingsConfigDict(env_file=".env", env_prefix="", extra="ignore")

    bot_token: str = Field(default="", description="Токен Telegram-бота")
    api_url: str = Field(default="http://localhost:5000/api/v1", description="Базовый URL REST API /api/v1")
    bot_service_token: str = Field(default="", description="Сервисный токен бота (X-Service-Token) для API")
    target_chat_id: str = Field(default="", description="Чат/канал для напоминаний")
    cron_schedule_hour: int = Field(default=9, ge=0, le=23)
    cron_schedule_minute: int = Field(default=0, ge=0, le=59)
    reminder_lead_days: int = Field(default=3, ge=0, le=30)

    @property
    def api_base(self) -> str:
        return self.api_url.rstrip("/")


settings = Settings()

