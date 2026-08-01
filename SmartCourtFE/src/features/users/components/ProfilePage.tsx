import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { useAuthStore } from "../../auth/store/useAuthStore";
import { UsersApi } from "../api/usersApi";
import type { ClientProfile, LawyerProfile } from "../api/usersApi";
import { AuthApi } from "../../auth/api/authApi";
import {
  LuUser,
  LuMail,
  LuPhone,
  LuMapPin,
  LuCalendar,
  LuBriefcase,
  LuAward,
  LuPencil,
  LuKey,
  LuTrash2,
  LuShieldCheck,
  LuLoader,
  LuX,
  LuTriangleAlert
} from "react-icons/lu";

const parseApiError = (err: any, defaultMsg: string) => {
  const apiErr = err?.response?.data;
  if (apiErr) {
    if (apiErr.message) return apiErr.message;
    if (apiErr.errors) {
      if (Array.isArray(apiErr.errors)) return apiErr.errors.join(" | ");
      if (typeof apiErr.errors === 'object') {
        return Object.entries(apiErr.errors)
          .map(([_, msgs]) => (Array.isArray(msgs) ? msgs.join(", ") : String(msgs)))
          .join(" | ");
      }
      return JSON.stringify(apiErr.errors);
    }
  }
  return defaultMsg;
};

