import { useAuth } from '../../hooks/useAuth'
import { useTheme } from '../../hooks/useTheme'
import { Link, useLocation } from 'react-router-dom'

const navItems = [
  { to: '/', label: 'Дашборд', icon: '📊' },
  { to: '/payments', label: 'Платежи', icon: '💳' },
  { to: '/occurrences', label: 'Периоды', icon: '📅' },
  { to: '/overdue', label: 'Просрочка', icon: '⚠️' }
]

const adminItems = [
  { to: '/users', label: 'Пользователи', icon: '👥' }
]

export default function Sidebar() {
  const { user, logout, isAdmin } = useAuth()
  const { theme, toggle } = useTheme()
  const location = useLocation()

  if (!user) return null

  const isActive = (path) => location.pathname === path

  return (
    <aside className="hidden md:flex flex-col w-64 h-screen bg-emerald-900 dark:bg-gray-900 text-white sticky top-0">
      <div className="p-6">
        <h1 className="text-2xl font-bold">Invoice Keeper</h1>
        <p className="text-emerald-200 text-sm mt-1">Учёт регулярных платежей</p>
      </div>

      <nav className="flex-1 px-4 space-y-2">
        {navItems.map((item) => (
          <Link
            key={item.to}
            to={item.to}
            className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${
              isActive(item.to)
                ? 'bg-emerald-700 dark:bg-gray-700 text-white'
                : 'text-emerald-100 hover:bg-emerald-800 dark:hover:bg-gray-800'
            }`}
          >
            <span>{item.icon}</span>
            <span className="font-medium">{item.label}</span>
          </Link>
        ))}
        {isAdmin && adminItems.map((item) => (
          <Link
            key={item.to}
            to={item.to}
            className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${
              isActive(item.to)
                ? 'bg-emerald-700 dark:bg-gray-700 text-white'
                : 'text-emerald-100 hover:bg-emerald-800 dark:hover:bg-gray-800'
            }`}
          >
            <span>{item.icon}</span>
            <span className="font-medium">{item.label}</span>
          </Link>
        ))}
      </nav>

      <div className="p-4 border-t border-emerald-800 dark:border-gray-700 space-y-4">
        <button
          onClick={toggle}
          className="flex items-center gap-2 text-emerald-100 hover:text-white text-sm"
        >
          <span>{theme === 'dark' ? '☀️' : '🌙'}</span>
          {theme === 'dark' ? 'Светлая тема' : 'Тёмная тема'}
        </button>
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm font-medium">{user.username}</p>
            <p className="text-xs text-emerald-300">{isAdmin ? 'Администратор' : 'Пользователь'}</p>
          </div>
          <button
            onClick={logout}
            className="text-sm text-red-300 hover:text-red-100 underline"
          >
            Выйти
          </button>
        </div>
      </div>
    </aside>
  )
}

