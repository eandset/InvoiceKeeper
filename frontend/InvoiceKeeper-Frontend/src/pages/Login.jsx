import { useState } from 'react';
import { Navigate, useLocation, useNavigate } from 'react-router-dom';
import { api, saveSession } from '../lib/api.js';
import { notifySessionChange, useSession } from '../hooks/useSession.js';

export default function Login() {
  const { session } = useSession();
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from || '/app';

  const [username, setUsername] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [busy, setBusy] = useState(false);

  if (session) return <Navigate to={from} replace />;

  async function submit(event) {
    event.preventDefault();
    setError('');
    setBusy(true);
    try {
      const response = await api.login(username.trim(), password);
      saveSession(response.token, response.username, response.role);
      notifySessionChange();
      navigate(from, { replace: true });
    } catch (err) {
      setError(err.status === 401 ? 'Неверный логин или пароль' : err.message);
    } finally {
      setBusy(false);
    }
  }

  function fillDemo() {
    setUsername('admin');
    setPassword('admin123');
    setError('');
  }

  return (
    <section className="auth">
      <div className="auth__copy">
        <span className="kicker">Вход</span>
        <h1 style={{ marginTop: 18 }}>Ваш реестр платежей</h1>
        <p>
          Администратор управляет платежами и пользователями, сотрудники отмечают оплату периодов.
          Сессия хранится в браузере, токен — JWT.
        </p>
        <div className="auth__facts">
          <span className="auth__fact">JWT · 24 часа</span>
          <span className="auth__fact">PostgreSQL</span>
          <span className="auth__fact">REST API</span>
        </div>
      </div>

      <div className="auth__card">
        <h2>Войти в систему</h2>
        <p className="card__hint">Используйте выданные учётные данные.</p>

        <form className="form" onSubmit={submit} style={{ marginTop: 22 }}>
          {error && (
            <div className="alert alert--danger" role="alert">
              {error}
            </div>
          )}

          <div className="form__field">
            <label htmlFor="username">Логин</label>
            <input
              id="username"
              name="username"
              autoComplete="username"
              value={username}
              onChange={(e) => setUsername(e.target.value)}
              placeholder="admin"
              required
            />
          </div>

          <div className="form__field">
            <label htmlFor="password">Пароль</label>
            <input
              id="password"
              name="password"
              type="password"
              autoComplete="current-password"
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              placeholder="••••••••"
              required
            />
          </div>

          <button type="submit" className="btn btn--primary btn--full" disabled={busy}>
            {busy ? 'Проверяем…' : 'Войти'}
          </button>
        </form>

        <div className="auth__demo">
          Демо: логин <code>admin</code>, пароль <code>admin123</code>
          <button type="button" onClick={fillDemo}>
            Подставить
          </button>
        </div>
      </div>
    </section>
  );
}
