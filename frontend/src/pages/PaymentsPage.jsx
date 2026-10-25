import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../lib/api'
import { Card, Button } from '../components/ui'

export default function PaymentsPage() {
  const [payments, setPayments] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    api.getPayments()
      .then(setPayments)
      .catch((err) => setError(err.message))
      .finally(() => setLoading(false))
  }, [])

  const handleDelete = async (id) => {
    if (!confirm('Удалить платёж?')) return
    try {
      await api.deletePayment(id)
      setPayments((prev) => prev.filter((p) => p.id !== id))
    } catch (err) {
      setError(err.message)
    }
  }

  if (loading) return <div className="p-8 text-gray-500 dark:text-gray-400">Загрузка...</div>

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Регулярные платежи</h1>
        <Link to="/payments/new" className="px-4 py-2 bg-emerald-700 hover:bg-emerald-800 text-white rounded-lg text-sm font-medium">+ Новый платёж</Link>
      </div>
      {error && <p className="text-red-500">{error}</p>}
      <Card>
        <table className="min-w-full">
          <thead className="bg-stone-50 dark:bg-gray-700">
            <tr>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Название</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Сумма</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Следующий</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200"></th>
            </tr>
          </thead>
          <tbody className="divide-y dark:divide-gray-700">
            {payments.map((p) => (
              <tr key={p.id} className="hover:bg-stone-50 dark:hover:bg-gray-700/50">
                <td className="px-4 py-3 text-gray-900 dark:text-white">{p.title}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{p.amount} {p.currency}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{p.nextPaymentDate ? new Date(p.nextPaymentDate).toLocaleDateString() : '-'}</td>
                <td className="px-4 py-3">
                  <button onClick={() => handleDelete(p.id)} className="text-red-600 hover:text-red-800 text-sm font-medium">Удалить</button>
                </td>
              </tr>
            ))}
            {payments.length === 0 && (
              <tr>
                <td colSpan={4} className="px-4 py-6 text-center text-gray-500 dark:text-gray-400">Нет платежей</td>
              </tr>
            )}
          </tbody>
        </table>
      </Card>
    </div>
  )
}
