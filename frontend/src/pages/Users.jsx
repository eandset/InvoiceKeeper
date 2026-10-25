import { useEffect, useState } from 'react';
import { api } from '../lib/api.js';
import { date } from '../lib/format.js';
import { useSession } from '../hooks/useSession.js';

export default function Users() {
  const { session } = useSession();
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [form, setForm] = useState({ username: '', password: '' });
  const [creating, setCreating] = useState(false);
  const [busyId, setBusyId] = useState(null);

  async function load() {
    setLoading(true);
    setError('');
    try {
      setItems((await api.users()) || []);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    load();
  }, []);

  async function submit(event) {
    event.preventDefault();
    setError('');
    setCreating(true);
    try {
      await api.createUser({ username: form.username.trim(), password: form.password, role: 'User' });
      setForm({ username: '', password: '' });
      setItems((await api.users()) || []);
    } catch (err) {
      setError(err.message);
    } finally {
      setCreating(false);
    }
  }

  async function remove(user) {
    if (!window.confirm(`Удалить пользователя «${user.username}»?`)) return;
    setBusyId(user.id);
    setError('');
    try {
      await api.deleteUser(user.id);
      setItems((await api.users()) || []);
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
          <span className="kicker">Администрирование</span>
          <h1>Пользователи</h1>
          <p className="page-head__lead">Администраторы создают сотрудников с ролью User и отзывают доступ.</p>
        </div>
      </div>

      {error && <div className="alert alert--danger">{error}</div>}

      <div className="grid-two" style={{ alignItems: 'start' }}>
        <section className="card">
          <header className="card__head">
            <h2>Список</h2>
            <span className="card__hint mono">{items.length}</span>
          </header>

          {loading ? (
            <div className="loading-block">
              <span className="spinner" /> Загружаем…
            </div>
          ) : items.length === 0 ? (
            <p className="card__hint">Пользователей нет.</p>
          ) : (
            <div className="rows">
              {items.map((u) => (
                <div className="row" key={u.id} style={{ gridTemplateColumns: 'minmax(0,1fr) auto auto' }}>
                  <span className="row__title">
                    {u.username}
                    {u.username === session?.user?.username && (
                      <span className="faint" style={{ marginLeft: 8, fontSize: 11 }}>
                        это вы
                      </span>
                    )}
                  </span>
                  <span className={`badge ${u.role === 'Admin' ? 'badge--danger' : 'badge--neutral'}`}>{u.role}</span>
                  {u.role !== 'Admin' ? (
                    <button
                      type="button"
                      className="btn btn--danger btn--sm"
                      disabled={busyId === u.id}
                      onClick={() => remove(u)}
                    >
                      Удалить
                    </button>
                  ) : (
                    <span className="faint" style={{ fontSize: 11 }}>
                      {date(u.createdAt)}
                    </span>
                  )}
                </div>
              ))}
            </div>
          )}
        </section>

        <section className="card card--form">
          <header className="card__head">
            <h2>Новый пользователь</h2>
          </header>
          <form className="form" onSubmit={submit}>
            <div className="form__field">
              <label htmlFor="new-username">Логин</label>
              <input
                id="new-username"
                value={form.username}
                onChange={(e) => setForm((p) => ({ ...p, username: e.target.value }))}
                placeholder="ivan"
                maxLength={50}
                required
              />
            </div>
            <div className="form__field">
              <label htmlFor="new-password">Пароль</label>
              <input
                id="new-password"
                type="password"
                value={form.password}
                onChange={(e) => setForm((p) => ({ ...p, password: e.target.value }))}
                placeholder="Минимум 8 символов"
                minLength={8}
                maxLength={128}
                autoComplete="new-password"
                required
              />
            </div>
            <div className="form__actions">
              <button type="submit" className="btn btn--primary" disabled={creating}>
                {creating ? 'Создаём…' : 'Создать пользователя'}
              </button>
            </div>
          </form>
        </section>
      </div>
    </>
  );
}
