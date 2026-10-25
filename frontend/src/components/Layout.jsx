import { Link } from 'react-router-dom'
import { useAuth } from '../hooks/useAuth'
import { useTheme } from '../hooks/useTheme'

export default function Layout({ children }) {
  const { user, logout, isAdmin } = useAuth()
  const { theme, toggle } = useTheme()

  return (
    <div className="min-h-screen bg-gray-50 dark:bg-gray-900">
      <nav className="bg-white dark:bg-gray-800 shadow">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16 items-center">
            <div className="flex items-center gap-6">
              <Link to="/" className="font-bold text-xl text-gray-900 dark:text-white">Invoice Keeper</Link>
              {user && (
                <>
                  <Link to="/payments" className="text-gray-700 dark:text-gray-300 hover:text-blue-600">Платежи</Link>
                  <Link to="/occurrences" className="text-gray-700 dark:text-gray-300 hover:text-blue-600">Периоды</Link>
                  <Link to="/overdue" className="text-gray-700 dark:text-gray-300 hover:text-blue-600">Просрочка</Link>
                  {isAdmin && <Link to="/users" className="text-gray-700 dark:text-gray-300 hover:text-blue-600">Пользователи</Link>}
                </>
              )}
            </div>
            <div className="flex items-center gap-4">
              <button onClick={toggle} className="text-sm text-gray-700 dark:text-gray-300 hover:text-blue-600">
                {theme === 'dark' ? 'Светло' : 'Темно'}
              </button>
              {user ? (
                <div className="flex items-center gap-3">
                  <span className="text-sm text-gray-700 dark:text-gray-300">{user.username}</span>
                  <button onClick={logout} className="text-sm text-red-600 hover:text-red-800">Выйти</button>
                </div>
              ) : (
                <Link to="/login" className="text-sm text-blue-600 hover:text-blue-800">Войти</Link>
              )}
            </div>
          </div>
        </div>
      </nav>
      <main>{children}</main>
    </div>
  )
}
