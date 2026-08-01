import { useState, useRef, useEffect } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  LuScale,
  LuSun,
  LuMoon,
  LuMenu,
  LuX,
  LuUser,
  LuLogOut,
  LuChevronDown
} from "react-icons/lu";
import { useAuthStore } from "../features/auth/store/useAuthStore";

interface NavbarProps {
  theme: "light" | "dark";
  toggleTheme: () => void;
}

export const Navbar = ({ theme, toggleTheme }: NavbarProps) => {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const [userMenuOpen, setUserMenuOpen] = useState(false);
  const userMenuRef = useRef<HTMLDivElement>(null);

  const { user, isAuthenticated, logout } = useAuthStore();
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
            <div className="flex items-center justify-center w-10 h-10 bg-gold/10 border border-gold/40 rounded-xl transition-all duration-300 group-hover:bg-gold/20">
              <LuScale className="w-5.5 h-5.5 text-gold" />
            </div>
            <span className="font-bold text-2xl tracking-tight text-text-primary">
              مستشار
            </span>
          </Link>
        </div>

        {/* Center Section: Navigation Links (Desktop) */}
        <div className="hidden md:flex items-center gap-8">
          <Link to="/" className="font-medium text-sm text-gold hover:text-gold-hover transition-colors">
            الرئيسية
          </Link>
          <a href="#services" className="font-medium text-sm text-text-secondary hover:text-gold transition-colors">
            الخدمات
          </a>
          <a href="#how-it-works" className="font-medium text-sm text-text-secondary hover:text-gold transition-colors">
            كيف يعمل
          </a>
          <Link to="/lawyers" className="font-medium text-sm text-text-secondary hover:text-gold transition-colors">
            إبحث عن محام
          </Link>
          <Link to="/about" className="font-medium text-sm text-text-secondary hover:text-gold transition-colors">
            من نحن
          </Link>
        </div>

        {/* Left Section: CTA Buttons & Theme Toggle & User Menu */}
        <div className="hidden md:flex items-center gap-4">
          {/* Theme Toggle Button */}
          <button 
            onClick={toggleTheme} 
            className="w-10 h-10 rounded-xl flex items-center justify-center border border-border-primary hover:bg-bg-primary text-text-secondary hover:text-text-primary transition-all cursor-pointer"
            aria-label="Toggle Theme"
          >
            {theme === "light" ? (
              <LuMoon className="w-5 h-5" />
            ) : (
              <LuSun className="w-5 h-5 text-gold" />
            )}
          </button>

          {isAuthenticated && user ? (
            /* Authenticated User Menu Dropdown */
            <div className="relative" ref={userMenuRef}>
              <button
                onClick={() => setUserMenuOpen(!userMenuOpen)}
                className="flex items-center gap-3 p-1.5 pr-3 rounded-xl border border-border-primary hover:border-gold/50 bg-bg-primary transition-all cursor-pointer"
              >
                <div className="w-8 h-8 rounded-lg bg-gold/20 text-gold font-bold flex items-center justify-center text-sm border border-gold/40">
                  {user.fullName ? user.fullName.charAt(0).toUpperCase() : <LuUser className="w-4 h-4" />}
                </div>
                <div className="flex flex-col items-start text-right">
                  <span className="text-xs font-bold text-text-primary line-clamp-1 max-w-[120px]">
                    {user.fullName}
                  </span>
                  <span className="text-[10px] text-gold font-semibold">
                    {getRoleLabel(user.role)}
                  </span>
                </div>
                <LuChevronDown className={`w-4 h-4 text-text-secondary transition-transform duration-200 ${userMenuOpen ? "rotate-180" : ""}`} />
              </button>

              {/* User Dropdown */}
              {userMenuOpen && (
                <div className="absolute left-0 mt-2 w-56 bg-bg-secondary border border-border-primary rounded-2xl shadow-xl py-2 z-50 animate-fade-in">
                  <div className="px-4 py-3 border-b border-border-primary">
                    <p className="text-sm font-bold text-text-primary truncate">{user.fullName}</p>
                    <p className="text-xs text-text-secondary truncate">{user.email}</p>
                  </div>

                  <Link
                    to="/profile"
                    onClick={() => setUserMenuOpen(false)}
                    className="flex items-center gap-3 px-4 py-2.5 text-sm text-text-primary hover:bg-gold/10 hover:text-gold transition-colors"
                  >
                    <LuUser className="w-4.5 h-4.5 text-gold" />
                    <span>الملف الشخصي</span>
                  </Link>

                  <button
                    onClick={handleLogout}
                    className="w-full flex items-center gap-3 px-4 py-2.5 text-sm text-red-500 hover:bg-red-500/10 transition-colors cursor-pointer border-t border-border-primary mt-1"
                  >
                    <LuLogOut className="w-4.5 h-4.5" />
                    <span>تسجيل الخروج</span>
                  </button>
                </div>
              )}
            </div>
          ) : (
            /* Guest Login/Register Buttons */
            <>
              <Link 
                to="/login" 
                className="text-sm font-medium text-text-secondary hover:text-text-primary transition-colors px-3 py-2"
              >
                تسجيل الدخول
              </Link>

              <Link 
                to="/register" 
                className="h-11 px-6 bg-gold hover:bg-gold-hover text-white font-semibold text-sm rounded-xl shadow-xs transition-all duration-200 hover:scale-[1.02] flex items-center justify-center"
              >
                إنشاء حساب
              </Link>
            </>
          )}
        </div>

        {/* Mobile Menu Button */}
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

      {/* Mobile Menu Drawer */}
      {mobileMenuOpen && (
        <div className="md:hidden w-full bg-bg-secondary border-t border-border-primary px-6 py-6 flex flex-col gap-4 animate-fade-in">
          <Link to="/" onClick={() => setMobileMenuOpen(false)} className="font-medium text-sm text-gold py-2">
            الرئيسية
          </Link>
          <a href="#services" onClick={() => setMobileMenuOpen(false)} className="font-medium text-sm text-text-secondary hover:text-gold py-2">
            الخدمات
          </a>
          <a href="#how-it-works" onClick={() => setMobileMenuOpen(false)} className="font-medium text-sm text-text-secondary hover:text-gold py-2">
            كيف يعمل
          </a>
          <Link to="/lawyers" onClick={() => setMobileMenuOpen(false)} className="font-medium text-sm text-text-secondary hover:text-gold py-2">
            إبحث عن محام
          </Link>
          <Link to="/about" onClick={() => setMobileMenuOpen(false)} className="font-medium text-sm text-text-secondary hover:text-gold py-2">
            من نحن
          </Link>
          <hr className="border-border-primary my-2" />
          
          {isAuthenticated && user ? (
            <div className="flex flex-col gap-3">
              <div className="flex items-center gap-3 p-3 bg-bg-primary rounded-xl border border-border-primary">
                <div className="w-10 h-10 rounded-lg bg-gold/20 text-gold font-bold flex items-center justify-center text-base border border-gold/40">
                  {user.fullName ? user.fullName.charAt(0).toUpperCase() : <LuUser className="w-5 h-5" />}
                </div>
                <div className="flex flex-col text-right">
                  <span className="text-sm font-bold text-text-primary">{user.fullName}</span>
                  <span className="text-xs text-gold font-semibold">{getRoleLabel(user.role)}</span>
                </div>
              </div>
              <Link 
                to="/profile" 
                onClick={() => setMobileMenuOpen(false)}
                className="h-11 border border-border-primary rounded-xl flex items-center justify-center gap-2 font-medium text-sm text-text-primary"
              >
                <LuUser className="w-4 h-4 text-gold" />
                <span>الملف الشخصي</span>
              </Link>
              <button 
                onClick={handleLogout}
                className="h-11 bg-red-500/10 text-red-500 font-semibold text-sm rounded-xl flex items-center justify-center gap-2"
              >
                <LuLogOut className="w-4 h-4" />
                <span>تسجيل الخروج</span>
              </button>
            </div>
          ) : (
            <div className="flex flex-col gap-3">
              <Link 
                to="/login" 
                onClick={() => setMobileMenuOpen(false)}
                className="h-11 border border-border-primary rounded-xl flex items-center justify-center font-medium text-sm text-text-primary"
              >
                تسجيل الدخول
              </Link>
              <Link 
                to="/register" 
                onClick={() => setMobileMenuOpen(false)}
                className="h-11 bg-gold text-white font-semibold text-sm rounded-xl flex items-center justify-center"
              >
                إنشاء حساب
              </Link>
            </div>
          )}
        </div>
      )}
    </nav>
  );
};