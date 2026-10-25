const TOKEN_KEY = 'ik-token';
const USER_KEY = 'ik-user';

export function readSession() {
  try {
    const token = localStorage.getItem(TOKEN_KEY);
    const raw = localStorage.getItem(USER_KEY);
    if (!token) return null;
    return { token, user: raw ? JSON.parse(raw) : null };
  } catch {
    return null;
  }
}

export function saveSession(token, username, role) {
  localStorage.setItem(TOKEN_KEY, token);
  localStorage.setItem(USER_KEY, JSON.stringify({ username, role }));
}

export function clearSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

export class ApiError extends Error {
  constructor(status, message) {
    super(message);
    this.status = status;
  }
}

async function request(path, { method = 'GET', body, auth = true } = {}) {
  const headers = { Accept: 'application/json' };
  if (body !== undefined) headers['Content-Type'] = 'application/json';

  if (auth) {
    const session = readSession();
    if (session?.token) headers.Authorization = `Bearer ${session.token}`;
  }

  const response = await fetch(`/api/v1${path}`, {
    method,
    headers,
    body: body === undefined ? undefined : JSON.stringify(body)
  });

  if (response.status === 401 && auth) {
    clearSession();
  }

  if (!response.ok) {
    let message = `Ошибка запроса (${response.status})`;
    try {
      const problem = await response.json();
      message = problem.detail || problem.title || message;
    } catch {
      /* тело не JSON — оставляем общий текст */
    }
    throw new ApiError(response.status, message);
  }

  if (response.status === 204) return null;
  const text = await response.text();
  return text ? JSON.parse(text) : null;
}

export const api = {
  login: (username, password) =>
    request('/auth/login', { method: 'POST', body: { username, password }, auth: false }),

  summary: () => request('/payments/summary'),
  occurrences: () => request('/payment-occurrences'),
  payments: () => request('/recurring-payments'),
  users: () => request('/users'),

  setOccurrenceStatus: (id, status) =>
    request(`/payment-occurrences/${id}`, { method: 'PATCH', body: { status, actualPaymentDate: null } }),

  createPayment: (payload) => request('/recurring-payments', { method: 'POST', body: payload }),
  deletePayment: (id) => request(`/recurring-payments/${id}`, { method: 'DELETE' }),
  createUser: (payload) => request('/users', { method: 'POST', body: payload }),
  deleteUser: (id) => request(`/users/${id}`, { method: 'DELETE' })
};
