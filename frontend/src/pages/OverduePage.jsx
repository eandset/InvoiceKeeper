import { useEffect, useState } from 'react'
import { api } from '../lib/api'

export default function OverduePage() {
  const [overdue, setOverdue] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    api.getOverdue()
      .then(setOverdue)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }, [])

  if (loading) return <div className="p-8">Загрузка...</div>

  return (
    <div className="p-8">
      <h1 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Просроченные периоды</h1>
      {error && <p className="text-red-500 mb-4">{error}</p>}
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden">
        <table className="min-w-full">
          <thead className="bg-gray-100 dark:bg-gray-700">
            <tr>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Название</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Сумма</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Срок</th>
            </tr>
          </thead>
          <tbody className="divide-y dark:divide-gray-700">
            {overdue.map((o) => (
              <tr key={o.id}>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{o.title}</td>
                <td className="px-4 py-3 text-red-600">{o.amount} {o.currency}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">
                  {o.dueDate ? new Date(o.dueDate).toLocaleDateString() : '-'}
                </td>
              </tr>
            ))}
            {overdue.length === 0 && (
              <tr>
                <td colSpan={3} className="px-4 py-6 text-center text-gray-500 dark:text-gray-400">Просрочек нет</td>
              </tr>
            )}
          </tbody>
        </table>
      </div>
    </div>
  )
}
