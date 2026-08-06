import { useState, useEffect } from "react";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { MainLayout } from './layouts/MainLayout';
import { Home } from "./pages/Home";
import { Register } from "./pages/Register";
import { Login } from "./pages/Login";
import { ForgotPassword } from "./pages/ForgotPassword";
import { ResetPassword } from "./pages/ResetPassword";
import { Profile } from "./pages/Profile";
import { Dashboard } from "./pages/Dashboard";
import { VerifyEmail } from "./pages/VerifyEmail";
import { Loader } from "./components/Loader";
import { GoogleOAuthProvider } from '@react-oauth/google';
import { Toaster } from "react-hot-toast";

import { useAuthStore } from "./features/auth/store/useAuthStore";

// Create TanStack Query client
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false,
      retry: false,
    },
  },
});

function App() {
  const [showLoader, setShowLoader] = useState(true);
  const [fadeLoader, setFadeLoader] = useState(false);

  useEffect(() => {
    // Initialize auth state
    useAuthStore.getState().initialize();

    // Start fading out the loader
    const fadeTimer = setTimeout(() => {
      setFadeLoader(true);
    }, 1500);

    // Completely remove the loader from DOM after transition completes
    const removeTimer = setTimeout(() => {
      setShowLoader(false);
    }, 2200);

    return () => {
      clearTimeout(fadeTimer);
      clearTimeout(removeTimer);
    };
  }, []);

  return (
    <GoogleOAuthProvider clientId="21307316304-ie0ousqrqgsmuurcvvoesht1r9o1bfhv.apps.googleusercontent.com">
      <QueryClientProvider client={queryClient}>
      <Toaster position="top-center" toastOptions={{ className: 'dark:bg-navy dark:text-white border border-border-primary' }} />
      <BrowserRouter>
        {showLoader && <Loader fadeOut={fadeLoader} />}
        <Routes>
          <Route element={<MainLayout />}>
            <Route path="/" element={<Home />} />
            <Route path="/register" element={<Register />} />
            <Route path="/login" element={<Login />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
            <Route path="/reset-password" element={<ResetPassword />} />
            <Route path="/auth/reset-password" element={<ResetPassword />} />
            <Route path="/profile" element={<Profile />} />
            <Route path="/verify-email" element={<VerifyEmail />} />
          </Route>
          {/* Routes without Navbar and Footer */}
          <Route path="/dashboard" element={<Dashboard />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>
    </GoogleOAuthProvider>
  );
}

export default App;
