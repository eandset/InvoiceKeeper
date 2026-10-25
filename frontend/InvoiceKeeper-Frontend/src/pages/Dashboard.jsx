import { useEffect, useState } from 'react';
import { Link } from 'react-router-dom';
import { api } from '../lib/api.js';
import { date, money, statusOf } from '../lib/format.js';
import { useSession } from '../hooks/useSession.js';

export default function Dashboard() {
  const { session, isAdmin } = useSession();
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

  const today = new Date();
  today.setHours(0, 0, 0, 0);

  const upcoming = state.occurrences
    .filter((o) => statusOf(o).key !== 'paid' && new Date(o.dueDate) >= today)
    .sort((a, b) => new Date(a.dueDate) - new Date(b.dueDate))
    .slice(0, 6);

  const overdue = state.occurrences.filter((o) => statusOf(o).key === 'overdue').slice(0, 6);

  return (
    <>
      <div className="page-head">
        <div className="page-head__title">
          <span className="kicker">Обзор</span>
          <h1>Привет, {session?.user?.username}</h1>
          <p className="page-head__lead">
            Сводка по неоплаченным периодам, ближайшие списания и просрочки — на одном экране.
          </p>
        </div>
        <div className="page-head__actions">
          <Link className="btn btn--ghost" to="/app/occurrences">
            Все периоды
          </Link>
          {isAdmin && (
            <Link className="btn btn--primary" to="/app/payments/new">
              + Новый платёж
            </Link>
          )}
        </div>
      </div>

      {state.error && <div className="alert alert--danger">{state.error}</div>}

      {state.loading ? (
        <div className="loading-block">
          <span className="spinner" /> Загружаем данные…
        </div>
      ) : (
        <>
          <div className="grid-stats">
            <article className="metric">
              <p className="metric__label">К оплате · RUB</p>
              <p className="metric__value">{money(state.summary?.totalRub, 'RUB')}</p>
              <p className="metric__hint">{state.summary?.pendingCount ?? 0} неоплаченных периодов</p>
            </article>
            <article className="metric">
              <p className="metric__label">К оплате · USD</p>
              <p className="metric__value metric__value--success">{money(state.summary?.totalUsd, 'USD')}</p>
              <p className="metric__hint">{money(state.summary?.totalEur, 'EUR')} в EUR</p>
            </article>
            <article className="metric">
              <p className="metric__label">Просрочено</p>
              <p className="metric__value metric__value--danger">{state.summary?.overdueCount ?? 0}</p>
              <p className="metric__hint">периодов с истёкшим сроком</p>
            </article>
            <article className="metric">
              <p className="metric__label">Всего периодов</p>
              <p className="metric__value metric__value--accent">{state.occurrences.length}</p>
              <p className="metric__hint">включая оплаченные</p>
            </article>
          </div>

          <div className="grid-two">
            <section className="card">
              <header className="card__head">
                <h2>Ближайшие платежи</h2>
                <Link className="card__link" to="/app/occurrences">
                  Все периоды
                </Link>
              </header>
              {upcoming.length === 0 ? (
                <p className="card__hint">Нет предстоящих платежей.</p>
              ) : (
                <div className="rows">
                  {upcoming.map((o) => (
                    <div className="row" key={o.id}>
                      <span className="row__title">{o.title}</span>
                      <span className="row__date mono">{date(o.dueDate)}</span>
                      <span className="row__amount mono">{money(o.amount, o.currency)}</span>
                    </div>
                  ))}
                </div>
              )}
            </section>

            <section className="card">
              <header className="card__head">
                <h2>Просроченные</h2>
                <Link className="card__link" to="/app/occurrences?status=overdue">
                  Открыть
                </Link>
              </header>
              {overdue.length === 0 ? (
                <p className="card__hint">Просрочек нет — отличная работа.</p>
              ) : (
                <div className="rows">
                  {overdue.map((o) => (
                    <div className="row row--overdue" key={o.id}>
                      <span className="row__title">{o.title}</span>
                      <span className="row__date mono">{date(o.dueDate)}</span>
                      <span className="row__amount mono">{money(o.amount, o.currency)}</span>
                    </div>
                  ))}
                </div>
              )}
            </section>
          </div>
        </>
      )}
    </>
  );
}
