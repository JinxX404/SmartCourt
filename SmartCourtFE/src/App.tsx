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
import { LawyersPage } from "./pages/LawyersPage";
import { LawyerProfilePage } from "./pages/LawyerProfilePage";
import { Loader } from "./components/Loader";
import { DashboardLayout } from "./layouts/DashboardLayout";
import { CaseDetails } from "./pages/CaseDetails";
import { CaseCandidates } from "./pages/CaseCandidates";
import { ProposalDetailPage } from "./pages/ProposalDetailPage";
import { ChatPage } from "./pages/ChatPage";
import { ContractDetailPage } from "./pages/ContractDetailPage";

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

      <QueryClientProvider client={queryClient}>
      <Toaster position="top-center" toastOptions={{ className: 'dark:bg-navy dark:text-white border border-border-primary' }} />
      <BrowserRouter>
        {showLoader && <Loader fadeOut={fadeLoader} />}
        <Routes>
          <Route element={<MainLayout />}>
            <Route path="/" element={<Home />} />
            <Route path="/lawyers" element={<LawyersPage />} />
            <Route path="/lawyers/:id" element={<LawyerProfilePage />} />
            <Route path="/register" element={<Register />} />
            <Route path="/login" element={<Login />} />
            <Route path="/forgot-password" element={<ForgotPassword />} />
            <Route path="/reset-password" element={<ResetPassword />} />
            <Route path="/auth/reset-password" element={<ResetPassword />} />
            <Route path="/profile" element={<Profile />} />
            <Route path="/verify-email" element={<VerifyEmail />} />
          </Route>
          {/* Dashboard Layout Routes (Sidebar & Mobile Header) */}
          <Route element={<DashboardLayout />}>
            <Route path="/dashboard" element={<Dashboard />} />
            <Route path="/dashboard/lawyers" element={<LawyersPage />} />
            <Route path="/dashboard/lawyers/:id" element={<LawyerProfilePage />} />
            <Route path="/dashboard/chat" element={<ChatPage />} />
            <Route path="/dashboard/chat/:conversationId" element={<ChatPage />} />
          </Route>

          {/* Other routes without Navbar, Footer, or Sidebar */}
          <Route path="/dashboard/cases/:id" element={<CaseDetails />} />
          <Route path="/dashboard/cases/:id/candidates" element={<CaseCandidates />} />
          <Route path="/dashboard/proposals/:id" element={<ProposalDetailPage />} />
          <Route path="/dashboard/contracts/:id" element={<ContractDetailPage />} />
          <Route path="/contract/:id" element={<ContractDetailPage />} />
        </Routes>
      </BrowserRouter>
    </QueryClientProvider>

  );
}

export default App;