export const ProfilePage = () => {
  const { user, logout } = useAuthStore();
  const queryClient = useQueryClient();
  const navigate = useNavigate();

  const [activeTab, setActiveTab] = useState<"info" | "security">("info");
  const [isEditModalOpen, setIsEditModalOpen] = useState(false);
  const [isPasswordModalOpen, setIsPasswordModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);

  // Form states for edit profile
  const [phoneNumber, setPhoneNumber] = useState("");
  const [dateOfBirth, setDateOfBirth] = useState("");
  const [address, setAddress] = useState("");
  const [bio, setBio] = useState("");
  const [yearsOfExperience, setYearsOfExperience] = useState(1);
  const [level, setLevel] = useState(1);
  const [specializationId, setSpecializationId] = useState("");

  // Password change state
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmNewPassword, setConfirmNewPassword] = useState("");

  // Delete account state
  const [deletePassword, setDeletePassword] = useState("");

  // Status & Feedback messages
  const [statusMsg, setStatusMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  const isLawyer = user?.role === "Lawyer";

  // Redirect to login if user is lost (e.g. token expired or manual URL navigation without auth)
  useEffect(() => {
    if (!user) {
      navigate("/login");
    }
  }, [user, navigate]);

  // Fetch Profile Data
  const { data: profile, isLoading } = useQuery({
    queryKey: ["userProfile", user?.id],
    queryFn: async () => {
      if (isLawyer) {
        return await UsersApi.getLawyerProfile();
      } else {
        return await UsersApi.getClientProfile();
      }
    },
    enabled: !!user,
  });

  // Populate edit form when profile is loaded or edit modal opens
  const handleOpenEditModal = () => {
    if (profile) {
      setPhoneNumber(profile.phoneNumber || "");
      setDateOfBirth(profile.dateOfBirth ? profile.dateOfBirth.split("T")[0] : "");
      setAddress(profile.address || "");
      if (isLawyer) {
        const lawyerProf = profile as LawyerProfile;
        setBio(lawyerProf.bio || "");
        setYearsOfExperience(lawyerProf.yearsOfExperience || 1);
        setLevel(lawyerProf.level || 1);
        setSpecializationId(lawyerProf.specializationId || "");
      }
    }
    setIsEditModalOpen(true);
  };

  // Update Profile Mutation
  const updateProfileMutation = useMutation({
    mutationFn: async () => {
      if (isLawyer) {
        return await UsersApi.updateLawyerProfile({
          phoneNumber,
          dateOfBirth,
          address,
          bio,
          yearsOfExperience: Number(yearsOfExperience),
          level: Number(level),
          specializationId: specializationId || "a6b4f7cb-2f0f-4f32-bf5f-08f2a6b3c701"
        });
      } else {
        return await UsersApi.updateClientProfile({
          phoneNumber,
          dateOfBirth,
          address
        });
      }
    },
    onSuccess: (res) => {
      queryClient.invalidateQueries({ queryKey: ["userProfile"] });
      setIsEditModalOpen(false);
      setStatusMsg({ type: "success", text: res?.message || "تم تحديث الملف الشخصي بنجاح" });
    },
    onError: (err: any) => {
      setStatusMsg({ type: "error", text: parseApiError(err, "فشل تحديث البيانات") });
    }
  });

  // Change Password Mutation
  const changePasswordMutation = useMutation({
    mutationFn: AuthApi.changePassword,
    onSuccess: (res) => {
      setIsPasswordModalOpen(false);
      setCurrentPassword("");
      setNewPassword("");
      setConfirmNewPassword("");
      setStatusMsg({ type: "success", text: res?.message || "تم تغيير كلمة المرور بنجاح" });
    },
    onError: (err: any) => {
      setStatusMsg({ type: "error", text: parseApiError(err, "فشل تغيير كلمة المرور") });
    }
  });

  // Delete Account Mutation
  const deleteAccountMutation = useMutation({
    mutationFn: async () => {
      if (isLawyer) {
        return await UsersApi.deleteLawyerProfile({ currentPassword: deletePassword });
      } else {
        return await UsersApi.deleteClientProfile({ currentPassword: deletePassword });
      }
    },
    onSuccess: () => {
      logout();
      window.location.href = "/login";
    },
    onError: (err: any) => {
      setStatusMsg({ type: "error", text: parseApiError(err, "فشل حذف الحساب") });
    }
  });

  const getLawyerLevelTitle = (lvl?: number) => {
    switch (lvl) {
      case 1:
        return "جدول عام (محامي تحت التمرين)";
      case 2:
        return "محاكم ابتدائية";
      case 3:
        return "محاكم استئناف";
      case 4:
        return "محكمة النقض";
      default:
        return "محامي ممارس";
    }
  };

  if (isLoading) {
    return (
      <div className="min-h-[70vh] flex items-center justify-center bg-surface dark:bg-navy">
        <div className="flex flex-col items-center gap-3">
          <LuLoader className="w-10 h-10 text-gold animate-spin" />
          <p className="text-sm font-bold text-text-secondary">جاري تحميل الملف الشخصي...</p>
        </div>
      </div>
    );
  }

  const clientProf = profile as ClientProfile;
  const lawyerProf = profile as LawyerProfile;

  return (
    <div className="min-h-screen bg-surface dark:bg-navy py-10 px-4 sm:px-8 transition-colors duration-300">
      <div className="max-w-5xl mx-auto space-y-8">

        {/* Global Toast Message */}
        {statusMsg && (
          <div
            className={`p-4 rounded-xl flex items-center justify-between border ${
              statusMsg.type === "success"
                ? "bg-green-50 dark:bg-green-950/40 border-green-200 dark:border-green-800 text-green-700 dark:text-green-300"
                : "bg-red-50 dark:bg-red-950/40 border-red-200 dark:border-red-800 text-red-700 dark:text-red-300"
            }`}
          >
            <span className="text-sm font-bold">{statusMsg.text}</span>
            <button onClick={() => setStatusMsg(null)} className="cursor-pointer">
              <LuX className="w-5 h-5" />
            </button>
          </div>
        )}

        {/* Cover Header Banner Card */}
        <div className="relative bg-white dark:bg-[#1a1d23] rounded-3xl overflow-hidden border border-border-primary shadow-premium">
          <div className="h-36 bg-gradient-to-r from-navy via-navy-light to-gold/30 relative">
            <div className="absolute inset-0 bg-[radial-gradient(#d4af37_1px,transparent_1px)] [background-size:16px_16px] opacity-20"></div>
          </div>

          <div className="px-8 pb-8 pt-0 relative flex flex-col sm:flex-row items-start sm:items-end justify-between gap-6 -mt-16">
            <div className="flex flex-col sm:flex-row items-center sm:items-end gap-5 text-center sm:text-right">
              {/* Avatar Initial */}
              <div className="w-28 h-28 rounded-2xl bg-gold text-white font-bold text-4xl flex items-center justify-center border-4 border-white dark:border-[#1a1d23] shadow-lg">
                {user?.fullName ? user.fullName.charAt(0).toUpperCase() : <LuUser />}
              </div>

              <div className="space-y-1">
                <div className="flex items-center justify-center sm:justify-start gap-3">
                  <h1 className="text-2xl font-bold text-text-primary">
                    {profile?.name || user?.fullName}
                  </h1>
                  <span className="px-3 py-1 text-xs font-bold rounded-full bg-gold/20 text-gold border border-gold/30">
                    {isLawyer ? "محامي" : "موكل"}
                  </span>
                </div>
                <p className="text-sm text-text-secondary dir-ltr text-right">{profile?.email || user?.email}</p>
              </div>
            </div>

            {/* Quick Actions */}
            <button
              onClick={handleOpenEditModal}
              className="w-full sm:w-auto h-11 px-6 bg-gold hover:bg-gold-hover text-white font-semibold text-sm rounded-xl shadow-xs transition-all flex items-center justify-center gap-2 cursor-pointer"
            >
              <LuPencil className="w-4.5 h-4.5" />
              <span>تعديل الملف الشخصي</span>
            </button>
          </div>

          {/* Navigation Tabs */}
          <div className="flex border-t border-border-primary px-8">
            <button
              onClick={() => setActiveTab("info")}
              className={`py-4 px-6 font-bold text-sm border-b-2 transition-colors cursor-pointer flex items-center gap-2 ${
                activeTab === "info"
                  ? "border-gold text-gold"
                  : "border-transparent text-text-secondary hover:text-text-primary"
              }`}
            >
              <LuUser className="w-4.5 h-4.5" />
              <span>البيانات الشخصية</span>
            </button>

            <button
              onClick={() => setActiveTab("security")}
              className={`py-4 px-6 font-bold text-sm border-b-2 transition-colors cursor-pointer flex items-center gap-2 ${
                activeTab === "security"
                  ? "border-gold text-gold"
                  : "border-transparent text-text-secondary hover:text-text-primary"
              }`}
            >
              <LuShieldCheck className="w-4.5 h-4.5" />
              <span>الأمان والحساب</span>
            </button>
          </div>
        </div>

        {/* Tab Content: Personal Info */}
        {activeTab === "info" && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

            {/* Basic Info Card */}
            <div className="bg-white dark:bg-[#1a1d23] rounded-2xl p-6 border border-border-primary shadow-xs space-y-6">
              <h3 className="text-lg font-bold text-text-primary flex items-center gap-2.5 pb-3 border-b border-border-primary">
                <LuUser className="text-gold w-5 h-5" />
                <span>المعلومات الأساسية</span>
              </h3>

              <div className="space-y-4">
                <div className="flex items-center gap-4">
                  <div className="w-10 h-10 rounded-xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center text-text-secondary">
                    <LuMail className="w-5 h-5" />
                  </div>
                  <div>
                    <p className="text-xs text-text-secondary">البريد الإلكتروني</p>
                    <p className="text-sm font-semibold text-text-primary">{profile?.email || user?.email}</p>
                  </div>
                </div>

                <div className="flex items-center gap-4">
                  <div className="w-10 h-10 rounded-xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center text-text-secondary">
                    <LuPhone className="w-5 h-5" />
                  </div>
                  <div>
                    <p className="text-xs text-text-secondary">رقم الهاتف</p>
                    <p className="text-sm font-semibold text-text-primary dir-ltr text-right">
                      {profile?.phoneNumber || "غير محدد"}
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-4">
                  <div className="w-10 h-10 rounded-xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center text-text-secondary">
                    <LuCalendar className="w-5 h-5" />
                  </div>
                  <div>
                    <p className="text-xs text-text-secondary">تاريخ الميلاد</p>
                    <p className="text-sm font-semibold text-text-primary">
                      {profile?.dateOfBirth ? profile.dateOfBirth.split("T")[0] : "غير محدد"}
                    </p>
                  </div>
                </div>

                <div className="flex items-center gap-4">
                  <div className="w-10 h-10 rounded-xl bg-gray-100 dark:bg-gray-800 flex items-center justify-center text-text-secondary">
                    <LuMapPin className="w-5 h-5" />
                  </div>
                  <div>
                    <p className="text-xs text-text-secondary">العنوان / المحافظة</p>
                    <p className="text-sm font-semibold text-text-primary">
                      {profile?.address || "غير محدد"}
                    </p>
                  </div>
                </div>
              </div>
            </div>

            {/* Role Specific Details Card */}
            <div className="bg-white dark:bg-[#1a1d23] rounded-2xl p-6 border border-border-primary shadow-xs space-y-6">
              <h3 className="text-lg font-bold text-text-primary flex items-center gap-2.5 pb-3 border-b border-border-primary">
                <LuBriefcase className="text-gold w-5 h-5" />
                <span>{isLawyer ? "البيانات المهنية والتنقيب" : "حالة الحساب والمستندات"}</span>
              </h3>

              {isLawyer ? (
                <div className="space-y-4">
                  <div className="flex items-center gap-4">
                    <div className="w-10 h-10 rounded-xl bg-gold/10 flex items-center justify-center text-gold">
                      <LuAward className="w-5 h-5" />
                    </div>
                    <div>
                      <p className="text-xs text-text-secondary">التخصص الرئيسي</p>
                      <p className="text-sm font-semibold text-text-primary">
                        {lawyerProf?.specializationName || "محاماة عامة"}
                      </p>
                    </div>
                  </div>

                  <div className="flex items-center gap-4">
                    <div className="w-10 h-10 rounded-xl bg-gold/10 flex items-center justify-center text-gold">
                      <LuBriefcase className="w-5 h-5" />
                    </div>
                    <div>
                      <p className="text-xs text-text-secondary">سنوات الخبرة</p>
                      <p className="text-sm font-semibold text-text-primary">
                        {lawyerProf?.yearsOfExperience || 1} سنوات
                      </p>
                    </div>
                  </div>

                  <div className="flex items-center gap-4">
                    <div className="w-10 h-10 rounded-xl bg-gold/10 flex items-center justify-center text-gold">
                      <LuShieldCheck className="w-5 h-5" />
                    </div>
                    <div>
                      <p className="text-xs text-text-secondary">درجة التقاضي المقيد بها</p>
                      <p className="text-sm font-semibold text-text-primary">
                        {getLawyerLevelTitle(lawyerProf?.level)}
                      </p>
                    </div>
                  </div>

                  {lawyerProf?.bio && (
                    <div className="pt-2 border-t border-border-primary">
                      <p className="text-xs text-text-secondary mb-1">نبذة عن المحامي</p>
                      <p className="text-sm text-text-primary leading-relaxed bg-surface dark:bg-navy p-3 rounded-xl">
                        {lawyerProf.bio}
                      </p>
                    </div>
                  )}
                </div>
              ) : (
                <div className="space-y-4">
                  <div className="p-4 rounded-xl bg-surface dark:bg-navy border border-border-primary">
                    <div className="flex items-center justify-between">
                      <span className="text-xs text-text-secondary">حالة الحساب:</span>
                      <span className="px-3 py-1 rounded-full text-xs font-bold bg-green-500/20 text-green-500">
                        {clientProf?.status || "نشط (Active)"}
                      </span>
                    </div>
                  </div>
                  <p className="text-xs text-text-secondary leading-relaxed">
                    حسابك كموكل يتيح لك استعراض كافة المحامين المعتمَدين، ورفع القضايا والاستشارات القانونية مباشرة بكل أمان.
                  </p>
                </div>
              )}
            </div>

          </div>
        )}

        {/* Tab Content: Security Settings */}
        {activeTab === "security" && (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">

            {/* Change Password Card */}
            <div className="bg-white dark:bg-[#1a1d23] rounded-2xl p-6 border border-border-primary shadow-xs space-y-4">
              <div className="flex items-center justify-between pb-3 border-b border-border-primary">
                <h3 className="text-lg font-bold text-text-primary flex items-center gap-2.5">
                  <LuKey className="text-gold w-5 h-5" />
                  <span>تغيير كلمة المرور</span>
                </h3>
              </div>
              <p className="text-xs text-text-secondary leading-relaxed">
                ينصح بتغيير كلمة المرور بشكل دوري للحفاظ على أمان حسابك وقضاياك.
              </p>

              <button
                onClick={() => setIsPasswordModalOpen(true)}
                className="w-full h-11 bg-gold hover:bg-gold-hover text-white font-bold text-sm rounded-xl transition-all flex items-center justify-center gap-2 cursor-pointer mt-4"
              >
                <LuKey className="w-4.5 h-4.5" />
                <span>تغيير كلمة المرور الآن</span>
              </button>
            </div>

            {/* Danger Zone: Delete Account */}
            <div className="bg-white dark:bg-[#1a1d23] rounded-2xl p-6 border border-red-200 dark:border-red-950 shadow-xs space-y-4">
              <div className="flex items-center justify-between pb-3 border-b border-red-200 dark:border-red-950">
                <h3 className="text-lg font-bold text-red-600 dark:text-red-400 flex items-center gap-2.5">
                  <LuTriangleAlert className="w-5 h-5" />
                  <span>منطقة الخطر (حذف الحساب)</span>
                </h3>
              </div>
              <p className="text-xs text-red-500/80 leading-relaxed">
                عند حذف حسابك، سيتم تعطيل وصولك لكافة القضايا والاستشارات نهائياً وإبطال أجهزة الجلسات المفتوحة.
              </p>

              <button
                onClick={() => setIsDeleteModalOpen(true)}
                className="w-full h-11 bg-red-600 hover:bg-red-700 text-white font-bold text-sm rounded-xl transition-all flex items-center justify-center gap-2 cursor-pointer mt-4"
              >
                <LuTrash2 className="w-4.5 h-4.5" />
                <span>حذف الحساب نهائياً</span>
              </button>
            </div>

          </div>
        )}

      </div>

      {/* EDIT PROFILE MODAL */}
      {isEditModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in">
          <div className="bg-white dark:bg-[#1a1d23]/95 backdrop-blur-xl w-full max-w-xl rounded-2xl p-8 shadow-premium dark:shadow-2xl border-t-4 border-gold space-y-6 relative overflow-hidden">
            <div className="flex items-center justify-between border-b border-gray-100 dark:border-gray-800 pb-4">
              <h3 className="text-xl font-bold text-navy dark:text-gold flex items-center gap-2">
                <LuPencil className="w-6 h-6" />
                <span>تحديث بيانات الملف الشخصي</span>
              </h3>
              <button onClick={() => setIsEditModalOpen(false)} className="text-gray-400 hover:text-red-500 transition-colors cursor-pointer">
                <LuX className="w-6 h-6" />
              </button>
            </div>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                updateProfileMutation.mutate();
              }}
              className="space-y-4"
            >
              <div>
                <label className="block text-xs font-bold text-text-primary mb-1">رقم الهاتف</label>
                <input
                  type="tel"
                  value={phoneNumber}
                  onChange={(e) => setPhoneNumber(e.target.value)}
                  placeholder="010xxxxxxxx"
                  className="w-full p-3 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-gold"
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-text-primary mb-1">تاريخ الميلاد</label>
                <input
                  type="date"
                  value={dateOfBirth}
                  onChange={(e) => setDateOfBirth(e.target.value)}
                  className="w-full p-3 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-gold"
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-text-primary mb-1">العنوان / المحافظة</label>
                <input
                  type="text"
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  placeholder="القاهرة، مصر"
                  className="w-full p-3 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-gold"
                />
              </div>

              {isLawyer && (
                <>
                  <div>
                    <label className="block text-xs font-bold text-text-primary mb-1">سنوات الخبرة</label>
                    <input
                      type="number"
                      min={1}
                      max={60}
                      value={yearsOfExperience}
                      onChange={(e) => setYearsOfExperience(Number(e.target.value))}
                      className="w-full p-3 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-gold"
                    />
                  </div>

                  <div>
                    <label className="block text-xs font-bold text-text-primary mb-1">درجة التقاضي</label>
                    <select
                      value={level}
                      onChange={(e) => setLevel(Number(e.target.value))}
                      className="w-full p-3 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-gold"
                    >
                      <option value={1}>جدول عام (محامي تحت التمرين)</option>
                      <option value={2}>محاكم ابتدائية</option>
                      <option value={3}>محاكم استئناف</option>
                      <option value={4}>محكمة النقض</option>
                    </select>
                  </div>

                  <div>
                    <label className="block text-xs font-bold text-text-primary mb-1">نبذة عنك (Bio)</label>
                    <textarea
                      rows={3}
                      value={bio}
                      onChange={(e) => setBio(e.target.value)}
                      placeholder="اكتب نبذة مختصرة عن خبراتك وقضاياك..."
                      className="w-full p-3 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-gold"
                    />
                  </div>
                </>
              )}

              <div className="flex items-center justify-end gap-3 pt-6 border-t border-gray-100 dark:border-gray-800 mt-4">
                <button
                  type="button"
                  onClick={() => setIsEditModalOpen(false)}
                  className="px-6 py-3 rounded-xl bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 text-sm font-bold text-navy dark:text-gray-200 transition-colors cursor-pointer"
                >
                  إلغاء
                </button>
                <button
                  type="submit"
                  disabled={updateProfileMutation.isPending}
                  className="px-6 py-2.5 rounded-xl bg-gold hover:bg-gold-hover text-white text-sm font-bold flex items-center gap-2 cursor-pointer shadow-xs"
                >
                  {updateProfileMutation.isPending && <LuLoader className="w-4 h-4 animate-spin" />}
                  <span>حفظ التغييرات</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* CHANGE PASSWORD MODAL */}
      {isPasswordModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in">
          <div className="bg-white dark:bg-[#1a1d23]/95 backdrop-blur-xl w-full max-w-md rounded-2xl p-8 shadow-premium dark:shadow-2xl border-t-4 border-gold space-y-6 relative overflow-hidden">
            <div className="flex items-center justify-between border-b border-gray-100 dark:border-gray-800 pb-4">
              <h3 className="text-xl font-bold text-navy dark:text-gold flex items-center gap-2">
                <LuKey className="w-6 h-6" />
                <span>تغيير كلمة المرور</span>
              </h3>
              <button onClick={() => setIsPasswordModalOpen(false)} className="text-gray-400 hover:text-red-500 transition-colors cursor-pointer">
                <LuX className="w-6 h-6" />
              </button>
            </div>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                if (newPassword !== confirmNewPassword) {
                  setStatusMsg({ type: "error", text: "كلمة المرور وتأكيد كلمة المرور غير متطابقتين" });
                  return;
                }
                changePasswordMutation.mutate({ currentPassword, newPassword, confirmNewPassword });
              }}
              className="space-y-4"
            >
              <div>
                <label className="block text-xs font-bold text-text-primary mb-1">كلمة المرور الحالية</label>
                <input
                  type="password"
                  required
                  value={currentPassword}
                  onChange={(e) => setCurrentPassword(e.target.value)}
                  className="w-full p-3 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-gold"
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-text-primary mb-1">كلمة المرور الجديدة</label>
                <input
                  type="password"
                  required
                  value={newPassword}
                  onChange={(e) => setNewPassword(e.target.value)}
                  className="w-full p-3 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-gold"
                />
              </div>

              <div>
                <label className="block text-xs font-bold text-text-primary mb-1">تأكيد كلمة المرور الجديدة</label>
                <input
                  type="password"
                  required
                  value={confirmNewPassword}
                  onChange={(e) => setConfirmNewPassword(e.target.value)}
                  className="w-full p-3 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-gold"
                />
              </div>

              <div className="flex items-center justify-end gap-3 pt-6 border-t border-gray-100 dark:border-gray-800 mt-4">
                <button
                  type="button"
                  onClick={() => setIsPasswordModalOpen(false)}
                  className="px-6 py-3 rounded-xl bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 text-sm font-bold text-navy dark:text-gray-200 transition-colors cursor-pointer"
                >
                  إلغاء
                </button>
                <button
                  type="submit"
                  disabled={changePasswordMutation.isPending}
                  className="px-6 py-2.5 rounded-xl bg-gold hover:bg-gold-hover text-white text-sm font-bold flex items-center gap-2 cursor-pointer shadow-xs"
                >
                  {changePasswordMutation.isPending && <LuLoader className="w-4 h-4 animate-spin" />}
                  <span>تأكيد التغيير</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* DELETE ACCOUNT MODAL */}
      {isDeleteModalOpen && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-fade-in">
          <div className="bg-white dark:bg-[#1a1d23]/95 backdrop-blur-xl w-full max-w-md rounded-2xl p-8 shadow-premium dark:shadow-2xl border-t-4 border-red-500 space-y-6 relative overflow-hidden">
            <div className="flex items-center justify-between border-b border-red-100 dark:border-red-900/50 pb-4">
              <h3 className="text-xl font-bold text-red-600 flex items-center gap-2">
                <LuTriangleAlert className="w-6 h-6" />
                <span>تأكيد حذف الحساب</span>
              </h3>
              <button onClick={() => setIsDeleteModalOpen(false)} className="text-gray-400 hover:text-red-500 transition-colors cursor-pointer">
                <LuX className="w-6 h-6" />
              </button>
            </div>

            <p className="text-xs text-text-secondary leading-relaxed">
              يرجى إدخال كلمة المرور الحالية لتأكيد حذف حسابك بشكل نهائي:
            </p>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                deleteAccountMutation.mutate();
              }}
              className="space-y-4"
            >
              <div>
                <input
                  type="password"
                  required
                  placeholder="أدخل كلمة المرور الحالية"
                  value={deletePassword}
                  onChange={(e) => setDeletePassword(e.target.value)}
                  className="w-full p-3 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-red-500"
                />
              </div>

              <div className="flex items-center justify-end gap-3 pt-6 border-t border-red-100 dark:border-red-900/50 mt-4">
                <button
                  type="button"
                  onClick={() => setIsDeleteModalOpen(false)}
                  className="px-6 py-3 rounded-xl bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 text-sm font-bold text-navy dark:text-gray-200 transition-colors cursor-pointer"
                >
                  إلغاء
                </button>
                <button
                  type="submit"
                  disabled={deleteAccountMutation.isPending}
                  className="px-6 py-2.5 rounded-xl bg-red-600 hover:bg-red-700 text-white text-sm font-bold flex items-center gap-2 cursor-pointer shadow-xs"
                >
                  {deleteAccountMutation.isPending && <LuLoader className="w-4 h-4 animate-spin" />}
                  <span>حذف الحساب نهائياً</span>
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

    </div>
  );
};
