import { useState } from "react";
import { Link } from "react-router-dom";
import { useMutation } from "@tanstack/react-query";
import { AuthApi } from "../api/authApi";
import {
  LuScale,
  LuMail,
  LuLoader,
  LuCheck,
  LuArrowRight
} from "react-icons/lu";

export const ForgotPasswordForm = () => {
  const [email, setEmail] = useState("");
  const [errorMsg, setErrorMsg] = useState<string | null>(null);
  const [successMsg, setSuccessMsg] = useState<string | null>(null);

  const { mutate, isPending } = useMutation({
    mutationFn: AuthApi.forgotPassword,
    onSuccess: (response) => {
      if (response.success) {
        setSuccessMsg(
          response.message ||
            "إذا كان البريد الإلكتروني مسجلاً، سيتم إرسال رابط إعادة تعيين كلمة المرور"
        );
      } else {
        setErrorMsg(response.message || "فشل إرسال طلب إعادة التعيين");
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
          setErrorMsg("فشل الاتصال بالخادم. يرجى التحقق من البريد الإلكتروني");
        }
      } else {
        setErrorMsg("حدث خطأ في الاتصال بالخادم. يرجى المحاولة لاحقاً");
      }
    }
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setErrorMsg(null);

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(email)) {
      setErrorMsg("يرجى إدخال بريد إلكتروني صالح.");
      return;
    }

    mutate(email);
  };

  return (
    // Outer wrapper for page background - responsive to dark mode
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
            <p className="text-2xl font-bold text-navy dark:text-white mt-2">استعادة كلمة المرور</p>
            <p className="text-xs text-gray-500 dark:text-gray-400 mt-1 max-w-xs leading-relaxed">
              أدخل بريدك الإلكتروني وستتلقى رابطاً لإعادة تعيين كلمة المرور الخاصة بك.
            </p>
          </div>

          <div className="w-full border-t border-gray-100 dark:border-gray-800 mb-8"></div>

          {successMsg ? (
            /* Success Message Card */
            <div className="flex flex-col items-center justify-center text-center p-8 bg-green-50/50 dark:bg-green-950/10 rounded-2xl border border-green-200/50 dark:border-green-900/30 my-4">
              <LuCheck className="w-16 h-16 text-green-500 mb-4 animate-bounce" />
              <h3 className="text-xl font-bold text-navy dark:text-white mb-2">تم الإرسال بنجاح!</h3>
              <p className="text-sm text-gray-600 dark:text-gray-300 leading-relaxed mb-6">
                {successMsg}
              </p>
              <Link
                to="/login"
                className="bg-gold hover:bg-gold-hover text-white font-bold py-3 px-8 rounded transition-colors duration-200 shadow-premium flex items-center gap-2"
              >
                <span>العودة لتسجيل الدخول</span>
                <LuArrowRight className="w-4 h-4 rotate-180" />
              </Link>
            </div>
          ) : (
            <>
              {/* Error Message Box */}
              {errorMsg && (
                <div className="mb-6 p-4 bg-red-50 dark:bg-red-950/30 border border-red-200 dark:border-red-900/50 rounded text-red-600 dark:text-red-400 text-sm font-bold text-center">
                  {errorMsg}
                </div>
              )}

              {/* Form */}
              <form onSubmit={handleSubmit} className="flex flex-col gap-5">
                {/* Email Input */}
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
                      placeholder="أدخل البريد الإلكتروني المسجل"
                      value={email}
                      onChange={(e) => setEmail(e.target.value)}
                      disabled={isPending}
                      className="block w-full pl-3 pr-10 py-3 bg-gray-50 dark:bg-transparent text-navy dark:text-white border border-gray-200 dark:border-gray-750 rounded focus:border-gold focus:ring-1 focus:ring-gold outline-none transition-shadow disabled:opacity-50"
                    />
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
                      <span>جاري الإرسال...</span>
                    </>
                  ) : (
                    <span>إرسال رابط التعيين</span>
                  )}
                </button>
              </form>
            </>
          )}

          {/* Back to Login Redirect */}
          <div className="mt-8 text-center">
            <Link to="/login" className="text-sm text-gold font-bold hover:underline inline-flex items-center gap-2 transition-all">
              <LuArrowRight className="w-4 h-4 rotate-180" />
              <span>تذكرت كلمة المرور؟ العودة لتسجيل الدخول</span>
            </Link>
          </div>

          {/* Decorative Bookmark Bottom */}
          <div className="absolute -bottom-12.5 left-0 right-0 flex h-12.5 -z-10">
            <div className="flex-1 relative bg-white dark:bg-[#1a1d23]/95 after:content-[''] after:absolute after:-bottom-5 after:left-0 after:w-full after:h-5 after:[clip-path:polygon(0_0,50%_100%,100%_0)] after:bg-white dark:after:bg-[#1a1d23]/95 transition-colors duration-300"></div>
          </div>

        </div>
      </main>
    </div>
  );
};
