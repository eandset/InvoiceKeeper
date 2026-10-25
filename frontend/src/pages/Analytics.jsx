import { useEffect, useState } from 'react';
import { api } from '../lib/api.js';
import { money, statusOf } from '../lib/format.js';

export default function Analytics() {
  const [state, setState] = useState({ loading: true, error: '', summary: null, occurrences: [] });

  useEffect(() => {
    let active = true;
    Promise.all([api.summary(), api.occurrences()])
      .then(([summary, occurrences]) => {
        if (active) setState({ loading: false, error: '', summary, occurrences: occurrences || [] });
      })
      .catch((err) => {
        if (active) setState({ loading: false, error: err.message, summary: null, occurrences: [] });
      });
    return () => {
      active = false;
    };
  }, []);

  const paid = state.occurrences.filter((o) => statusOf(o).key === 'paid').length;
  const overdue = state.occurrences.filter((o) => statusOf(o).key === 'overdue').length;
  const total = state.occurrences.length;
  const unpaid = total - paid;

  const currencies = [
    { code: 'RUB', value: state.summary?.totalRub },
    { code: 'USD', value: state.summary?.totalUsd },
    { code: 'EUR', value: state.summary?.totalEur }
  ];
  const max = Math.max(...currencies.map((c) => Number(c.value) || 0), 1);

  const shares = [
    { label: 'Оплачено', value: paid, tone: 'var(--success)' },
    { label: 'Просрочено', value: overdue, tone: 'var(--danger)' },
    { label: 'Ожидает оплаты', value: Math.max(unpaid - overdue, 0), tone: 'var(--warn)' }
  ];

  return (
    <>
      <div className="page-head">
        <div className="page-head__title">
          <span className="kicker">Аналитика</span>
          <h1>Аналитика</h1>
          <p className="page-head__lead">Структура периодов и суммы к оплате по каждой валюте.</p>
        </div>
      </div>

      {state.error && <div className="alert alert--danger">{state.error}</div>}

      {state.loading ? (
        <div className="loading-block">
          <span className="spinner" /> Считаем показатели…
        </div>
      ) : (
        <>
          <div className="grid-stats">
            <article className="metric">
              <p className="metric__label">Всего периодов</p>
              <p className="metric__value">{total}</p>
            </article>
            <article className="metric">
              <p className="metric__label">Оплачено</p>
              <p className="metric__value metric__value--success">{paid}</p>
            </article>
            <article className="metric">
              <p className="metric__label">Просрочено</p>
              <p className="metric__value metric__value--danger">{overdue}</p>
            </article>
            <article className="metric">
              <p className="metric__label">К оплате</p>
              <p className="metric__value metric__value--warn">{Math.max(unpaid - overdue, 0)}</p>
            </article>
          </div>

          <div className="grid-two">
            <section className="card">
              <header className="card__head">
                <h2>Суммы к оплате по валютам</h2>
              </header>
              <div className="bars">
                {currencies.map((c) => (
                  <div key={c.code}>
                    <div className="bar__top">
                      <span className="bar__label">{c.code}</span>
                      <span className="bar__value">{money(c.value, c.code)}</span>
                    </div>
                    <div className="bar__track">
                      <div
                        className="bar__fill"
                        style={{ width: `${Math.round(((Number(c.value) || 0) / max) * 100)}%` }}
                      />
                    </div>
                  </div>
                ))}
              </div>
            </section>

            <section className="card">
              <header className="card__head">
                <h2>Структура периодов</h2>
              </header>
              <div className="bars">
                {shares.map((s) => (
                  <div key={s.label}>
                    <div className="bar__top">
                      <span className="bar__label">{s.label}</span>
                      <span className="bar__value">{s.value}</span>
                    </div>
                    <div className="bar__track">
                      <div
                        className="bar__fill"
                        style={{
                          width: `${total ? Math.round((s.value / total) * 100) : 0}%`,
                          background: s.tone,
                          boxShadow: 'none'
                        }}
                      />
                    </div>
                  </div>
                ))}
              </div>
              <p className="card__hint" style={{ marginTop: 22 }}>
                Просроченные периоды считаются по фактическому сроку оплаты, а не по дате создания.
              </p>
            </section>
          </div>
        </>
      )}
    </>
  );
}
