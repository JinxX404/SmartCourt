import axios from 'axios';

// Base API Client
// Authentication is handled via HttpOnly cookies set by the server.
// The browser automatically sends cookies with every request thanks to withCredentials: true.
export const apiClient = axios.create({
  baseURL: import.meta.env.DEV ? '' : 'http://localhost:5049',
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  },
});

// No request interceptor needed – cookies are sent automatically by the browser.

// Flag to prevent multiple refresh calls simultaneously
let isRefreshing = false;
let failedQueue: any[] = [];

const processQueue = (error: any) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve();
    }
  });
  failedQueue = [];
};

// Response Interceptor: Auto-handle 401 Unauthorized via HttpOnly Refresh Token
apiClient.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    const requestUrl = originalRequest?.url || '';
    const isAuthEndpoint =
      requestUrl.includes('/api/auth/login') ||
      requestUrl.includes('/api/auth/register') ||
      requestUrl.includes('/api/auth/refresh') ||
      requestUrl.includes('/api/auth/forgot-password') ||
      requestUrl.includes('/api/auth/reset-password');

    // If error is 401 and request has not been retried yet (and not an auth endpoint itself)
    if (error.response?.status === 401 && !originalRequest._retry && !isAuthEndpoint) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then(() => {
            // Cookie was refreshed by the server, just retry
            return apiClient(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // Call refresh endpoint – both refresh token (cookie) and new access token (cookie)
        // are handled by the server automatically.
        const refreshUrl = import.meta.env.DEV
          ? '/api/auth/refresh'
          : 'http://localhost:5049/api/auth/refresh';
        await axios.post(
          refreshUrl,
          {},
          { withCredentials: true }
        );

        processQueue(null);
        isRefreshing = false;

        // Retry the original request – the new access token cookie is already set
        return apiClient(originalRequest);
      } catch (refreshError) {
        processQueue(refreshError);
        isRefreshing = false;

        // Refresh token failed/expired: trigger logout
        window.dispatchEvent(new Event('auth:logout'));
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);
