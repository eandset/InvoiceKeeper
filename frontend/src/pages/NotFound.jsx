import { Link } from 'react-router-dom';

export default function NotFound() {
  return (
    <section className="section" style={{ paddingTop: 60 }}>
      <span className="kicker">404</span>
      <h1 style={{ fontSize: 'clamp(2rem, 5vw, 3.4rem)', marginTop: 16 }}>Страница не найдена</h1>
      <p className="muted" style={{ marginTop: 14, maxWidth: '46ch' }}>
        Возможно, ссылка устарела. Вернитесь на главную или откройте панель.
      </p>
      <div className="hero__actions">
        <Link className="btn btn--primary" to="/">
          На главную
        </Link>
        <Link className="btn btn--ghost" to="/app">
          В панель
        </Link>
      </div>
    </section>
  );
}
