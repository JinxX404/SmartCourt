import { HiLogin } from "react-icons/hi";

export const Navbar = () => {
  return (
    <nav className="w-full h-17.75 bg-navy border-b border-[#1F2937] flex flex-col justify-end sticky top-0 z-50">

      <div className="w-full max-w-7xl h-17.5 mx-auto px-6 flex items-center justify-between">
        
        <div className="flex items-center gap-6">

          <div className="flex items-center gap-1 cursor-pointer">
            <span className="font-bold text-2xl text-gold tracking-[-1.2px]">
              ↀ
            </span>
            <span className="font-bold text-2xl text-white tracking-[-1.2px]">
              مستشار
            </span>
          </div>
        </div>

        <div className="hidden md:flex items-center gap-8">
          <a href="#" className="font-medium text-sm text-gold">
            الرئيسية
          </a>
          <a href="#" className="font-medium text-sm text-white hover:text-gold transition-colors">
            كيف يعمل
          </a>
          <a href="#" className="font-medium text-sm text-white hover:text-gold transition-colors">
            إبحث عن محام
          </a>
          <a href="#" className="font-medium text-sm text-white hover:text-gold transition-colors">
            من نحن
          </a>
        </div>

        <div className="flex items-center gap-3">
          <button className="h-9 px-4 gap-2 bg-secondary-gray rounded-sm flex items-center justify-center transition-opacity hover:opacity-90">
            <span className=" font-normal text-sm text-white">
              إنشاء حساب
            </span>

          </button>

          <button className="h-9.5 px-4 border border-gold rounded-sm flex items-center justify-center transition-colors hover:bg-gold hover:text-navy group">
            <span className=" font-normal text-sm text-gold group-hover:text-navy transition-colors">
              تسجيل الدخول
            </span>
            <HiLogin className="text-gold group-hover:text-navy" />
          </button>
        </div>

      </div>
    </nav>
  );
};