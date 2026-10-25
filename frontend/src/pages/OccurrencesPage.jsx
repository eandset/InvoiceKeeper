import { useEffect, useState } from 'react'
import { api } from '../lib/api'
import { Card } from '../components/ui'

export default function OccurrencesPage() {
  const [occurrences, setOccurrences] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [filter, setFilter] = useState({ isPaid: '', isOverdue: '' })

  const load = async () => {
    try {
      const params = {}
      if (filter.isPaid !== '') params.isPaid = filter.isPaid === 'true'
      if (filter.isOverdue !== '') params.isOverdue = filter.isOverdue === 'true'
      const data = await api.getOccurrences(params)
      setOccurrences(data)
    } catch (err) {
      setError(err.message)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    load()
  }, [filter])

  const markPaid = async (id) => {
    try {
      await api.updateOccurrence(id, { isPaid: true, actualPaymentDate: new Date().toISOString() })
      load()
    } catch (err) {
      setError(err.message)
    }
  }

  if (loading) return <div className="text-gray-500 dark:text-gray-400">Загрузка...</div>

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Периоды оплаты</h1>
      {error && <p className="text-red-500">{error}</p>}
      <div className="flex gap-4">
        <select value={filter.isPaid} onChange={(e) => setFilter({ ...filter, isPaid: e.target.value })} className="rounded-lg border px-3 py-2 dark:bg-gray-700 dark:text-white border-gray-300 dark:border-gray-600">
          <option value="">Все статусы</option>
          <option value="true">Оплачено</option>
          <option value="false">Не оплачено</option>
        </select>
        <select value={filter.isOverdue} onChange={(e) => setFilter({ ...filter, isOverdue: e.target.value })} className="rounded-lg border px-3 py-2 dark:bg-gray-700 dark:text-white border-gray-300 dark:border-gray-600">
          <option value="">Все</option>
          <option value="true">Просрочено</option>
          <option value="false">Не просрочено</option>
        </select>
      </div>
      <Card>
        <table className="min-w-full">
          <thead className="bg-stone-50 dark:bg-gray-700">
            <tr>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Название</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Сумма</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Срок</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Статус</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200"></th>
            </tr>
          </thead>
          <tbody className="divide-y dark:divide-gray-700">
            {occurrences.map((o) => (
              <tr key={o.id} className="hover:bg-stone-50 dark:hover:bg-gray-700/50">
                <td className="px-4 py-3 text-gray-900 dark:text-white">{o.title}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{o.amount} {o.currency}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">
                  {o.dueDate ? new Date(o.dueDate).toLocaleDateString() : '-'} ({o.daysUntilDue} дн.)
                </td>
                <td className="px-4 py-3">
                  <span className={`inline-flex px-2 py-1 rounded-full text-xs font-medium ${
                    o.status === 'Paid' ? 'bg-emerald-100 text-emerald-700' :
                    o.status === 'Overdue' ? 'bg-rose-100 text-rose-700' : 'bg-amber-100 text-amber-700'
                  }`}>
                    {o.status === 'Paid' ? 'Оплачено' : o.status === 'Overdue' ? 'Просрочено' : 'Не оплачено'}
                  </span>
                </td>
                <td className="px-4 py-3">
                  {o.status !== 'Paid' && (
                    <button onClick={() => markPaid(o.id)} className="text-emerald-700 hover:text-emerald-900 text-sm font-medium">Оплатить</button>
                  )}
                </td>
              </tr>
            ))}
            {occurrences.length === 0 && (
              <tr>
                <td colSpan={5} className="px-4 py-6 text-center text-gray-500 dark:text-gray-400">Нет периодов</td>
              </tr>
            )}
          </tbody>
        </table>
      </Card>
    </div>
  )
}
