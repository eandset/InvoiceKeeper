import { useEffect, useState } from 'react'
import { api } from '../lib/api'
import { useAuth } from '../hooks/useAuth'
import { Card, Button } from '../components/ui'

export default function UsersPage() {
  const { isAdmin } = useAuth()
  const [users, setUsers] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [form, setForm] = useState({ username: '', password: '', role: 'User' })
  const [formErrors, setFormErrors] = useState({})

  useEffect(() => {
    load()
  }, [])

  const load = () => {
    api.getUsers()
      .then(setUsers)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }

  const validate = () => {
    const newErrors = {}
    if (!form.username.trim()) newErrors.username = 'Введите логин'
    if (!form.password) newErrors.password = 'Введите пароль'
    else if (form.password.length < 4) newErrors.password = 'Минимум 4 символа'
    setFormErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleCreate = async (e) => {
    e.preventDefault()
    if (!validate()) return
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

  if (loading) return <div className="text-gray-500 dark:text-gray-400">Загрузка...</div>

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Пользователи</h1>
      {error && <p className="text-red-500">{error}</p>}
      {isAdmin && (
        <Card>
          <form onSubmit={handleCreate} className="flex flex-wrap items-end gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Логин</label>
              <input value={form.username} onChange={(e) => setForm({ ...form, username: e.target.value })} className={`rounded-lg border px-3 py-2 dark:bg-gray-700 dark:text-white ${formErrors.username ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}`} />
              {formErrors.username && <p className="text-red-500 text-xs mt-1">{formErrors.username}</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Пароль</label>
              <input type="password" value={form.password} onChange={(e) => setForm({ ...form, password: e.target.value })} className={`rounded-lg border px-3 py-2 dark:bg-gray-700 dark:text-white ${formErrors.password ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}`} />
              {formErrors.password && <p className="text-red-500 text-xs mt-1">{formErrors.password}</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Роль</label>
              <select value="User" disabled className="rounded-lg border px-3 py-2 dark:bg-gray-700 dark:text-white border-gray-300 dark:border-gray-600 bg-gray-100 dark:bg-gray-600">
                <option value="User">User</option>
              </select>
            </div>
            <Button type="submit">Добавить</Button>
          </form>
        </Card>
      )}
      <Card>
        <table className="min-w-full">
          <thead className="bg-stone-50 dark:bg-gray-700">
            <tr>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Логин</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Роль</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200"></th>
            </tr>
          </thead>
          <tbody className="divide-y dark:divide-gray-700">
            {users.map((u) => (
              <tr key={u.id} className="hover:bg-stone-50 dark:hover:bg-gray-700/50">
                <td className="px-4 py-3 text-gray-900 dark:text-white">{u.username}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{u.role}</td>
                <td className="px-4 py-3">
                  {isAdmin && u.role !== 'Admin' && <button onClick={() => handleDelete(u.id)} className="text-red-600 hover:text-red-800 text-sm font-medium">Удалить</button>}
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
      </Card>
    </div>
  )
}
