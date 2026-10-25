import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../lib/api'

export default function PaymentNewPage() {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    title: '',
    amount: '',
    currency: 'RUB',
    nextPaymentDate: '',
    periodicity: 'monthly'
  })
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value })
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    setSaving(true)
    try {
      await api.createPayment({
        title: form.title,
        amount: Number(form.amount),
        currency: form.currency,
        nextPaymentDate: form.nextPaymentDate,
        periodicity: form.periodicity
      })
      navigate('/payments')
    } catch (err) {
      setError(err.message)
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="p-8 max-w-xl">
      <h1 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Новый платёж</h1>
      {error && <p className="text-red-500 mb-4">{error}</p>}
      <form onSubmit={handleSubmit} className="space-y-4 bg-white dark:bg-gray-800 p-6 rounded-lg shadow">
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Название</label>
          <input name="title" value={form.title} onChange={handleChange} required className="w-full mt-1 rounded border px-3 py-2 dark:bg-gray-700 dark:text-white dark:border-gray-600" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Сумма</label>
          <input name="amount" type="number" step="0.01" value={form.amount} onChange={handleChange} required className="w-full mt-1 rounded border px-3 py-2 dark:bg-gray-700 dark:text-white dark:border-gray-600" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Валюта</label>
          <select name="currency" value={form.currency} onChange={handleChange} className="w-full mt-1 rounded border px-3 py-2 dark:bg-gray-700 dark:text-white dark:border-gray-600">
            <option value="RUB">RUB</option>
            <option value="USD">USD</option>
            <option value="EUR">EUR</option>
          </select>
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Следующий платёж</label>
          <input name="nextPaymentDate" type="date" value={form.nextPaymentDate} onChange={handleChange} required className="w-full mt-1 rounded border px-3 py-2 dark:bg-gray-700 dark:text-white dark:border-gray-600" />
        </div>
        <div>
          <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">Периодичность</label>
          <select name="periodicity" value={form.periodicity} onChange={handleChange} className="w-full mt-1 rounded border px-3 py-2 dark:bg-gray-700 dark:text-white dark:border-gray-600">
            <option value="monthly">monthly</option>
            <option value="weekly">weekly</option>
            <option value="yearly">yearly</option>
          </select>
        </div>
        <button disabled={saving} className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700 disabled:opacity-50">{saving ? 'Сохранение...' : 'Создать'}</button>
      </form>
    </div>
  )
}
