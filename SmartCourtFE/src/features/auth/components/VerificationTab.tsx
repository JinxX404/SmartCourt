import React, { useState, useMemo, useEffect } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import imageCompression from 'browser-image-compression';
import { useAuthStore } from "../store/useAuthStore";
import { DocumentUploadCard } from "./DocumentUploadCard";
import { AuthApi } from "../api/authApi";
import { UsersApi } from "../../users/api/usersApi";
import type { LawyerProfile } from "../../users/api/usersApi";
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
  LuMap,
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

export const SPECIALIZATIONS_LIST = [
  { value: 0, label: "قانون الأسرة" },
  { value: 1, label: "القانون المدني" },
  { value: 2, label: "القانون التجاري" },
  { value: 3, label: "القانون الإداري ومجلس الدولة" },
  { value: 4, label: "القانون الجنائي" },
  { value: 5, label: "قانون العمل" },
  { value: 6, label: "القانون الدستوري" },
  { value: 7, label: "القانون الضريبي" },
  { value: 8, label: "القانون الجمركي" },
  { value: 9, label: "قانون الشركات" },
  { value: 10, label: "العقود" },
  { value: 11, label: "الملكية الفكرية" },
  { value: 12, label: "التحكيم" },
  { value: 13, label: "البنوك والتمويل" },
  { value: 14, label: "الاستثمار" },
  { value: 15, label: "العقارات والتسجيل العقاري" },
  { value: 16, label: "التنفيذ" },
  { value: 17, label: "التأمين" },
  { value: 18, label: "البيئة" },
  { value: 19, label: "الاتصالات وتكنولوجيا المعلومات" },
  { value: 20, label: "الجرائم الإلكترونية" },
];

export const getSpecializationLabel = (val: number) => {
  return SPECIALIZATIONS_LIST.find(s => s.value === val)?.label || "غير محدد";
};

