import { HiLogin } from "react-icons/hi";
import { LuScale, LuUserPlus } from "react-icons/lu";
import { Link } from "react-router-dom";

export const Navbar = () => {
  return (
    <nav className="w-full h-17.75 bg-navy border-b border-[#1F2937] flex flex-col justify-end sticky top-0 z-50">

      <div className="w-full h-17.5 mx-auto px-6 flex items-center justify-between">
        
        {/* Logo - Linked to Home */}
        <div className="flex items-center gap-6">
          <Link to="/" className="flex items-center gap-1 cursor-pointer">
            <div className="flex items-center justify-center w-10 h-10 border-[3.3px] border-gold rounded-md mx-2">
              <LuScale className="w-6 h-6 text-gold" />
            </div>
            <span className="font-bold text-2xl text-white tracking-[-1.2px]">
              مستشار
            </span>
          </Link>
        </div>

        {/* Center Navigation Links */}
        <div className="hidden md:flex items-center gap-8">
          <Link to="/" className="font-medium text-sm text-gold">
            الرئيسية
          </Link>
          <Link to="/how-it-works" className="font-medium text-sm text-white hover:text-gold transition-colors">
            كيف يعمل
          </Link>
          <Link to="/lawyers" className="font-medium text-sm text-white hover:text-gold transition-colors">
            إبحث عن محام
          </Link>
          <Link to="/about" className="font-medium text-sm text-white hover:text-gold transition-colors">
            من نحن
          </Link>
        </div>

        {/* Authentication Actions */}
        <div className="flex items-center gap-3">
          
          {/* Register Link - Styled identically to your previous button */}
          <Link 
            to="/register" 
            className="h-9 px-4 gap-2 bg-secondary-gray rounded-sm flex items-center justify-center transition-opacity hover:opacity-90"
          >
            <span className="font-normal text-sm text-white">
              إنشاء حساب
            </span>
            <LuUserPlus className="text-white" />
          </Link>

          {/* Login Link */}
          <Link 
            to="/login" 
            className="h-9.5 px-4 border border-gold rounded-sm flex items-center justify-center transition-colors hover:bg-gold hover:text-navy group"
          >
            <span className="font-normal text-sm text-gold group-hover:text-navy transition-colors">
              تسجيل الدخول
            </span>
            <HiLogin className="text-gold group-hover:text-navy" />
          </Link>
          
        </div>

      </div>
    </nav>
  );
};