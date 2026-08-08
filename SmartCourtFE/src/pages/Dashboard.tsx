import { useState, useEffect, useMemo } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useAuthStore } from "../features/auth/store/useAuthStore";
import { ProfilePage } from "../features/users";
import { UserStatusBadge } from "../features/auth/components/UserStatusBadge";
import { VerificationTab } from "../features/auth/components/VerificationTab";
import { AdminVerificationsTab } from "../features/admin/verifications/components/AdminVerificationsTab";
import { useQuery } from "@tanstack/react-query";
import { AuthApi } from "../features/auth/api/authApi";
import { UsersApi } from "../features/users/api/usersApi";
import { calculateProfileCompletion } from "../utils/profileCompletion";

import {
  LuScale,
  LuLayoutDashboard,
  LuFolder,
  LuFilePlus,
  LuUsers,
  LuShieldCheck,
  LuMessageSquare,
  LuSettings,
  LuCircleHelp,
  LuLogOut,
  LuSearch,
  LuSlidersHorizontal,
  LuTriangleAlert,
  LuClock,
  LuCircleCheck,
  LuFileText,
  LuMenu,
  LuX,
  LuUser
} from "react-icons/lu";

export const Dashboard = () => {
  const { user, logout } = useAuthStore();
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();

  // Active tab state from URL or default to 'cases'
  const tabFromUrl = searchParams.get("tab") || "cases";
  const [activeTab, setActiveTab] = useState<string>(tabFromUrl);
  const [sidebarOpen, setSidebarOpen] = useState(false);
  
  const { data: documentsData } = useQuery({
    queryKey: ["user", "verifications", "documents", user?.id],
    queryFn: () => AuthApi.getUserVerificationDocuments(user!.id),
    enabled: !!user?.id && (user?.role === 'Lawyer' || user?.role === 'Client'),
  });

  const { data: profileData } = useQuery({
    queryKey: ["user", "profile", user?.id],
    queryFn: () => user?.role === 'Lawyer' ? UsersApi.getLawyerProfile() : UsersApi.getClientProfile(),
    enabled: !!user?.id && (user?.role === 'Lawyer' || user?.role === 'Client'),
  });

  // Calculate actual progress based on profile and documents
  const targetProgress = useMemo(() => {
    return calculateProfileCompletion(
      user, 
      profileData || null, 
      documentsData?.data?.documents || []
    );
  }, [user, profileData, documentsData]);

  const [displayedProgress, setDisplayedProgress] = useState(0);
  const [showCompletionText, setShowCompletionText] = useState(false);
  
  useEffect(() => {
    // Reset states when targetProgress changes (or initially)
    setDisplayedProgress(0);
    setShowCompletionText(false);

    // Start progress animation after 800ms delay
    const progressTimer = setTimeout(() => {
      setDisplayedProgress(targetProgress);
    }, 800);
    
    // Show text after progress animation completes (800ms delay + 1500ms transition)
    const textTimer = setTimeout(() => {
      setShowCompletionText(true);
    }, 2300);

    return () => {
      clearTimeout(progressTimer);
      clearTimeout(textTimer);
    };
  }, [targetProgress]);

  const profilePictureDoc = documentsData?.data?.documents?.find((d: any) =>
    (d.documentType === 'OfficialProfilePicture' || d.documentType === 7) && d.isCurrent
  );
  const isPictureApproved = profilePictureDoc?.status === 'Verified' || profilePictureDoc?.status === 2;

  const { data: profilePicContent } = useQuery({
    queryKey: ["documentContent", profilePictureDoc?.documentId],
    queryFn: () => AuthApi.getDocumentContent(profilePictureDoc!.documentId),
    enabled: !!profilePictureDoc?.documentId && isPictureApproved,
  });

  const profilePictureUrl = isPictureApproved ? (profilePicContent?.data?.downloadUrl || null) : null;

  useEffect(() => {
    if (!user) {
      navigate("/login");
    }
  }, [user, navigate]);

  const handleTabChange = (tab: string) => {
    setActiveTab(tab);
    setSearchParams({ tab });
    setSidebarOpen(false);
  };

  const handleLogout = () => {
    logout();
    navigate("/login");
  };

  const getRoleLabel = (role?: string) => {
    switch (role) {
      case "Lawyer":
        return "محامي ";
      case "Client":
        return "موكل";
      case "Admin":
        return "مسؤول منصة";
      default:
        return "مستخدم";
    }
  };

  return (
    <div className="min-h-screen bg-[#f4f5f8] dark:bg-[#0d1017] text-text-primary flex flex-col md:flex-row transition-colors duration-300">

      {/* Mobile Header Bar */}
      <div className="md:hidden flex items-center justify-between p-4 bg-[#121620] text-white sticky top-0 z-40 border-b border-gray-800">
        <div className="flex items-center gap-3">
          <div className="w-9 h-9 rounded-full bg-gold/20 text-gold flex items-center justify-center border border-gold/40 font-bold overflow-hidden">
            {profilePictureUrl ? (
              <img src={profilePictureUrl} alt={user?.fullName || "Profile"} className="w-full h-full object-cover" />
            ) : user?.fullName ? (
              user.fullName.charAt(0).toUpperCase()
            ) : (
              <LuScale className="w-5 h-5" />
            )}
          </div>
          <div>
            <div className="flex items-center gap-2">
              <p className="text-sm font-bold text-white">{user?.fullName}</p>
              <UserStatusBadge status={user?.status} role={user?.role} />
            </div>
            <p className="text-[10px] text-gold font-bold">{getRoleLabel(user?.role)}</p>
          </div>
        </div>
        <div className="flex items-center gap-1">

          <button
            onClick={() => setSidebarOpen(!sidebarOpen)}
            className="p-2 rounded-xl bg-gray-800 text-white"
          >
            {sidebarOpen ? <LuX className="w-6 h-6" /> : <LuMenu className="w-6 h-6" />}
          </button>
        </div>
      </div>

      {/* RIGHT SIDEBAR */}
      <aside
        className={`fixed md:sticky top-0 right-0 z-50 h-screen w-72 bg-[#121620] text-gray-300 flex flex-col p-6 transition-transform duration-300 border-l border-gray-800/60 overflow-visible gap-6 ${sidebarOpen ? "translate-x-0" : "translate-x-full md:translate-x-0"
          }`}
      >
        <div className="absolute top-4 left-4 hidden md:block">

        </div>
        <div className="space-y-6">

          {/* Top Profile Avatar Header */}
          <div className="flex flex-col items-center text-center pt-2 pb-2">

            {/* Avatar & Progress Wrapper */}
            <div className="relative w-[140px] h-[140px] flex items-center justify-center mt-2 mb-4">

              {/* SVG Progress Ring */}
              <svg className="absolute top-0 left-0 w-full h-full -rotate-90 drop-shadow-[0_0_10px_rgba(212,175,55,0.2)]" viewBox="0 0 140 140">
                <defs>
                  <linearGradient id="goldGradient" x1="100%" y1="0%" x2="0%" y2="100%">
                    <stop offset="0%" stopColor="#d4af37" />
                    <stop offset="100%" stopColor="rgba(212, 175, 55, 0.2)" />
                  </linearGradient>
                </defs>
                {/* Background Track */}
                <circle
                  cx="70" cy="70" r="66"
                  fill="none"
                  stroke="rgba(212, 175, 55, 0.05)"
                  strokeWidth="3.5"
                />
                {/* Progress Stroke */}
                <circle
                  cx="70" cy="70" r="66"
                  fill="none"
                  stroke="url(#goldGradient)"
                  strokeWidth="3.5"
                  strokeLinecap="round"
                  strokeDasharray={2 * Math.PI * 66}
                  strokeDashoffset={(2 * Math.PI * 66) * (1 - displayedProgress / 100)}
                  className="transition-[stroke-dashoffset] duration-[1500ms] ease-out"
                />
              </svg>

              {/* Inner Avatar Image */}
              <div className="w-[124px] h-[124px] rounded-full bg-[#121620] flex items-center justify-center overflow-hidden z-10 relative border-[3px] border-[#121620]">
                {profilePictureUrl ? (
                  <img src={profilePictureUrl} alt={user?.fullName || "Profile"} className="w-full h-full object-cover" />
                ) : user?.fullName ? (
                  <span className="text-gold font-bold text-5xl">{user.fullName.charAt(0).toUpperCase()}</span>
                ) : (
                  <LuUser className="w-12 h-12 text-gold" />
                )}
              </div>
            </div>

            {/* Account Completion Text */}
            {(user?.role === 'Lawyer' || user?.role === 'Client') && (
              <p className={`text-[11px] text-gold font-bold tracking-widest mt-1 mb-2 transition-opacity duration-700 ease-in-out ${showCompletionText ? 'opacity-100' : 'opacity-0'}`}>
                نسبة اكتمال الحساب {targetProgress}%
              </p>
            )}

            <div className="flex flex-col items-center justify-center mt-1 w-full px-2">
              <div className="flex items-center gap-2 mb-1">
                <h2 className="text-sm font-bold text-white tracking-wide">
                  {user?.fullName || "مستخدم"}
                </h2>
                <UserStatusBadge status={user?.status} role={user?.role} />
              </div>
              <p className="text-xs text-gold font-bold tracking-wide">
                {getRoleLabel(user?.role)}
              </p>
            </div>
          </div>

          {/* Main Navigation List */}
          <nav className="space-y-1 pt-2">
            <button
              onClick={() => handleTabChange("overview")}
              className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold transition-all cursor-pointer ${activeTab === "overview"
                ? "bg-gold/15 text-gold border-r-4 border-gold"
                : "text-gray-400 hover:text-white hover:bg-white/5"
                }`}
            >
              <LuLayoutDashboard className="w-5 h-5" />
              <span>الرئيسية</span>
            </button>

            <button
              onClick={() => handleTabChange("new-case")}
              className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold transition-all cursor-pointer ${activeTab === "new-case"
                ? "bg-gold/15 text-gold border-r-4 border-gold"
                : "text-gray-400 hover:text-white hover:bg-white/5"
                }`}
            >
              <LuFilePlus className="w-5 h-5" />
              <span>رفع قضية جديدة</span>
            </button>

            <button
              onClick={() => handleTabChange("cases")}
              className={`w-full flex items-center justify-between px-4 py-3 rounded-xl text-sm font-bold transition-all cursor-pointer ${activeTab === "cases"
                ? "bg-gold/20 text-gold border-r-4 border-gold shadow-sm"
                : "text-gray-400 hover:text-white hover:bg-white/5"
                }`}
            >
              <div className="flex items-center gap-3">
                <LuFolder className="w-5 h-5" />
                <span>قضاياي</span>
              </div>
              <span className="w-2 h-2 rounded-full bg-gold"></span>
            </button>

            <button
              onClick={() => navigate("/lawyers")}
              className="w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold text-gray-400 hover:text-white hover:bg-white/5 transition-all cursor-pointer"
            >
              <LuUsers className="w-5 h-5" />
              <span>البحث عن محامين</span>
            </button>

            {user?.role !== 'Admin' && (
              <button
                onClick={() => handleTabChange("verification")}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold transition-all cursor-pointer ${activeTab === "verification"
                  ? "bg-gold/15 text-gold border-r-4 border-gold"
                  : "text-gray-400 hover:text-white hover:bg-white/5"
                  }`}
              >
                <LuShieldCheck className={`w-5 h-5 ${(user?.status === 'Unverified' || user?.status === 'Rejected') ? 'text-red-500' : user?.status === 'PendingReview' ? 'text-amber-500' : 'text-green-500'}`} />
                <span>التوثيق</span>
                {(user?.status === 'Unverified' || user?.status === 'Rejected') && <div className="mr-auto w-2 h-2 rounded-full bg-red-500 animate-pulse"></div>}
              </button>
            )}

            {/* Admin Only Tabs */}
            {user?.role === 'Admin' && (
              <button
                onClick={() => handleTabChange("admin-verifications")}
                className={`w-full flex items-center justify-between px-4 py-3 rounded-xl text-sm font-bold transition-all cursor-pointer ${activeTab === "admin-verifications"
                  ? "bg-gold/15 text-gold border-r-4 border-gold"
                  : "text-gray-400 hover:text-white hover:bg-white/5"
                  }`}
              >
                <div className="flex items-center gap-3">
                  <LuShieldCheck className="w-5 h-5 text-amber-500" />
                  <span>إدارة التوثيقات</span>
                </div>
              </button>
            )}

            <button
              onClick={() => handleTabChange("chats")}
              className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold transition-all cursor-pointer ${activeTab === "chats"
                ? "bg-gold/15 text-gold border-r-4 border-gold"
                : "text-gray-400 hover:text-white hover:bg-white/5"
                }`}
            >
              <LuMessageSquare className="w-5 h-5" />
              <span>المحادثات</span>
            </button>

            <button
              onClick={() => handleTabChange("settings")}
              className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl text-sm font-bold transition-all cursor-pointer ${activeTab === "settings"
                ? "bg-gold/15 text-gold border-r-4 border-gold"
                : "text-gray-400 hover:text-white hover:bg-white/5"
                }`}
            >
              <LuSettings className="w-5 h-5" />
              <span>الإعدادات والبروفايل</span>
            </button>
          </nav>
        </div>

        {/* Bottom Sidebar Footer */}
        <div className="pt-6 border-t border-gray-800/80 space-y-2">
          <button className="w-full flex items-center gap-3 px-4 py-2.5 rounded-xl text-sm font-medium text-gray-400 hover:text-white transition-all cursor-pointer">
            <LuCircleHelp className="w-5 h-5" />
            <span>مركز المساعدة</span>
          </button>

          <button
            onClick={handleLogout}
            className="w-full flex items-center gap-3 px-4 py-2.5 rounded-xl text-sm font-bold text-red-500 hover:bg-red-500/10 transition-all cursor-pointer"
          >
            <LuLogOut className="w-5 h-5" />
            <span>تسجيل الخروج</span>
          </button>
        </div>
      </aside>

      {/* MAIN CONTENT AREA */}
      <main className="flex-1 p-4 sm:p-8 overflow-y-auto">



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

        {(activeTab === "new-case" || activeTab === "chats") && (
          <div className="min-h-[50vh] flex flex-col items-center justify-center bg-white dark:bg-[#1a1d23] rounded-3xl p-12 text-center border border-gray-200 dark:border-gray-800">
            <LuFileText className="w-16 h-16 text-gold mb-4 animate-pulse" />
            <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
              صفحة {activeTab === "new-case" ? "رفع قضية جديدة" : "المحادثات"}
            </h3>
            <p className="text-xs text-gray-500 dark:text-gray-400 max-w-sm">
              جاري تجهيز هذه الصفحة بالتفصيل خطوة بخطوة حسب خطة التطوير والملاحظات...
            </p>
          </div>
        )}

      </main>

    </div>
  );
};
