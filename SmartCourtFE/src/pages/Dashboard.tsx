import { useEffect } from "react";
import { useNavigate, useOutletContext } from "react-router-dom";
import { ProfilePage } from "../features/users";
import { VerificationTab } from "../features/auth/components/VerificationTab";
import { AdminVerificationsTab } from "../features/admin/verifications/components/AdminVerificationsTab";
import { LawyersPage } from "./LawyersPage";
import { CreateCaseTab } from "../features/cases";

import {
  LuSearch,
  LuSlidersHorizontal,
  LuTriangleAlert,
  LuClock,
  LuCircleCheck,
  LuFileText,
} from "react-icons/lu";

export const Dashboard = () => {
  const navigate = useNavigate();
  const { user, activeTab } = useOutletContext<{ user: any; activeTab: string }>();

  useEffect(() => {
    if (!user) {
      navigate("/login");
    }
  }, [user, navigate]);

  return (
    <main className="flex-1 p-4 sm:p-8 overflow-y-auto w-full">



        {/* Tab: Settings / Profile (Integrates the ProfilePage) */}
        {activeTab === "settings" && (
          <div>
            <ProfilePage />
          </div>
        )}

        {/* Tab: Admin Verifications */}
        {activeTab === "admin-verifications" && user?.role === 'Admin' && (
          <div className="animate-fade-in">
            <AdminVerificationsTab />
          </div>
        )}

        {/* Tab: Cases ("قضاياي" - Matching exact screenshot) */}
        {(activeTab === "cases" || activeTab === "overview") && (
          <div className="space-y-6">

            {/* Top Bar Header */}
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
              <div>
                <h1 className="text-2xl font-bold text-gray-900 dark:text-white">إدارة القضايا</h1>
                <p className="text-xs text-gray-500 dark:text-gray-400 mt-1">
                  نظرة شاملة ومتابعة دقيقة لجميع ملفاتك القانونية.
                </p>
              </div>

              {/* Search & Filter */}
              <div className="flex items-center gap-3 w-full sm:w-auto">
                <div className="relative flex-1 sm:w-72">
                  <LuSearch className="absolute right-3.5 top-1/2 -translate-y-1/2 text-gray-400 w-4 h-4" />
                  <input
                    type="text"
                    placeholder="إبحث برقم القضية أو العنوان..."
                    className="w-full pl-4 pr-10 py-2.5 bg-white dark:bg-[#1a1d23] border border-gray-200 dark:border-gray-800 rounded-xl text-xs outline-none focus:border-gold shadow-xs"
                  />
                </div>

                <button className="h-10 px-4 bg-white dark:bg-[#1a1d23] border border-gray-200 dark:border-gray-800 rounded-xl text-xs font-bold text-gray-700 dark:text-gray-300 hover:border-gold flex items-center gap-2 cursor-pointer shadow-xs">
                  <LuSlidersHorizontal className="w-4 h-4 text-gold" />
                  <span>تصفية</span>
                </button>
              </div>
            </div>

            {/* Grid Layout: Left Column (Stats & Alerts) + Right Column (Cases List) */}
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-6">

              {/* Cases List Main Card (8 Cols) */}
              <div className="lg:col-span-8 space-y-4">
                <div className="bg-white dark:bg-[#1a1d23] rounded-3xl p-6 border border-gray-200/80 dark:border-gray-800 shadow-xs">

                  <div className="flex items-center justify-between pb-4 mb-4 border-b border-gray-100 dark:border-gray-800">
                    <h3 className="text-base font-bold text-gray-900 dark:text-white">قائمة القضايا</h3>
                    <span className="px-3 py-1 rounded-full text-xs font-bold bg-navy text-white dark:bg-gold dark:text-navy">
                      إجمالي: 4 قضايا
                    </span>
                  </div>

                  {/* Cases Cards */}
                  <div className="space-y-4">

                    {/* Item 1 */}
                    <div className="p-5 rounded-2xl bg-amber-50/40 dark:bg-amber-950/10 border border-amber-200/60 dark:border-amber-900/30 flex items-start justify-between gap-4">
                      <div className="space-y-2">
                        <div className="flex items-center gap-2">
                          <span className="px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-amber-200/80 text-amber-800 dark:bg-amber-900 dark:text-amber-200">
                            قيد المراجعة
                          </span>
                          <span className="text-xs font-mono font-bold text-gray-400">#CASE-2023-089</span>
                        </div>
                        <h4 className="text-sm font-bold text-gray-900 dark:text-white">
                          نزاع تجاري حول عقد توريد مواد بناء
                        </h4>
                        <p className="text-xs text-gray-500 dark:text-gray-400 leading-relaxed">
                          تم تقديم اللائحة الجوابية بانتظار تحديد موعد الجلسة القادمة...
                        </p>
                      </div>

                      <div className="flex flex-col items-end gap-3 shrink-0">
                        <div className="w-10 h-10 rounded-xl bg-amber-100 dark:bg-amber-900/40 text-amber-700 dark:text-amber-300 flex items-center justify-center">
                          <LuClock className="w-5 h-5" />
                        </div>
                        <span className="text-[10px] text-gray-400 font-medium">آخر تحديث: قبل ساعتين</span>
                      </div>
                    </div>

                    {/* Item 2 */}
                    <div className="p-5 rounded-2xl bg-red-50/40 dark:bg-red-950/10 border border-red-200/60 dark:border-red-900/30 flex items-start justify-between gap-4">
                      <div className="space-y-2">
                        <div className="flex items-center gap-2">
                          <span className="px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-red-200/80 text-red-800 dark:bg-red-900 dark:text-red-200 flex items-center gap-1">
                            <LuTriangleAlert className="w-3 h-3" />
                            <span>إجراء مطلوب</span>
                          </span>
                          <span className="text-xs font-mono font-bold text-gray-400">#CASE-2023-042</span>
                        </div>
                        <h4 className="text-sm font-bold text-gray-900 dark:text-white">
                          قضية تصفية تركة - عائلة الشمري
                        </h4>
                        <p className="text-xs text-gray-500 dark:text-gray-400 leading-relaxed">
                          بانتظار سداد رسوم الخبرة المحاسبية
                        </p>
                      </div>

                      <div className="flex flex-col items-end gap-3 shrink-0">
                        <div className="w-10 h-10 rounded-xl bg-red-100 dark:bg-red-900/40 text-red-600 dark:text-red-300 flex items-center justify-center">
                          <LuTriangleAlert className="w-5 h-5" />
                        </div>
                        <span className="text-[10px] text-gray-400 font-medium">آخر تحديث: الأمس</span>
                      </div>
                    </div>

                    {/* Item 3 */}
                    <div className="p-5 rounded-2xl bg-blue-50/40 dark:bg-blue-950/10 border border-blue-200/60 dark:border-blue-900/30 flex items-start justify-between gap-4">
                      <div className="space-y-2">
                        <div className="flex items-center gap-2">
                          <span className="px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-blue-200/80 text-blue-800 dark:bg-blue-900 dark:text-blue-200">
                            جلسة محددة
                          </span>
                          <span className="text-xs font-mono font-bold text-gray-400">#CASE-2023-112</span>
                        </div>
                        <h4 className="text-sm font-bold text-gray-900 dark:text-white">
                          دعوى عمالية - المطالبة بمكافأة نهاية الخدمة
                        </h4>
                        <p className="text-xs text-gray-500 dark:text-gray-400 leading-relaxed">
                          الجلسة القادمة بتاريخ 15 نوفمبر 2023
                        </p>
                      </div>

                      <div className="flex flex-col items-end gap-3 shrink-0">
                        <div className="w-10 h-10 rounded-xl bg-blue-100 dark:bg-blue-900/40 text-blue-600 dark:text-blue-300 flex items-center justify-center">
                          <LuFileText className="w-5 h-5" />
                        </div>
                        <span className="text-[10px] text-gray-400 font-medium">آخر تحديث: 3 أيام</span>
                      </div>
                    </div>

                    {/* Item 4 */}
                    <div className="p-5 rounded-2xl bg-gray-50 dark:bg-gray-800/30 border border-gray-200/60 dark:border-gray-800 flex items-start justify-between gap-4">
                      <div className="space-y-2">
                        <div className="flex items-center gap-2">
                          <span className="px-2.5 py-0.5 rounded-full text-[10px] font-bold bg-gray-200 text-gray-700 dark:bg-gray-700 dark:text-gray-300">
                            مغلقة
                          </span>
                          <span className="text-xs font-mono font-bold text-gray-400">#CASE-2022-884</span>
                        </div>
                        <h4 className="text-sm font-bold text-gray-900 dark:text-white">
                          تسجيل علامة تجارية
                        </h4>
                        <p className="text-xs text-gray-500 dark:text-gray-400 leading-relaxed">
                          تم استخراج الشهادة النهائية وتسليمها للعميل
                        </p>
                      </div>

                      <div className="flex flex-col items-end gap-3 shrink-0">
                        <div className="w-10 h-10 rounded-xl bg-gray-200/60 dark:bg-gray-700/50 text-gray-500 flex items-center justify-center">
                          <LuCircleCheck className="w-5 h-5" />
                        </div>
                        <span className="text-[10px] text-gray-400 font-medium">مغلقة: قبل شهرين</span>
                      </div>
                    </div>

                  </div>

                </div>
              </div>

              {/* Sidebar Widgets (4 Cols) */}
              <div className="lg:col-span-4 space-y-6">

                {/* Urgent Action Card */}
                <div className="bg-[#121620] rounded-3xl p-6 text-white space-y-5 shadow-lg border border-gray-800 relative overflow-hidden">
                  <div className="flex items-center gap-2 text-gold">
                    <LuTriangleAlert className="w-5 h-5" />
                    <h3 className="text-sm font-bold">إجراء مطلوب عاجل</h3>
                  </div>

                  <p className="text-xs text-gray-300 leading-relaxed">
                    يوجد طلب سداد رسوم للقضية <span className="text-gold font-mono font-bold">#CASE-2023-042</span> يجب تنفيذه قبل انتهاء المهلة المحددة.
                  </p>

                  <div className="grid grid-cols-2 gap-4 py-3 border-y border-gray-800">
                    <div>
                      <p className="text-[10px] text-gray-400">المبلغ المطلوب</p>
                      <p className="text-base font-bold text-gold mt-0.5">2,500 ر.س</p>
                    </div>
                    <div>
                      <p className="text-[10px] text-gray-400">المهلة المتبقية</p>
                      <p className="text-base font-bold text-white mt-0.5">48 ساعة</p>
                    </div>
                  </div>

                  <button className="w-full h-11 bg-gold hover:bg-gold-hover text-white font-bold text-sm rounded-xl transition-all shadow-md cursor-pointer">
                    استكمال الدفع الآن
                  </button>
                </div>

                {/* Portfolio Summary Card */}
                <div className="bg-white dark:bg-[#1a1d23] rounded-3xl p-6 border border-gray-200/80 dark:border-gray-800 shadow-xs space-y-5">
                  <h3 className="text-base font-bold text-gray-900 dark:text-white pb-3 border-b border-gray-100 dark:border-gray-800">
                    ملخص المحفظة
                  </h3>

                  <div className="grid grid-cols-2 gap-4">
                    <div className="p-4 rounded-2xl bg-gray-50 dark:bg-gray-800/40 text-center">
                      <p className="text-2xl font-bold text-navy dark:text-gold">5</p>
                      <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 mt-1">قضايا نشطة</p>
                    </div>

                    <div className="p-4 rounded-2xl bg-gray-50 dark:bg-gray-800/40 text-center">
                      <p className="text-2xl font-bold text-gold">2</p>
                      <p className="text-xs font-semibold text-gray-500 dark:text-gray-400 mt-1">قضايا كسبت</p>
                    </div>
                  </div>

                  {/* Progress Bar */}
                  <div className="space-y-2">
                    <div className="flex justify-between text-xs font-bold">
                      <span className="text-gray-500">نسبة الإنجاز العام</span>
                      <span className="text-gold">75%</span>
                    </div>
                    <div className="w-full h-2 rounded-full bg-gray-100 dark:bg-gray-800 overflow-hidden">
                      <div className="h-full bg-gold rounded-full w-[75%]"></div>
                    </div>
                  </div>

                  {/* Upcoming Appointments */}
                  <div className="pt-3 border-t border-gray-100 dark:border-gray-800 space-y-3">
                    <p className="text-xs font-bold text-gray-900 dark:text-white">المواعيد القادمة</p>

                    <div className="flex items-start gap-2.5 text-xs text-gray-600 dark:text-gray-300">
                      <span className="w-2 h-2 rounded-full bg-gold mt-1.5 shrink-0"></span>
                      <div>
                        <p className="font-semibold">جلسة استماع - قضية #112</p>
                        <p className="text-[10px] text-gray-400">غداً</p>
                      </div>
                    </div>

                    <div className="flex items-start gap-2.5 text-xs text-gray-600 dark:text-gray-300">
                      <span className="w-2 h-2 rounded-full bg-gold mt-1.5 shrink-0"></span>
                      <div>
                        <p className="font-semibold">موعد تسليم مذكرة رد</p>
                        <p className="text-[10px] text-gray-400">12 نوفمبر</p>
                      </div>
                    </div>
                  </div>

                </div>

              </div>

            </div>

          </div>
        )}

        {activeTab === "verification" && <VerificationTab />}
        {activeTab === "lawyers" && <LawyersPage />}

        {activeTab === "new-case" && (
          <div className="animate-fade-in">
            <CreateCaseTab />
          </div>
        )}

        {activeTab === "chats" && (
          <div className="min-h-[50vh] flex flex-col items-center justify-center bg-white dark:bg-[#1a1d23] rounded-3xl p-12 text-center border border-gray-200 dark:border-gray-800">
            <LuFileText className="w-16 h-16 text-gold mb-4 animate-pulse" />
            <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
              صفحة المحادثات
            </h3>
            <p className="text-xs text-gray-500 dark:text-gray-400 max-w-sm">
              جاري تجهيز هذه الصفحة بالتفصيل خطوة بخطوة حسب خطة التطوير والملاحظات...
            </p>
          </div>
        )}

      </main>
  );
};
