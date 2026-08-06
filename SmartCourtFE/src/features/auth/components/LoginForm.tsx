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
  LuUser,
  LuGavel
} from "react-icons/lu";
import { useGoogleLogin } from "@react-oauth/google";
import { motion, AnimatePresence } from "framer-motion";

export const LoginForm = () => {
  const [showPassword, setShowPassword] = useState(false);
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  
  // Google Role Selection State
  const [showRoleModal, setShowRoleModal] = useState(false);
  const [pendingGoogleToken, setPendingGoogleToken] = useState<string | null>(null);
  const [selectedRole, setSelectedRole] = useState<'client' | 'lawyer'>('client');
  
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

  // Mutation for Google Login
  const googleMutation = useMutation({
    mutationFn: AuthApi.googleLogin,
    onSuccess: (response) => {
      if (response.success && response.data) {
        setShowRoleModal(false);
        setPendingGoogleToken(null);
        const { user } = response.data as any;
        loginStore(user);
        navigate("/", { replace: true });
      } else {
        setErrorMsg(response.message || "خطأ غير متوقع أثناء تسجيل الدخول");
      }
    },
    onError: (error: any) => {
      const apiError = error.response?.data;
      if (apiError?.message === "ROLE_REQUIRED") {
        setShowRoleModal(true);
      } else if (apiError) {
        setErrorMsg(apiError.message || "فشل تسجيل الدخول بواسطة جوجل");
      } else {
        setErrorMsg("حدث خطأ في الاتصال بالخادم. يرجى المحاولة لاحقاً");
      }
    }
  });

  const handleGoogleLogin = useGoogleLogin({
    onSuccess: (codeResponse) => {
      setPendingGoogleToken(codeResponse.access_token);
      googleMutation.mutate({ idToken: codeResponse.access_token });
    },
    onError: (error) => {
      console.log('Google Login Failed', error);
      setErrorMsg("تم إلغاء عملية تسجيل الدخول بواسطة جوجل");
    }
  });

  const handleRoleSubmit = () => {
    if (pendingGoogleToken) {
      googleMutation.mutate({ idToken: pendingGoogleToken, role: selectedRole });
    }
  };

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

          {/* Quick Auth Divider */}
          <div className="relative flex items-center py-6">
            <div className="grow border-t border-gray-200 dark:border-gray-800"></div>
            <span className="shrink-0 mx-4 text-gray-400 dark:text-gray-500 text-sm font-bold">أو المتابعة باستخدام</span>
            <div className="grow border-t border-gray-200 dark:border-gray-800"></div>
          </div>

          {/* Google Button */}
          <button
            type="button"
            onClick={() => handleGoogleLogin()}
            disabled={googleMutation.isPending}
            className="w-full bg-white dark:bg-transparent hover:bg-gray-50 dark:hover:bg-gray-800 border border-gray-200 dark:border-gray-700 text-navy dark:text-gray-200 font-bold text-sm py-3 px-6 rounded transition-colors duration-200 flex items-center justify-center gap-3 cursor-pointer disabled:opacity-50"
          >
            {googleMutation.isPending && !showRoleModal ? (
              <LuLoader className="w-5 h-5 animate-spin" />
            ) : (
              <svg className="w-5 h-5" viewBox="0 0 24 24">
                <path d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" fill="#4285F4"></path>
                <path d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" fill="#34A853"></path>
                <path d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.07H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.93l2.85-2.22.81-.62z" fill="#FBBC05"></path>
                <path d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.07l3.66 2.84c.87-2.6 3.3-4.53 6.16-4.53z" fill="#EA4335"></path>
              </svg>
            )}
            Google
          </button>

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

      {/* Role Selection Modal */}
      <AnimatePresence>
        {showRoleModal && (
          <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
            <motion.div
              initial={{ opacity: 0 }}
              animate={{ opacity: 1 }}
              exit={{ opacity: 0 }}
              className="absolute inset-0 bg-navy/60 backdrop-blur-sm"
              onClick={() => setShowRoleModal(false)}
            />
            <motion.div
              initial={{ scale: 0.95, opacity: 0, y: 20 }}
              animate={{ scale: 1, opacity: 1, y: 0 }}
              exit={{ scale: 0.95, opacity: 0, y: 20 }}
              className="relative bg-white dark:bg-surface w-full max-w-md rounded-2xl shadow-2xl overflow-hidden"
            >
              <div className="p-6">
                <h3 className="text-xl font-bold text-navy dark:text-white mb-2 text-center">إكمال التسجيل</h3>
                <p className="text-sm text-gray-500 dark:text-gray-400 text-center mb-6">يرجى اختيار نوع الحساب للمتابعة</p>

                <div className="flex gap-4 mb-6">
                  <button
                    onClick={() => setSelectedRole('client')}
                    className={`flex-1 flex flex-col items-center p-4 rounded-xl border-2 transition-all cursor-pointer ${
                      selectedRole === 'client'
                        ? 'border-gold bg-gold/5 text-gold'
                        : 'border-gray-200 dark:border-gray-700 text-gray-500 hover:border-gold/50'
                    }`}
                  >
                    <LuUser className="w-8 h-8 mb-2" />
                    <span className="font-bold">عميل/موكل</span>
                  </button>
                  <button
                    onClick={() => setSelectedRole('lawyer')}
                    className={`flex-1 flex flex-col items-center p-4 rounded-xl border-2 transition-all cursor-pointer ${
                      selectedRole === 'lawyer'
                        ? 'border-gold bg-gold/5 text-gold'
                        : 'border-gray-200 dark:border-gray-700 text-gray-500 hover:border-gold/50'
                    }`}
                  >
                    <LuGavel className="w-8 h-8 mb-2" />
                    <span className="font-bold">محامٍ</span>
                  </button>
                </div>

                <div className="flex gap-3">
                  <button
                    onClick={() => setShowRoleModal(false)}
                    className="flex-1 py-3 px-4 rounded-xl font-bold text-gray-600 dark:text-gray-300 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 transition-colors cursor-pointer"
                  >
                    إلغاء
                  </button>
                  <button
                    onClick={handleRoleSubmit}
                    disabled={googleMutation.isPending}
                    className="flex-1 py-3 px-4 rounded-xl font-bold text-white bg-gold hover:bg-gold-hover transition-colors flex items-center justify-center gap-2 cursor-pointer disabled:opacity-50"
                  >
                    {googleMutation.isPending ? (
                      <LuLoader className="w-5 h-5 animate-spin" />
                    ) : (
                      <span>متابعة</span>
                    )}
                  </button>
                </div>
              </div>
            </motion.div>
          </div>
        )}
      </AnimatePresence>

    </div>
  );
};