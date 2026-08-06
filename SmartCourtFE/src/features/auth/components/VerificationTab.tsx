import { useState, useMemo, useEffect } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import imageCompression from 'browser-image-compression';
import { useAuthStore } from "../store/useAuthStore";
import { DocumentUploadCard } from "./DocumentUploadCard";
import { AuthApi } from "../api/authApi";
import { UsersApi } from "../../users/api/usersApi";
import type { LawyerProfile } from "../../users/api/usersApi";
import { ConfirmModal } from "../../../components/ConfirmModal";
import toast from "react-hot-toast";
import {
  LuIdCard,
  LuImage,
  LuUser,
  LuInfo,
  LuTriangleAlert,
  LuLoader,
  LuClock,
  LuFileText,
  LuBriefcase,
  LuShieldCheck,
  LuMail,
  LuPhone,
  LuCalendar,
  LuMapPin,
  LuPencil,
  LuSave,
  LuX
} from "react-icons/lu";

const getLawyerLevelTitle = (lvl?: number) => {
  switch (lvl) {
    case 1: return "جدول عام (محامي تحت التمرين)";
    case 2: return "محاكم ابتدائية";
    case 3: return "محاكم استئناف";
    case 4: return "محكمة النقض";
    default: return "محامي ممارس";
  }
};

