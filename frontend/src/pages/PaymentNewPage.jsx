import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import { api } from '../lib/api'
import { Card, Button } from '../components/ui'

export default function PaymentNewPage() {
  const navigate = useNavigate()
  const [form, setForm] = useState({
    title: '',
    amount: '',
    currency: 'RUB',
    nextPaymentDate: '',
    periodicity: 'monthly'
  })
  const [errors, setErrors] = useState({})
  const [error, setError] = useState('')
  const [saving, setSaving] = useState(false)

  const validate = () => {
    const newErrors = {}
    if (!form.title.trim()) newErrors.title = 'Введите название'
    if (!form.amount) newErrors.amount = 'Введите сумму'
    else if (Number(form.amount) <= 0) newErrors.amount = 'Сумма должна быть больше 0'
    if (!form.nextPaymentDate) newErrors.nextPaymentDate = 'Выберите дату'
    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }

  const handleChange = (e) => {
    setForm({ ...form, [e.target.name]: e.target.value })
    if (errors[e.target.name]) {
      setErrors({ ...errors, [e.target.name]: '' })
    }
  }

  const handleSubmit = async (e) => {
    e.preventDefault()
    setError('')
    if (!validate()) return

    setSaving(true)
    try {
      await api.createPayment({
        title: form.title.trim(),
        amount: Number(form.amount),
        currency: form.currency,
        nextPaymentDate: form.nextPaymentDate,
        periodicity: form.periodicity
      })
      navigate('/payments')
    } catch (err) {
      setError(err.message || 'Не удалось создать платёж')
    } finally {
      setSaving(false)
    }
  }

  return (
    <div className="max-w-xl">
      <h1 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Новый платёж</h1>
      {error && <p className="text-red-500 mb-4">{error}</p>}
      <Card>
        <form onSubmit={handleSubmit} className="space-y-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Название</label>
            <input name="title" value={form.title} onChange={handleChange} className={`w-full rounded-lg border px-3 py-2 dark:bg-gray-700 dark:text-white ${errors.title ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}`} />
            {errors.title && <p className="text-red-500 text-xs mt-1">{errors.title}</p>}
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Сумма</label>
              <input name="amount" type="number" step="0.01" value={form.amount} onChange={handleChange} className={`w-full rounded-lg border px-3 py-2 dark:bg-gray-700 dark:text-white ${errors.amount ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}`} />
              {errors.amount && <p className="text-red-500 text-xs mt-1">{errors.amount}</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Валюта</label>
              <select name="currency" value={form.currency} onChange={handleChange} className="w-full rounded-lg border px-3 py-2 dark:bg-gray-700 dark:text-white border-gray-300 dark:border-gray-600">
                <option value="RUB">RUB</option>
                <option value="USD">USD</option>
                <option value="EUR">EUR</option>
              </select>
            </div>
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Дата платежа</label>
              <input name="nextPaymentDate" type="date" value={form.nextPaymentDate} onChange={handleChange} className={`w-full rounded-lg border px-3 py-2 dark:bg-gray-700 dark:text-white ${errors.nextPaymentDate ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'}`} />
              {errors.nextPaymentDate && <p className="text-red-500 text-xs mt-1">{errors.nextPaymentDate}</p>}
            </div>
            <div>
              <label className="block text-sm font-medium text-gray-700 dark:text-gray-300 mb-1">Периодичность</label>
              <select name="periodicity" value={form.periodicity} onChange={handleChange} className="w-full rounded-lg border px-3 py-2 dark:bg-gray-700 dark:text-white border-gray-300 dark:border-gray-600">
                <option value="monthly">Раз в месяц</option>
                <option value="weekly">Раз в неделю</option>
                <option value="yearly">Раз в год</option>
              </select>
            </div>
          </div>
          <Button type="submit" disabled={saving}>{saving ? 'Сохранение...' : 'Создать'}</Button>
        </form>
      </Card>
    </div>
  )
}
