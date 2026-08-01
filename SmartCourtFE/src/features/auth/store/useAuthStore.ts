import { create } from 'zustand';
import { setAccessToken } from '../../../api/apiClient';

export interface User {
  id: string;
  email: string;
  fullName: string;
  role: 'Client' | 'Lawyer' | 'Admin';
}

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  login: (user: User) => void;
  logout: () => void;
  initialize: () => void;
}

export const useAuthStore = create<AuthState>((set) => ({
  user: null,
  isAuthenticated: false,
  login: (user) => {
    // Persist only minimal user details locally for fast application boot
    localStorage.setItem('user', JSON.stringify(user));
    set({ user, isAuthenticated: true });
  },
  logout: () => {
    // Clear user state from store and local storage
    localStorage.removeItem('user');
    setAccessToken(null);
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
