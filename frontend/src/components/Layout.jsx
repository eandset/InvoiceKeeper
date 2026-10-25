import { useAuth } from '../hooks/useAuth'
import { useTheme } from '../hooks/useTheme'
import { Link } from 'react-router-dom'
import Sidebar from './layout/Sidebar'
import { Menu, Moon, Sun } from 'lucide-react'

export default function Layout({ children }) {
  const { user, logout } = useAuth()
  const { theme, toggle } = useTheme()

  return (
    <div className="min-h-screen bg-stone-50 dark:bg-gray-900 flex">
      <Sidebar />
      <div className="flex-1 flex flex-col min-w-0">
        <header className="md:hidden bg-emerald-900 dark:bg-gray-900 text-white px-4 py-4 flex justify-between items-center sticky top-0 z-10">
          <Link to="/" className="text-lg font-bold">Invoice Keeper</Link>
          <div className="flex items-center gap-3">
            <button onClick={toggle} className="text-xl">{theme === 'dark' ? <Sun size={20} /> : <Moon size={20} />}</button>
            {user && <button onClick={logout} className="text-sm underline">Выйти</button>}
          </div>
        </header>
        <main className="flex-1 p-4 md:p-8 overflow-auto">{children}</main>
      </div>
    </div>
  )
}
