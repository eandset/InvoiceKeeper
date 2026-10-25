import { useEffect, useState } from 'react'
import { api } from '../lib/api'
import { useAuth } from '../hooks/useAuth'

export default function UsersPage() {
  const { isAdmin } = useAuth()
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [form, setForm] = useState({ username: '', password: '', role: 'User' })

  useEffect(() => {
    load()
  }, [])

  const load = () => {
    api.getUsers()
      .then(setUsers)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }

  const handleCreate = async (e) => {
    e.preventDefault()
    try {
      await api.createUser(form)
      setForm({ username: '', password: '', role: 'User' })
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  const handleDelete = async (id) => {
    if (!confirm('Удалить пользователя?')) return
    try {
      await api.deleteUser(id)
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  if (loading) return <div className="p-8">Загрузка...</div>

  return (
    <div className="p-8">
      <h1 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Пользователи</h1>
      {error && <p className="text-red-500 mb-4">{error}</p>}
      {isAdmin && (
        <form onSubmit={handleCreate} className="mb-6 flex gap-2 items-end bg-white dark:bg-gray-800 p-4 rounded-lg shadow">
          <div>
            <label className="block text-xs text-gray-600 dark:text-gray-400">Логин</label>
            <input value={form.username} onChange={(e) => setForm({ ...form, username: e.target.value })} required className="rounded border px-2 py-1 dark:bg-gray-700 dark:text-white dark:border-gray-600" />
          </div>
          <div>
            <label className="block text-xs text-gray-600 dark:text-gray-400">Пароль</label>
            <input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} required className="rounded border px-2 py-1 dark:bg-gray-700 dark:text-white dark:border-gray-600" />
          </div>
          <div>
            <label className="block text-xs text-gray-600 dark:text-gray-400">Роль</label>
            <select value={form.role} onChange={(e) => setForm({ ...form, role: e.target.value })} className="rounded border px-2 py-1 dark:bg-gray-700 dark:text-white dark:border-gray-600">
              <option value="User">User</option>
              <option value="Admin">Admin</option>
            </select>
          </div>
          <button className="px-4 py-1 bg-blue-600 text-white rounded hover:bg-blue-700">Добавить</button>
        </form>
      )}
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden">
        <table className="min-w-full">
          <thead className="bg-gray-100 dark:bg-gray-700">
            <tr>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Логин</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Роль</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Действия</th>
            </tr>
          </thead>
          <tbody className="divide-y dark:divide-gray-700">
            {users.map((u) => (
              <tr key={u.id}>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{u.username}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{u.role}</td>
                <td className="px-4 py-3">
                  {isAdmin && <button onClick={() => handleDelete(u.id)} className="text-red-600 hover:text-red-800 text-sm">Удалить</button>}
                </td>
              </tr>
            ))}
            {users.length === 0 && (
              <tr>
                <td colSpan={3} className="px-4 py-6 text-center text-gray-500 dark:text-gray-400">Нет пользователей</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
