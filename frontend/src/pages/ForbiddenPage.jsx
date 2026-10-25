export default function ForbiddenPage() {
  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50 dark:bg-gray-900 text-center px-4">
      <h1 className="text-6xl font-bold text-gray-900 dark:text-white mb-4">403</h1>
      <p className="text-xl text-gray-600 dark:text-gray-300 mb-6">Доступ запрещён</p>
      <a href="/" className="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">На главную</a>
    </div>
  )
}
