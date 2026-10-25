import { Link } from 'react-router-dom';
import PixelRing from '../components/PixelRing.jsx';
import { useSession } from '../hooks/useSession.js';

const FEATURES = [
  {
    title: 'Регулярные платежи',
    text: 'Аренда серверов, подписки, связь и счета — в едином реестре с понятной периодичностью.',
    icon: (
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round">
        <path d="M21 12a9 9 0 1 0-2.64 6.36" />
        <path d="M21 4v5h-5" />
      </svg>
    )
  },
  {
    title: 'Просрочки под присмотром',
    text: 'Система сама отмечает истёкшие сроки и подсвечивает их раньше, чем это заметит бухгалтерия.',
    icon: (
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round">
        <path d="M12 9v4" />
        <path d="M12 17h.01" />
        <path d="M10.3 3.9 2.4 17.6A2 2 0 0 0 4.1 20.6h15.8a2 2 0 0 0 1.7-3L13.7 3.9a2 2 0 0 0-3.4 0Z" />
      </svg>
    )
  },
  {
    title: 'Мультивалютность',
    text: 'RUB, USD и EUR считаются раздельно: итоги по каждой валюте без ручной конвертации.',
    icon: (
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round">
        <path d="M4 7h11a4 4 0 0 1 0 8H4" />
        <path d="M7 4 4 7l3 3" />
        <path d="M20 17H9a4 4 0 0 1 0-8" />
      </svg>
    )
  },
  {
    title: 'Роли и доступ',
    text: 'Администратор ведёт платежи и пользователей, сотрудники отмечают оплату без лишних прав.',
    icon: (
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.7" strokeLinecap="round">
        <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
        <circle cx="9" cy="7" r="4" />
        <path d="M22 21v-2a4 4 0 0 0-3-3.87" />
      </svg>
    )
  }
];

const STEPS = [
  {
    n: '01',
    title: 'Заведите платёж',
    text: 'Название, сумма, валюта и периодичность. Первый период создаётся автоматически.'
  },
  {
    n: '02',
    title: 'Оплачивайте по сроку',
    text: 'Отмечайте оплату одним действием — система сразу сформирует следующий период.'
  },
  {
    n: '03',
    title: 'Смотрите аналитику',
    text: 'Сводка по валютам, просрочкам и предстоящим списаниям всегда перед глазами.'
  }
];

export default function Landing() {
  const { session } = useSession();
  const primaryTo = session ? '/app' : '/login';
  const primaryLabel = session ? 'Открыть панель' : 'Войти в систему';

  return (
    <div className="landing">
      <section className="hero">
        <div className="hero__grid">
          <div className="hero__content">
            <div className="hero__eyebrow">
              <span>
                <i />
                Invoice Keeper
              </span>
              <span>Финансовый контроль</span>
            </div>

            <h1 className="hero__title">
              <span className="hero__title-line">Регулярные</span>
              <span className="hero__title-line">платежи под</span>
              <span className="hero__title-line">
                <em>контролем</em>
              </span>
            </h1>

            <p className="hero__lead">
              Аренда инфраструктуры, подписки и счета больше не теряются в календаре. Один реестр,
              автоматические периоды, просрочки и мультивалютная аналитика — без таблиц и ручных напоминаний.
            </p>

            <div className="hero__actions">
              <Link className="btn btn--primary btn--lg" to={primaryTo}>
                {primaryLabel}
              </Link>
              <a className="btn btn--ghost btn--lg" href="#features">
                Что внутри
              </a>
            </div>

            <p className="hero__note">
              Демо-доступ: <code>admin</code> / <code>admin123</code>
            </p>
          </div>

          <div className="hero__visual">
            <PixelRing />
            <span className="hero__visual-caption">Digital Pixels</span>
          </div>
        </div>

        <div className="hero__panel" style={{ marginTop: 56 }}>
          <div className="hero__panel-row">
            <span>Валюты</span>
            <strong>RUB · USD · EUR</strong>
          </div>
          <div className="hero__panel-row">
            <span>Периодичность</span>
            <strong>месяц / неделя / квартал / год</strong>
          </div>
          <div className="hero__panel-row">
            <span>Доступ</span>
            <strong>
              JWT · роли <em>Admin</em> / User
            </strong>
          </div>
        </div>
      </section>

      <div className="strip">
        <span>Единый реестр платежей</span>
        <span>Автоматические периоды</span>
        <span>Контроль просрочек</span>
        <span>Аналитика по валютам</span>
        <span>REST API и Swagger</span>
      </div>

      <section className="section" id="features">
        <div className="section__head">
          <div>
            <span className="section__index">01 — Возможности</span>
            <h2>Всё, что нужно для спокойного учёта</h2>
          </div>
          <p>
            Никакого Excel и ручных напоминаний: Invoice Keeper держит регулярные списания
            в одном месте и показывает, что требует внимания прямо сейчас.
          </p>
        </div>

        <div className="features">
          {FEATURES.map((feature) => (
            <article className="feature" key={feature.title}>
              <span className="feature__icon">{feature.icon}</span>
              <h3>{feature.title}</h3>
              <p>{feature.text}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="section">
        <div className="stats">
          <article className="stat-card">
            <p className="stat-card__value is-accent">24/7</p>
            <p className="stat-card__label">Доступ к реестру</p>
            <p className="stat-card__hint">Панель работает в браузере, данные — в PostgreSQL</p>
          </article>
          <article className="stat-card">
            <p className="stat-card__value">3</p>
            <p className="stat-card__label">Валюты</p>
            <p className="stat-card__hint">Отдельные итоги по RUB, USD и EUR</p>
          </article>
          <article className="stat-card">
            <p className="stat-card__value">1 клик</p>
            <p className="stat-card__label">Отметка оплаты</p>
            <p className="stat-card__hint">Следующий период создаётся автоматически</p>
          </article>
        </div>
      </section>

      <section className="section" id="how">
        <div className="section__head">
          <div>
            <span className="section__index">02 — Как работает</span>
            <h2>Три шага до порядка</h2>
          </div>
        </div>

        <div className="process">
          {STEPS.map((step) => (
            <article className="process__step" key={step.n}>
              <span className="process__num">{step.n}</span>
              <h3>{step.title}</h3>
              <p>{step.text}</p>
            </article>
          ))}
        </div>
      </section>

      <section className="cta">
        <h2>Начните с одного платежа</h2>
        <p>Войдите в демо-аккаунт и посмотрите, как выглядит полный цикл: платёж, период, оплата, аналитика.</p>
        <Link className="btn btn--primary btn--lg" to={primaryTo}>
          {primaryLabel}
        </Link>
      </section>
    </div>
  );
}
