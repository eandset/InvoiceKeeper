import { useAuth } from '../../hooks/useAuth'
import { useTheme } from '../../hooks/useTheme'
import { Link, useLocation } from 'react-router-dom'
import { LayoutDashboard, CreditCard, CalendarDays, AlertCircle, Users, LogOut, Moon, Sun, Bot } from 'lucide-react'

const BOT_URL = import.meta.env.VITE_BOT_URL || '#'

const navItems = [
  { to: '/', label: 'Дашборд', Icon: LayoutDashboard },
  { to: '/payments', label: 'Платежи', Icon: CreditCard },
  { to: '/occurrences', label: 'Периоды', Icon: CalendarDays },
  { to: '/overdue', label: 'Просрочка', Icon: AlertCircle }
]

const adminItems = [
  { to: '/users', label: 'Пользователи', Icon: Users }
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
        {navItems.map(({ to, label, Icon }) => (
          <Link
            key={to}
            to={to}
            className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${
              isActive(to)
                ? 'bg-emerald-700 dark:bg-gray-700 text-white'
                : 'text-emerald-100 hover:bg-emerald-800 dark:hover:bg-gray-800'
            }`}
          >
            <Icon size={20} />
            <span className="font-medium">{label}</span>
          </Link>
        ))}
        {isAdmin && adminItems.map(({ to, label, Icon }) => (
          <Link
            key={to}
            to={to}
            className={`flex items-center gap-3 px-4 py-3 rounded-lg transition-colors ${
              isActive(to)
                ? 'bg-emerald-700 dark:bg-gray-700 text-white'
                : 'text-emerald-100 hover:bg-emerald-800 dark:hover:bg-gray-800'
            }`}
          >
            <Icon size={20} />
            <span className="font-medium">{label}</span>
          </Link>
        ))}
      </nav>

      <div className="p-4 border-t border-emerald-800 dark:border-gray-700 space-y-4">
        <a
          href={BOT_URL}
          target="_blank"
          rel="noopener noreferrer"
          className="flex items-center gap-2 text-emerald-100 hover:text-white text-sm"
        >
          <Bot size={18} />
          Telegram-бот
        </a>
        <button
          onClick={toggle}
          className="flex items-center gap-2 text-emerald-100 hover:text-white text-sm"
        >
          {theme === 'dark' ? <Sun size={18} /> : <Moon size={18} />}
          {theme === 'dark' ? 'Светлая тема' : 'Тёмная тема'}
        </button>
        <div className="flex items-center justify-between">
          <div>
            <p className="text-sm font-medium">{user.username}</p>
            <p className="text-xs text-emerald-300">{isAdmin ? 'Администратор' : 'Пользователь'}</p>
          </div>
          <button
            onClick={logout}
            className="text-sm text-red-300 hover:text-red-100 underline flex items-center gap-1"
          >
            <LogOut size={16} />
            Выйти
          </button>
        </div>
      </div>
    </aside>
  )
}
