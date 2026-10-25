import { useCallback, useEffect, useState } from 'react';
import { clearSession, readSession } from '../lib/api.js';

const EVENT = 'ik-session-change';

export function notifySessionChange() {
  window.dispatchEvent(new Event(EVENT));
}

export function useSession() {
  const [session, setSession] = useState(() => readSession());

  useEffect(() => {
    const sync = () => setSession(readSession());
    window.addEventListener(EVENT, sync);
    window.addEventListener('storage', sync);
    return () => {
      window.removeEventListener(EVENT, sync);
      window.removeEventListener('storage', sync);
    };
  }, []);

  const logout = useCallback(() => {
    clearSession();
    notifySessionChange();
  }, []);

  return { session, logout, isAdmin: session?.user?.role === 'Admin' };
}
