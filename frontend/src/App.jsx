import { Routes, Route, Navigate } from 'react-router-dom'
import { AuthProvider } from './hooks/useAuth'
import { ThemeProvider } from './hooks/useTheme'
import Layout from './components/Layout'
import PrivateRoute from './components/PrivateRoute'
import LoginPage from './pages/LoginPage'
import DashboardPage from './pages/DashboardPage'
import PaymentsPage from './pages/PaymentsPage'
import PaymentNewPage from './pages/PaymentNewPage'
import OccurrencesPage from './pages/OccurrencesPage'
import OverduePage from './pages/OverduePage'
import UsersPage from './pages/UsersPage'
import NotFoundPage from './pages/NotFoundPage'
import ForbiddenPage from './pages/ForbiddenPage'

function App() {
  return (
    <ThemeProvider>
      <AuthProvider>
        <Layout>
          <Routes>
            <Route path="/login" element={<LoginPage />} />
            <Route path="/" element={<PrivateRoute><DashboardPage /></PrivateRoute>} />
            <Route path="/payments" element={<PrivateRoute><PaymentsPage /></PrivateRoute>} />
            <Route path="/payments/new" element={<PrivateRoute><PaymentNewPage /></PrivateRoute>} />
            <Route path="/occurrences" element={<PrivateRoute><OccurrencesPage /></PrivateRoute>} />
            <Route path="/overdue" element={<PrivateRoute><OverduePage /></PrivateRoute>} />
            <Route path="/users" element={<PrivateRoute><UsersPage /></PrivateRoute>} />
            <Route path="/forbidden" element={<ForbiddenPage />} />
            <Route path="*" element={<NotFoundPage />} />
          </Routes>
        </Layout>
      </AuthProvider>
    </ThemeProvider>
  )
}

export default App
