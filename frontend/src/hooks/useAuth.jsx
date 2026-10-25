import { createContext, useContext, useEffect, useState } from 'react'
import { api } from '../lib/api'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null)
  const [loading, setLoading] = useState(true)

  useEffect(() => {
    const token = localStorage.getItem('token')
    if (!token) {
      setLoading(false)
      return
    }
    api.me()
      .then(setUser)
      .catch(() => {
        localStorage.removeItem('token')
      })
      .finally(() => setLoading(false))
  }, [])

  const login = async (username, password) => {
    const data = await api.login(username, password)
    if (data.token) {
      localStorage.setItem('token', data.token)
      const me = await api.me()
      setUser(me)
      window.location.href = '/'
    }
  }

  const logout = async () => {
    await api.logout().catch(() => {})
    localStorage.removeItem('token')
    setUser(null)
    window.location.href = '/login'
  }

  const isAdmin = user?.role === 'Admin'

  return (
    <AuthContext.Provider value={{ user, loading, login, logout, isAdmin }}>
      {children}
    </AuthContext.Provider>
  )
}

export function useAuth() {
  return useContext(AuthContext)
}
