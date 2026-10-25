import { useEffect, useState } from 'react'
import { api } from '../lib/api'

export default function DocsPage() {
  const [swaggerUrl, setSwaggerUrl] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    api.health()
      .then(() => setSwaggerUrl('/swagger/index.html'))
      .catch((err) => setError(err.message))
  }, [])

  return (
    <div className="p-8">
      <h1 className="text-2xl font-bold mb-6 text-gray-900 dark:text-white">Документация API</h1>
      {error && <p className="text-red-500 mb-4">{error}</p>}
      {swaggerUrl && (
        <div className="bg-white dark:bg-gray-800 p-6 rounded-lg shadow">
          <p className="text-gray-700 dark:text-gray-300 mb-4">
            Документация API доступна через Swagger UI:
          </p>
          <a
            href={swaggerUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="text-blue-600 hover:underline text-lg"
          >
            Открыть Swagger UI
          </a>
        </div>
      )}
    </div>
  )
}
