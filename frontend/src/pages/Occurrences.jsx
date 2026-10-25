import { useEffect, useMemo, useState } from 'react';
import { useSearchParams } from 'react-router-dom';
import { api } from '../lib/api.js';
import { date, money, statusOf } from '../lib/format.js';

const FILTERS = [
  { id: '', label: 'Все' },
  { id: 'unpaid', label: 'Не оплачены' },
  { id: 'overdue', label: 'Просрочены' },
  { id: 'paid', label: 'Оплачены' }
];

export default function Occurrences() {
  const [params, setParams] = useSearchParams();
  const filter = params.get('status') || '';
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [busyId, setBusyId] = useState(null);

  async function load() {
    setLoading(true);
    setError('');
    try {
      setItems((await api.occurrences()) || []);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  const visible = useMemo(() => {
    if (!filter) return items;
    return items.filter((o) => statusOf(o).key === filter);
  }, [items, filter]);

  async function togglePaid(occurrence) {
    const paid = statusOf(occurrence).key === 'paid';
    setBusyId(occurrence.id);
    setError('');
    try {
      await api.setOccurrenceStatus(occurrence.id, !paid);
      setItems((await api.occurrences()) || []);
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
          <span className="kicker">Периоды</span>
          <h1>Периоды платежей</h1>
          <p className="page-head__lead">Каждый период — отдельная оплата. Отметьте оплату или снимите отметку.</p>
        </div>
        <div className="filters">
          {FILTERS.map((f) => (
            <button
              key={f.id || 'all'}
              type="button"
              className={`chip ${filter === f.id ? 'chip--active' : ''}`}
              onClick={() => (f.id ? setParams({ status: f.id }) : setParams({}))}
            >
              {f.label}
            </button>
          ))}
        </div>
      </div>

      {error && <div className="alert alert--danger">{error}</div>}

      {loading ? (
        <div className="loading-block">
          <span className="spinner" /> Загружаем периоды…
        </div>
      ) : visible.length === 0 ? (
        <div className="empty-state">
          <p>Периоды не найдены.</p>
        </div>
      ) : (
        <div className="table-wrap">
          <table className="table">
            <thead>
              <tr>
                <th>Название</th>
                <th>Сумма</th>
                <th>Срок оплаты</th>
                <th>Статус</th>
                <th className="col-actions">Действие</th>
              </tr>
            </thead>
            <tbody>
              {visible.map((o) => {
                const status = statusOf(o);
                const paid = status.key === 'paid';
                return (
                  <tr key={o.id}>
                    <td>
                      <span className="table__title">{o.title}</span>
                    </td>
                    <td className="mono">{money(o.amount, o.currency)}</td>
                    <td className="mono">{date(o.dueDate)}</td>
                    <td>
                      <span
                        className={`badge ${
                          paid ? 'badge--success' : status.key === 'overdue' ? 'badge--danger' : 'badge--neutral'
                        }`}
                      >
                        {status.label}
                      </span>
                    </td>
                    <td className="col-actions">
                      <button
                        type="button"
                        className={`btn btn--sm ${paid ? 'btn--ghost' : 'btn--success'}`}
                        disabled={busyId === o.id}
                        onClick={() => togglePaid(o)}
                      >
                        {busyId === o.id ? '…' : paid ? 'Снять оплату' : 'Оплатить'}
                      </button>
                    </td>
                  </tr>
                );
              })}
            </tbody>
          </table>
        </div>
      )}
    </>
  );
}
