import { useEffect, useState } from 'react'
import { api } from '../lib/api'
import { Card } from '../components/ui'

export default function DocsPage() {
  const [swaggerUrl, setSwaggerUrl] = useState('')
  const [error, setError] = useState('')

  useEffect(() => {
    api.health()
      .then(() => setSwaggerUrl('/swagger/index.html'))
      .catch((err) => setError(err.message))
  }, [])

  return (
    <div className="space-y-6">
      <h1 className="text-2xl font-bold text-gray-900 dark:text-white">Документация API</h1>
      {error && <p className="text-red-500">{error}</p>}
      <Card className="text-center py-12">
        <p className="text-gray-700 dark:text-gray-300 mb-4">Документация API доступна через Swagger UI.</p>
        {swaggerUrl ? (
          <a
            href={swaggerUrl}
            target="_blank"
            rel="noopener noreferrer"
            className="inline-block px-6 py-3 bg-emerald-700 hover:bg-emerald-800 text-white rounded-lg font-medium"
          >
            Открыть Swagger UI
          </a>
        ) : (
          <p className="text-gray-500">Загрузка...</p>
        )}
      </Card>
    </div>
  )
}
