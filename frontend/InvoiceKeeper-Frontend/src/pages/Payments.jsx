import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../lib/api.js';
import { date, money, periodicityLabel } from '../lib/format.js';
import { useSession } from '../hooks/useSession.js';

export default function Payments() {
  const { isAdmin } = useSession();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [busyId, setBusyId] = useState(null);

  async function load() {
    setLoading(true);
    setError('');
    try {
      setItems((await api.payments()) || []);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  async function remove(payment) {
    if (!window.confirm(`Удалить платёж «${payment.title}»?`)) return;
    setBusyId(payment.id);
    setError('');
    try {
      await api.deletePayment(payment.id);
      setItems((await api.payments()) || []);
    } catch (err) {
      setError(err.message);
    } finally {
      setBusyId(null);
    }
  }

  return (
    <>
      <div className="page-head">
        <div className="page-head__title">
          <span className="kicker">Платежи</span>
          <h1>Регулярные платежи</h1>
          <p className="page-head__lead">Шаблоны регулярных списаний и дата следующего срока оплаты.</p>
        </div>
        {isAdmin && (
          <div className="page-head__actions">
            <Link className="btn btn--primary" to="/app/payments/new">
              + Добавить платёж
            </Link>
          </div>
        )}
      </div>

      {error && <div className="alert alert--danger">{error}</div>}

      {loading ? (
        <div className="loading-block">
          <span className="spinner" /> Загружаем платежи…
        </div>
      ) : items.length === 0 ? (
        <div className="empty-state">
          <p>Платежей пока нет. {isAdmin ? 'Добавьте первый регулярный платёж.' : 'Администратор ещё не создал платежи.'}</p>
        </div>
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Название</th>
                <th>Сумма</th>
                <th>Периодичность</th>
                <th>Следующий срок</th>
                <th>Периоды</th>
                {isAdmin && <th className="col-actions">Действие</th>}
              </tr>
            </thead>
            <tbody>
              {items.map((p) => (
                <tr key={p.id}>
                  <td>
                    <span className="table__title">{p.title}</span>
                    {p.description && <span className="table__sub">{p.description}</span>}
                  </td>
                  <td className="mono">{money(p.amount, p.currency)}</td>
                  <td>{periodicityLabel[p.periodicity] || p.periodicity}</td>
                  <td className="mono">{date(p.nextPaymentDate)}</td>
                  <td className="mono">
                    {p.paidOccurrencesCount ?? 0} / {(p.paidOccurrencesCount ?? 0) + (p.unpaidOccurrencesCount ?? 0)}
                  </td>
                  {isAdmin && (
                    <td className="col-actions">
                      <button
                        type="button"
                        className="btn btn--danger btn--sm"
                        disabled={busyId === p.id}
                        onClick={() => remove(p)}
                      >
                        Удалить
                      </button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
