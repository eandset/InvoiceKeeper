"""Асинхронный HTTP-клиент для REST API бэкенда (ASP.NET Core, /api/v1)."""
from __future__ import annotations

import logging
from typing import Any

import aiohttp

logger = logging.getLogger(__name__)


class ApiError(Exception):
    def __init__(self, status: int, message: str) -> None:
        super().__init__(f"API error {status}: {message}")
        self.status = status
        self.message = message


class ApiClient:
    """Клиент к сервисным эндпоинтам бэкенда по X-Service-Token."""

    def __init__(self, base_url: str, service_token: str = "") -> None:
        self._base = base_url.rstrip("/")
        self._service_token = service_token or ""
        self._session: aiohttp.ClientSession | None = None

    async def _get_session(self) -> aiohttp.ClientSession:
        if self._session is None or self._session.closed:
            timeout = aiohttp.ClientTimeout(total=20)
            self._session = aiohttp.ClientSession(timeout=timeout)
        return self._session

    @property
    def _headers(self) -> dict[str, str]:
        headers: dict[str, str] = {}
        if self._service_token:
            headers["X-Service-Token"] = self._service_token
        return headers

    async def _request(self, method: str, path: str, **kwargs: Any) -> Any:
        session = await self._get_session()
        async with session.request(
            method,
            f"{self._base}{path}",
            headers=self._headers,
            **kwargs,
        ) as resp:
            if resp.status == 401:
                raise ApiError(401, "unauthorized")
            if resp.status >= 400:
                raise ApiError(resp.status, await resp.text())
            if resp.status == 204:
                return None
            return await resp.json()

    async def close(self) -> None:
        if self._session and not self._session.closed:
            await self._session.close()

    # ---------- Сервисные эндпоинты бота ----------

    async def get_reminders(self, within_days: int = 3) -> list[dict]:
        """GET /api/v1/payment-occurrences/reminders?withinDays=..."""
        return await self._request("GET", f"/payment-occurrences/reminders?withinDays={within_days}") or []

    async def mark_reminder_sent(self, occurrence_id: str) -> None:
        """PATCH /api/v1/payment-occurrences/{id}/reminder-sent"""
        await self._request("PATCH", f"/payment-occurrences/{occurrence_id}/reminder-sent")

    async def health(self) -> bool:
        try:
            session = await self._get_session()
            async with session.get(f"{self._base}/health") as resp:
                return resp.status == 200
        except Exception:
            return False
