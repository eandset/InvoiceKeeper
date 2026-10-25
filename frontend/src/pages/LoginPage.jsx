import { useState } from 'react'
import { useAuth } from '../hooks/useAuth'

export default function LoginPage() {
  const { login } = useAuth()
  const [form, setForm] = useState({ username: '', password: '' })
  const [errors, setErrors] = useState({})
  const [error, setError] = useState('')
  const [submitting, setSubmitting] = useState(false)

  const validate = () => {
    const newErrors = {}
    if (!form.username.trim()) newErrors.username = 'Введите логин'
    if (!form.password) newErrors.password = 'Введите пароль'
    if (form.password && form.password.length < 4) newErrors.password = 'Пароль слишком короткий'
    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    if (!validate()) return

    setSubmitting(true)
    try {
      await login(form.username, form.password)
    } catch (err) {
      const msg = err.message
      if (msg.includes('401') || msg.includes('Unauthorized')) {
        setError('Неверный логин или пароль')
      } else if (msg.includes('NetworkError') || msg.includes('fetch')) {
        setError('Не удалось подключиться к серверу')
      } else {
        setError(msg || 'Ошибка входа')
      }
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-stone-100 dark:bg-gray-900 px-4">
      <div className="w-full max-w-md bg-white dark:bg-gray-800 rounded-2xl shadow-xl p-8">
        <div className="mb-8 text-center">
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Invoice Keeper</h1>
          <p className="text-gray-500 dark:text-gray-400 mt-1">Вход в систему</p>
        </div>

        <form onSubmit={handleSubmit} className="space-y-5">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Логин</label>
            <input
              type="text"
              value={form.username}
              onChange={(e) => setForm({ ...form, username: e.target.value })}
              className={`w-full rounded-lg border px-4 py-2 bg-white dark:bg-gray-700 text-gray-900 dark:text-white outline-none focus:ring-2 focus:ring-emerald-500 ${
                errors.username ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'
              }`}
            />
            {errors.username && <p className="text-red-500 text-xs mt-1">{errors.username}</p>}
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Пароль</label>
            <input
              type="password"
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
              className={`w-full rounded-lg border px-4 py-2 bg-white dark:bg-gray-700 text-gray-900 dark:text-white outline-none focus:ring-2 focus:ring-emerald-500 ${
                errors.password ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'
              }`}
            />
            {errors.password && <p className="text-red-500 text-xs mt-1">{errors.password}</p>}
          </div>

          {error && <p className="text-red-500 text-sm text-center">{error}</p>}

          <button
            type="submit"
            disabled={submitting}
            className="w-full py-2.5 bg-emerald-700 hover:bg-emerald-800 text-white rounded-lg font-medium disabled:opacity-50"
          >
            {submitting ? 'Вход...' : 'Войти'}
          </button>
        </form>
      </div>
    </div>
  )
}
