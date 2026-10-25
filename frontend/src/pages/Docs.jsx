import { Link } from 'react-router-dom';

const LINKS = [
  { href: '/swagger/index.html', label: 'Swagger UI', hint: 'Интерактивная документация REST API' },
  { href: '/api/v1/docs', label: 'Редирект API-документации', hint: 'Служебный адрес бэкенда' }
];

export default function Docs() {
  return (
    <>
      <div className="page-head">
        <div className="page-head__title">
          <span className="kicker">Документация</span>
          <h1>REST API</h1>
          <p className="page-head__lead">
            Все операции панели доступны через API бэкенда: платежи, периоды, сводка, аналитика и пользователи.
          </p>
        </div>
        <div className="page-head__actions">
          <Link className="btn btn--ghost" to="/app">
            В панель
          </Link>
        </div>
      </div>

      <div className="grid-two">
        {LINKS.map((item) => (
          <section className="card" key={item.href}>
            <header className="card__head">
              <h2>{item.label}</h2>
            </header>
            <p className="card__hint">{item.hint}</p>
            <div className="form__actions">
              <a className="btn btn--primary" href={item.href} target="_blank" rel="noreferrer">
                Открыть
              </a>
            </div>
          </section>
        ))}
      </div>

      <section className="card" style={{ marginTop: 22 }}>
        <header className="card__head">
          <h2>Ключевые эндпоинты</h2>
        </header>
        <div className="rows">
          {[
            ['POST', '/api/v1/auth/login', 'Вход и выдача JWT'],
            ['GET', '/api/v1/payments/summary', 'Итоги к оплате по валютам'],
            ['GET', '/api/v1/payment-occurrences', 'Периоды платежей'],
            ['PATCH', '/api/v1/payment-occurrences/{id}', 'Отметка оплаты периода'],
            ['GET', '/api/v1/recurring-payments', 'Регулярные платежи'],
            ['GET', '/api/v1/users', 'Пользователи (только Admin)']
          ].map(([method, path, hint]) => (
            <div className="row" key={method + path} style={{ gridTemplateColumns: 'auto minmax(0,1fr)' }}>
              <span className="mono faint" style={{ fontSize: 11, letterSpacing: '0.08em' }}>
                {method}
              </span>
              <span>
                <span className="row__title mono">{path}</span>
                <span className="table__sub">{hint}</span>
              </span>
            </div>
          ))}
        </div>
      </section>
    </>
  );
}
