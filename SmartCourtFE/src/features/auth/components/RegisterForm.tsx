import { useState } from "react";
import { Link } from "react-router-dom";
import { 
  LuScale, 
  LuUser, 
  LuMail, 
  LuLock, 
  LuShieldCheck, 
  LuArrowLeft, 
  LuGavel 
} from "react-icons/lu";

export const RegisterForm = () => {
  const [activeRole, setActiveRole] = useState<'client' | 'lawyer'>('client');

  return (
    // Outer wrapper for the page background and mesh gradient
    <div className="relative min-h-screen flex items-start justify-center pt-12 pb-32 overflow-hidden bg-surface w-full">
      
      {/* Background Mesh Gradients */}
      <div className="fixed inset-0 z-0 pointer-events-none">
        <div className="absolute top-[-20%] left-[-10%] w-[70vw] h-[70vw] rounded-full bg-navy/5 blur-[100px]"></div>
        <div className="absolute bottom-[-20%] right-[-10%] w-[60vw] h-[60vw] rounded-full bg-gold/10 blur-[100px]"></div>
      </div>

      <main className="w-full max-w-lg relative z-10 mx-4 sm:mx-0">
        
        {/* Hanging Bookmark Container */}
        <div className="relative bg-white shadow-premium pt-12 pb-12 px-8 sm:px-14 z-10">
          
          {/* Header */}
          <div className="flex flex-col items-center text-center mb-8">
            <div className="flex items-center justify-center gap-3 mb-2 border-b-2 border-gold/40 pb-2">
              <LuScale className="w-10 h-10 text-gold" />
              <h1 className="text-4xl font-bold text-navy tracking-tight">مستشار</h1>
            </div>
            <p className="text-gray-500 font-medium mt-1">مرحباً بك</p>
          </div>

          <h2 className="text-2xl font-bold text-navy text-center mb-8">
            تسجيل <span className="relative inline-block">
              {activeRole === 'client' ? 'عميل جديد' : 'محامٍ جديد'}
              <span className="absolute bottom-1 left-0 w-full h-1 bg-linear-to-r from-transparent via-gold to-transparent opacity-50"></span>
            </span>
          </h2>

          {/* Form */}
          <form className="flex flex-col gap-5">
            
            {/* Name */}
            <div>
              <label className="block text-sm font-bold text-navy mb-2" htmlFor="name">
                الاسم الكامل
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <LuUser className="text-gray-400" />
                </div>
                <input 
                  id="name" 
                  name="name" 
                  type="text" 
                  required
                  placeholder="أدخل اسمك الكامل" 
                  className="block w-full pl-3 pr-10 py-3 bg-gray-50 text-navy border border-gray-200 focus:border-gold focus:ring-1 focus:ring-gold outline-none transition-shadow"
                />
              </div>
            </div>

            {/* Email */}
            <div>
              <label className="block text-sm font-bold text-navy mb-2" htmlFor="email">
                البريد الإلكتروني
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <LuMail className="text-gray-400" />
                </div>
                <input 
                  id="email" 
                  name="email" 
                  type="email" 
                  dir="ltr"
                  required
                  placeholder="user@example.com" 
                  className="block w-full pl-3 pr-10 py-3 bg-gray-50 text-navy border border-gray-200 focus:border-gold focus:ring-1 focus:ring-gold outline-none text-right transition-shadow"
                />
              </div>
            </div>

            {/* Password */}
            <div>
              <label className="block text-sm font-bold text-navy mb-2" htmlFor="password">
                كلمة المرور
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <LuLock className="text-gray-400" />
                </div>
                <input 
                  id="password" 
                  name="password" 
                  type="password" 
                  dir="ltr"
                  required
                  placeholder="••••••••" 
                  className="block w-full pl-3 pr-10 py-3 bg-gray-50 text-navy border border-gray-200 focus:border-gold focus:ring-1 focus:ring-gold outline-none text-right transition-shadow"
                />
              </div>
            </div>

            {/* Confirm Password */}
            <div>
              <label className="block text-sm font-bold text-navy mb-2" htmlFor="confirm_password">
                تأكيد كلمة المرور
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <LuShieldCheck className="text-gray-400" />
                </div>
                <input 
                  id="confirm_password" 
                  name="confirm_password" 
                  type="password" 
                  dir="ltr"
                  required
                  placeholder="••••••••" 
                  className="block w-full pl-3 pr-10 py-3 bg-gray-50 text-navy border border-gray-200 focus:border-gold focus:ring-1 focus:ring-gold outline-none text-right transition-shadow"
                />
              </div>
            </div>

            {/* Quick Auth Divider */}
            <div className="relative flex items-center py-2">
              <div className="grow border-t border-gray-200"></div>
              <span className="shrink-0 mx-4 text-gray-400 text-sm font-bold">أو</span>
              <div className="grow border-t border-gray-200"></div>
            </div>

            {/* Google Button */}
            <button 
              type="button"
              className="w-full bg-white hover:bg-gray-50 border border-gray-200 text-navy font-bold text-sm py-3 px-6 rounded transition-colors duration-200 flex items-center justify-center gap-3"
            >
              <svg className="w-5 h-5" viewBox="0 0 24 24">
                <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"></path>
                <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"></path>
                <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"></path>
                <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"></path>
              </svg>
              التسجيل بواسطة Google
            </button>

            {/* Terms Checkbox */}
            <div className="flex items-start gap-2 mt-4">
              <div className="flex items-center h-5">
                <input 
                  id="terms" 
                  name="terms" 
                  type="checkbox" 
                  required
                  className="w-4 h-4 text-gold border-gray-300 rounded focus:ring-gold bg-gray-50 cursor-pointer"
                />
              </div>
              <label htmlFor="terms" className="text-sm text-gray-500 cursor-pointer">
                أوافق على <Link to="/terms" className="text-gold font-bold hover:underline">الشروط والأحكام</Link> و<Link to="/privacy" className="text-gold font-bold hover:underline">سياسة الخصوصية</Link>
              </label>
            </div>

            {/* Submit Button */}
            <button 
              type="submit"
              className="w-full bg-gold hover:bg-gold-hover text-white font-bold py-4 px-6 rounded transition-colors duration-200 mt-2 flex items-center justify-center gap-2"
            >
              <span>إنشاء حساب</span>
              <LuArrowLeft className="w-5 h-5" />
            </button>
          </form>

          {/* Login Redirect */}
          <div className="mt-6 text-center">
            <p className="text-sm text-gray-500">
              لديك حساب بالفعل؟ <Link to="/login" className="text-gold font-bold hover:underline">تسجيل الدخول</Link>
            </p>
          </div>

          <div className="absolute -bottom-12.5 left-0 right-0 flex h-12.5 -z-10">
            
            {/* Right Tab: Client (First in DOM natively renders on the right in RTL) */}
            <button 
              type="button"
              onClick={() => setActiveRole('client')}
              className={`
                flex-1 relative flex items-center justify-center font-bold transition-all duration-300 ease-in-out
                after:content-[''] after:absolute after:-bottom-5 after:left-0 after:w-full after:h-5 after:[clip-path:polygon(0_0,50%_100%,100%_0)] after:transition-colors after:duration-300
                ${activeRole === 'client' 
                  ? 'bg-gold text-white z-10 after:bg-gold' 
                  : 'bg-navy text-gray-400 z-0 hover:text-white hover:-translate-y-1 after:bg-navy'
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
                flex-1 relative flex items-center justify-center font-bold transition-all duration-300 ease-in-out
                after:content-[''] after:absolute after:-bottom-5 after:left-0 after:w-full after:h-5 after:[clip-path:polygon(0_0,50%_100%,100%_0)] after:transition-colors after:duration-300
                ${activeRole === 'lawyer' 
                  ? 'bg-gold text-white z-10 after:bg-gold' 
                  : 'bg-navy text-gray-400 z-0 hover:text-white hover:-translate-y-1 after:bg-navy'
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