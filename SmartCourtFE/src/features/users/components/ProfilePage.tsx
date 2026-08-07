import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery, useMutation } from "@tanstack/react-query";
import { useAuthStore } from "../../auth/store/useAuthStore";
import { UsersApi } from "../api/usersApi";
import { AuthApi } from "../../auth/api/authApi";
import toast from "react-hot-toast";
import { motion, AnimatePresence } from "framer-motion";
import {
  LuUser,
  LuKey,
  LuTrash2,
  LuLoader,
  LuX,
  LuTriangleAlert,
  LuEye,
  LuEyeOff,
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
  const { user } = useAuthStore();
  const logout = useAuthStore((state) => state.logout);
  const navigate = useNavigate();

  const [isPasswordModalOpen, setIsPasswordModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmNewPassword, setConfirmNewPassword] = useState("");
  
  const [showCurrentPassword, setShowCurrentPassword] = useState(false);
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmNewPassword, setShowConfirmNewPassword] = useState(false);

  // Delete account state
  const [deletePassword, setDeletePassword] = useState("");

  // Redirect to login if user is lost (e.g. token expired or manual URL navigation without auth)
  useEffect(() => {
    if (!user) {
      navigate("/login");
    }
  }, [user, navigate]);

  // Fetch Profile Data
  const { data: profile, isLoading } = useQuery({
    queryKey: ["user", "profile", user?.id],
    queryFn: async () => {
      if (user?.role === "Lawyer") {
        return await UsersApi.getLawyerProfile();
      } else if (user?.role === "Client") {
        return await UsersApi.getClientProfile();
      }
      return null;
    },
    enabled: !!user?.id && user?.role !== "Admin",
  });

  const { data: documents } = useQuery({
    queryKey: ["user", "verifications", "documents", user?.id],
    queryFn: () => AuthApi.getUserVerificationDocuments(user!.id),
    enabled: !!user?.id && user?.role === 'Lawyer',
  });

  const profilePictureDoc = documents?.data?.documents?.find((d: any) => 
    (d.documentType === 'OfficialProfilePicture' || d.documentType === 3) && d.isCurrent
  );
  const isPictureApproved = profilePictureDoc?.status === 'Verified' || profilePictureDoc?.status === 2;
  
  const { data: profilePicContent } = useQuery({
    queryKey: ["documentContent", profilePictureDoc?.documentId],
    queryFn: () => AuthApi.getDocumentContent(profilePictureDoc!.documentId),
    enabled: !!profilePictureDoc?.documentId && isPictureApproved,
  });

  const profilePictureUrl = isPictureApproved ? (profilePicContent?.data?.downloadUrl || null) : null;



  // Change Password Mutation
  const changePasswordMutation = useMutation({
    mutationFn: AuthApi.changePassword,
    onSuccess: (res) => {
      setIsPasswordModalOpen(false);
      setCurrentPassword("");
      setNewPassword("");
      setConfirmNewPassword("");
      toast.success(res?.message || "تم تغيير كلمة المرور بنجاح");
    },
    onError: (err: any) => {
      toast.error(parseApiError(err, "فشل تغيير كلمة المرور"));
    }
  });

  // Delete Account Mutation
  const deleteAccountMutation = useMutation({
    mutationFn: async () => {
      if (user?.role === "Lawyer") {
        return await UsersApi.deleteLawyerProfile({ currentPassword: deletePassword });
      } else if (user?.role === "Client") {
        return await UsersApi.deleteClientProfile({ currentPassword: deletePassword });
      }
      throw new Error("لا يمكن حذف حساب الإدارة من هنا");
    },
    onSuccess: () => {
      logout();
      window.location.href = "/login";
    },
    onError: (err: any) => {
      toast.error(parseApiError(err, "فشل حذف الحساب"));
    }
  });

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

  return (
    <div className="min-h-screen bg-surface dark:bg-navy py-10 px-4 sm:px-8 transition-colors duration-300">
      <div className="max-w-5xl mx-auto space-y-8">



        {/* Cover Header Banner Card */}
        <div className="relative bg-white dark:bg-[#1a1d23] rounded-3xl overflow-hidden border border-border-primary shadow-premium">
          <div className="h-36 bg-gradient-to-r from-navy via-navy-light to-gold/30 relative">
            <div className="absolute inset-0 bg-[radial-gradient(#d4af37_1px,transparent_1px)] [background-size:16px_16px] opacity-20"></div>
          </div>

          <div className="px-8 pb-8 pt-0 relative flex flex-col sm:flex-row items-start sm:items-end justify-between gap-6 -mt-16">
            <div className="flex flex-col sm:flex-row items-center sm:items-end gap-5 text-center sm:text-right">
              {/* Avatar Initial */}
              <div className="w-28 h-28 rounded-2xl bg-gold text-white font-bold text-4xl flex items-center justify-center border-4 border-white dark:border-[#1a1d23] shadow-lg overflow-hidden">
                {profilePictureUrl ? (
                  <img src={profilePictureUrl} alt={user?.fullName || "Profile"} className="w-full h-full object-cover" />
                ) : user?.fullName ? (
                  user.fullName.charAt(0).toUpperCase()
                ) : (
                  <LuUser />
                )}
              </div>

              <div className="space-y-1">
                <div className="flex items-center justify-center sm:justify-start gap-3">
                  <h1 className="text-2xl font-bold text-text-primary">
                    {profile?.name || user?.fullName}
                  </h1>
                  <span className="px-3 py-1 text-xs font-bold rounded-full bg-gold/20 text-gold border border-gold/30">
                    {user?.role === 'Admin' ? 'مدير' : user?.role === 'Lawyer' ? 'محامي' : 'موكل'}
                  </span>
                </div>
                <p className="text-sm text-text-secondary dir-ltr text-right">{profile?.email || user?.email}</p>
              </div>
            </div>
          </div>
        </div>

        {/* Main Content Area: Security & Account */}
        <div className="w-full">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6 items-start animate-fade-in">

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

                <AnimatePresence mode="wait">
                  {isPasswordModalOpen ? (
                    <motion.form
                      key="form"
                      initial={{ height: 0, opacity: 0 }}
                      animate={{ height: "auto", opacity: 1 }}
                      exit={{ height: 0, opacity: 0 }}
                      transition={{ duration: 0.3 }}
                      onSubmit={(e) => {
                        e.preventDefault();
                        if (newPassword !== confirmNewPassword) {
                          toast.error("كلمة المرور وتأكيد كلمة المرور غير متطابقتين");
                          return;
                        }
                        changePasswordMutation.mutate({ currentPassword, newPassword, confirmNewPassword });
                      }}
                      className="space-y-4 pt-2 overflow-hidden"
                    >
                      <div>
                        <label className="block text-xs font-bold text-text-primary mb-1">كلمة المرور الحالية</label>
                        <div className="relative">
                          <input
                            type={showCurrentPassword ? "text" : "password"}
                            required
                            value={currentPassword}
                            onChange={(e) => setCurrentPassword(e.target.value)}
                            className="w-full p-2.5 pl-10 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-gold"
                          />
                          <button
                            type="button"
                            onClick={() => setShowCurrentPassword(!showCurrentPassword)}
                            className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gold transition-colors cursor-pointer"
                          >
                            {showCurrentPassword ? <LuEyeOff className="w-5 h-5" /> : <LuEye className="w-5 h-5" />}
                          </button>
                        </div>
                      </div>

                      <div>
                        <label className="block text-xs font-bold text-text-primary mb-1">كلمة المرور الجديدة</label>
                        <div className="relative">
                          <input
                            type={showNewPassword ? "text" : "password"}
                            required
                            value={newPassword}
                            onChange={(e) => setNewPassword(e.target.value)}
                            className="w-full p-2.5 pl-10 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-gold"
                          />
                          <button
                            type="button"
                            onClick={() => setShowNewPassword(!showNewPassword)}
                            className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gold transition-colors cursor-pointer"
                          >
                            {showNewPassword ? <LuEyeOff className="w-5 h-5" /> : <LuEye className="w-5 h-5" />}
                          </button>
                        </div>
                      </div>

                      <div>
                        <label className="block text-xs font-bold text-text-primary mb-1">تأكيد كلمة المرور الجديدة</label>
                        <div className="relative">
                          <input
                            type={showConfirmNewPassword ? "text" : "password"}
                            required
                            value={confirmNewPassword}
                            onChange={(e) => setConfirmNewPassword(e.target.value)}
                            className="w-full p-2.5 pl-10 bg-surface dark:bg-navy border border-border-primary rounded-xl text-sm outline-none focus:border-gold"
                          />
                          <button
                            type="button"
                            onClick={() => setShowConfirmNewPassword(!showConfirmNewPassword)}
                            className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400 hover:text-gold transition-colors cursor-pointer"
                          >
                            {showConfirmNewPassword ? <LuEyeOff className="w-5 h-5" /> : <LuEye className="w-5 h-5" />}
                          </button>
                        </div>
                      </div>

                      <div className="flex items-center gap-3 pt-4">
                        <button
                          type="button"
                          onClick={() => {
                            setIsPasswordModalOpen(false);
                            setCurrentPassword("");
                            setNewPassword("");
                            setConfirmNewPassword("");
                          }}
                          className="flex-1 h-10 rounded-xl bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 text-sm font-bold text-navy dark:text-gray-200 transition-colors cursor-pointer"
                        >
                          إلغاء
                        </button>
                        <button
                          type="submit"
                          disabled={changePasswordMutation.isPending}
                          className="flex-1 h-10 rounded-xl bg-gold hover:bg-gold-hover text-white text-sm font-bold flex items-center justify-center gap-2 cursor-pointer shadow-xs"
                        >
                          {changePasswordMutation.isPending && <LuLoader className="w-4 h-4 animate-spin" />}
                          <span>تأكيد التغيير</span>
                        </button>
                      </div>
                    </motion.form>
                  ) : (
                    <motion.div
                      key="button"
                      initial={{ height: 0, opacity: 0 }}
                      animate={{ height: "auto", opacity: 1 }}
                      exit={{ height: 0, opacity: 0 }}
                      transition={{ duration: 0.3 }}
                      className="overflow-hidden mt-4"
                    >
                      <button
                        onClick={() => setIsPasswordModalOpen(true)}
                        className="w-full h-11 bg-gold hover:bg-gold-hover text-white font-bold text-sm rounded-xl transition-all flex items-center justify-center gap-2 cursor-pointer"
                      >
                        <LuKey className="w-4.5 h-4.5" />
                        <span>تغيير كلمة المرور الآن</span>
                      </button>
                    </motion.div>
                  )}
                </AnimatePresence>
              </div>

              {/* Danger Zone: Delete Account */}
              {user?.role !== "Admin" && (
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
              )}

            </div>
          </div>
        </div>




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
