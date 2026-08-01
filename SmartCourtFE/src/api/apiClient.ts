import axios from 'axios';

// In-memory storage for Access Token (secure from XSS)
let _accessToken: string | null = null;

export const setAccessToken = (token: string | null) => {
  _accessToken = token;
};

export const getAccessToken = () => _accessToken;

// Base API Client
export const apiClient = axios.create({
  baseURL: import.meta.env.DEV ? '' : 'http://smartcourt.runasp.net',
  withCredentials: true, // Crucial: automatically sends/receives cookies (like HttpOnly refresh token)
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request Interceptor: Automatically inject Bearer Token from memory
apiClient.interceptors.request.use(
  (config) => {
    const token = getAccessToken();
    if (token && config.headers) {
      config.headers.Authorization = `Bearer ${token}`;
    }
    return config;
  },
  (error) => Promise.reject(error)
);

// Flag to prevent multiple refresh calls simultaneously
let isRefreshing = false;
let failedQueue: any[] = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
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
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return apiClient(originalRequest);
          })
          .catch((err) => Promise.reject(err));
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // Call refresh endpoint with credentials (sends HttpOnly refresh token cookie automatically)
        const refreshUrl = import.meta.env.DEV
          ? '/api/auth/refresh'
          : 'http://smartcourt.runasp.net/api/auth/refresh';
        const response = await axios.post(
          refreshUrl,
          {}, // No body needed because refreshToken is in HttpOnly cookie
          { withCredentials: true }
        );

        if (response.data?.success && response.data?.data) {
          const newAccessToken = response.data.data.accessToken || response.data.data.token;

          // Store new token in memory
          setAccessToken(newAccessToken);

          apiClient.defaults.headers.common.Authorization = `Bearer ${newAccessToken}`;
          originalRequest.headers.Authorization = `Bearer ${newAccessToken}`;

          processQueue(null, newAccessToken);
          isRefreshing = false;

          return apiClient(originalRequest);
        }
      } catch (refreshError) {
        processQueue(refreshError, null);
        isRefreshing = false;

        // Refresh token failed/expired: clear memory token and trigger logout
        setAccessToken(null);
        window.dispatchEvent(new Event('auth:logout'));
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);
