import { Navigate, Route, Routes } from 'react-router-dom';
import Layout from './components/Layout.jsx';
import ProtectedRoute from './components/ProtectedRoute.jsx';
import Landing from './pages/Landing.jsx';
import Login from './pages/Login.jsx';
import Dashboard from './pages/Dashboard.jsx';
import Occurrences from './pages/Occurrences.jsx';
import Payments from './pages/Payments.jsx';
import NewPayment from './pages/NewPayment.jsx';
import Analytics from './pages/Analytics.jsx';
import Users from './pages/Users.jsx';
import Docs from './pages/Docs.jsx';
import NotFound from './pages/NotFound.jsx';

export default function App() {
  return (
    <Routes>
      <Route element={<Layout />}>
        <Route path="/" element={<Landing />} />
        <Route path="/login" element={<Login />} />
        <Route path="/docs" element={<Docs />} />

        <Route
          path="/app"
          element={
            <ProtectedRoute>
              <Dashboard />
            </ProtectedRoute>
          }
        />
        <Route
          path="/app/occurrences"
          element={
            <ProtectedRoute>
              <Occurrences />
            </ProtectedRoute>
          }
        />
        <Route
          path="/app/payments"
          element={
            <ProtectedRoute>
              <Payments />
            </ProtectedRoute>
          }
        />
        <Route
          path="/app/payments/new"
          element={
            <ProtectedRoute adminOnly>
              <NewPayment />
            </ProtectedRoute>
          }
        />
        <Route
          path="/app/analytics"
          element={
            <ProtectedRoute>
              <Analytics />
            </ProtectedRoute>
          }
        />
        <Route
          path="/app/users"
          element={
            <ProtectedRoute adminOnly>
              <Users />
            </ProtectedRoute>
          }
        />

        <Route path="/dashboard" element={<Navigate to="/app" replace />} />
        <Route path="/payments" element={<Navigate to="/app/payments" replace />} />
        <Route path="/payments/new" element={<Navigate to="/app/payments/new" replace />} />
        <Route path="/occurrences" element={<Navigate to="/app/occurrences" replace />} />
        <Route path="/overdue" element={<Navigate to="/app/occurrences?status=overdue" replace />} />
        <Route path="/users" element={<Navigate to="/app/users" replace />} />
        <Route path="*" element={<NotFound />} />
      </Route>
    </Routes>
  );
}
