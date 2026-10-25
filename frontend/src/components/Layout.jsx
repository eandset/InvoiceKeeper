import { useEffect, useState } from 'react';
import { Link, NavLink, Outlet, useLocation, useNavigate } from 'react-router-dom';
import { useSession } from '../hooks/useSession.js';
import ThemeControls from './ThemeControls.jsx';

const BOT_URL = import.meta.env.VITE_BOT_URL || '';
const HAS_BOT_LINK = BOT_URL && BOT_URL !== '#' && !BOT_URL.includes('your_bot_username');

const NAV = [
  { to: '/app', label: 'Обзор', end: true },
  { to: '/app/occurrences', label: 'Периоды' },
  { to: '/app/payments', label: 'Платежи' },
  { to: '/app/analytics', label: 'Аналитика' },
  { to: '/docs', label: 'API' }
];

export default function Layout() {
  const [menuOpen, setMenuOpen] = useState(false);
  const { session, logout, isAdmin } = useSession();
  const location = useLocation();
  const navigate = useNavigate();
  const isLanding = location.pathname === '/';

  useEffect(() => {
    setMenuOpen(false);
  }, [location.pathname]);

  const links = session
    ? isAdmin
      ? [...NAV, { to: '/app/users', label: 'Пользователи' }]
      : NAV
    : [];

  function handleLogout() {
    logout();
    navigate('/');
  }

  return (
    <div className="shell">
      <header className="topbar">
        <div className="topbar__inner">
          <Link className="brand" to={session ? '/app' : '/'} aria-label="Invoice Keeper — на главную">
            <span className="brand__mark">IK</span>
            <span className="brand__text">
              <strong>Invoice Keeper</strong>
              <small>Regular Payments</small>
            </span>
          </Link>

          <nav className={`nav ${menuOpen ? 'is-open' : ''}`} id="primary-nav">
            {session ? (
              links.map((item) => (
                <NavLink
                  key={item.to}
                  to={item.to}
                  end={item.end}
                  className={({ isActive }) => `nav__link ${isActive ? 'is-active' : ''}`}
                >
                  {item.label}
                </NavLink>
              ))
            ) : (
              <>
                <a className="nav__link" href="/#features">
                  Возможности
                </a>
                <a className="nav__link" href="/#how">
                  Как работает
                </a>
                <NavLink to="/login" className="nav__link">
                  Войти
                </NavLink>
              </>
            )}
          </nav>

          <div className="topbar__actions">
            <ThemeControls />

            {session ? (
              <>
                <span className="user-pill">
                  <span className="user-pill__avatar">{session.user?.username?.[0]?.toUpperCase() || '?'}</span>
                  <span className="user-pill__name">{session.user?.username}</span>
                </span>
                <button type="button" className="btn btn--ghost btn--sm" onClick={handleLogout}>
                  Выйти
                </button>
              </>
            ) : (
              !isLanding && (
                <Link className="btn btn--primary btn--sm" to="/login">
                  Войти
                </Link>
              )
            )}

            <button
              type="button"
              className="burger"
              aria-label="Открыть меню"
              aria-expanded={menuOpen}
              onClick={() => setMenuOpen((v) => !v)}
            >
              <span />
              <span />
              <span />
            </button>
          </div>
        </div>
      </header>

      <main className={isLanding ? 'page page--landing' : 'page'}>
        <Outlet />
      </main>

      <footer className="footer">
        <div className="footer__inner">
          <div className="footer__brand">
            <strong>Invoice Keeper</strong>
            <span>© {new Date().getFullYear()}</span>
          </div>
          <div className="footer__meta">
            <span>Регулярные платежи · RUB / USD / EUR</span>
            {HAS_BOT_LINK && (
              <a href={BOT_URL} target="_blank" rel="noreferrer">
                Telegram-бот
              </a>
            )}
            <a href="/swagger/index.html" target="_blank" rel="noreferrer">
              Swagger
            </a>
          </div>
        </div>
      </footer>
    </div>
  );
}
