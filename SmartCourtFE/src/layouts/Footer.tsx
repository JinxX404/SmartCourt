// src/components/Footer.tsx
import { LuScale, LuTwitter, LuLinkedin, LuGithub } from "react-icons/lu";
import { Link } from "react-router-dom";

export const Footer = () => {
  return (
    // Main wrapper matching Figma's exact #12151C background and padding
    <footer className="w-full bg-[#12151c] pt-16 pb-8 flex flex-col items-center">
      
      {/* Main Container */}
      <div className="w-full px-6 flex flex-col gap-12">
        
        {/* Top Section: Logo and Links */}
        <div className="flex flex-col md:flex-row justify-between items-center gap-8 md:gap-0">
          
          {/* Logo (Placed first in the DOM so it sits on the right in RTL) */}
          <div className="flex flex-row items-center gap-2">
            <div className="flex items-center justify-center w-10 h-10 border-[3.3px] border-gold rounded-md mx-2">
              <LuScale className="w-6 h-6 text-gold" />
            </div>
            <span className="font-cairo font-bold text-2xl text-white tracking-tight">
              مستشار
            </span>
          </div>

          {/* Nav - Footer Links */}
          <nav className="flex flex-row flex-wrap justify-center items-center gap-6">
            <Link to="/about" className="text-[#9ca3af] text-sm hover:text-white transition-colors duration-200">
              من نحن
            </Link>
            <Link to="/privacy" className="text-[#9ca3af] text-sm hover:text-white transition-colors duration-200">
              سياسة الخصوصية
            </Link>
            <Link to="/terms" className="text-[#9ca3af] text-sm hover:text-white transition-colors duration-200">
              الشروط والأحكام
            </Link>
          </nav>
        </div>

        {/* Bottom Section: Divider, Copyright, and Socials */}
        <div className="w-full border-t border-[#1f2937] pt-8 flex flex-col-reverse md:flex-row justify-between items-center gap-6 md:gap-0">
          
          {/* Copyright Text */}
          <span className="text-[#6b7280] text-xs">
            © 2026 منصة مستشار. جميع الحقوق محفوظة.
          </span>

          {/* Social Icons */}
          <div className="flex flex-row items-center gap-4">
            <a href="#" className="text-[#6b7280] hover:text-gold transition-colors duration-200" aria-label="Twitter">
              <LuTwitter className="w-5 h-5" />
            </a>
            <a href="#" className="text-[#6b7280] hover:text-gold transition-colors duration-200" aria-label="LinkedIn">
              <LuLinkedin className="w-5 h-5" />
            </a>
            <a href="#" className="text-[#6b7280] hover:text-gold transition-colors duration-200" aria-label="Github">
              <LuGithub className="w-5 h-5" />
            </a>
          </div>

        </div>
      </div>
    </footer>
  );
};