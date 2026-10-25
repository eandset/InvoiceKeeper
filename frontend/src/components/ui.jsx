export function Card({ children, className = '' }) {
  return (
    <div className={`bg-white dark:bg-gray-800 rounded-2xl shadow-sm border border-stone-100 dark:border-gray-700 p-6 ${className}`}>
      {children}
    </div>
  )
}

export function Button({ children, variant = 'primary', className = '', ...props }) {
  const variants = {
    primary: 'bg-emerald-700 hover:bg-emerald-800 text-white',
    danger: 'bg-red-600 hover:bg-red-700 text-white',
    ghost: 'bg-transparent hover:bg-stone-100 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-200'
  }

  return (
    <button
      className={`px-4 py-2 rounded-lg font-medium transition-colors disabled:opacity-50 ${variants[variant]} ${className}`}
      {...props}
    >
      {children}
    </button>
  )
}

export function Input({ label, error, ...props }) {
  return (
    <div className="space-y-1">
      {label && <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">{label}</label>}
      <input
        className={`w-full rounded-lg border px-3 py-2 bg-white dark:bg-gray-700 text-gray-900 dark:text-white outline-none focus:ring-2 focus:ring-emerald-500 ${
          error ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'
        }`}
        {...props}
      />
      {error && <p className="text-red-500 text-xs">{error}</p>}
    </div>
  )
}

export function Select({ label, error, children, ...props }) {
  return (
    <div className="space-y-1">
      {label && <label className="block text-sm font-medium text-gray-700 dark:text-gray-300">{label}</label>}
      <select
        className={`w-full rounded-lg border px-3 py-2 bg-white dark:bg-gray-700 text-gray-900 dark:text-white outline-none focus:ring-2 focus:ring-emerald-500 ${
          error ? 'border-red-500' : 'border-gray-300 dark:border-gray-600'
        }`}
        {...props}
      >
        {children}
      </select>
      {error && <p className="text-red-500 text-xs">{error}</p>}
    </div>
  )
}
