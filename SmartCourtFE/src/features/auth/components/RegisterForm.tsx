import { useState } from "react";
import { Link } from "react-router-dom";
import { 
  LuScale, 
  LuUser, 
  LuMail, 
  LuLock, 
  LuShieldCheck, 
  LuGavel 
} from "react-icons/lu";

export const RegisterForm = () => {
  const [activeRole, setActiveRole] = useState<'client' | 'lawyer'>('client');

  return (
    // Outer wrapper for the page background - responsive to dark mode
    <div className="relative min-h-screen flex items-start justify-center pt-12 pb-32 overflow-hidden bg-surface dark:bg-navy dark:bg-[url('/LoginDarkModeimg.png')] dark:bg-cover dark:bg-center dark:bg-no-repeat w-full transition-colors duration-300">
      
      {/* Background Mesh Gradients (Only in light mode) */}
      <div className="fixed inset-0 z-0 pointer-events-none dark:hidden">
        <div className="absolute top-[-20%] left-[-10%] w-[70vw] h-[70vw] rounded-full bg-navy/5 blur-[100px]"></div>
        <div className="absolute bottom-[-20%] right-[-10%] w-[60vw] h-[60vw] rounded-full bg-gold/10 blur-[100px]"></div>
      </div>

      <main className="w-full max-w-2xl relative z-10 mx-4 sm:mx-0">
        
        {/* Hanging Bookmark Container - dynamic themes & unrolling animation */}
        <div className="relative bg-white dark:bg-[#1a1d23]/95 backdrop-blur-xl shadow-premium dark:shadow-2xl pt-12 pb-12 px-8 sm:px-14 z-10 border-t-4 border-gold animate-unroll">
          
          {/* Header */}
          <div className="flex flex-col items-center text-center mb-6">
            <div className="flex items-center justify-center gap-3 mb-2 border-b-2 border-gold/40 pb-2 w-fit mx-auto">
              <LuScale className="w-10 h-10 text-gold" />
              <h1 className="text-4xl font-bold text-navy dark:text-gold tracking-tight">مستشار</h1>
            </div>
            <p className="text-sm font-bold text-gold tracking-widest uppercase mb-1">انضم إلينا</p>
            <p className="text-2xl font-bold text-navy dark:text-white">أنشئ حسابك</p>
            <p className="text-xs text-gray-400 dark:text-gray-400 mt-1">خطوة واحدة فقط...</p>
          </div>

          <div className="w-full border-t border-gray-100 dark:border-gray-800 mb-6"></div>

          {/* Form with 2-Column Grid */}
          <form className="grid grid-cols-1 md:grid-cols-2 gap-5">
            
            {/* Full Name */}
            <div>
              <label className="block text-sm font-bold text-navy dark:text-gray-200 mb-2" htmlFor="name">
                الاسم الكامل
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <LuUser className="text-gray-400 dark:text-gray-500" />
                </div>
                <input 
                  id="name" 
                  name="name" 
                  type="text" 
                  required
                  placeholder="الاسم الكامل" 
                  className="block w-full pl-3 pr-10 py-3 bg-gray-50 dark:bg-transparent text-navy dark:text-white border border-gray-200 dark:border-gray-750 rounded focus:border-gold focus:ring-1 focus:ring-gold outline-none transition-shadow"
                />
              </div>
            </div>

            {/* Email */}
            <div>
              <label className="block text-sm font-bold text-navy dark:text-gray-200 mb-2" htmlFor="email">
                البريد الإلكتروني
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <LuMail className="text-gray-400 dark:text-gray-500" />
                </div>
                <input 
                  id="email" 
                  name="email" 
                  type="email" 
                  dir="ltr"
                  required
                  placeholder="user@example.com" 
                  className="block w-full pl-3 pr-10 py-3 bg-gray-50 dark:bg-transparent text-navy dark:text-white border border-gray-200 dark:border-gray-750 rounded focus:border-gold focus:ring-1 focus:ring-gold outline-none text-right transition-shadow"
                />
              </div>
            </div>

            {/* Phone Number */}
            <div>
              <label className="block text-sm font-bold text-navy dark:text-gray-200 mb-2" htmlFor="phone">
                رقم الهاتف
              </label>
              <div className="relative flex rounded border border-gray-200 dark:border-gray-750 bg-gray-50 dark:bg-transparent overflow-hidden focus-within:border-gold focus-within:ring-1 focus-within:ring-gold transition-shadow">
                <span className="flex items-center px-3 bg-gray-150 dark:bg-gray-800 text-gray-500 dark:text-gray-400 text-sm font-bold border-l border-gray-200 dark:border-gray-750" dir="ltr">
                  +20
                </span>
                <input 
                  id="phone" 
                  name="phone" 
                  type="tel" 
                  required
                  placeholder="10xxxxxxxx" 
                  className="block w-full pl-3 pr-4 py-3 bg-transparent text-navy dark:text-white outline-none"
                />
              </div>
            </div>

            {/* Password */}
            <div>
              <label className="block text-sm font-bold text-navy dark:text-gray-200 mb-2" htmlFor="password">
                كلمة المرور
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <LuLock className="text-gray-400 dark:text-gray-500" />
                </div>
                <input 
                  id="password" 
                  name="password" 
                  type="password" 
                  dir="ltr"
                  required
                  placeholder="••••••••" 
                  className="block w-full pl-3 pr-10 py-3 bg-gray-50 dark:bg-transparent text-navy dark:text-white border border-gray-200 dark:border-gray-750 rounded focus:border-gold focus:ring-1 focus:ring-gold outline-none text-right transition-shadow"
                />
              </div>
            </div>

            {/* Confirm Password (Full Width in Grid) */}
            <div className="md:col-span-2">
              <label className="block text-sm font-bold text-navy dark:text-gray-200 mb-2" htmlFor="confirm_password">
                تأكيد كلمة المرور
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <LuShieldCheck className="text-gray-400 dark:text-gray-500" />
                </div>
                <input 
                  id="confirm_password" 
                  name="confirm_password" 
                  type="password" 
                  dir="ltr"
                  required
                  placeholder="••••••••" 
                  className="block w-full pl-3 pr-10 py-3 bg-gray-50 dark:bg-transparent text-navy dark:text-white border border-gray-200 dark:border-gray-750 rounded focus:border-gold focus:ring-1 focus:ring-gold outline-none text-right transition-shadow"
                />
              </div>
            </div>

            {/* Terms Checkbox (Full Width) */}
            <div className="flex items-start gap-2 mt-2 md:col-span-2">
              <div className="flex items-center h-5">
                <input 
                  id="terms" 
                  name="terms" 
                  type="checkbox" 
                  required
                  className="w-4 h-4 text-gold border-gray-300 dark:border-gray-600 rounded focus:ring-gold bg-transparent cursor-pointer"
                />
              </div>
              <label htmlFor="terms" className="text-sm text-gray-500 dark:text-gray-400 cursor-pointer">
                أوافق على <Link to="/terms" className="text-gold font-bold hover:underline">الشروط والأحكام</Link> و<Link to="/privacy" className="text-gold font-bold hover:underline">سياسة الخصوصية</Link>
              </label>
            </div>

            {/* Submit Button (Full Width) */}
            <div className="md:col-span-2">
              <button 
                type="submit"
                className="w-full bg-gold hover:bg-gold-hover text-white font-bold py-4 px-6 rounded transition-colors duration-200 mt-2 flex items-center justify-center gap-2 cursor-pointer shadow-premium"
              >
                <span>إنشاء الحساب</span>
              </button>
            </div>
          </form>

          {/* Login Redirect */}
          <div className="mt-6 text-center">
            <p className="text-sm text-gray-500 dark:text-gray-400">
              لديك حساب بالفعل؟ <Link to="/login" className="text-gold font-bold hover:underline">تسجيل الدخول</Link>
            </p>
          </div>

          {/* Hanging Bookmark Bottom Tabs - dynamic theme styles */}
          <div className="absolute -bottom-12.5 left-0 right-0 flex h-12.5 -z-10">
            
            {/* Right Tab: Client */}
            <button 
              type="button"
              onClick={() => setActiveRole('client')}
              className={`
                flex-1 relative flex items-center justify-center font-bold transition-all duration-300 ease-in-out cursor-pointer
                after:content-[''] after:absolute after:-bottom-5 after:left-0 after:w-full after:h-5 after:[clip-path:polygon(0_0,50%_100%,100%_0)] after:transition-colors after:duration-300
                ${activeRole === 'client' 
                  ? 'bg-gold text-white z-10 after:bg-gold' 
                  : 'bg-navy text-gray-400 dark:text-gray-500 z-0 hover:text-white hover:-translate-y-1 after:bg-navy dark:after:bg-[#1a1d23]/95'
                }
              `}
            >
              <LuUser className="ml-2 w-5 h-5" />
              تسجيل كعميل
            </button>

            {/* Left Tab: Lawyer */}
            <button 
              type="button"
              onClick={() => setActiveRole('lawyer')}
              className={`
                flex-1 relative flex items-center justify-center font-bold transition-all duration-300 ease-in-out cursor-pointer
                after:content-[''] after:absolute after:-bottom-5 after:left-0 after:w-full after:h-5 after:[clip-path:polygon(0_0,50%_100%,100%_0)] after:transition-colors after:duration-300
                ${activeRole === 'lawyer' 
                  ? 'bg-gold text-white z-10 after:bg-gold' 
                  : 'bg-navy text-gray-400 dark:text-gray-500 z-0 hover:text-white hover:-translate-y-1 after:bg-navy dark:after:bg-[#1a1d23]/95'
                }
              `}
            >
              <LuGavel className="ml-2 w-5 h-5" />
              تسجيل كمحامٍ
            </button>

          </div>
        </div>
      </main>
    </div>
  );
};