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
    """Клиент к /api/v1 с поддержкой заголовка X-Service-Token."""

    def __init__(self, base_url: str, token: str = "") -> None:
        self._base = base_url.rstrip("/")
        self._token = token or None
        self._session: aiohttp.ClientSession | None = None

    async def _get_session(self) -> aiohttp.ClientSession:
        if self._session is None or self._session.closed:
            timeout = aiohttp.ClientTimeout(total=20)
            self._session = aiohttp.ClientSession(timeout=timeout)
        return self._session

    @property
    def _headers(self) -> dict[str, str]:
        return {"X-Service-Token": self._token} if self._token else {}

    async def login(self, username: str, password: str) -> str:
        """POST /auth/login -> токен"""
        session = await self._get_session()
        async with session.post(
                f"{self._base}/auth/login",
                json={"username": username, "password": password},
        ) as resp:
            if resp.status != 200:
                raise ApiError(resp.status, await resp.text())
            data = await resp.json()
            self._token = data.get("token", data.get("accessToken", ""))
            logger.info("API login ok")
            return self._token

    async def _request(self, method: str, path: str, **kwargs: Any) -> Any:
        session = await self._get_session()
        async with session.request(method, f"{self._base}{path}", headers=self._headers, **kwargs) as resp:
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

    # ---------- API эндпоинты по ТЗ ----------

    async def get_reminders(self, within_days: int = 3) -> list[dict]:
        """1. GET /api/v1/payment-occurrences/reminders?withinDays=3"""
        return await self._request("GET", f"/payment-occurrences/reminders?withinDays={within_days}") or []

    async def mark_reminder_sent(self, occurrence_id: str, overdue: bool = False) -> None:
        """2. PATCH /api/v1/payment-occurrences/{id}/reminder-sent"""
        q = "true" if overdue else "false"
        await self._request("PATCH", f"/payment-occurrences/{occurrence_id}/reminder-sent?overdue={q}")

    async def get_summary(self) -> dict:
        """3. GET /api/v1/payments/summary"""
        return await self._request("GET", "/payments/summary") or {}

    async def get_occurrences(self, status: str | None = None) -> list[dict]:
        """4. GET /api/v1/payment-occurrences"""
        path = "/payment-occurrences"
        if status:
            path += f"?status={status}"
        return await self._request("GET", path) or []

    async def get_occurrence(self, occurrence_id: str) -> dict | None:
        """5. GET /api/v1/payment-occurrences/{id}"""
        return await self._request("GET", f"/payment-occurrences/{occurrence_id}")

    async def get_recurring_payments(self) -> list[dict]:
        """6. GET /api/v1/recurring-payments"""
        return await self._request("GET", "/recurring-payments") or []

    async def health(self) -> bool:
        try:
            session = await self._get_session()
            async with session.get(f"{self._base}/health") as resp:
                return resp.status == 200
        except Exception:
            return False