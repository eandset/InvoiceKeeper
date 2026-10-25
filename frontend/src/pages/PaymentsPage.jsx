import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { api } from '../lib/api'

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

  if (loading) return <div className="p-8">Загрузка...</div>

  return (
    <div className="p-8">
      <div className="flex justify-between items-center mb-6">
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Регулярные платежи</h1>
        <Link to="/payments/new" className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">Добавить</Link>
      </div>
      {error && <p className="text-red-500 mb-4">{error}</p>}
      <div className="bg-white dark:bg-gray-800 rounded-lg shadow overflow-hidden">
        <table className="min-w-full">
          <thead className="bg-gray-100 dark:bg-gray-700">
            <tr>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Название</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Сумма</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Следующий</th>
              <th className="text-left px-4 py-3 text-sm font-medium text-gray-700 dark:text-gray-200">Действия</th>
            </tr>
          </thead>
          <tbody className="divide-y dark:divide-gray-700">
            {payments.map((p) => (
              <tr key={p.id}>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{p.title}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">{p.amount} {p.currency}</td>
                <td className="px-4 py-3 text-gray-900 dark:text-white">
                  {p.nextPaymentDate ? new Date(p.nextPaymentDate).toLocaleDateString() : '-'}
                </td>
                <td className="px-4 py-3">
                  <button
                    onClick={() => handleDelete(p.id)}
                    className="text-red-600 hover:text-red-800 text-sm"
                  >
                    Удалить
                  </button>
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
      </div>
    </div>
  )
}
