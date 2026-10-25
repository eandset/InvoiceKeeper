import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { api } from '../lib/api.js';

function todayPlus(days) {
  const d = new Date();
  d.setDate(d.getDate() + days);
  return d.toISOString().slice(0, 10);
}

export default function NewPayment() {
  const navigate = useNavigate();
  const [form, setForm] = useState({
    title: '',
    amount: '',
    currency: 'RUB',
    periodicity: 'monthly',
    nextPaymentDate: todayPlus(7)
  });
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  function update(field) {
    return (event) => setForm((prev) => ({ ...prev, [field]: event.target.value }));
  }

  async function submit(event) {
    event.preventDefault();
    setError('');
    setBusy(true);
    try {
      await api.createPayment({
        title: form.title.trim(),
        amount: Number(form.amount),
        currency: form.currency,
        periodicity: form.periodicity,
        nextPaymentDate: form.nextPaymentDate
      });
      navigate('/app/payments');
    } catch (err) {
      setError(err.message);
    } finally {
      setBusy(false);
    }
  }

  return (
    <>
      <div className="page-head">
        <div className="page-head__title">
          <span className="kicker">Новый платёж</span>
          <h1>Добавить платёж</h1>
          <p className="page-head__lead">Первый период создастся автоматически на указанную дату.</p>
        </div>
        <Link className="btn btn--ghost" to="/app/payments">
          Назад к списку
        </Link>
      </div>

      <div className="card card--form">
        <form className="form" onSubmit={submit}>
          {error && <div className="alert alert--danger">{error}</div>}

          <div className="form__grid">
            <div className="form__field form__field--full">
              <label htmlFor="title">Название</label>
              <input
                id="title"
                value={form.title}
                onChange={update('title')}
                placeholder="Аренда VPS"
                maxLength={255}
                required
              />
            </div>

            <div className="form__field">
              <label htmlFor="amount">Сумма</label>
              <input
                id="amount"
                type="number"
                min="0.01"
                step="0.01"
                value={form.amount}
                onChange={update('amount')}
                placeholder="1500.00"
                required
              />
            </div>

            <div className="form__field">
              <label htmlFor="currency">Валюта</label>
              <select id="currency" value={form.currency} onChange={update('currency')}>
                <option value="RUB">RUB — рубль</option>
                <option value="USD">USD — доллар</option>
                <option value="EUR">EUR — евро</option>
              </select>
            </div>

            <div className="form__field">
              <label htmlFor="periodicity">Периодичность</label>
              <select id="periodicity" value={form.periodicity} onChange={update('periodicity')}>
                <option value="monthly">Ежемесячно</option>
                <option value="weekly">Еженедельно</option>
                <option value="quarterly">Ежеквартально</option>
                <option value="yearly">Ежегодно</option>
              </select>
            </div>

            <div className="form__field">
              <label htmlFor="nextPaymentDate">Первый срок оплаты</label>
              <input
                id="nextPaymentDate"
                type="date"
                value={form.nextPaymentDate}
                onChange={update('nextPaymentDate')}
                required
              />
            </div>
          </div>

          <div className="form__actions">
            <button type="submit" className="btn btn--primary" disabled={busy}>
              {busy ? 'Создаём…' : 'Создать платёж'}
            </button>
            <Link className="btn btn--ghost" to="/app/payments">
              Отмена
            </Link>
          </div>
        </form>
      </div>
    </>
  );
}
