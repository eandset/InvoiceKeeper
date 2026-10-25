import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../lib/api'

function Card({ title, value, children }) {
  return (
    <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
      <h3 className="text-sm font-medium text-gray-500 dark:text-gray-400">{title}</h3>
      {value !== undefined && <p className="mt-2 text-3xl font-bold text-gray-900 dark:text-white">{value}</p>}
      {children}
    </div>
  )
}

export default function DashboardPage() {
  const [summary, setSummary] = useState(null)
  const [overdue, setOverdue] = useState([])
  const [occurrences, setOccurrences] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    async function load() {
      try {
        const [s, o, occ] = await Promise.all([
          api.getSummary().catch(() => null),
          api.getOverdue().catch(() => []),
          api.getOccurrences({ isPaid: false }).catch(() => [])
        ])
        setSummary(s)
        setOverdue(o)
        setOccurrences(occ.slice(0, 5))
      } catch (err) {
        setError(err.message)
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])

  if (loading) return <div className="p-8">Загрузка...</div>

  return (
    <div className="p-8 space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Дашборд</h1>
      {error && <p className="text-red-500">{error}</p>}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Card title="К оплате" value={summary?.pendingCount ?? '-'} />
        <Card title="Просрочено" value={summary?.overdueCount ?? '-'} />
        <Card title="Сумма (RUB)" value={summary?.totalRub?.toFixed(2) ?? '-'} />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold mb-4 text-gray-900 dark:text-white">Ближайшие платежи</h2>
          <ul className="divide-y dark:divide-gray-700">
            {occurrences.map((o) => (
              <li key={o.id} className="py-3">
                <div className="flex justify-between">
                  <span className="font-medium text-gray-900 dark:text-white">{o.title}</span>
                  <span className="text-gray-600 dark:text-gray-300">{o.amount} {o.currency}</span>
                </div>
                <div className="text-sm text-gray-500 dark:text-gray-400">
                  {o.dueDate ? new Date(o.dueDate).toLocaleDateString() : '-'} ({o.daysUntilDue} дн.)
                </div>
              </li>
            ))}
            {occurrences.length === 0 && <p className="text-gray-500 dark:text-gray-400">Нет данных</p>}
          </ul>
          <Link to="/occurrences" className="mt-4 inline-block text-blue-600 hover:underline">Все периоды</Link>
        </div>

        <div className="bg-white dark:bg-gray-800 rounded-lg shadow p-6">
          <h2 className="text-lg font-semibold mb-4 text-gray-900 dark:text-white">Просроченные</h2>
          <ul className="divide-y dark:divide-gray-700">
            {overdue.map((o) => (
              <li key={o.id} className="py-3">
                <div className="flex justify-between">
                  <span className="font-medium text-gray-900 dark:text-white">{o.title}</span>
                  <span className="text-red-600">{o.amount} {o.currency}</span>
                </div>
                <div className="text-sm text-gray-500 dark:text-gray-400">
                  {o.dueDate ? new Date(o.dueDate).toLocaleDateString() : '-'}
                </div>
              </li>
            ))}
            {overdue.length === 0 && <p className="text-gray-500 dark:text-gray-400">Просрочек нет</p>}
          </ul>
          <Link to="/overdue" className="mt-4 inline-block text-blue-600 hover:underline">Все просроченные</Link>
        </div>
      </div>
    </div>
  )
}
