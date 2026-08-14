import { useEffect, useState, useRef } from "react";
import { useSearchParams, useNavigate, Link } from "react-router-dom";
import { AuthApi } from "../features/auth/api/authApi";
import {
  LuScale,
  LuLoader,
  LuCheck,
  LuX,
  LuMail,
  LuArrowRight
} from "react-icons/lu";

export const VerifyEmail = () => {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const userId = searchParams.get("userId");
  const token = searchParams.get("token");

  const [status, setStatus] = useState<'loading' | 'success' | 'error'>('loading');
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  
  // Resend Verification State
  const [resendEmail, setResendEmail] = useState("");
  const [resendLoading, setResendLoading] = useState(false);
  const [resendSuccess, setResendSuccess] = useState<string | null>(null);
  const [resendError, setResendError] = useState<string | null>(null);
  
  // Prevent double-firing in React 18 Strict Mode
  const hasAttempted = useRef(false);

  useEffect(() => {
    const performVerification = async () => {
      if (hasAttempted.current) return;
      hasAttempted.current = true;

      if (!userId || !token) {
        setStatus('error');
        setErrorMessage("الرابط غير صالح. يرجى التأكد من الضغط على الرابط الصحيح في بريدك الإلكتروني.");
        return;
      }

      try {
        const response = await AuthApi.confirmEmail(userId, token);
        if (response.success) {
          setStatus('success');
          // Automatically redirect to login after 3 seconds
          const timer = setTimeout(() => {
            navigate("/login");
          }, 3500);
          return () => clearTimeout(timer);
        } else {
          setStatus('error');
          setErrorMessage(response.message || "فشل التحقق من البريد الإلكتروني.");
        }
      } catch (error: any) {
        setStatus('error');
        const apiError = error.response?.data;
        setErrorMessage(apiError?.message || "حدث خطأ أثناء محاولة تفعيل الحساب أو أن الرابط منتهي الصلاحية.");
      }
    };

    performVerification();
  }, [userId, token, navigate]);

  const handleResend = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!resendEmail) return;

    setResendLoading(true);
    setResendSuccess(null);
    setResendError(null);

    try {
      const response = await AuthApi.resendVerification(resendEmail);
      if (response.success) {
        setResendSuccess(response.message || "تم إعادة إرسال رابط التفعيل بنجاح. يرجى مراجعة بريدك الإلكتروني.");
      } else {
        setResendError(response.message || "فشل إرسال البريد الإلكتروني.");
      }
    } catch (error: any) {
      const apiError = error.response?.data;
      setResendError(apiError?.message || "حدث خطأ أثناء محاولة إعادة إرسال الرابط. يرجى المحاولة لاحقاً.");
    } finally {
      setResendLoading(false);
    }
  };

  return (
    <div className="relative min-h-screen flex items-start justify-center pt-16 pb-32 overflow-hidden bg-surface dark:bg-navy dark:bg-[url('/LoginDarkModeimg.png')] dark:bg-cover dark:bg-center dark:bg-no-repeat w-full transition-colors duration-300">
      
      {/* Background Mesh Gradients (Only in light mode) */}
      <div className="fixed inset-0 z-0 pointer-events-none dark:hidden">
        <div className="absolute top-[-20%] left-[-10%] w-[70vw] h-[70vw] rounded-full bg-navy/5 blur-[100px]"></div>
        <div className="absolute bottom-[-20%] right-[-10%] w-[60vw] h-[60vw] rounded-full bg-gold/10 blur-[100px]"></div>
      </div>

      <main className="w-full max-w-xl relative z-10 mx-4 sm:mx-0">
        <div className="relative bg-white dark:bg-[#1a1d23]/95 backdrop-blur-xl shadow-premium dark:shadow-2xl pt-12 pb-12 px-8 sm:px-12 border-t-4 border-gold animate-unroll text-center">
          
          {/* Header Logo */}
          <div className="flex items-center justify-center gap-3 mb-6 border-b-2 border-gold/40 pb-2 w-fit mx-auto">
            <LuScale className="w-10 h-10 text-gold" />
            <h1 className="text-4xl font-bold text-navy dark:text-gold tracking-tight">مستشار</h1>
          </div>

          <div className="w-full border-t border-gray-100 dark:border-gray-800 mb-8"></div>

          {status === 'loading' && (
            <div className="flex flex-col items-center py-8">
              <LuLoader className="w-16 h-16 text-gold animate-spin mb-4" />
              <h2 className="text-2xl font-bold text-navy dark:text-white mb-2">جاري التحقق وتفعيل حسابك</h2>
              <p className="text-sm text-gray-500 dark:text-gray-400">
                يرجى الانتظار لحظة بينما نتحقق من معلومات البريد الإلكتروني الخاص بك...
              </p>
            </div>
          )}

          {status === 'success' && (
            <div className="flex flex-col items-center py-6">
              <LuCheck className="w-16 h-16 text-green-500 mb-4 animate-bounce" />
              <h2 className="text-2xl font-bold text-navy dark:text-white mb-3">تم تفعيل حسابك بنجاح!</h2>
              <p className="text-sm text-gray-600 dark:text-gray-300 leading-relaxed mb-6">
                تهانينا، تم تأكيد بريدك الإلكتروني وتفعيل حسابك بالكامل. 
                سيتم توجيهك تلقائياً لصفحة تسجيل الدخول خلال ثوانٍ...
              </p>
              <div className="flex items-center gap-2 text-xs text-gray-400 dark:text-gray-500">
                <LuLoader className="w-4 h-4 animate-spin text-gold" />
                <span>جاري الانتقال لصفحة تسجيل الدخول...</span>
              </div>
              <Link
                to="/login"
                className="mt-6 inline-flex items-center gap-2 bg-gold hover:bg-gold-hover text-white font-bold py-3 px-8 rounded transition-colors duration-200 shadow-premium"
              >
                <span>تسجيل الدخول الآن</span>
                <LuArrowRight className="w-4 h-4" />
              </Link>
            </div>
          )}

          {status === 'error' && (
            <div className="flex flex-col items-center py-4">
              <LuX className="w-16 h-16 text-red-500 mb-4" />
              <h2 className="text-2xl font-bold text-navy dark:text-white mb-2">فشل تفعيل الحساب</h2>
              <p className="text-sm text-red-600 dark:text-red-400 leading-relaxed mb-6 max-w-md">
                {errorMessage}
              </p>

              <div className="w-full border-t border-gray-100 dark:border-gray-800 my-6"></div>

              {/* Resend Verification Link Section */}
              <div className="w-full text-right">
                <h3 className="text-sm font-bold text-navy dark:text-white mb-3">هل انتهت صلاحية الرابط؟ أرسل رابطاً جديداً:</h3>
                
                {resendSuccess && (
                  <div className="mb-4 p-3 bg-green-50 dark:bg-green-950/20 border border-green-200 dark:border-green-900/30 rounded text-green-600 dark:text-green-400 text-sm font-bold text-center">
                    {resendSuccess}
                  </div>
                )}
                
                {resendError && (
                  <div className="mb-4 p-3 bg-red-50 dark:bg-red-950/20 border border-red-200 dark:border-red-900/30 rounded text-red-600 dark:text-red-400 text-sm font-bold text-center">
                    {resendError}
                  </div>
                )}

                <form onSubmit={handleResend} className="flex gap-2">
                  <div className="relative flex-1">
                    <LuMail className="absolute right-3 top-3.5 text-gray-400" />
                    <input
                      type="email"
                      required
                      placeholder="بريدك الإلكتروني"
                      value={resendEmail}
                      onChange={(e) => setResendEmail(e.target.value)}
                      disabled={resendLoading}
                      className="block w-full pl-3 pr-10 py-3 bg-gray-50 dark:bg-transparent text-navy dark:text-white border border-gray-200 dark:border-gray-750 rounded focus:border-gold focus:ring-1 focus:ring-gold outline-none text-right transition-shadow disabled:opacity-50"
                    />
                  </div>
                  <button
                    type="submit"
                    disabled={resendLoading}
                    className="bg-gold hover:bg-gold-hover text-white font-bold px-5 rounded transition-colors duration-200 flex items-center justify-center gap-2 cursor-pointer shadow-premium disabled:opacity-50"
                  >
                    {resendLoading ? <LuLoader className="w-5 h-5 animate-spin" /> : "إرسال رابط جديد"}
                  </button>
                </form>
              </div>

              <Link
                to="/register"
                className="mt-8 text-sm text-gold font-bold hover:underline"
              >
                العودة لإنشاء الحساب
              </Link>
            </div>
          )}
          
        </div>
      </main>
    </div>
  );
};
export default VerifyEmail;