export const VerificationTab = () => {
  const { user, login } = useAuthStore();
  const queryClient = useQueryClient();

  const { data: documentsResponse, isLoading: isLoadingDocs, refetch } = useQuery({
    queryKey: ["user", "verifications", "documents", user?.id],
    queryFn: () => AuthApi.getUserVerificationDocuments(user!.id),
    enabled: !!user?.id,
  });

  // Fetch lawyer profile for personal & professional data tab
  const { data: profile, isLoading: isLoadingProfile } = useQuery({
    queryKey: ["userProfile", user?.id],
    queryFn: async () => {
      if (user?.role === "Lawyer") {
        return await UsersApi.getLawyerProfile();
      } else {
        return await UsersApi.getClientProfile();
      }
    },
    enabled: !!user,
    refetchInterval: 5000, // Auto-check status changes from Admin every 5 seconds
  });

  // Document upload state
  const [nationalIdFront, setNationalIdFront] = useState<File | null>(null);
  const [nationalIdBack, setNationalIdBack] = useState<File | null>(null);
  const [selfie, setSelfie] = useState<File | null>(null);
  const [barCard, setBarCard] = useState<File | null>(null);
  const [barCardBack, setBarCardBack] = useState<File | null>(null);
  const [officialProfilePicture, setOfficialProfilePicture] = useState<File | null>(null);

  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);

  // Modal State for confirmations
  const [showProfileConfirmModal, setShowProfileConfirmModal] = useState(false);
  const [showDocConfirmModal, setShowDocConfirmModal] = useState(false);

  // SubTab state: default to 'professional' (البيانات الشخصية والمهنية) as tab 1, and 'documents' as tab 2
  const [activeSubTab, setActiveSubTab] = useState<'professional' | 'documents'>('professional');

  // Edit profile state
  const [isEditingInfo, setIsEditingInfo] = useState(false);
  const [phoneNumber, setPhoneNumber] = useState("");
  const [nationalNumber, setNationalNumber] = useState("");
  const [dateOfBirth, setDateOfBirth] = useState("");
  const [address, setAddress] = useState("");
  const [bio, setBio] = useState("");
  const [yearsOfExperience, setYearsOfExperience] = useState(1);
  const [level, setLevel] = useState(1);
  const [specializationId, setSpecializationId] = useState("");

  const isLawyer = user?.role === 'Lawyer';
  const lawyerProf = profile as LawyerProfile;

  // Check if profile data is filled
  const isProfileComplete = useMemo(() => {
    if (!profile) return false;
    const hasPhone = !!profile.phoneNumber && profile.phoneNumber.trim() !== "" && profile.phoneNumber !== "غير محدد";
    const hasAddress = !!profile.address && profile.address.trim() !== "" && profile.address !== "غير محدد";
    return hasPhone && hasAddress;
  }, [profile]);

  // Sync profile data and status to store & edit state
  useEffect(() => {
    if (profile) {
      if (user && profile.status && user.status !== profile.status) {
        login({ ...user, status: profile.status as any });
      }
      setPhoneNumber(profile.phoneNumber || "");
      setNationalNumber(profile.nationalNumber || "");
      setDateOfBirth(profile.dateOfBirth ? profile.dateOfBirth.split("T")[0] : "");
      setAddress(profile.address || "");
      if (isLawyer) {
        const lp = profile as LawyerProfile;
        setBio(lp.bio || "");
        setYearsOfExperience(lp.yearsOfExperience || 1);
        setLevel(lp.level || 1);
        setSpecializationId(lp.specializationId || "");
      }
    }
  }, [profile, isLawyer, user, login]);

  // Handle switching tabs with validation check
  const handleTabChange = (targetTab: 'professional' | 'documents') => {
    if (targetTab === 'documents' && !isProfileComplete) {
      toast.error("يرجى تعبئة بياناتك الشخصية والمهنية أولاً وحفظها قبل الانتقال لرفع المستندات.");
      return;
    }
    setActiveSubTab(targetTab);
  };

  // Update Profile Mutation
  const updateProfileMutation = useMutation({
    mutationFn: async () => {
      const formattedDob = dateOfBirth && dateOfBirth.trim() !== "" ? dateOfBirth : undefined;
      if (isLawyer) {
        return await UsersApi.updateLawyerProfile({
          phoneNumber,
          nationalNumber: nationalNumber && nationalNumber.trim() !== "" ? nationalNumber : undefined,
          dateOfBirth: formattedDob,
          address,
          bio,
          yearsOfExperience: Number(yearsOfExperience),
          level: Number(level),
          specializationId: specializationId || undefined
        });
      } else {
        return await UsersApi.updateClientProfile({
          phoneNumber,
          nationalNumber: nationalNumber && nationalNumber.trim() !== "" ? nationalNumber : undefined,
          dateOfBirth: formattedDob,
          address
        });
      }
    },
    onSuccess: () => {
      setShowProfileConfirmModal(false);
      if (user) {
        login({ ...user, status: 'PendingReview' });
      }
      toast("تم حفظ التعديلات وحسابك الآن (قيد المراجعة) لمراجعة البيانات الجديدة من الأدمن.", {
        icon: "⚠️",
        duration: 6000
      });
      setIsEditingInfo(false);
      queryClient.invalidateQueries({ queryKey: ["userProfile", user?.id] });
    },
    onError: (err: any) => {
      setShowProfileConfirmModal(false);
      let msg = "فشل حفظ البيانات";
      const apiErr = err?.response?.data;
      if (apiErr?.message) {
        msg = apiErr.message;
      } else if (apiErr?.errors) {
        if (Array.isArray(apiErr.errors)) {
          msg = apiErr.errors.join(" | ");
        } else if (typeof apiErr.errors === 'object') {
          msg = Object.values(apiErr.errors).flat().join(" | ");
        }
      }
      toast.error(msg);
    }
  });

  const handleSaveProfileClick = () => {
    if (user?.status === 'Active') {
      setShowProfileConfirmModal(true);
    } else {
      updateProfileMutation.mutate();
    }
  };

  const docs = useMemo(() => {
    return documentsResponse?.data?.documents || [];
  }, [documentsResponse]);

  const getDocInfo = (type: number) => {
    const doc = docs.find((d: any) => d.documentType === type && d.isCurrent);
    if (!doc) return { status: undefined, reason: undefined, name: undefined, id: undefined };

    let statusStr = doc.status;
    if (typeof doc.status === 'number') {
      statusStr = doc.status === 1 ? 'Pending' : doc.status === 2 ? 'Verified' : doc.status === 3 ? 'Rejected' : 'Expired';
    } else if (typeof doc.status === 'string') {
      if (doc.status.toLowerCase() === 'verified') statusStr = 'Verified';
      else if (doc.status.toLowerCase() === 'pending') statusStr = 'Pending';
      else if (doc.status.toLowerCase() === 'rejected') statusStr = 'Rejected';
      else if (doc.status.toLowerCase() === 'expired') statusStr = 'Expired';
    }

    return { status: statusStr, reason: doc.rejectionReason, name: doc.fileName, id: doc.documentId };
  };

  const idFrontInfo = getDocInfo(1);
  const idBackInfo = getDocInfo(2);
  const selfieInfo = getDocInfo(5);
  const barCardInfo = getDocInfo(3);
  const barCardBackInfo = getDocInfo(4);
  const officialProfilePictureInfo = getDocInfo(7);

  const hasSelectedNewFiles = !!(nationalIdFront || nationalIdBack || selfie || barCard || barCardBack || officialProfilePicture);
  const hasPendingDocs = useMemo(() => {
    return docs.some((d: any) => d.isCurrent && (d.status === 1 || d.status === 'Pending'));
  }, [docs]);



  const handleDocSubmitClick = () => {
    if (!user) return;

    if (!isProfileComplete) {
      setError("يرجى تعبئة بياناتك الشخصية والمهنية وحفظها أولاً من التابة الأولى قبل إرسال مستندات التوثيق.");
      toast.error("يرجى تعبئة بياناتك الشخصية والمهنية أولاً وحفظها قبل إرسال المستندات.");
      return;
    }

    if (user?.status === 'Active') {
      setShowDocConfirmModal(true);
    } else {
      executeDocUpload();
    }
  };

  const executeDocUpload = async () => {
    setShowDocConfirmModal(false);
    if (!user) return;

    const missingFront = (!idFrontInfo.status || idFrontInfo.status === 'Rejected') && !nationalIdFront;
    const missingBack = (!idBackInfo.status || idBackInfo.status === 'Rejected') && !nationalIdBack;
    const missingSelfie = (!selfieInfo.status || selfieInfo.status === 'Rejected') && !selfie;
    const missingBarCard = isLawyer && (!barCardInfo.status || barCardInfo.status === 'Rejected') && !barCard;
    const missingBarCardBack = isLawyer && (!barCardBackInfo.status || barCardBackInfo.status === 'Rejected') && !barCardBack;
    const missingOfficialProfilePicture = isLawyer && (!officialProfilePictureInfo.status || officialProfilePictureInfo.status === 'Rejected') && !officialProfilePicture;

    if (missingFront || missingBack || missingSelfie || missingBarCard || missingBarCardBack || missingOfficialProfilePicture) {
      setError("الرجاء إرفاق جميع المستندات المطلوبة (الجديدة أو المرفوضة).");
      return;
    }

    if (!nationalIdFront && !nationalIdBack && !selfie && !barCard && !barCardBack && !officialProfilePicture) {
      setError("لم تقم باختيار أي مستندات جديدة للرفع.");
      return;
    }

    const MAX_FILE_SIZE_MB = 5;
    const MAX_FILE_SIZE_BYTES = MAX_FILE_SIZE_MB * 1024 * 1024;

    const hasSizeErrors = [nationalIdFront, nationalIdBack, selfie, barCard, barCardBack, officialProfilePicture].some(f => f && f.size > MAX_FILE_SIZE_BYTES);

    if (hasSizeErrors) {
      setError("يوجد صورة يتجاوز حجمها الحد الأقصى المسموح (5 ميجابايت)، يرجى مراجعة التنبيهات باللون الأحمر أسفل الصور المرفوضة.");
      toast.error("عذراً، بعض الصور حجمها كبير جداً.");
      return;
    }

    setIsLoading(true);
    setError(null);
    setSuccess(false);

    console.time('Total Time');
    console.time('Image Compression');

    try {
      const futureDate = new Date();
      futureDate.setFullYear(futureDate.getFullYear() + 10);
      const formattedDate = futureDate.toISOString().split('T')[0];

      const compressImage = async (file: File) => {
        const options = {
          maxWidthOrHeight: 1280,
          initialQuality: 0.7,
          useWebWorker: true
        };
        try {
          const compressed = await imageCompression(file, options);
          return compressed as File;
        } catch (error) {
          console.error('Error compressing image:', error);
          return file; // Fallback to original
        }
      };

      const tasks: Promise<{ file: File; type: number; expirationDate: string }>[] = [];

      if (nationalIdFront) {
        tasks.push(compressImage(nationalIdFront).then(res => ({ file: res, type: 1, expirationDate: formattedDate })));
      }

      if (nationalIdBack) {
        tasks.push(compressImage(nationalIdBack).then(res => ({ file: res, type: 2, expirationDate: formattedDate })));
      }

      if (selfie) {
        tasks.push(compressImage(selfie).then(res => ({ file: res, type: 5, expirationDate: formattedDate })));
      }

      if (isLawyer && barCard) {
        tasks.push(compressImage(barCard).then(res => ({ file: res, type: 3, expirationDate: formattedDate })));
      }

      if (isLawyer && barCardBack) {
        tasks.push(compressImage(barCardBack).then(res => ({ file: res, type: 4, expirationDate: formattedDate })));
      }

      if (isLawyer && officialProfilePicture) {
        tasks.push(compressImage(officialProfilePicture).then(res => ({ file: res, type: 7, expirationDate: formattedDate })));
      }

      const documents = await Promise.all(tasks);

      if (documents.length === 0) {
        setError("لم تقم بتحديد أي مستندات جديدة للرفع.");
        setIsLoading(false);
        return;
      }

      console.timeEnd('Image Compression');
      console.time('API Request');

      const response = await AuthApi.submitVerificationDocuments({
        userId: user.id,
        documents
      });

      console.timeEnd('API Request');

      if (response.data?.failedDocuments?.length > 0 && response.data?.uploadedDocuments?.length === 0) {
        setError(response.data.failedDocuments[0].error || "حدث خطأ أثناء رفع المستندات.");
        setIsLoading(false);
        return;
      }

      if (user) {
        login({ ...user, status: 'PendingReview' });
      }
      toast.success("تم إرسال المستندات بنجاح. حسابك الآن قيد المراجعة من الأدمن.");

      await refetch();

      console.timeEnd('Total Time');
      setSuccess(true);
      setNationalIdFront(null);
      setNationalIdBack(null);
      setSelfie(null);
      setBarCard(null);
      setBarCardBack(null);
      setOfficialProfilePicture(null);

      // Reset file input UI values manually if needed
      const fileInputs = document.querySelectorAll('input[type="file"]') as NodeListOf<HTMLInputElement>;
      fileInputs.forEach(input => input.value = "");
    } catch (err: any) {
      console.error("Error submitting documents:", err);

      let errorMessage = "حدث خطأ أثناء رفع المستندات. تأكد من حجم وصيغة الصور.";
      if (err.response?.data?.errors && err.response.data.errors.length > 0) {
        errorMessage = err.response.data.errors.join(" | ");
      } else if (err.response?.data?.message) {
        errorMessage = err.response.data.message;
      }

      setError(errorMessage);
      setIsLoading(false);
    }
  };

  const isAllSubmittedOrVerified =
    (idFrontInfo.status === 'Pending' || idFrontInfo.status === 'Verified') &&
    (idBackInfo.status === 'Pending' || idBackInfo.status === 'Verified') &&
    (selfieInfo.status === 'Pending' || selfieInfo.status === 'Verified') &&
    (!isLawyer || ((barCardInfo.status === 'Pending' || barCardInfo.status === 'Verified') && (barCardBackInfo.status === 'Pending' || barCardBackInfo.status === 'Verified') && (officialProfilePictureInfo.status === 'Pending' || officialProfilePictureInfo.status === 'Verified')));

  const getFileSizeError = (file: File | null) => {
    if (file && file.size > 5 * 1024 * 1024) {
      return "حجم الصورة أكبر من 5 ميجابايت. يرجى اختيار صورة أصغر.";
    }
    return null;
  };

  return (
    <div className="space-y-6">
      {/* Profile Edit Confirmation Modal */}
      <ConfirmModal
        isOpen={showProfileConfirmModal}
        onClose={() => setShowProfileConfirmModal(false)}
        onConfirm={() => updateProfileMutation.mutate()}
        title="إعادة مراجعة الحساب"
        description="تعديل بياناتك الشخصية والمهنية يتطلب إعادة مراجعة حسابك بواسطة إدارة المنصة وتحويل حالتك إلى (قيد المراجعة). هل أنت متأكد من الحفظ والاستمرار؟"
        type="warning"
        confirmText="نعم، احفظ ورسّل للمراجعة"
        cancelText="إلغاء"
        isLoading={updateProfileMutation.isPending}
      />

      {/* Document Edit Confirmation Modal */}
      <ConfirmModal
        isOpen={showDocConfirmModal}
        onClose={() => setShowDocConfirmModal(false)}
        onConfirm={executeDocUpload}
        title="إعادة مراجعة المستندات"
        description="إرسال مستندات جديدة أو معدلة سيعيد حسابك إلى حالة (قيد المراجعة) لإعادة اعتماد الوثائق الجديدة من الأدمن. هل أنت متأكد من الاستمرار؟"
        type="warning"
        confirmText="نعم، ارسل للمراجعة"
        cancelText="إلغاء"
        isLoading={isLoading}
      />

      {/* Top Banner Status */}
      {user?.status === 'Active' && !hasPendingDocs && !hasSelectedNewFiles ? (
        <div className="bg-green-50 dark:bg-green-500/10 border border-green-200 dark:border-green-500/20 rounded-2xl p-4 flex gap-4 shadow-xs">
          <LuShieldCheck className="w-6 h-6 text-green-500 shrink-0 mt-0.5" />
          <div>
            <h4 className="text-sm font-bold text-green-800 dark:text-green-400">حسابك موثق بالكامل</h4>
            <p className="text-xs text-green-600 dark:text-green-500/80 mt-1">
              تم اعتماد جميع مستندات التوثيق الخاصة بك بنجاح. يمكنك معاينة مستنداتك أو اضغط زر "تعديل" على أي مستند لرفع صورة جديدة وإعادة مراجعتها.
            </p>
          </div>
        </div>
      ) : user?.status === 'PendingReview' || hasPendingDocs ? (
        <div className="bg-amber-50 dark:bg-amber-500/10 border border-amber-200 dark:border-amber-500/20 rounded-2xl p-4 flex gap-4 shadow-xs">
          <LuClock className="w-6 h-6 text-amber-500 shrink-0 mt-0.5" />
          <div>
            <h4 className="text-sm font-bold text-amber-800 dark:text-amber-400">قيد المراجعة</h4>
            <p className="text-xs text-amber-600 dark:text-amber-500/80 mt-1">
              لقد استلمنا مستنداتك بنجاح ونقوم حالياً بمراجعتها من إدارة المنصة.
            </p>
          </div>
        </div>
      ) : (
        <div className="bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-500/20 rounded-2xl p-4 flex gap-4 shadow-xs">
          <LuTriangleAlert className="w-6 h-6 text-red-500 shrink-0 mt-0.5" />
          <div>
            <h4 className="text-sm font-bold text-red-800 dark:text-red-400">الحساب غير موثق</h4>
            <p className="text-xs text-red-600 dark:text-red-500/80 mt-1">
              يرجى استكمال إجراءات التوثيق لتتمكن من الاستفادة من كافة خدمات المنصة.
            </p>
          </div>
        </div>
      )}

      <div className="bg-white dark:bg-[#1a1d23] rounded-3xl p-6 lg:p-8 border border-gray-200/80 dark:border-gray-800 shadow-xs relative">
        {(isLoadingDocs || isLoadingProfile) && (
          <div className="absolute inset-0 bg-white/50 dark:bg-[#1a1d23]/50 z-20 flex items-center justify-center rounded-3xl backdrop-blur-sm">
            <LuLoader className="w-8 h-8 animate-spin text-gold" />
          </div>
        )}

        {/* Page Header */}
        <div className="text-center mb-6">
          <div className="w-16 h-16 bg-gold/10 text-gold rounded-2xl flex items-center justify-center mx-auto mb-4">
            <LuIdCard className="w-8 h-8" />
          </div>
          <h2 className="text-2xl font-bold text-gray-900 dark:text-white mb-2">صفحة التوثيق</h2>
          <p className="text-sm text-gray-500 dark:text-gray-400">
            يرجى تعبئة البيانات الشخصية والمهنية أولاً ثم إرفاق المستندات المطلوبة لإتمام التوثيق.
          </p>
        </div>

        {/* Sub Tabs: 1st Tab = Personal & Professional Data, 2nd Tab = Documents */}
        <div className="flex justify-center gap-2 mb-8">
          <button
            onClick={() => handleTabChange('professional')}
            className={`flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-bold transition-all cursor-pointer ${activeSubTab === 'professional'
              ? 'bg-gold text-white shadow-md'
              : 'bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700'
              }`}
          >
            <LuBriefcase className="w-4 h-4" />
            البيانات الشخصية والمهنية
          </button>
          <button
            onClick={() => handleTabChange('documents')}
            className={`flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-bold transition-all cursor-pointer ${activeSubTab === 'documents'
              ? 'bg-gold text-white shadow-md'
              : 'bg-gray-100 dark:bg-gray-800 text-gray-600 dark:text-gray-400 hover:bg-gray-200 dark:hover:bg-gray-700'
              }`}
          >
            <LuFileText className="w-4 h-4" />
            الوثائق والمستندات
          </button>
        </div>

        {error && (
          <div className="mb-6 p-4 bg-red-50 dark:bg-red-500/10 text-red-600 dark:text-red-400 text-sm font-bold rounded-xl text-center">
            {error}
          </div>
        )}

        {success && (
          <div className="mb-6 p-4 bg-green-50 dark:bg-green-500/10 text-green-600 dark:text-green-400 text-sm font-bold rounded-xl text-center">
            تم إرسال المستندات بنجاح! سيتم مراجعتها قريباً.
          </div>
        )}

        {/* SubTab 1: Personal & Professional Data */}
        {activeSubTab === 'professional' && (
          <div className="space-y-6">
            <div className="flex justify-between items-center border-b border-gray-200 dark:border-gray-700 pb-3">
              <h3 className="text-base font-bold text-gray-800 dark:text-gray-200 flex items-center gap-2">
                <LuUser className="w-5 h-5 text-gold" />
                بيانات التوثيق الشخصية والمهنية
              </h3>
              {!isEditingInfo ? (
                <button
                  onClick={() => setIsEditingInfo(true)}
                  className="flex items-center gap-1.5 px-4 py-2 bg-gold/10 hover:bg-gold/20 text-gold font-bold text-xs rounded-xl transition-all cursor-pointer"
                >
                  <LuPencil className="w-4 h-4" />
                  <span>تعديل البيانات</span>
                </button>
              ) : (
                <div className="flex items-center gap-2">
                  <button
                    onClick={() => setIsEditingInfo(false)}
                    className="flex items-center gap-1 px-3 py-1.5 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 text-gray-600 dark:text-gray-300 font-bold text-xs rounded-xl transition-all cursor-pointer"
                  >
                    <LuX className="w-4 h-4" />
                    <span>إلغاء</span>
                  </button>
                  <button
                    onClick={handleSaveProfileClick}
                    disabled={updateProfileMutation.isPending}
                    className="flex items-center gap-1.5 px-4 py-1.5 bg-green-600 hover:bg-green-700 text-white font-bold text-xs rounded-xl transition-all cursor-pointer"
                  >
                    {updateProfileMutation.isPending ? (
                      <LuLoader className="w-4 h-4 animate-spin" />
                    ) : (
                      <LuSave className="w-4 h-4" />
                    )}
                    <span>حفظ التعديلات</span>
                  </button>
                </div>
              )}
            </div>

            {/* 2 Big Main Cards Side-by-Side */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 items-start">

              {/* CARD 1: المعلومات الأساسية */}
              <div className="bg-gray-50/70 dark:bg-gray-800/40 rounded-2xl p-6 border border-gray-200 dark:border-gray-700 space-y-4 shadow-xs">
                <h4 className="text-sm font-bold text-gray-800 dark:text-gray-200 flex items-center gap-2 pb-2 border-b border-gray-200 dark:border-gray-700">
                  <LuMail className="w-4.5 h-4.5 text-gold" />
                  المعلومات الأساسية
                </h4>

                <div className="space-y-4">
                  {/* Email (Read Only) */}
                  <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                    <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                      <LuMail className="w-4 h-4" />
                    </div>
                    <div className="overflow-hidden">
                      <p className="text-[11px] text-gray-500 dark:text-gray-400">البريد الإلكتروني</p>
                      <p className="text-xs font-bold text-gray-800 dark:text-white truncate dir-ltr text-right">{profile?.email || user?.email}</p>
                    </div>
                  </div>

                  {/* Phone */}
                  <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                    <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                      <LuPhone className="w-4 h-4" />
                    </div>
                    <div className="flex-1">
                      <p className="text-[11px] text-gray-500 dark:text-gray-400">رقم الهاتف</p>
                      {isEditingInfo ? (
                        <input
                          type="tel"
                          value={phoneNumber}
                          onChange={(e) => setPhoneNumber(e.target.value)}
                          placeholder="011xxxxxxxx أو +2011xxxxxxxx"
                          className="w-full mt-1 p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold"
                        />
                      ) : (
                        <p className="text-xs font-bold text-gray-800 dark:text-white dir-ltr text-right">{profile?.phoneNumber || "غير محدد"}</p>
                      )}
                    </div>
                  </div>

                  {/* National Number */}
                  <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                    <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                      <LuIdCard className="w-4 h-4" />
                    </div>
                    <div className="flex-1">
                      <p className="text-[11px] text-gray-500 dark:text-gray-400">الرقم القومي</p>
                      {isEditingInfo ? (
                        <input
                          type="text"
                          value={nationalNumber}
                          onChange={(e) => setNationalNumber(e.target.value.replace(/\D/g, '').slice(0, 14))}
                          placeholder="14 رقم"
                          maxLength={14}
                          className="w-full mt-1 p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold"
                        />
                      ) : (
                        <p className="text-xs font-bold text-gray-800 dark:text-white dir-ltr text-right">{profile?.nationalNumber || "غير محدد"}</p>
                      )}
                    </div>
                  </div>

                  {/* Date of Birth */}
                  <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                    <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                      <LuCalendar className="w-4 h-4" />
                    </div>
                    <div className="flex-1">
                      <p className="text-[11px] text-gray-500 dark:text-gray-400">تاريخ الميلاد</p>
                      {isEditingInfo ? (
                        <input
                          type="date"
                          value={dateOfBirth}
                          onChange={(e) => setDateOfBirth(e.target.value)}
                          className="w-full mt-1 p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold"
                        />
                      ) : (
                        <p className="text-xs font-bold text-gray-800 dark:text-white">{profile?.dateOfBirth ? profile.dateOfBirth.split("T")[0] : "غير محدد"}</p>
                      )}
                    </div>
                  </div>

                  {/* Address */}
                  <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                    <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                      <LuMapPin className="w-4 h-4" />
                    </div>
                    <div className="flex-1">
                      <p className="text-[11px] text-gray-500 dark:text-gray-400">العنوان / المحافظة</p>
                      {isEditingInfo ? (
                        <input
                          type="text"
                          value={address}
                          onChange={(e) => setAddress(e.target.value)}
                          placeholder="القاهرة، مصر"
                          className="w-full mt-1 p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold"
                        />
                      ) : (
                        <p className="text-xs font-bold text-gray-800 dark:text-white">{profile?.address || "غير محدد"}</p>
                      )}
                    </div>
                  </div>
                </div>
              </div>

              {/* CARD 2: البيانات المهنية */}
              <div className="bg-gray-50/70 dark:bg-gray-800/40 rounded-2xl p-6 border border-gray-200 dark:border-gray-700 space-y-4 shadow-xs">
                <h4 className="text-sm font-bold text-gray-800 dark:text-gray-200 flex items-center gap-2 pb-2 border-b border-gray-200 dark:border-gray-700">
                  <LuBriefcase className="w-4.5 h-4.5 text-gold" />
                  البيانات المهنية
                </h4>

                <div className="space-y-4">


                  {/* Years of Experience */}
                  {isLawyer && (
                    <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                      <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                        <LuBriefcase className="w-4 h-4" />
                      </div>
                      <div className="flex-1">
                        <p className="text-[11px] text-gray-500 dark:text-gray-400">سنوات الخبرة</p>
                        {isEditingInfo ? (
                          <input
                            type="number"
                            min={1}
                            max={60}
                            value={yearsOfExperience}
                            onChange={(e) => setYearsOfExperience(Number(e.target.value))}
                            className="w-full mt-1 p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold"
                          />
                        ) : (
                          <p className="text-xs font-bold text-gray-800 dark:text-white">
                            {lawyerProf?.yearsOfExperience || 1} سنوات
                          </p>
                        )}
                      </div>
                    </div>
                  )}

                  {/* Lawyer Level */}
                  {isLawyer && (
                    <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                      <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                        <LuShieldCheck className="w-4 h-4" />
                      </div>
                      <div className="flex-1">
                        <p className="text-[11px] text-gray-500 dark:text-gray-400">درجة التقاضي المقيد بها</p>
                        {isEditingInfo ? (
                          <select
                            value={level}
                            onChange={(e) => setLevel(Number(e.target.value))}
                            className="w-full mt-1 p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold"
                          >
                            <option value={1}>جدول عام (محامي تحت التمرين)</option>
                            <option value={2}>محاكم ابتدائية</option>
                            <option value={3}>محاكم استئناف</option>
                            <option value={4}>محكمة النقض</option>
                          </select>
                        ) : (
                          <p className="text-xs font-bold text-gray-800 dark:text-white">
                            {getLawyerLevelTitle(lawyerProf?.level)}
                          </p>
                        )}
                      </div>
                    </div>
                  )}

                  {/* Bio */}
                  {isLawyer && (
                    <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800">
                      <p className="text-[11px] text-gray-500 dark:text-gray-400 mb-1">نبذة عن المحامي</p>
                      {isEditingInfo ? (
                        <textarea
                          rows={3}
                          value={bio}
                          onChange={(e) => setBio(e.target.value)}
                          placeholder="اكتب نبذة مختصرة عن خبراتك وقضاياك..."
                          className="w-full p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold"
                        />
                      ) : (
                        <p className="text-xs text-gray-800 dark:text-white leading-relaxed">
                          {lawyerProf?.bio || "لا توجد نبذة مختصرة مكتوبة."}
                        </p>
                      )}
                    </div>
                  )}
                </div>
              </div>
            </div>

            {/* Info Banner */}
            <div className="bg-gold/10 border-r-4 border-gold p-4 rounded-xl flex items-start gap-3">
              <LuInfo className="w-5 h-5 text-gold shrink-0 mt-0.5" />
              <p className="text-sm font-bold text-gray-700 dark:text-gray-300">
                ملاحظة: عند التعديل على بياناتك الشخصية والمهنية، سيتم إعادة مراجعة حسابك بواسطة إدارة المنصة وتحويل حالتك تلقائياً إلى (قيد المراجعة).
              </p>
            </div>
          </div>
        )}

        {/* SubTab 2: Documents */}
        {activeSubTab === 'documents' && (
          <>
            {!isProfileComplete && (
              <div className="mb-6 p-4 bg-amber-50 dark:bg-amber-500/10 border border-amber-200 dark:border-amber-500/20 text-amber-800 dark:text-amber-400 text-sm font-bold rounded-xl flex items-center gap-3">
                <LuTriangleAlert className="w-5 h-5 text-amber-500 shrink-0" />
                <span>تنبيه: يرجى تعبئة بياناتك الشخصية والمهنية وحفظها أولاً من التابة الأولى قبل رفع مستندات التوثيق.</span>
              </div>
            )}

            <div className="grid grid-cols-1 md:grid-cols-2 gap-6 mb-8">
              <DocumentUploadCard
                label="بطاقة الرقم القومي (الأمام)"
                icon={<LuIdCard className="w-8 h-8" />}
                onFileSelect={setNationalIdFront}
                selectedFile={nationalIdFront}
                status={idFrontInfo.status}
                rejectionReason={idFrontInfo.reason}
                error={getFileSizeError(nationalIdFront)}
                existingImageUrl={idFrontInfo.id ? AuthApi.getDocumentImageUrl(idFrontInfo.id) : undefined}
              />
              <DocumentUploadCard
                label="بطاقة الرقم القومي (الخلف)"
                icon={<LuIdCard className="w-8 h-8" />}
                onFileSelect={setNationalIdBack}
                selectedFile={nationalIdBack}
                status={idBackInfo.status}
                rejectionReason={idBackInfo.reason}
                error={getFileSizeError(nationalIdBack)}
                existingImageUrl={idBackInfo.id ? AuthApi.getDocumentImageUrl(idBackInfo.id) : undefined}
              />
              {isLawyer && (
                <DocumentUploadCard
                  label="كارنيه النقابة (الأمام)"
                  icon={<LuImage className="w-8 h-8" />}
                  onFileSelect={setBarCard}
                  selectedFile={barCard}
                  status={barCardInfo.status}
                  rejectionReason={barCardInfo.reason}
                  error={getFileSizeError(barCard)}
                  existingImageUrl={barCardInfo.id ? AuthApi.getDocumentImageUrl(barCardInfo.id) : undefined}
                />
              )}
              {isLawyer && (
                <DocumentUploadCard
                  label="كارنيه النقابة (الخلف)"
                  icon={<LuImage className="w-8 h-8" />}
                  onFileSelect={setBarCardBack}
                  selectedFile={barCardBack}
                  status={barCardBackInfo.status}
                  rejectionReason={barCardBackInfo.reason}
                  error={getFileSizeError(barCardBack)}
                  existingImageUrl={barCardBackInfo.id ? AuthApi.getDocumentImageUrl(barCardBackInfo.id) : undefined}
                />
              )}

              <DocumentUploadCard
                label="صورة شخصية وانت ممسك بالبطاقة"
                icon={<LuUser className="w-8 h-8" />}
                onFileSelect={setSelfie}
                selectedFile={selfie}
                status={selfieInfo.status}
                rejectionReason={selfieInfo.reason}
                error={getFileSizeError(selfie)}
                existingImageUrl={selfieInfo.id ? AuthApi.getDocumentImageUrl(selfieInfo.id) : undefined}
              />
              {isLawyer && (
                <DocumentUploadCard
                  label="صورة شخصية رسمية للبروفايل"
                  icon={<LuUser className="w-8 h-8" />}
                  onFileSelect={setOfficialProfilePicture}
                  selectedFile={officialProfilePicture}
                  status={officialProfilePictureInfo.status}
                  rejectionReason={officialProfilePictureInfo.reason}
                  error={getFileSizeError(officialProfilePicture)}
                  existingImageUrl={officialProfilePictureInfo.id ? AuthApi.getDocumentImageUrl(officialProfilePictureInfo.id) : undefined}
                />
              )}
            </div>

            {/* Info Banner */}
            <div className="bg-gold/10 border-r-4 border-gold p-4 rounded-xl flex items-start gap-3 mb-8">
              <LuInfo className="w-5 h-5 text-gold shrink-0 mt-0.5" />
              <p className="text-sm font-bold text-gray-700 dark:text-gray-300">
                تنبيه: تعديل أو إرسال مستندات جديدة سيعيد حسابك إلى حالة (قيد المراجعة) ليتم اعتماد الوثائق الجديدة من قِبل الأدمن.
              </p>
            </div>

            <div className="flex justify-center">
              {(!isAllSubmittedOrVerified || hasSelectedNewFiles) && (
                <button
                  onClick={handleDocSubmitClick}
                  disabled={isLoading}
                  className="w-full md:w-auto px-12 py-3 bg-[#c79a5e] hover:bg-[#b08752] disabled:opacity-50 disabled:cursor-not-allowed text-white text-sm font-bold rounded-xl shadow-lg hover:shadow-xl hover:-translate-y-0.5 transition-all flex items-center justify-center gap-2 cursor-pointer"
                >
                  {isLoading ? (
                    <>
                      <LuLoader className="w-5 h-5 animate-spin" />
                      جاري الإرسال...
                    </>
                  ) : (
                    hasSelectedNewFiles ? "إرسال المستندات المعدلة للمراجعة" : "إرسال للمراجعة"
                  )}
                </button>
              )}
            </div>
          </>
        )}
      </div>
    </div>
  );
};
