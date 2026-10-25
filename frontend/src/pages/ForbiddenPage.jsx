import { Link } from 'react-router-dom'

export default function ForbiddenPage() {
  return (
    <div className="min-h-[calc(100vh-4rem)] flex flex-col items-center justify-center text-center px-4">
      <h1 className="text-6xl font-bold text-gray-900 dark:text-white mb-4">403</h1>
      <p className="text-xl text-gray-600 dark:text-gray-300 mb-6">Доступ запрещён</p>
      <Link to="/" className="px-6 py-3 bg-emerald-700 hover:bg-emerald-800 text-white rounded-lg font-medium">На главную</Link>
    </div>
  )
}
