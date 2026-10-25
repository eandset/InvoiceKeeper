import { Navigate, useLocation } from 'react-router-dom';
import { readSession } from '../lib/api.js';

export default function ProtectedRoute({ children, adminOnly = false }) {
  const location = useLocation();
  const session = readSession();

  if (!session) {
    return <Navigate to="/login" replace state={{ from: location.pathname }} />;
  }

  if (adminOnly && session.user?.role !== 'Admin') {
    return <Navigate to="/app" replace />;
  }

  return children;
}
