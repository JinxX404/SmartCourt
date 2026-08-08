import { useState, useRef, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import { motion, AnimatePresence } from "framer-motion";
import {
  LuScale,
  LuSun,
  LuMoon,
  LuMenu,
  LuX,
  LuUser,
  LuLogOut,
  LuChevronDown,
  LuLayoutDashboard
} from "react-icons/lu";
import { useAuthStore } from "../features/auth/store/useAuthStore";
import { UserStatusBadge } from "../features/auth/components/UserStatusBadge";

interface NavbarProps {
  theme: "light" | "dark";
  toggleTheme: () => void;
}

export const Navbar = ({ theme, toggleTheme }: NavbarProps) => {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const userMenuRef = useRef<HTMLDivElement>(null);

  const { user, isAuthenticated, logout } = useAuthStore();
  
  console.log('Current User in Navbar:', user);
  const navigate = useNavigate();

  // Close dropdown on outside click
  useEffect(() => {
    const handleClickOutside = (e: MouseEvent) => {
      if (userMenuRef.current && !userMenuRef.current.contains(e.target as Node)) {
        setUserMenuOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleLogout = () => {
    logout();
    setUserMenuOpen(false);
    setMobileMenuOpen(false);
    navigate("/login");
  };

  const getRoleLabel = (role?: string) => {
    switch (role) {
      case "Lawyer":
        return "محامي";
      case "Client":
        return "موكل";
      case "Admin":
        return "مسؤول";
      default:
        return "مستخدم";
    }
  };

  return (
    <nav className="w-full bg-bg-secondary border-b border-border-primary sticky top-0 z-50 transition-colors duration-300 shadow-xs">
      <div className="w-full max-w-7xl mx-auto px-6 h-20 flex items-center justify-between">
        
        {/* Right Section: Logo */}
        <div className="flex items-center">
          <Link to="/" className="flex items-center gap-2.5 group">
            <div className="flex items-center justify-center w-10 h-10 bg-gold/10 border border-gold/40 rounded-lg transition-all duration-300 group-hover:bg-gold/20">
              <LuScale className="w-5.5 h-5.5 text-gold" />
            </div>
            <span className="font-bold text-2xl tracking-tight text-text-primary">
              مستشار
            </span>
          </Link>
        </div>

        {/* Center Section: Navigation Links (Desktop & Tablet) */}
        <div className="hidden md:flex items-center gap-4 lg:gap-8">
          <Link to="/" className="font-medium text-xs md:text-sm text-gold hover:text-gold-hover transition-colors whitespace-nowrap">
            الرئيسية
          </Link>
          <a href="#services" className="font-medium text-xs md:text-sm text-text-secondary hover:text-gold transition-colors whitespace-nowrap">
            الخدمات
          </a>
          <a href="#how-it-works" className="font-medium text-xs md:text-sm text-text-secondary hover:text-gold transition-colors whitespace-nowrap">
            كيف يعمل
          </a>
          <Link to="/lawyers" className="font-medium text-xs md:text-sm text-text-secondary hover:text-gold transition-colors whitespace-nowrap">
            إبحث عن محام
          </Link>
          <Link to="/about" className="font-medium text-xs md:text-sm text-text-secondary hover:text-gold transition-colors whitespace-nowrap">
            من نحن
          </Link>
        </div>

        {/* Left Section: CTA Buttons & Theme Toggle & User Menu */}
        <div className="hidden md:flex items-center gap-3 lg:gap-4">
          {/* Theme Toggle Button */}
          <button 
            onClick={toggleTheme} 
            className="w-9 h-9 lg:w-10 lg:h-10 rounded-lg flex items-center justify-center border border-border-primary hover:bg-bg-primary text-text-secondary hover:text-text-primary transition-all cursor-pointer shrink-0"
            aria-label="Toggle Theme"
          >
            {theme === "light" ? (
              <LuMoon className="w-4.5 h-4.5 lg:w-5 lg:h-5" />
            ) : (
              <LuSun className="w-4.5 h-4.5 lg:w-5 lg:h-5 text-gold" />
            )}
          </button>

          {isAuthenticated && user ? (
            /* Authenticated User Menu Dropdown */
            <div className="relative" ref={userMenuRef}>
              <button
                onClick={() => setUserMenuOpen(!userMenuOpen)}
                className="flex items-center gap-2.5 px-3 py-2 rounded-lg text-sm font-bold text-text-primary hover:text-gold hover:bg-gold/5 transition-all cursor-pointer"
              >
                <div className="relative w-8 h-8 rounded-full bg-gold/15 text-gold flex items-center justify-center text-sm shrink-0">
                  <LuUser className="w-4.5 h-4.5" />
                  {user.status === 'Unverified' && (
                    <span className="absolute -top-1 -right-1 w-3 h-3 bg-red-500 border-2 border-white dark:border-[#1a1d23] rounded-full animate-pulse"></span>
                  )}
                  {user.status === 'PendingReview' && (
                    <span className="absolute -top-1 -right-1 w-3 h-3 bg-amber-500 border-2 border-white dark:border-[#1a1d23] rounded-full animate-pulse"></span>
                  )}
                  {user.status === 'Rejected' && (
                    <span className="absolute -top-1 -right-1 w-3 h-3 bg-rose-500 border-2 border-white dark:border-[#1a1d23] rounded-full animate-pulse"></span>
                  )}
                </div>
                <span dir="auto" className="hidden md:inline font-bold">
                  {user.fullName ? user.fullName.split(' ')[0] : 'مرحباً'}
                </span>
                <LuChevronDown className={`w-4 h-4 text-text-secondary transition-transform duration-200 hidden md:block ${userMenuOpen ? "rotate-180" : ""}`} />
              </button>

              {/* User Dropdown */}
              {userMenuOpen && (
                <div className="absolute left-0 top-full mt-3 w-64 bg-white dark:bg-[#1a1d23] border border-border-primary rounded-2xl shadow-premium py-2 z-50 animate-unroll overflow-hidden">
                  <div className="px-5 py-4 border-b border-border-primary bg-gray-50/50 dark:bg-navy/20">
                    <p className="text-base font-bold text-text-primary truncate" dir="auto">{user.fullName}</p>
                    <div className="mt-2">
                      <UserStatusBadge status={user.status} role={user.role} />
                    </div>
                  </div>

                  <div className="p-2">
                    <Link
                      to="/dashboard"
                      onClick={() => setUserMenuOpen(false)}
                      className="flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-bold text-text-primary hover:bg-gold/10 hover:text-gold transition-colors"
                    >
                      <LuLayoutDashboard className="w-5 h-5 text-gold" />
                      <span>لوحة التحكم</span>
                    </Link>

                    <button
                      onClick={handleLogout}
                      className="w-full flex items-center gap-3 px-3 py-2.5 rounded-lg text-sm font-bold text-red-500 hover:bg-red-50 dark:hover:bg-red-500/10 transition-colors cursor-pointer mt-1"
                    >
                      <LuLogOut className="w-5 h-5" />
                      <span>تسجيل الخروج</span>
                    </button>
                  </div>
                </div>
              )}
            </div>
          ) : (
            /* Guest Login/Register Buttons */
            <>
              <Link 
                to="/login" 
                className="text-xs md:text-sm font-medium text-text-secondary hover:text-text-primary transition-colors px-2 lg:px-3 py-2 whitespace-nowrap"
              >
                تسجيل الدخول
              </Link>

              <Link 
                to="/register" 
                className="h-9 lg:h-11 px-4 lg:px-6 bg-gold hover:bg-gold-hover text-white font-semibold text-xs md:text-sm rounded-lg shadow-xs transition-all duration-200 flex items-center justify-center whitespace-nowrap"
              >
                إنشاء حساب
              </Link>
            </>
          )}
        </div>

        {/* Mobile Menu Button (Only for phone screens below md) */}
        <div className="flex md:hidden items-center gap-3">
          <button 
            onClick={toggleTheme} 
            className="w-9 h-9 rounded-lg flex items-center justify-center border border-border-primary text-text-secondary cursor-pointer"
          >
            {theme === "light" ? <LuMoon className="w-4.5 h-4.5" /> : <LuSun className="w-4.5 h-4.5 text-gold" />}
          </button>
          
          <button 
            onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
            className="w-9 h-9 rounded-lg flex items-center justify-center border border-border-primary text-text-primary cursor-pointer"
          >
            {mobileMenuOpen ? <LuX className="w-5 h-5" /> : <LuMenu className="w-5 h-5" />}
          </button>
        </div>

      </div>

      {/* Mobile Menu Drawer (Side Panel) */}
      <AnimatePresence>
        {mobileMenuOpen && (
          <>
            {/* Overlay */}
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              transition={{ duration: 0.3 }}
              className="fixed inset-0 bg-black/60 backdrop-blur-sm z-40 md:hidden"
              onClick={() => setMobileMenuOpen(false)}
            />

            {/* Drawer */}
            <motion.div
              initial={{ x: "100%" }}
              animate={{ x: 0 }}
              exit={{ x: "100%" }}
              transition={{ type: "spring", bounce: 0, duration: 0.4 }}
              className="fixed top-0 right-0 w-3/4 max-w-sm h-screen bg-bg-secondary border-l border-border-primary shadow-2xl z-50 flex flex-col p-6 md:hidden overflow-y-auto"
            >
              <div className="flex items-center justify-between mb-8">
                <Link to="/" onClick={() => setMobileMenuOpen(false)} className="flex items-center gap-2 group">
                  <div className="flex items-center justify-center w-10 h-10 bg-gold/10 border border-gold/40 rounded-lg">
                    <LuScale className="w-5.5 h-5.5 text-gold" />
                  </div>
                  <span className="font-bold text-2xl tracking-tight text-text-primary">مستشار</span>
                </Link>
                <button 
                  onClick={() => setMobileMenuOpen(false)}
                  className="w-10 h-10 rounded-lg flex items-center justify-center bg-bg-primary border border-border-primary text-text-primary"
                >
                  <LuX className="w-5 h-5" />
                </button>
              </div>

              <div className="flex flex-col gap-2">
                <Link to="/" onClick={() => setMobileMenuOpen(false)} className="font-bold text-base text-gold py-3 border-b border-border-primary">الرئيسية</Link>
                <a href="#services" onClick={() => setMobileMenuOpen(false)} className="font-bold text-base text-text-primary hover:text-gold py-3 border-b border-border-primary">الخدمات</a>
                <a href="#how-it-works" onClick={() => setMobileMenuOpen(false)} className="font-bold text-base text-text-primary hover:text-gold py-3 border-b border-border-primary">كيف يعمل</a>
                <Link to="/lawyers" onClick={() => setMobileMenuOpen(false)} className="font-bold text-base text-text-primary hover:text-gold py-3 border-b border-border-primary">إبحث عن محام</Link>
                <Link to="/about" onClick={() => setMobileMenuOpen(false)} className="font-bold text-base text-text-primary hover:text-gold py-3">من نحن</Link>
              </div>

              <div className="mt-6 pt-6 border-t border-border-primary">
                {isAuthenticated && user ? (
                  <div className="flex flex-col gap-3">
                    <div className="flex items-center gap-3 p-3 bg-bg-primary rounded-lg border border-border-primary mb-2">
                      <div className="relative w-10 h-10 rounded-lg bg-gold/20 text-gold font-bold flex items-center justify-center text-base border border-gold/40">
                        {user.fullName ? user.fullName.charAt(0).toUpperCase() : <LuUser className="w-5 h-5" />}
                        {user.status === 'Unverified' && (
                          <span className="absolute -top-1 -right-1 w-3.5 h-3.5 bg-red-500 border-2 border-bg-primary rounded-full animate-pulse"></span>
                        )}
                      </div>
                      <div className="flex flex-col text-right">
                        <span className="text-sm font-bold text-text-primary">{user.fullName}</span>
                        <span className="text-xs text-gold font-semibold">{getRoleLabel(user.role)}</span>
                      </div>
                    </div>
                    <Link 
                      to="/dashboard" 
                      onClick={() => setMobileMenuOpen(false)}
                      className="h-12 border border-border-primary rounded-lg flex items-center justify-center gap-2 font-bold text-sm text-text-primary bg-bg-primary"
                    >
                      <LuLayoutDashboard className="w-4.5 h-4.5 text-gold" />
                      <span>لوحة التحكم</span>
                    </Link>
                    <button 
                      onClick={handleLogout}
                      className="h-12 bg-red-500/10 hover:bg-red-500/20 text-red-500 font-bold text-sm rounded-lg flex items-center justify-center gap-2 transition-colors cursor-pointer"
                    >
                      <LuLogOut className="w-4.5 h-4.5" />
                      <span>تسجيل الخروج</span>
                    </button>
                  </div>
                ) : (
                  <div className="flex flex-col gap-3">
                    <Link 
                      to="/login" 
                      onClick={() => setMobileMenuOpen(false)}
                      className="h-12 border border-border-primary bg-bg-primary rounded-lg flex items-center justify-center font-bold text-sm text-text-primary"
                    >
                      تسجيل الدخول
                    </Link>
                    <Link 
                      to="/register" 
                      onClick={() => setMobileMenuOpen(false)}
                      className="h-12 bg-gold text-white font-bold text-sm rounded-lg flex items-center justify-center"
                    >
                      إنشاء حساب
                    </Link>
                  </div>
                )}
              </div>
            </motion.div>
          </>
        )}
      </AnimatePresence>
    </nav>
  );
};