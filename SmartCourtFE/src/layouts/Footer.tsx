import { LuScale, LuTwitter, LuLinkedin, LuGithub, LuMail, LuPhone, LuMapPin } from "react-icons/lu";
import { Link } from "react-router-dom";

export const Footer = () => {
  return (
    <footer className="w-full bg-[#0b0f19] border-t border-[#1a1d23] pt-16 pb-8 text-gray-400 font-sans transition-colors duration-300">
      <div className="w-full max-w-7xl mx-auto px-6">
        
        {/* Main Grid */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-10 mb-12 text-right">
          
          {/* Column 1: Brand Info */}
          <div className="flex flex-col gap-4">
            <Link to="/" className="flex items-center gap-2 group justify-start">
              <div className="flex items-center justify-center w-9 h-9 bg-gold/10 border border-gold/40 rounded-lg">
                <LuScale className="w-5 h-5 text-gold" />
              </div>
              <span className="font-bold text-xl text-white tracking-tight">
                مستشار
              </span>
            </Link>
            <p className="text-sm leading-relaxed text-gray-400">
              المنصة القانونية الذكية الأولى المدعومة بالذكاء الاصطناعي لحماية حقوقك وصياغة مستنداتك القانونية بكل أمان وموثوقية.
            </p>
          </div>

          {/* Column 2: Services */}
          <div className="flex flex-col gap-3">
            <h4 className="text-white font-semibold text-sm tracking-wide">الخدمات</h4>
            <ul className="flex flex-col gap-2.5 text-sm">
              <li>
                <a href="#services" className="hover:text-gold transition-colors">استشارة قانونية فورية</a>
              </li>
              <li>
                <a href="#services" className="hover:text-gold transition-colors">صياغة ومراجعة العقود</a>
              </li>
              <li>
                <a href="#services" className="hover:text-gold transition-colors">تحليل المستندات والملفات</a>
              </li>
              <li>
                <a href="#services" className="hover:text-gold transition-colors">الدعم الذكي المتكامل</a>
              </li>
            </ul>
          </div>

          {/* Column 3: Quick Links */}
          <div className="flex flex-col gap-3">
            <h4 className="text-white font-semibold text-sm tracking-wide">روابط سريعة</h4>
            <ul className="flex flex-col gap-2.5 text-sm">
              <li>
                <Link to="/about" className="hover:text-gold transition-colors">عن مستشار</Link>
              </li>
              <li>
                <Link to="/lawyers" className="hover:text-gold transition-colors">ابحث عن محام</Link>
              </li>
              <li>
                <Link to="/privacy" className="hover:text-gold transition-colors">سياسة الخصوصية</Link>
              </li>
              <li>
                <Link to="/terms" className="hover:text-gold transition-colors">الشروط والأحكام</Link>
              </li>
            </ul>
          </div>

          {/* Column 4: Contact info */}
          <div className="flex flex-col gap-3">
            <h4 className="text-white font-semibold text-sm tracking-wide">تواصل معنا</h4>
            <ul className="flex flex-col gap-2.5 text-sm">
              <li className="flex items-center gap-2.5 justify-start">
                <LuMail className="w-4 h-4 text-gold shrink-0" />
                <span>support@smartcourt.sa</span>
              </li>
              <li className="flex items-center gap-2.5 justify-start">
                <LuPhone className="w-4 h-4 text-gold shrink-0" />
                <span>+966 800 124 9999</span>
              </li>
              <li className="flex items-center gap-2.5 justify-start">
                <LuMapPin className="w-4 h-4 text-gold shrink-0" />
                <span>الرياض، المملكة العربية السعودية</span>
              </li>
            </ul>
          </div>

        </div>

        {/* Divider */}
        <div className="w-full border-t border-[#1a1d23] pt-8 flex flex-col-reverse md:flex-row justify-between items-center gap-4">
          {/* Copyright */}
          <span className="text-xs text-gray-500">
            © 2026 منصة مستشار (SmartCourt). جميع الحقوق محفوظة.
          </span>

          {/* Social Links */}
          <div className="flex items-center gap-4">
            <a href="#" className="w-8 h-8 rounded-lg bg-gray-900 border border-[#1a1d23] flex items-center justify-center text-gray-500 hover:text-gold hover:border-gold/30 transition-all" aria-label="Twitter">
              <LuTwitter className="w-4 h-4" />
            </a>
            <a href="#" className="w-8 h-8 rounded-lg bg-gray-900 border border-[#1a1d23] flex items-center justify-center text-gray-500 hover:text-gold hover:border-gold/30 transition-all" aria-label="LinkedIn">
              <LuLinkedin className="w-4 h-4" />
            </a>
            <a href="#" className="w-8 h-8 rounded-lg bg-gray-900 border border-[#1a1d23] flex items-center justify-center text-gray-500 hover:text-gold hover:border-gold/30 transition-all" aria-label="Github">
              <LuGithub className="w-4 h-4" />
            </a>
          </div>
        </div>

      </div>
    </footer>
  );
};