const SearchableSelect = ({ value, onChange, options }: { value: number, onChange: (val: number) => void, options: { value: number, label: string }[] }) => {
  const [isOpen, setIsOpen] = React.useState(false);
  const [searchTerm, setSearchTerm] = React.useState("");
  const dropdownRef = React.useRef<HTMLDivElement>(null);

  React.useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const selectedOption = options.find(o => o.value === value);
  const filteredOptions = options.filter(o => o.label.includes(searchTerm));

  return (
    <div className="relative" ref={dropdownRef}>
      <div
        className="w-full p-2 h-9 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs flex justify-between items-center cursor-pointer transition-colors hover:border-gray-400 dark:hover:border-gray-600"
        onClick={() => {
          setIsOpen(!isOpen);
          if (!isOpen) setSearchTerm("");
        }}
      >
        <span className="truncate text-gray-800 dark:text-gray-200">{selectedOption?.label || "اختر تخصصاً..."}</span>
        <svg className={`w-3.5 h-3.5 text-gray-400 transition-transform ${isOpen ? 'rotate-180' : ''}`} fill="none" stroke="currentColor" viewBox="0 0 24 24"><path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 9l-7 7-7-7"></path></svg>
      </div>

      {isOpen && (
        <div className="absolute z-50 w-full mt-1 bg-white dark:bg-gray-800 border border-gray-200 dark:border-gray-700 rounded-lg shadow-xl top-full left-0 overflow-hidden">
          <div className="p-2 border-b border-gray-100 dark:border-gray-700 bg-gray-50/50 dark:bg-gray-900/50">
            <input
              type="text"
              className="w-full p-1.5 bg-white dark:bg-gray-900 border border-gray-300 dark:border-gray-700 rounded-md text-xs outline-none focus:border-gold dark:text-white"
              placeholder="ابحث في التخصصات..."
              value={searchTerm}
              onChange={e => setSearchTerm(e.target.value)}
              autoFocus
            />
          </div>
          <div className="max-h-40 overflow-y-auto">
            {filteredOptions.length > 0 ? filteredOptions.map(opt => (
              <div
                key={opt.value}
                className={`p-2.5 text-xs cursor-pointer hover:bg-gold/10 transition-colors ${opt.value === value ? 'bg-gold/5 font-bold text-gold border-r-2 border-gold' : 'text-gray-700 dark:text-gray-300'}`}
                onClick={() => {
                  onChange(opt.value);
                  setIsOpen(false);
                  setSearchTerm("");
                }}
              >
                {opt.label}
              </div>
            )) : (
              <div className="p-3 text-xs text-red-500 text-center font-medium bg-red-50/50 dark:bg-red-900/10">
                عفواً، برجاء الاختيار من الاقتراحات المتاحة
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
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



  // Modal State for confirmations

  // Phone Verification Modal State
  const [showPhoneVerifyModal, setShowPhoneVerifyModal] = useState(false);
  const [phoneToVerify, setPhoneToVerify] = useState("");
  const [otpCode, setOtpCode] = useState("");
  const [phoneVerifyStep, setPhoneVerifyStep] = useState<1 | 2>(1);


  // Edit profile state
  const [isEditingPersonalInfo, setIsEditingPersonalInfo] = useState(false);
  const [isEditingProfessionalInfo, setIsEditingProfessionalInfo] = useState(false);
  const [savingSection, setSavingSection] = useState<'personal' | 'professional' | null>(null);
  const [nationalNumber, setNationalNumber] = useState("");
  const [dateOfBirth, setDateOfBirth] = useState("");
  const [address, setAddress] = useState("");
  const [governorate, setGovernorate] = useState("");
  const [city, setCity] = useState("");
  const [gender, setGender] = useState<number>(0);
  const [bio, setBio] = useState("");
  const [level, setLevel] = useState(1);
  const [specializations, setSpecializations] = useState<Array<{ specialization: number; yearsOfExperience: number; casesHandled: number }>>([
    { specialization: 1, yearsOfExperience: 1, casesHandled: 0 }
  ]);

  const isLawyer = user?.role === 'Lawyer';
  const lawyerProf = profile as LawyerProfile;



  // Sync profile data and status to store & edit state
  useEffect(() => {
    if (profile) {
      if (user && profile.status && user.status !== profile.status) {
        login({ ...user, status: profile.status as any });
      }
      setNationalNumber(profile.nationalNumber || "");
      setDateOfBirth(profile.dateOfBirth ? profile.dateOfBirth.split("T")[0] : "");
      setAddress(profile.address || "");
      // Gender doesn't exist on standard client profile in frontend right now, but assuming we can cast or it doesn't break
      if ((profile as any).gender === "Female" || (profile as any).gender === 1) setGender(1);
      else setGender(0);
      setGovernorate((profile as any).governorate || "");
      setCity((profile as any).city || "");

      if (isLawyer) {
        const lp = profile as any;
        setBio(lp.bio || "");
        setLevel(lp.level || 1);

        if (lp.specializations && lp.specializations.length > 0) {
          setSpecializations(lp.specializations.map((s: any) => ({
            specialization: s.specialization,
            yearsOfExperience: s.yearsOfExperience,
            casesHandled: s.casesHandled
          })));
        } else {
          let specNum = 1;
          if (lp.specializationName === "FamilyLaw") specNum = 0;
          else if (lp.specializationName === "CivilLaw") specNum = 1;
          else if (lp.specializationName === "CommercialLaw") specNum = 2;
          else if (lp.specializationName === "AdministrativeAndStateCouncilLaw") specNum = 3;
          else if (lp.specializationName === "CriminalLaw") specNum = 4;
          else if (lp.specializationName === "LaborLaw") specNum = 5;

          setSpecializations([{
            specialization: specNum,
            yearsOfExperience: lp.yearsOfExperience || 1,
            casesHandled: lp.casesHandled || 0
          }]);
        }
      }
    }
  }, [profile, isLawyer, user, login]);

  // Phone Verification Mutations
  const sendOtpMutation = useMutation({
    mutationFn: (phone: string) => AuthApi.sendPhoneVerificationToken(phone),
    onSuccess: () => {
      toast.success("تم إرسال رمز التحقق بنجاح");
      setPhoneVerifyStep(2);
    },
    onError: (err: any) => {
      const msg = err?.response?.data?.message || "فشل إرسال رمز التحقق";
      toast.error(msg);
    }
  });

  const confirmOtpMutation = useMutation({
    mutationFn: () => AuthApi.confirmPhoneVerification(phoneToVerify, otpCode),
    onSuccess: () => {
      toast.success("تم توثيق رقم الهاتف بنجاح");
      setShowPhoneVerifyModal(false);
      setPhoneToVerify("");
      setOtpCode("");
      setPhoneVerifyStep(1);
      queryClient.invalidateQueries({ queryKey: ["userProfile", user?.id] });
    },
    onError: (err: any) => {
      const msg = err?.response?.data?.message || "فشل توثيق رقم الهاتف";
      toast.error(msg);
    }
  });

  // Update Profile Mutation
  const updateProfileMutation = useMutation({
    mutationFn: async () => {
      const formattedDob = dateOfBirth && dateOfBirth.trim() !== "" ? dateOfBirth : undefined;
      if (isLawyer) {
        return await UsersApi.updateLawyerProfile({
          nationalNumber: nationalNumber && nationalNumber.trim() !== "" ? nationalNumber : undefined,
          dateOfBirth: formattedDob,
          address,
          governorate: governorate && governorate.trim() !== "" ? governorate : undefined,
          city: city && city.trim() !== "" ? city : undefined,
          gender: gender,
          bio,
          level: Number(level),
          specializations: specializations
        });
      } else {
        return await UsersApi.updateClientProfile({
          nationalNumber: nationalNumber && nationalNumber.trim() !== "" ? nationalNumber : undefined,
          dateOfBirth: formattedDob,
          address,
          governorate: governorate && governorate.trim() !== "" ? governorate : undefined,
          city: city && city.trim() !== "" ? city : undefined,
          gender: gender,
        });
      }
    },
    onSuccess: () => {
      toast.success("تم حفظ التعديلات بنجاح.");
      setIsEditingPersonalInfo(false);
      setIsEditingProfessionalInfo(false);
      setSavingSection(null);
      queryClient.invalidateQueries({ queryKey: ["userProfile", user?.id] });
    },
    onError: (err: any) => {
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

  const handleSaveProfileClick = async (section: 'personal' | 'professional') => {
    // 1. Validation
    if (section === 'personal') {
      if (!nationalNumber || nationalNumber.trim().length !== 14) {
        toast.error("الرقم القومي مطلوب ويجب أن يتكون من 14 رقم");
        return;
      }
      if (!dateOfBirth || dateOfBirth.trim() === "") {
        toast.error("تاريخ الميلاد مطلوب");
        return;
      }
      if (!address || address.trim() === "") {
        toast.error("العنوان مطلوب");
        return;
      }
      if (!governorate || governorate.trim() === "") {
        toast.error("المحافظة مطلوبة");
        return;
      }
      if (!city || city.trim() === "") {
        toast.error("المدينة/المركز مطلوب");
        return;
      }
    } else {
      if (!specializations || specializations.length === 0) {
        toast.error("يجب إضافة تخصص واحد على الأقل");
        return;
      }
      if (specializations.some((s: any) => !s.specialization && s.specialization !== 0)) {
        toast.error("يجب تحديد جميع التخصصات المضافة");
        return;
      }
    }

    setSavingSection(section);

    // 2. Determine docs to upload
    let docsToUpload: Array<{ file: File, type: number, setFile: React.Dispatch<React.SetStateAction<File | null>> }> = [];
    if (section === 'personal') {
      if (nationalIdFront) docsToUpload.push({ file: nationalIdFront, type: 1, setFile: setNationalIdFront });
      if (nationalIdBack) docsToUpload.push({ file: nationalIdBack, type: 2, setFile: setNationalIdBack });
      if (selfie) docsToUpload.push({ file: selfie, type: 5, setFile: setSelfie });
    } else {
      if (barCard) docsToUpload.push({ file: barCard, type: 3, setFile: setBarCard });
      if (barCardBack) docsToUpload.push({ file: barCardBack, type: 4, setFile: setBarCardBack });
      if (officialProfilePicture) docsToUpload.push({ file: officialProfilePicture, type: 7, setFile: setOfficialProfilePicture });
    }

    // 3. Check for text changes
    let hasTextChanges = false;
    if (profile) {
      if (section === 'personal') {
        if ((profile.nationalNumber || "") !== nationalNumber) hasTextChanges = true;
        if ((profile.dateOfBirth ? profile.dateOfBirth.split("T")[0] : "") !== dateOfBirth) hasTextChanges = true;
        if ((profile.address || "") !== address) hasTextChanges = true;
        if (((profile as any).governorate || "") !== governorate) hasTextChanges = true;
        if (((profile as any).city || "") !== city) hasTextChanges = true;
        const profileGender = (profile as any).gender === "Female" || (profile as any).gender === 1 ? 1 : 0;
        if (profileGender !== gender) hasTextChanges = true;
      } else {
        const lp = profile as any;
        if ((lp.bio || "") !== bio) hasTextChanges = true;
        if ((lp.level || 1) !== level) hasTextChanges = true;
        if (lp.specializations && lp.specializations.length > 0) {
          if (lp.specializations.length !== specializations.length) {
            hasTextChanges = true;
          } else {
            for (let i = 0; i < specializations.length; i++) {
              const s1 = lp.specializations[i];
              const s2 = specializations[i];
              if (s1.specialization !== s2.specialization || s1.yearsOfExperience !== s2.yearsOfExperience || s1.casesHandled !== s2.casesHandled) {
                hasTextChanges = true;
                break;
              }
            }
          }
        } else {
          hasTextChanges = true;
        }
      }
    } else {
      hasTextChanges = true;
    }

    if (!hasTextChanges && docsToUpload.length === 0) {
      toast.error("لم تقم بإجراء أي تعديلات للحفظ.");
      setSavingSection(null);
      if (section === 'personal') setIsEditingPersonalInfo(false);
      if (section === 'professional') setIsEditingProfessionalInfo(false);
      return;
    }

    try {
      // 4. Upload Docs first
      if (docsToUpload.length > 0) {
        for (const doc of docsToUpload) {
          const compressed = await imageCompression(doc.file, { maxSizeMB: 1, maxWidthOrHeight: 1920, useWebWorker: true });
          const formattedDate = new Date(); formattedDate.setFullYear(formattedDate.getFullYear() + 10);
          await AuthApi.submitVerificationDocuments({
            userId: user!.id,
            documents: [{ file: compressed, type: doc.type, expirationDate: formattedDate.toISOString().split('T')[0] }]
          });
        }
        await refetch();
        for (const doc of docsToUpload) {
          doc.setFile(null); // Clear selected file after refetch
        }
      }

      // 5. Update Profile text
      if (hasTextChanges) {
        await updateProfileMutation.mutateAsync();
      } else if (docsToUpload.length > 0) {
        toast.success("تم حفظ وإرسال المستندات بنجاح.");
        if (user && user.status !== 'PendingReview') {
          login({ ...user, status: 'PendingReview' });
        }
        setSavingSection(null);
        if (section === 'personal') setIsEditingPersonalInfo(false);
        if (section === 'professional') setIsEditingProfessionalInfo(false);
      }
    } catch (err: any) {
      toast.error(err.message || "حدث خطأ غير متوقع أثناء الحفظ.");
      setSavingSection(null);
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





  const getFileSizeError = (file: File | null) => {
    if (file && file.size > 5 * 1024 * 1024) {
      return "حجم الصورة أكبر من 5 ميجابايت. يرجى اختيار صورة أصغر.";
    }
    return null;
  };

  const currentStatus = (profile as any)?.accountStatus || user?.status;
  const currentRejectionReason = (profile as any)?.rejectionReason || user?.rejectionReason;

  return (
    <div className="space-y-6">




      {/* Top Banner Status */}
      {currentStatus === 'Active' && !hasPendingDocs && !hasSelectedNewFiles ? (
        <div className="bg-green-50 dark:bg-green-500/10 border border-green-200 dark:border-green-500/20 rounded-2xl p-4 flex gap-4 shadow-xs">
          <LuShieldCheck className="w-6 h-6 text-green-500 shrink-0 mt-0.5" />
          <div>
            <h4 className="text-sm font-bold text-green-800 dark:text-green-400">حسابك موثق بالكامل</h4>
            <p className="text-xs text-green-600 dark:text-green-500/80 mt-1">
              تم اعتماد جميع مستندات التوثيق الخاصة بك بنجاح. يمكنك معاينة مستنداتك أو اضغط زر "تعديل" على أي مستند لرفع صورة جديدة وإعادة مراجعتها.
            </p>
          </div>
        </div>
      ) : currentStatus === 'PendingReview' || hasPendingDocs ? (
        <div className="bg-amber-50 dark:bg-amber-500/10 border border-amber-200 dark:border-amber-500/20 rounded-2xl p-4 flex gap-4 shadow-xs">
          <LuClock className="w-6 h-6 text-amber-500 shrink-0 mt-0.5" />
          <div>
            <h4 className="text-sm font-bold text-amber-800 dark:text-amber-400">قيد المراجعة</h4>
            <p className="text-xs text-amber-600 dark:text-amber-500/80 mt-1">
              لقد استلمنا مستنداتك بنجاح ونقوم حالياً بمراجعتها من إدارة المنصة.
            </p>
          </div>
        </div>
      ) : currentStatus === 'Rejected' ? (
        <div className="bg-red-50 dark:bg-red-500/10 border border-red-200 dark:border-red-500/20 rounded-2xl p-4 flex flex-col gap-2 shadow-xs">
          <div className="flex gap-4">
            <LuTriangleAlert className="w-6 h-6 text-red-500 shrink-0 mt-0.5" />
            <div>
              <h4 className="text-sm font-bold text-red-800 dark:text-red-400">تم رفض الحساب</h4>
              <p className="text-xs text-red-600 dark:text-red-500/80 mt-1">
                تم رفض بيانات التوثيق الخاصة بك من قبل الإدارة. يرجى تعديل البيانات وإعادة الإرسال.
              </p>
            </div>
          </div>
          {currentRejectionReason && (
            <div className="mr-10 mt-1 p-3 bg-red-100 dark:bg-red-900/30 rounded-lg border border-red-200 dark:border-red-800/50">
              <span className="text-xs font-bold text-red-800 dark:text-red-300 block mb-1">سبب الرفض:</span>
              <p className="text-sm text-red-700 dark:text-red-200">{currentRejectionReason}</p>
            </div>
          )}
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
            يرجى تعبئة البيانات الشخصية والمهنية وإرفاق المستندات المطلوبة لإتمام التوثيق.
          </p>
        </div>



        {/* Page Title */}
        <div className="flex justify-between items-center border-b border-gray-200 dark:border-gray-700 pb-3 mb-6">
          <h3 className="text-base font-bold text-gray-800 dark:text-gray-200 flex items-center gap-2">
            <LuUser className="w-5 h-5 text-gold" />
            بيانات التوثيق والمستندات
          </h3>
        </div>

        {/* ═══════════════════════════════════════════════════════════════ */}
        {/* SECTION 0: بيانات التواصل                                     */}
        {/* ═══════════════════════════════════════════════════════════════ */}
        <div className="space-y-6 mb-10">
          <h4 className="text-sm font-bold text-gray-800 dark:text-gray-200 flex items-center gap-2 pb-2 border-b border-gray-200 dark:border-gray-700">
            <LuMail className="w-4.5 h-4.5 text-gold" />
            بيانات التواصل
          </h4>
          <div className="bg-gray-50/70 dark:bg-gray-800/40 rounded-2xl p-6 border border-gray-200 dark:border-gray-700 shadow-xs grid grid-cols-1 md:grid-cols-2 gap-4">
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
                <div className="flex items-center justify-between mt-1">
                  <p className="text-xs font-bold text-gray-800 dark:text-white dir-ltr text-right">{profile?.phoneNumber || "غير محدد"}</p>
                  <button
                    onClick={() => setShowPhoneVerifyModal(true)}
                    className="text-[10px] bg-gold/10 text-gold px-2 py-1 rounded-md font-bold hover:bg-gold/20 transition-all cursor-pointer"
                  >
                    تغيير / توثيق
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>

        {/* ═══════════════════════════════════════════════════════════════ */}
        {/* SECTION 1: البيانات الشخصية + وثائق إثبات الشخصية             */}
        {/* ═══════════════════════════════════════════════════════════════ */}
        <div className={`space-y-6 mb-10 transition-all duration-300 ${isEditingPersonalInfo ? 'ring-2 ring-gold rounded-3xl p-4 shadow-[0_0_15px_rgba(212,175,55,0.15)] bg-gold/5 dark:bg-gold/5 -mx-4' : ''}`}>
          <div className="flex justify-between items-center pb-2 border-b border-gray-200 dark:border-gray-700">
            <h4 className="text-sm font-bold text-gray-800 dark:text-gray-200 flex items-center gap-2">
              <LuIdCard className="w-4.5 h-4.5 text-gold" />
              المعلومات الأساسية وإثبات الشخصية
            </h4>
            {!isEditingPersonalInfo ? (
              <button
                onClick={() => setIsEditingPersonalInfo(true)}
                className="flex items-center gap-1.5 px-3 py-1.5 bg-gold/10 hover:bg-gold/20 text-gold font-bold text-[11px] rounded-xl transition-all cursor-pointer"
              >
                <LuPencil className="w-3 h-3" />
                <span>تعديل البيانات</span>
              </button>
            ) : (
                <button
                  onClick={() => setIsEditingPersonalInfo(false)}
                  className="flex items-center gap-1 px-3 py-1.5 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 text-gray-600 dark:text-gray-300 font-bold text-[11px] rounded-xl transition-all cursor-pointer"
                >
                  <LuX className="w-3 h-3" />
                  <span>إلغاء التعديل</span>
                </button>
              )}
          </div>

          {/* Personal Data Fields */}
          <div className="bg-gray-50/70 dark:bg-gray-800/40 rounded-2xl p-6 border border-gray-200 dark:border-gray-700 space-y-4 shadow-xs">
            <div className="space-y-4">

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* National Number */}
                <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                  <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                    <LuIdCard className="w-4 h-4" />
                  </div>
                  <div className="flex-1">
                    <p className="text-[11px] text-gray-500 dark:text-gray-400">الرقم القومي <span className="text-red-500 mr-1">*</span></p>
                    {isEditingPersonalInfo ? (
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
                    <p className="text-[11px] text-gray-500 dark:text-gray-400">تاريخ الميلاد <span className="text-red-500 mr-1">*</span></p>
                    {isEditingPersonalInfo ? (
                      <input
                        type="date"
                        max={new Date().toISOString().split("T")[0]}
                        value={dateOfBirth}
                        onChange={(e) => setDateOfBirth(e.target.value)}
                        className="w-full mt-1 p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold"
                      />
                    ) : (
                      <p className="text-xs font-bold text-gray-800 dark:text-white">{profile?.dateOfBirth ? profile.dateOfBirth.split("T")[0] : "غير محدد"}</p>
                    )}
                  </div>
                </div>
              </div>

              {/* Governorate and City */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                  <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                    <LuMap className="w-4 h-4" />
                  </div>
                  <div className="flex-1">
                    <p className="text-[11px] text-gray-500 dark:text-gray-400">المحافظة <span className="text-red-500 mr-1">*</span></p>
                    {isEditingPersonalInfo ? (
                      <input type="text" value={governorate} onChange={(e) => setGovernorate(e.target.value)} placeholder="القاهرة"
                        className="w-full mt-1 p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold" />
                    ) : (
                      <p className="text-xs font-bold text-gray-800 dark:text-white">{(profile as any)?.governorate || "غير محدد"}</p>
                    )}
                  </div>
                </div>
                <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                  <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                    <LuMapPin className="w-4 h-4" />
                  </div>
                  <div className="flex-1">
                    <p className="text-[11px] text-gray-500 dark:text-gray-400">المدينة / المركز <span className="text-red-500 mr-1">*</span></p>
                    {isEditingPersonalInfo ? (
                      <input type="text" value={city} onChange={(e) => setCity(e.target.value)} placeholder="مدينة نصر"
                        className="w-full mt-1 p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold" />
                    ) : (
                      <p className="text-xs font-bold text-gray-800 dark:text-white">{(profile as any)?.city || "غير محدد"}</p>
                    )}
                  </div>
                </div>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {/* Gender */}
                <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                  <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                    <LuUser className="w-4 h-4" />
                  </div>
                  <div className="flex-1">
                    <p className="text-[11px] text-gray-500 dark:text-gray-400">النوع <span className="text-red-500 mr-1">*</span></p>
                    {isEditingPersonalInfo ? (
                      <select value={gender} onChange={(e) => setGender(Number(e.target.value))}
                        className="w-full mt-1 p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold">
                        <option value={0}>ذكر</option>
                        <option value={1}>أنثى</option>
                      </select>
                    ) : (
                      <p className="text-xs font-bold text-gray-800 dark:text-white">{((profile as any)?.gender === "Female" || (profile as any)?.gender === 1) ? "أنثى" : "ذكر"}</p>
                    )}
                  </div>
                </div>

                {/* Address */}
                <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                  <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                    <LuMapPin className="w-4 h-4" />
                  </div>
                  <div className="flex-1">
                    <p className="text-[11px] text-gray-500 dark:text-gray-400">العنوان التفصيلي <span className="text-red-500 mr-1">*</span></p>
                    {isEditingPersonalInfo ? (
                      <input type="text" value={address} onChange={(e) => setAddress(e.target.value)} placeholder="شارع التحرير، الدقي"
                        className="w-full mt-1 p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold" />
                    ) : (
                      <p className="text-xs font-bold text-gray-800 dark:text-white">{profile?.address || "غير محدد"}</p>
                    )}
                  </div>
                </div>
              </div>
            </div>
          </div>

          {/* Identity Documents - directly under personal data */}
          <h4 className="text-sm font-bold text-gray-800 dark:text-gray-200 flex items-center gap-2 pt-2">
            <LuFileText className="w-4.5 h-4.5 text-gold" />
            وثائق إثبات الشخصية
          </h4>
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
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
          </div>
          
          {/* Section Submit Button */}
          <div className="flex justify-end pt-4 border-t border-gray-200 dark:border-gray-700 mt-6">
            <button
              onClick={() => handleSaveProfileClick('personal')}
              disabled={savingSection === 'personal' || updateProfileMutation.isPending}
              className="flex items-center gap-2 px-8 py-3 bg-gold hover:bg-gold-light text-black font-bold text-sm rounded-xl transition-all cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed shadow-md hover:shadow-lg"
            >
              {(savingSection === 'personal' || updateProfileMutation.isPending) ? <LuLoader className="w-5 h-5 animate-spin" /> : <LuSave className="w-5 h-5" />}
              <span>حفظ وإرسال</span>
            </button>
          </div>
        </div>

        {/* ═══════════════════════════════════════════════════════════════ */}
        {/* SECTION 2: البيانات المهنية + وثائق الإثبات المهني (محامي فقط) */}
        {/* ═══════════════════════════════════════════════════════════════ */}
        {isLawyer && (
          <div className={`space-y-6 mb-10 transition-all duration-300 ${isEditingProfessionalInfo ? 'ring-2 ring-gold rounded-3xl p-4 shadow-[0_0_15px_rgba(212,175,55,0.15)] bg-gold/5 dark:bg-gold/5 -mx-4' : ''}`}>
            <div className="flex justify-between items-center pb-2 border-b border-gray-200 dark:border-gray-700">
              <h4 className="text-sm font-bold text-gray-800 dark:text-gray-200 flex items-center gap-2">
                <LuBriefcase className="w-4.5 h-4.5 text-gold" />
                البيانات المهنية وإثبات الممارسة
              </h4>
              {!isEditingProfessionalInfo ? (
                <button
                  onClick={() => setIsEditingProfessionalInfo(true)}
                  className="flex items-center gap-1.5 px-3 py-1.5 bg-gold/10 hover:bg-gold/20 text-gold font-bold text-[11px] rounded-xl transition-all cursor-pointer"
                >
                  <LuPencil className="w-3 h-3" />
                  <span>تعديل البيانات</span>
                </button>
              ) : (
                <button
                  onClick={() => setIsEditingProfessionalInfo(false)}
                  className="flex items-center gap-1 px-3 py-1.5 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 text-gray-600 dark:text-gray-300 font-bold text-[11px] rounded-xl transition-all cursor-pointer"
                >
                  <LuX className="w-3 h-3" />
                  <span>إلغاء التعديل</span>
                </button>
              )}
            </div>

            {/* Professional Data Fields */}
            <div className="bg-gray-50/70 dark:bg-gray-800/40 rounded-2xl p-6 border border-gray-200 dark:border-gray-700 space-y-4 shadow-xs">
              <div className="space-y-4">
                {/* Specializations List */}
                <div className="space-y-4 border-b border-gray-200 dark:border-gray-700 pb-4">
                  <div className="flex items-center justify-between">
                    <p className="text-sm font-bold text-gray-800 dark:text-gray-200">التخصصات والخبرات</p>
                    {isEditingProfessionalInfo && (
                      <button
                        onClick={() => setSpecializations([...specializations, { specialization: 1, yearsOfExperience: 1, casesHandled: 0 }])}
                        className="text-xs font-bold text-gold flex items-center gap-1 hover:underline cursor-pointer"
                      >
                        + إضافة تخصص
                      </button>
                    )}
                  </div>

                  {specializations.map((spec, index) => (
                    <div key={index} className="bg-white dark:bg-gray-900 rounded-xl p-4 border border-gray-200 dark:border-gray-800 relative space-y-3">
                      {isEditingProfessionalInfo && specializations.length > 1 && (
                        <button
                          onClick={() => setSpecializations(specializations.filter((_, i) => i !== index))}
                          className="absolute top-3 left-3 text-red-500 hover:text-red-700 transition-colors cursor-pointer"
                          title="إزالة هذا التخصص"
                        >
                          <LuX className="w-4 h-4" />
                        </button>
                      )}

                      <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                        <div className="flex-1">
                          <p className="text-[11px] text-gray-500 dark:text-gray-400 mb-1">التخصص <span className="text-red-500 mr-1">*</span></p>
                          {isEditingProfessionalInfo ? (
                            <SearchableSelect
                              value={spec.specialization}
                              onChange={(val) => {
                                const newSpecs = [...specializations];
                                newSpecs[index].specialization = val;
                                setSpecializations(newSpecs);
                              }}
                              options={SPECIALIZATIONS_LIST}
                            />
                          ) : (
                            <p className="text-xs font-bold text-gray-800 dark:text-white">
                              {getSpecializationLabel(spec.specialization)}
                            </p>
                          )}
                        </div>

                        <div className="flex-1">
                          <p className="text-[11px] text-gray-500 dark:text-gray-400 mb-1">سنوات الخبرة <span className="text-red-500 mr-1">*</span></p>
                          {isEditingProfessionalInfo ? (
                            <input type="number" min={1} max={60} value={spec.yearsOfExperience}
                              onChange={(e) => {
                                const newSpecs = [...specializations];
                                newSpecs[index].yearsOfExperience = Number(e.target.value);
                                setSpecializations(newSpecs);
                              }}
                              className="w-full p-2 h-9 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold" />
                          ) : (
                            <p className="text-xs font-bold text-gray-800 dark:text-white">{spec.yearsOfExperience} سنوات</p>
                          )}
                        </div>

                        <div className="flex-1">
                          <p className="text-[11px] text-gray-500 dark:text-gray-400 mb-1">القضايا المنجزة <span className="text-red-500 mr-1">*</span></p>
                          {isEditingProfessionalInfo ? (
                            <input type="number" min={0} value={spec.casesHandled}
                              onChange={(e) => {
                                const newSpecs = [...specializations];
                                newSpecs[index].casesHandled = Number(e.target.value);
                                setSpecializations(newSpecs);
                              }}
                              className="w-full p-2 h-9 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold" />
                          ) : (
                            <p className="text-xs font-bold text-gray-800 dark:text-white">{spec.casesHandled} قضية</p>
                          )}
                        </div>
                      </div>
                    </div>
                  ))}
                </div>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                  {/* Lawyer Level */}
                  <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800 flex items-center gap-3">
                    <div className="w-9 h-9 rounded-lg bg-gold/10 flex items-center justify-center text-gold shrink-0">
                      <LuShieldCheck className="w-4 h-4" />
                    </div>
                    <div className="flex-1">
                      <p className="text-[11px] text-gray-500 dark:text-gray-400">درجة التقاضي المقيد بها <span className="text-red-500 mr-1">*</span></p>
                      {isEditingProfessionalInfo ? (
                        <select value={level} onChange={(e) => setLevel(Number(e.target.value))}
                          className="w-full mt-1 p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold">
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
                </div>

                {/* Bio */}
                <div className="bg-white dark:bg-gray-900 rounded-xl p-3.5 border border-gray-200 dark:border-gray-800">
                  <p className="text-[11px] text-gray-500 dark:text-gray-400 mb-1">نبذة عن المحامي <span className="text-red-500 mr-1">*</span></p>
                  {isEditingProfessionalInfo ? (
                    <textarea rows={3} value={bio} onChange={(e) => setBio(e.target.value)} placeholder="اكتب نبذة مختصرة عن خبراتك وقضاياك..."
                      className="w-full p-2 bg-gray-50 dark:bg-gray-800 border border-gray-300 dark:border-gray-700 rounded-lg text-xs outline-none focus:border-gold" />
                  ) : (
                    <p className="text-xs text-gray-800 dark:text-white leading-relaxed">
                      {lawyerProf?.bio || "لا توجد نبذة مختصرة مكتوبة."}
                    </p>
                  )}
                </div>
              </div>
            </div>

            {/* Professional Documents - directly under professional data */}
            <h4 className="text-sm font-bold text-gray-800 dark:text-gray-200 flex items-center gap-2 pt-2">
              <LuFileText className="w-4.5 h-4.5 text-gold" />
              وثائق إثبات الممارسة المهنية
            </h4>
            <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
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
            </div>
            
            {/* Section Submit Button */}
            <div className="flex justify-end pt-4 border-t border-gray-200 dark:border-gray-700 mt-6">
              <button
                onClick={() => handleSaveProfileClick('professional')}
                disabled={savingSection === 'professional' || updateProfileMutation.isPending}
                className="flex items-center gap-2 px-8 py-3 bg-gold hover:bg-gold-light text-black font-bold text-sm rounded-xl transition-all cursor-pointer disabled:opacity-50 disabled:cursor-not-allowed shadow-md hover:shadow-lg"
              >
                {(savingSection === 'professional' || updateProfileMutation.isPending) ? <LuLoader className="w-5 h-5 animate-spin" /> : <LuSave className="w-5 h-5" />}
                <span>حفظ وإرسال</span>
              </button>
            </div>
          </div>
        )}

        {/* ═══════════════════════════════════════════════════════════════ */}
        {/* Info Banner + Unified Save Button                              */}
        {/* ═══════════════════════════════════════════════════════════════ */}
        <div className="bg-gold/10 border-r-4 border-gold p-4 rounded-xl flex items-start gap-3 mb-6">
          <LuInfo className="w-5 h-5 text-gold shrink-0 mt-0.5" />
          <p className="text-sm font-bold text-gray-700 dark:text-gray-300">
            ملاحظة: تعديل أو إرسال بيانات ومستندات جديدة سيعيد حسابك إلى حالة (قيد المراجعة) ليتم اعتماد التوثيق من قِبل إدارة المنصة.
          </p>
        </div>


      </div>

      {/* Phone Verification Modal */}
      {showPhoneVerifyModal && (
        <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 sm:p-6">
          <div className="absolute inset-0 bg-black/60 backdrop-blur-sm" onClick={() => setShowPhoneVerifyModal(false)}></div>
          <div className="relative bg-white dark:bg-[#1a1d23] rounded-3xl p-6 sm:p-8 w-full max-w-md shadow-2xl border border-gray-100 dark:border-gray-800 flex flex-col items-center animate-in fade-in zoom-in-95 duration-200">
            <div className="w-16 h-16 bg-gold/10 text-gold rounded-full flex items-center justify-center mb-6">
              <LuPhone className="w-8 h-8" />
            </div>

            <h3 className="text-xl font-bold text-gray-900 dark:text-white mb-2 text-center">
              توثيق رقم الهاتف
            </h3>
            <p className="text-sm text-gray-500 dark:text-gray-400 text-center mb-6">
              {phoneVerifyStep === 1
                ? "أدخل رقم هاتفك لتلقي رمز التحقق."
                : "أدخل الرمز المكون من 6 أرقام المرسل إلى هاتفك."}
            </p>

            {phoneVerifyStep === 1 ? (
              <div className="w-full space-y-4">
                <div>
                  <label className="block text-xs font-bold text-gray-700 dark:text-gray-300 mb-1.5">رقم الهاتف</label>
                  <input
                    type="tel"
                    value={phoneToVerify}
                    onChange={(e) => setPhoneToVerify(e.target.value)}
                    placeholder="011xxxxxxxx أو +2011xxxxxxxx"
                    className="w-full px-4 py-2.5 bg-gray-50 dark:bg-gray-800/50 border border-gray-200 dark:border-gray-700 rounded-xl text-sm focus:border-gold outline-none transition-all dir-ltr"
                    dir="ltr"
                  />
                </div>
                <div className="flex gap-3 pt-4">
                  <button
                    onClick={() => setShowPhoneVerifyModal(false)}
                    className="flex-1 py-2.5 px-4 bg-gray-100 hover:bg-gray-200 dark:bg-gray-800 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300 font-bold text-sm rounded-xl transition-all cursor-pointer"
                  >
                    إلغاء
                  </button>
                  <button
                    onClick={() => sendOtpMutation.mutate(phoneToVerify)}
                    disabled={!phoneToVerify || sendOtpMutation.isPending}
                    className="flex-1 py-2.5 px-4 bg-gold hover:bg-gold-600 text-white font-bold text-sm rounded-xl transition-all disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer flex justify-center items-center"
                  >
                    {sendOtpMutation.isPending ? <LuLoader className="w-5 h-5 animate-spin" /> : "إرسال الرمز"}
                  </button>
                </div>
              </div>
            ) : (
              <div className="w-full space-y-4">
                <div>
                  <label className="block text-xs font-bold text-gray-700 dark:text-gray-300 mb-1.5 text-center">رمز التحقق</label>
                  <input
                    type="text"
                    maxLength={6}
                    value={otpCode}
                    onChange={(e) => setOtpCode(e.target.value)}
                    placeholder="------"
                    className="w-full px-4 py-3 bg-gray-50 dark:bg-gray-800/50 border border-gray-200 dark:border-gray-700 rounded-xl text-center text-xl tracking-[0.5em] font-mono focus:border-gold outline-none transition-all dir-ltr"
                    dir="ltr"
                  />
                </div>
                <div className="flex gap-3 pt-4">
                  <button
                    onClick={() => setPhoneVerifyStep(1)}
                    className="flex-1 py-2.5 px-4 bg-gray-100 hover:bg-gray-200 dark:bg-gray-800 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300 font-bold text-sm rounded-xl transition-all cursor-pointer"
                  >
                    تغيير الرقم
                  </button>
                  <button
                    onClick={() => confirmOtpMutation.mutate()}
                    disabled={otpCode.length !== 6 || confirmOtpMutation.isPending}
                    className="flex-1 py-2.5 px-4 bg-green-600 hover:bg-green-700 text-white font-bold text-sm rounded-xl transition-all disabled:opacity-50 disabled:cursor-not-allowed cursor-pointer flex justify-center items-center"
                  >
                    {confirmOtpMutation.isPending ? <LuLoader className="w-5 h-5 animate-spin" /> : "تأكيد وتوثيق"}
                  </button>
                </div>
                <div className="mt-4 text-center">
                  <button
                    onClick={() => sendOtpMutation.mutate(phoneToVerify)}
                    disabled={sendOtpMutation.isPending}
                    className="text-xs text-gray-500 hover:text-gold dark:text-gray-400 dark:hover:text-gold font-bold underline transition-colors bg-transparent border-none cursor-pointer"
                  >
                    لم تستلم الرمز؟ إعادة إرسال
                  </button>
                </div>
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};
