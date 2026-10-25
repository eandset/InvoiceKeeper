import { useEffect, useState } from 'react'
import { api } from '../lib/api'
import { Card } from '../components/ui'

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

  if (loading) return <div className="text-gray-500 dark:text-gray-400">Загрузка...</div>

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Просроченные периоды</h1>
      {error && <p className="text-red-500">{error}</p>}
      <Card>
        <table className="min-w-full">
          <thead className="bg-stone-50 dark:bg-gray-700">
            <tr>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Название</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Сумма</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Срок</th>
            </tr>
          </thead>
          <tbody className="divide-y dark:divide-gray-700">
            {overdue.map((o) => (
              <tr key={o.id} className="hover:bg-stone-50 dark:hover:bg-gray-700/50">
                <td className="px-4 py-3 text-gray-900 dark:text-white">{o.title}</td>
                <td className="px-4 py-3 text-rose-600 font-medium">{o.amount} {o.currency}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{o.dueDate ? new Date(o.dueDate).toLocaleDateString() : '-'}</td>
              </tr>
            ))}
            {overdue.length === 0 && (
              <tr>
                <td colSpan={3} className="px-4 py-6 text-center text-gray-500 dark:text-gray-400">Просрочек нет</td>
              </tr>
            )}
          </tbody>
        </table>
      </Card>
    </div>
  )
}
