import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../lib/api'
import { Card } from '../components/ui'

function Widget({ title, value, subtitle, accent = 'emerald' }) {
  const accents = {
    emerald: 'bg-emerald-50 dark:bg-emerald-900/20 text-emerald-700 dark:text-emerald-300',
    amber: 'bg-amber-50 dark:bg-amber-900/20 text-amber-700 dark:text-amber-300',
    rose: 'bg-rose-50 dark:bg-rose-900/20 text-rose-700 dark:text-rose-300'
  }

  return (
    <Card className={`${accents[accent]}`}>
      <h3 className="text-sm font-medium opacity-80">{title}</h3>
      <p className="mt-2 text-3xl font-bold">{value}</p>
      {subtitle && <p className="text-xs mt-1 opacity-70">{subtitle}</p>}
    </Card>
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
        setOverdue(o.slice(0, 5))
        setOccurrences(occ.slice(0, 5))
      } catch (err) {
        setError(err.message)
      } finally {
        setLoading(false)
      }
    }
    load()
  }, [])

  if (loading) return <div className="p-8 text-gray-500 dark:text-gray-400">Загрузка...</div>

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Дашборд</h1>
        <Link to="/payments/new" className="px-4 py-2 bg-emerald-700 hover:bg-emerald-800 text-white rounded-lg text-sm font-medium">+ Новый платёж</Link>
      </div>
      {error && <p className="text-red-500">{error}</p>}

      <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
        <Widget title="К оплате" value={summary?.pendingCount ?? 0} accent="emerald" />
        <Widget title="Просрочено" value={summary?.overdueCount ?? 0} accent="rose" />
        <Widget title="Сумма (RUB)" value={`${(summary?.totalRub ?? 0).toFixed(2)} `} accent="amber" />
      </div>

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        <Card>
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Ближайшие платежи</h2>
            <Link to="/occurrences" className="text-sm text-emerald-700 dark:text-emerald-400 hover:underline">Все</Link>
          </div>
          <ul className="divide-y dark:divide-gray-700">
            {occurrences.map((o) => (
              <li key={o.id} className="py-3 flex justify-between items-center">
                <div>
                <p className="font-medium text-gray-900 dark:text-white">{o.title}</p>
                <p className="text-xs text-gray-500 dark:text-gray-400">{o.dueDate ? new Date(o.dueDate).toLocaleDateString() : '-'}</p>
                </div>
                <span className="text-sm font-medium text-gray-700 dark:text-gray-300">{o.amount} {o.currency}</span>
              </li>
            ))}
            {occurrences.length === 0 && <p className="text-gray-500 dark:text-gray-400 py-4 text-center">Нет данных</p>}
          </ul>
        </Card>

        <Card>
          <div className="flex justify-between items-center mb-4">
            <h2 className="text-lg font-semibold text-gray-900 dark:text-white">Просроченные</h2>
            <Link to="/overdue" className="text-sm text-emerald-700 dark:text-emerald-400 hover:underline">Все</Link>
          </div>
          <ul className="divide-y dark:divide-gray-700">
            {overdue.map((o) => (
              <li key={o.id} className="py-3 flex justify-between items-center">
                <div>
                <p className="font-medium text-gray-900 dark:text-white">{o.title}</p>
                <p className="text-xs text-gray-500 dark:text-gray-400">{o.dueDate ? new Date(o.dueDate).toLocaleDateString() : '-'}</p>
                </div>
                <span className="text-sm font-medium text-rose-600">{o.amount} {o.currency}</span>
              </li>
            ))}
            {overdue.length === 0 && <p className="text-gray-500 dark:text-gray-400 py-4 text-center">Просрочек нет</p>}
          </ul>
        </Card>
      </div>
    </div>
  )
}
