import { LuScale, LuSun, LuMoon, LuMenu, LuX } from "react-icons/lu";
import { useState } from "react";
import { Link } from "react-router-dom";

interface NavbarProps {
  theme: "light" | "dark";
  toggleTheme: () => void;
}

export const Navbar = ({ theme, toggleTheme }: NavbarProps) => {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);

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

        {/* Left Section: CTA Buttons & Theme Toggle */}
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

          {/* Login Link */}
          <Link 
            to="/login" 
            className="text-sm font-medium text-text-secondary hover:text-text-primary transition-colors px-3 py-2"
          >
            تسجيل الدخول
          </Link>

          {/* Register Link (Gold CTA) */}
          <Link 
            to="/register" 
            className="h-11 px-6 bg-gold hover:bg-gold-hover text-white font-semibold text-sm rounded-xl shadow-xs transition-all duration-200 hover:scale-[1.02] flex items-center justify-center"
          >
            إنشاء حساب
          </Link>
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
        </div>
      )}
    </nav>
  );
};