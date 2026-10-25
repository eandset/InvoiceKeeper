import { useEffect, useState } from 'react'
import { api } from '../lib/api'

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

  if (loading) return <div className="p-8">Загрузка...</div>

  return (
    <div className="p-8">
      <h1 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Периоды оплаты</h1>
      {error && <p className="text-red-500 mb-4">{error}</p>}
      <div className="flex gap-4 mb-4">
        <select value={filter.isPaid} onChange={(e) => setFilter({ ...filter, isPaid: e.target.value })} className="rounded border px-3 py-2 dark:bg-gray-700 dark:text-white dark:border-gray-600">
          <option value="">Все статусы</option>
          <option value="true">Оплачено</option>
          <option value="false">Не оплачено</option>
        </select>
        <select value={filter.isOverdue} onChange={(e) => setFilter({ ...filter, isOverdue: e.target.value })} className="rounded border px-3 py-2 dark:bg-gray-700 dark:text-white dark:border-gray-600">
          <option value="">Все</option>
          <option value="true">Просрочено</option>
          <option value="false">Не просрочено</option>
        </select>
      </div>
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden">
        <table className="min-w-full">
          <thead className="bg-gray-100 dark:bg-gray-700">
            <tr>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Название</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Сумма</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Срок</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Статус</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Действия</th>
            </tr>
          </thead>
          <tbody className="divide-y dark:divide-gray-700">
            {occurrences.map((o) => (
              <tr key={o.id}>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{o.title}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{o.amount} {o.currency}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">
                  {o.dueDate ? new Date(o.dueDate).toLocaleDateString() : '-'} ({o.daysUntilDue} дн.)
                </td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{o.status}</td>
                <td className="px-4 py-3">
                  {o.status !== 'Paid' && (
                    <button onClick={() => markPaid(o.id)} className="text-green-600 hover:text-green-800 text-sm">Оплатить</button>
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
      </div>
    </div>
  )
}
