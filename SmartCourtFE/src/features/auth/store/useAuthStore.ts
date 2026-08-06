import { create } from 'zustand';
import axios from 'axios';

export interface User {
  id: string;
  email: string;
  fullName: string;
  role: 'Client' | 'Lawyer' | 'Admin';
  status: 'Active' | 'Unverified' | 'PendingReview' | 'Suspended' | 'Deleted' | 'Rejected';
}

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  login: (user: User) => void;
  logout: () => void;
  initialize: () => void;
}

const getInitialState = () => {
  if (typeof window === 'undefined') return { user: null, isAuthenticated: false };
  const savedUser = localStorage.getItem('user');
  if (savedUser) {
    try {
      return { user: JSON.parse(savedUser), isAuthenticated: true };
    } catch {
      localStorage.removeItem('user');
    }
  }
  return { user: null, isAuthenticated: false };
};

const initialState = getInitialState();

export const useAuthStore = create<AuthState>((set) => ({
  user: initialState.user,
  isAuthenticated: initialState.isAuthenticated,
  login: (user) => {
    // Persist only minimal user details locally for fast application boot
    localStorage.setItem('user', JSON.stringify(user));
    set({ user, isAuthenticated: true });
  },
  logout: () => {
    // Clear user state from store and local storage
    localStorage.removeItem('user');
    
    // Fire and forget revoke request to clear HttpOnly cookies on the server
    try {
      const revokeUrl = import.meta.env.DEV ? '/api/auth/revoke' : 'http://localhost:5049/api/auth/revoke';
      axios.post(revokeUrl, {}, { withCredentials: true }).catch(() => {});
    } catch {}

    set({ user: null, isAuthenticated: false });
  },
  initialize: () => {
    const savedUser = localStorage.getItem('user');
    if (savedUser) {
      try {
        set({ user: JSON.parse(savedUser), isAuthenticated: true });
      } catch {
        localStorage.removeItem('user');
      }
    }
  }
}));

// Bind standard logout event from Axios interceptor to reset state
if (typeof window !== 'undefined') {
  window.addEventListener('auth:logout', () => {
    useAuthStore.getState().logout();
  });
}
