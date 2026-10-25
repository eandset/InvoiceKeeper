const API_BASE = '/api/v1'

function getToken() {
  return localStorage.getItem('token')
}

function authHeaders() {
  const token = getToken()
  return {
    'Content-Type': 'application/json',
    ...(token ? { Authorization: `Bearer ${token}` } : {})
  }
}

async function request(method, path, body) {
  const url = `${API_BASE}${path}`
  const options = {
    method,
    headers: authHeaders()
  }
  if (body !== undefined) {
    options.body = JSON.stringify(body)
  }
  const response = await fetch(url, options)
  if (response.status === 401) {
    localStorage.removeItem('token')
    localStorage.removeItem('user')
    window.location.href = '/login'
    throw new Error('Unauthorized')
  }
  if (response.status === 403) {
    throw new Error('Forbidden')
  }
  if (response.status === 404) {
    throw new Error('NotFound')
  }
  if (!response.ok) {
    const text = await response.text()
    throw new Error(text || `HTTP ${response.status}`)
  }
  if (response.status === 204) {
    return null
  }
  return response.json()
}

export const api = {
  login: (username, password) => request('POST', '/auth/login', { username, password }),
  me: () => request('GET', '/auth/me'),
  logout: () => request('POST', '/auth/logout'),

  health: () => request('GET', '/health'),

  getPayments: () => request('GET', '/recurring-payments'),
  getPayment: (id) => request('GET', `/recurring-payments/${id}`),
  createPayment: (data) => request('POST', '/recurring-payments', data),
  deletePayment: (id) => request('DELETE', `/recurring-payments/${id}`),

  getOccurrences: (params) => {
    const query = new URLSearchParams()
    if (params) {
      if (params.isPaid !== undefined) query.append('isPaid', params.isPaid)
      if (params.isOverdue !== undefined) query.append('isOverdue', params.isOverdue)
      if (params.dueBefore) query.append('dueBefore', params.dueBefore)
      if (params.dueAfter) query.append('dueAfter', params.dueAfter)
    }
    const qs = query.toString()
    return request('GET', `/payment-occurrences${qs ? '?' + qs : ''}`)
  },
  getOccurrence: (id) => request('GET', `/payment-occurrences/${id}`),
  updateOccurrence: (id, data) => request('PATCH', `/payment-occurrences/${id}`, data),

  getSummary: () => request('GET', '/payments/summary'),
  getOverdue: () => request('GET', '/analytics/overdue'),
  recalculateOverdue: () => request('POST', '/payment-occurrences/recalculate-overdue'),

  getUsers: () => request('GET', '/users'),
  getUser: (id) => request('GET', `/users/${id}`),
  createUser: (data) => request('POST', '/users', data),
  updateUser: (id, data) => request('PATCH', `/users/${id}`, data),
  deleteUser: (id) => request('DELETE', `/users/${id}`)
}
