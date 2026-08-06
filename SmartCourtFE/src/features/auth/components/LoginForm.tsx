import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useMutation } from "@tanstack/react-query";
import { useAuthStore } from "../store/useAuthStore";
import { AuthApi } from "../api/authApi";
import {
  LuScale,
  LuMail,
  LuLock,
  LuEye,
  LuEyeOff,
  LuLoader,
} from "react-icons/lu";

export const LoginForm = () => {
  const [showPassword, setShowPassword] = useState(false);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  

  
  const navigate = useNavigate();
  const loginStore = useAuthStore((state) => state.login);

  // Mutation for Auth API Login
  const { mutate, isPending } = useMutation({
    mutationFn: AuthApi.login,
    onSuccess: (response) => {
      if (response.success && response.data) {
        const { user } = response.data as any;
        loginStore(user);
        navigate("/", { replace: true }); // Redirect to home page upon successful login
      } else {
        setErrorMsg(response.message || "خطأ غير متوقع أثناء تسجيل الدخول");
      }
    },
    onError: (error: any) => {
      const apiError = error.response?.data;
      if (apiError) {
        if (apiError.message) {
          setErrorMsg(apiError.message);
        } else if (apiError.errors) {
          if (Array.isArray(apiError.errors)) {
            setErrorMsg(apiError.errors.join(" | "));
          } else if (typeof apiError.errors === 'object') {
            const messages = Object.entries(apiError.errors)
              .map(([field, msgs]) => {
                const fieldMsgs = Array.isArray(msgs) ? msgs.join(", ") : String(msgs);
                return `${field}: ${fieldMsgs}`;
              })
              .join(" | ");
            setErrorMsg(messages);
          } else {
            setErrorMsg(JSON.stringify(apiError.errors));
          }
        } else {
          setErrorMsg("بيانات الدخول غير صحيحة. يرجى التحقق وإعادة المحاولة");
        }
      } else {
        setErrorMsg("حدث خطأ في الاتصال بالخادم. يرجى المحاولة لاحقاً");
      }
    }
  });



  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg(null);
    mutate({ email, password });
  };

  return (
    // Outer wrapper for the page background - responsive to dark mode
    <div className="relative min-h-screen flex items-start justify-center pt-12 pb-32 overflow-hidden bg-surface dark:bg-navy dark:bg-[url('/LoginDarkModeimg.png')] dark:bg-cover dark:bg-center dark:bg-no-repeat w-full transition-colors duration-300">

      {/* Background Mesh Gradients (Only in light mode) */}
      <div className="fixed inset-0 z-0 pointer-events-none dark:hidden">
        <div className="absolute top-[-20%] left-[-10%] w-[70vw] h-[70vw] rounded-full bg-navy/5 blur-[100px]"></div>
        <div className="absolute bottom-[-20%] right-[-10%] w-[60vw] h-[60vw] rounded-full bg-gold/10 blur-[100px]"></div>
      </div>

      <main className="w-full max-w-lg relative z-10 mx-4 sm:mx-0">

        {/* Hanging Bookmark Container - dynamic themes & unrolling animation */}
        <div className="relative bg-white dark:bg-[#1a1d23]/95 backdrop-blur-xl shadow-premium dark:shadow-2xl pt-12 pb-12 px-8 sm:px-14 z-10 border-t-4 border-gold animate-unroll">

          {/* Header */}
          <div className="flex flex-col items-center text-center mb-8">
            <div className="flex items-center justify-center gap-3 mb-2 border-b-2 border-gold/40 pb-2 w-fit mx-auto">
              <LuScale className="w-10 h-10 text-gold" />
              <h1 className="text-4xl font-bold text-navy dark:text-gold tracking-tight">مستشار</h1>
            </div>
            <p className="text-gray-500 dark:text-gray-300 font-medium mt-1">مرحباً بك مجدداً</p>
          </div>

          <div className="w-full border-t border-gray-100 dark:border-gray-800 mb-8"></div>

          {/* Error Message Box */}
          {errorMsg && (
            <div className="mb-6 p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900/50 rounded text-red-600 dark:text-red-400 text-sm font-bold text-center animate-pulse">
              {errorMsg}
            </div>
          )}

          {/* Form */}
          <form onSubmit={handleSubmit} className="flex flex-col gap-5">

            {/* Email / Phone */}
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
                  required
                  placeholder="أدخل البريد الإلكتروني"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  disabled={isPending}
                  className="block w-full pl-3 pr-10 py-3 bg-gray-50 dark:bg-transparent text-navy dark:text-white border border-gray-200 dark:border-gray-750 rounded focus:border-gold focus:ring-1 focus:ring-gold outline-none transition-shadow disabled:opacity-50"
                />
              </div>
            </div>

            {/* Password */}
            <div>
              <div className="flex justify-between items-center mb-2">
                <label className="block text-sm font-bold text-navy dark:text-gray-200" htmlFor="password">
                  كلمة المرور
                </label>
                <Link to="/forgot-password" className="text-sm text-gold font-bold hover:underline transition-all">
                  نسيت كلمة المرور؟
                </Link>
              </div>
              <div className="relative">
                <div className="absolute inset-y-0 right-0 pr-3 flex items-center pointer-events-none">
                  <LuLock className="text-gray-400 dark:text-gray-500" />
                </div>
                <input
                  id="password"
                  name="password"
                  type={showPassword ? "text" : "password"}
                  dir="ltr"
                  required
                  placeholder="••••••••"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  disabled={isPending}
                  className="block w-full pl-12 pr-10 py-3 bg-gray-50 dark:bg-transparent text-navy dark:text-white border border-gray-200 dark:border-gray-750 rounded focus:border-gold focus:ring-1 focus:ring-gold outline-none text-right transition-shadow disabled:opacity-50"
                />

                {/* Password Visibility Toggle */}
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 left-0 pl-4 flex items-center text-gray-400 dark:text-gray-500 hover:text-gold transition-colors focus:outline-none cursor-pointer"
                >
                  {showPassword ? <LuEyeOff className="w-5 h-5" /> : <LuEye className="w-5 h-5" />}
                </button>
              </div>
            </div>

            {/* Submit Button */}
            <button
              type="submit"
              disabled={isPending}
              className="w-full bg-gold hover:bg-gold-hover text-white font-bold py-4 px-6 rounded transition-colors duration-200 mt-2 flex items-center justify-center gap-2 cursor-pointer shadow-premium disabled:opacity-50"
            >
              {isPending ? (
                <>
                  <LuLoader className="w-5 h-5 animate-spin" />
                  <span>جاري تسجيل الدخول...</span>
                </>
              ) : (
                <span>تسجيل الدخول</span>
              )}
            </button>
          </form>



          {/* Sign Up Redirect */}
          <div className="mt-8 text-center">
            <p className="text-sm text-gray-500 dark:text-gray-400">
              ليس لديك حساب؟ <Link to="/register" className="text-gold font-bold hover:underline">إنشاء حساب جديد</Link>
            </p>
          </div>

          {/* Decorative Bookmark Bottom (Single V-cut responsive to theme) */}
          <div className="absolute -bottom-12.5 left-0 right-0 flex h-12.5 -z-10">
            <div className="flex-1 relative bg-white dark:bg-[#1a1d23]/95 after:content-[''] after:absolute after:-bottom-5 after:left-0 after:w-full after:h-5 after:[clip-path:polygon(0_0,50%_100%,100%_0)] after:bg-white dark:after:bg-[#1a1d23]/95 transition-colors duration-300"></div>
          </div>

        </div>
      </main>



    </div>
  );
};