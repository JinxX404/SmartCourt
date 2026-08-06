import { useState } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { AdminVerificationsApi, type VerificationListDto } from "../api/adminVerificationsApi";
import { SecureImage } from "./SecureImage";
import { LuCheck, LuX, LuEye, LuLoader } from "react-icons/lu";
import toast from "react-hot-toast";

export const AdminVerificationsTab = () => {
  const queryClient = useQueryClient();
  const [selectedLawyerId, setSelectedLawyerId] = useState<string | null>(null);
  const [rejectingDocId, setRejectingDocId] = useState<string | null>(null);
  const [rejectReason, setRejectReason] = useState("");
  const [fullSizeImageUrl, setFullSizeImageUrl] = useState<string | null>(null);

  // Fetch pending verifications list
  const { data: pendingResponse, isLoading: isLoadingPending } = useQuery({
    queryKey: ["admin", "verifications", "pending"],
    queryFn: AdminVerificationsApi.getPendingVerifications,
  });

  // Fetch details for the selected lawyer
  const { data: detailsResponse, isLoading: isLoadingDetails } = useQuery({
    queryKey: ["admin", "verifications", "details", selectedLawyerId],
    queryFn: () => AdminVerificationsApi.getVerificationDetails(selectedLawyerId!),
    enabled: !!selectedLawyerId,
  });

  // Mutation to approve/reject a document
  const { mutate: reviewDoc, isPending: isReviewing } = useMutation({
    mutationFn: ({ docId, decision, reason }: { docId: string; decision: "Approve" | "Reject"; reason?: string }) =>
      AdminVerificationsApi.reviewDocument(docId, decision, reason),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("تم التحديث بنجاح");
        setRejectingDocId(null);
        setRejectReason("");
        queryClient.invalidateQueries({ queryKey: ["admin", "verifications", "details", selectedLawyerId] });
      } else {
        toast.error(response.message || "حدث خطأ");
      }
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.message || "حدث خطأ أثناء التحديث");
    },
  });

  // Mutation to approve entire user account profile
  const { mutate: approveAccount, isPending: isApprovingAccount } = useMutation({
    mutationFn: (userId: string) => AdminVerificationsApi.approveUserAccount(userId),
    onSuccess: (response) => {
      if (response.success) {
        toast.success("تم اعتماد بيانات واستكمال التوثيق بنجاح");
        queryClient.invalidateQueries({ queryKey: ["admin", "verifications", "pending"] });
        queryClient.invalidateQueries({ queryKey: ["admin", "verifications", "details", selectedLawyerId] });
      } else {
        toast.error(response.message || "حدث خطأ");
      }
    },
    onError: (err: any) => {
      toast.error(err.response?.data?.message || "حدث خطأ أثناء اعتماد الحساب");
    }
  });

  const handleReview = (docId: string, decision: "Approve" | "Reject") => {
    if (decision === "Reject") {
      setRejectingDocId(docId);
      setRejectReason("");
      return;
    }
    reviewDoc({ docId, decision });
  };

  const confirmReject = (docId: string) => {
    if (!rejectReason.trim()) {
      toast.error("يرجى إدخال سبب الرفض");
      return;
    }
    reviewDoc({ docId, decision: "Reject", reason: rejectReason.trim() });
  };

  const pendingList = pendingResponse?.data as VerificationListDto[] || [];
  const details = detailsResponse?.data as any;

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
        <div>
          <h1 className="text-2xl font-bold text-gray-900 dark:text-white">طلبات التوثيق</h1>
          <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
            مراجعة واعتماد الوثائق المرفوعة من قبل المحامين والعملاء.
          </p>
        </div>
      </div>

      {/* Main Layout: List on one side, Details on the other if selected */}
      <div className="flex flex-col lg:flex-row gap-6">

        {/* Left Side: List of Pending Users */}
        <div className={`w-full ${selectedLawyerId ? 'lg:w-1/3' : ''} bg-white dark:bg-[#1a1d23] rounded-3xl p-6 border border-gray-200/80 dark:border-gray-800 shadow-sm overflow-hidden flex flex-col`}>
          <h2 className="text-lg font-bold text-gray-800 dark:text-white mb-4">قائمة الانتظار</h2>

          {isLoadingPending ? (
            <div className="flex justify-center p-8"><LuLoader className="w-8 h-8 animate-spin text-gold" /></div>
          ) : pendingList.length === 0 ? (
            <div className="text-center p-8 text-gray-500 dark:text-gray-400 text-sm font-bold">لا يوجد طلبات توثيق قيد الانتظار حالياً.</div>
          ) : (
            <div className="flex flex-col gap-3 overflow-y-auto max-h-[600px] pr-2 custom-scrollbar">
              {pendingList.map((req) => (
                <div
                  key={req.lawyerId}
                  onClick={() => setSelectedLawyerId(req.lawyerId)}
                  className={`p-4 rounded-xl border-2 cursor-pointer transition-all ${selectedLawyerId === req.lawyerId
                    ? 'border-gold bg-gold/5'
                    : 'border-gray-100 dark:border-gray-800 hover:border-gray-300 dark:hover:border-gray-700'
                    }`}
                >
                  <div className="font-bold text-gray-800 dark:text-white mb-1 flex items-center justify-between">
                    <span>{req.fullName}</span>
                    {req.role === 'Lawyer' && <span className="bg-blue-100 text-blue-700 text-[10px] px-2 py-0.5 rounded font-bold">محامي</span>}
                    {req.role === 'Client' && <span className="bg-purple-100 text-purple-700 text-[10px] px-2 py-0.5 rounded font-bold">موكل</span>}
                    {req.role !== 'Lawyer' && req.role !== 'Client' && <span className="bg-gray-100 text-gray-700 text-[10px] px-2 py-0.5 rounded font-bold">{req.role || 'غير محدد'}</span>}
                  </div>
                  <div className="text-xs text-gray-500 dark:text-gray-400 flex justify-between items-center mt-2">
                    <span className="truncate max-w-[150px]">{req.email}</span>
                    <div className="flex gap-1">
                      {req.pendingDocumentCount > 0 && <span className="text-amber-500 bg-amber-50 dark:bg-amber-500/10 px-1.5 py-0.5 rounded text-[10px] font-bold" title="وثائق قيد المراجعة">{req.pendingDocumentCount} قيد المراجعة</span>}
                      {req.rejectedDocumentCount > 0 && <span className="text-red-500 bg-red-50 dark:bg-red-500/10 px-1.5 py-0.5 rounded text-[10px] font-bold" title="وثائق مرفوضة">{req.rejectedDocumentCount} مرفوض</span>}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Right Side: Details & Documents */}
        {selectedLawyerId && (
          <div className="w-full lg:w-2/3 bg-white dark:bg-[#1a1d23] rounded-3xl p-6 border border-gray-200/80 dark:border-gray-800 shadow-sm flex flex-col">
            <div className="flex justify-between items-center mb-6">
              <h2 className="text-lg font-bold text-gray-800 dark:text-white">تفاصيل الوثائق</h2>
              <button
                onClick={() => setSelectedLawyerId(null)}
                className="text-gray-400 hover:text-gray-600 dark:hover:text-gray-200"
              >
                <LuX className="w-5 h-5" />
              </button>
            </div>

            {isLoadingDetails ? (
              <div className="flex justify-center p-12"><LuLoader className="w-8 h-8 animate-spin text-gold" /></div>
            ) : !details ? (
              <div className="text-center p-8 text-red-500">حدث خطأ في جلب التفاصيل.</div>
            ) : (
              <div className="space-y-6">

                {/* User Info Header with Extended Profile Details */}
                <div className="bg-gray-50 dark:bg-gray-800/50 p-5 rounded-2xl border border-gray-200 dark:border-gray-700 space-y-4">
                  <div className="flex items-center justify-between border-b border-gray-200 dark:border-gray-700 pb-3">
                    <div className="flex items-center gap-3">
                      <div className="w-10 h-10 rounded-xl bg-gold/10 text-gold font-bold flex items-center justify-center text-lg">
                        {details.fullName ? details.fullName.charAt(0).toUpperCase() : "U"}
                      </div>
                      <div>
                        <h3 className="font-bold text-gray-900 dark:text-white text-base flex items-center gap-2">
                          {details.fullName}
                          <span className="px-2.5 py-0.5 rounded-full text-xs font-bold bg-gold/20 text-gold border border-gold/30">
                            {details.role === "Lawyer" ? "محامي" : "موكل"}
                          </span>
                        </h3>
                        <p className="text-xs text-gray-500 dark:text-gray-400 dir-ltr text-right">{details.email}</p>
                      </div>
                    </div>

                    <div className="flex items-center gap-2">
                      {details.accountStatus === 'PendingReview' && !details.documents?.some((doc: any) => doc.status === 'Pending') && (
                        <button
                          onClick={() => approveAccount(details.lawyerId)}
                          disabled={isApprovingAccount}
                          className="flex items-center gap-1.5 px-4 py-2 bg-green-600 hover:bg-green-700 text-white font-bold text-xs rounded-xl shadow-sm transition-all cursor-pointer disabled:opacity-50"
                          title="اعتماد التعديلات الشخصية أو المهنية"
                        >
                          {isApprovingAccount ? (
                            <LuLoader className="w-4 h-4 animate-spin" />
                          ) : (
                            <LuCheck className="w-4 h-4" />
                          )}
                          <span>اعتماد الحساب والتعديلات</span>
                        </button>
                      )}
                    </div>
                  </div>

                  <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-4 gap-3 text-xs">
                    <div className="bg-white dark:bg-gray-900 p-2.5 rounded-xl border border-gray-200/60 dark:border-gray-800">
                      <span className="text-gray-400 block mb-0.5">رقم الهاتف</span>
                      <span className="font-bold text-gray-800 dark:text-gray-200 dir-ltr block text-right">{details.phoneNumber || "غير محدد"}</span>
                    </div>

                    <div className="bg-white dark:bg-gray-900 p-2.5 rounded-xl border border-gray-200/60 dark:border-gray-800">
                      <span className="text-gray-400 block mb-0.5">الرقم القومي</span>
                      <span className="font-bold text-gray-800 dark:text-gray-200 dir-ltr block text-right">{details.nationalNumber || "غير محدد"}</span>
                    </div>

                    <div className="bg-white dark:bg-gray-900 p-2.5 rounded-xl border border-gray-200/60 dark:border-gray-800">
                      <span className="text-gray-400 block mb-0.5">تاريخ الميلاد</span>
                      <span className="font-bold text-gray-800 dark:text-gray-200">{details.dateOfBirth ? details.dateOfBirth.split("T")[0] : "غير محدد"}</span>
                    </div>

                    <div className="bg-white dark:bg-gray-900 p-2.5 rounded-xl border border-gray-200/60 dark:border-gray-800">
                      <span className="text-gray-400 block mb-0.5">العنوان / المحافظة</span>
                      <span className="font-bold text-gray-800 dark:text-gray-200 truncate block">{details.address || "غير محدد"}</span>
                    </div>

                    {details.role === "Lawyer" && (
                      <>
                        <div className="bg-white dark:bg-gray-900 p-2.5 rounded-xl border border-gray-200/60 dark:border-gray-800">
                          <span className="text-gray-400 block mb-0.5">درجة التقاضي</span>
                          <span className="font-bold text-gray-800 dark:text-gray-200 truncate block">
                            {details.level === 1 ? "جدول عام (محامي تحت التمرين)" : details.level === 2 ? "محاكم ابتدائية" : details.level === 3 ? "محاكم استئناف" : details.level === 4 ? "محكمة النقض" : "غير محدد"}
                          </span>
                        </div>

                        <div className="bg-white dark:bg-gray-900 p-2.5 rounded-xl border border-gray-200/60 dark:border-gray-800">
                          <span className="text-gray-400 block mb-0.5">التخصص الرئيسي</span>
                          <span className="font-bold text-gray-800 dark:text-gray-200 truncate block">{details.specializationName || "محاماة عامة"}</span>
                        </div>

                        <div className="bg-white dark:bg-gray-900 p-2.5 rounded-xl border border-gray-200/60 dark:border-gray-800">
                          <span className="text-gray-400 block mb-0.5">سنوات الخبرة</span>
                          <span className="font-bold text-gray-800 dark:text-gray-200">{details.yearsOfExperience ? `${details.yearsOfExperience} سنوات` : "1 سنوات"}</span>
                        </div>
                      </>
                    )}
                  </div>

                  {details.role === "Lawyer" && details.bio && (
                    <div className="bg-white dark:bg-gray-900 p-3 rounded-xl border border-gray-200/60 dark:border-gray-800 text-xs">
                      <span className="text-gray-400 block mb-1 font-bold">نبذة عن المحامي</span>
                      <p className="text-gray-700 dark:text-gray-300 leading-relaxed">{details.bio}</p>
                    </div>
                  )}
                </div>

                {/* Documents Grid */}
                <div className="grid grid-cols-1 sm:grid-cols-2 gap-6">
                  {details.documents?.map((doc: any) => (
                    <div key={doc.documentId} className="border border-gray-200 dark:border-gray-700 rounded-xl overflow-hidden shadow-sm bg-gray-50 dark:bg-gray-800/50">
                      {/* Document Header */}
                      <div className="bg-white dark:bg-[#1a1d23] p-3 border-b border-gray-200 dark:border-gray-700 flex justify-between items-center">
                        <span className="font-bold text-xs text-gray-800 dark:text-gray-200">
                          {formatDocType(doc.documentType)}
                        </span>
                        <DocStatusBadge status={doc.status} />
                      </div>

                      {/* Document Image */}
                      <div className="bg-black/5 dark:bg-black/20 h-48 relative flex items-center justify-center overflow-hidden group p-2">
                        <SecureImage
                          url={AdminVerificationsApi.getDocumentImageUrl(doc.documentId)}
                          className="max-h-full max-w-full object-contain"
                          alt={doc.documentType}
                        />
                        {/* Overlay to view full image */}
                        <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center">
                          <button
                            onClick={async () => {
                              try {
                                const res = await import("../../../../api/apiClient").then(m => m.apiClient.get(AdminVerificationsApi.getDocumentImageUrl(doc.documentId)));
                                if (res.data?.data?.downloadUrl) {
                                  setFullSizeImageUrl(res.data.data.downloadUrl);
                                }
                              } catch (err) {
                                import("react-hot-toast").then(m => m.default.error("فشل فتح الصورة"));
                              }
                            }}
                            className="text-white flex items-center gap-2 bg-white/20 hover:bg-white/30 px-4 py-2 rounded-xl font-bold backdrop-blur-sm cursor-pointer"
                          >
                            <LuEye className="w-5 h-5" /> عرض بحجم كامل
                          </button>
                        </div>
                      </div>

                      {/* Action Buttons */}
                      <div className="p-3 bg-white dark:bg-[#1a1d23] border-t border-gray-200 dark:border-gray-700 flex gap-2">
                        <button
                          onClick={() => handleReview(doc.documentId, "Approve")}
                          disabled={doc.status === 'Accepted' || doc.status === 'Verified' || isReviewing}
                          className={`flex-1 py-2 flex items-center justify-center gap-2 rounded-xl text-xs font-bold transition-colors ${doc.status === 'Accepted' || doc.status === 'Verified'
                            ? 'bg-green-100 dark:bg-green-900/30 text-green-600 dark:text-green-400 opacity-60 cursor-not-allowed'
                            : 'bg-green-500 text-white hover:bg-green-600 shadow-sm cursor-pointer'
                            }`}
                        >
                          <LuCheck className="w-4 h-4" /> قبول
                        </button>
                        <button
                          onClick={() => handleReview(doc.documentId, "Reject")}
                          disabled={doc.status === 'Rejected' || isReviewing}
                          className={`flex-1 py-2 flex items-center justify-center gap-2 rounded-xl text-xs font-bold transition-colors ${doc.status === 'Rejected'
                            ? 'bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400 opacity-60 cursor-not-allowed'
                            : 'bg-red-500 text-white hover:bg-red-600 shadow-sm cursor-pointer'
                            }`}
                        >
                          <LuX className="w-4 h-4" /> {doc.status === 'Verified' || doc.status === 'Accepted' ? 'تغيير لرفض' : 'رفض'}
                        </button>
                      </div>

                      {rejectingDocId === doc.documentId && (
                        <div className="p-3 bg-red-50 dark:bg-red-900/10 border-t border-red-100 dark:border-red-900/30 flex flex-col gap-2">
                          <input
                            type="text"
                            value={rejectReason}
                            onChange={(e) => setRejectReason(e.target.value)}
                            placeholder="اكتب سبب الرفض هنا..."
                            className="w-full text-xs p-2 border border-red-200 dark:border-red-900/50 rounded-lg focus:outline-none focus:border-red-400 bg-white dark:bg-gray-800 text-gray-800 dark:text-gray-200"
                            autoFocus
                          />
                          <div className="flex gap-2">
                            <button
                              onClick={() => confirmReject(doc.documentId)}
                              disabled={isReviewing}
                              className="flex-1 bg-red-600 hover:bg-red-700 text-white py-1.5 rounded-lg text-xs font-bold"
                            >
                              تأكيد الرفض
                            </button>
                            <button
                              onClick={() => {
                                setRejectingDocId(null);
                                setRejectReason("");
                              }}
                              disabled={isReviewing}
                              className="flex-1 bg-gray-200 dark:bg-gray-700 hover:bg-gray-300 dark:hover:bg-gray-600 text-gray-800 dark:text-gray-200 py-1.5 rounded-lg text-xs font-bold"
                            >
                              إلغاء
                            </button>
                          </div>
                        </div>
                      )}

                      {doc.rejectionReason && (
                        <div className="p-3 text-xs text-red-600 bg-red-50 dark:bg-red-900/10 font-bold border-t border-red-100 dark:border-red-900/30">
                          سبب الرفض: {doc.rejectionReason}
                        </div>
                      )}
                    </div>
                  ))}
                </div>

              </div>
            )}
          </div>
        )}
      </div>

      {/* Full Size Image Modal */}
      {fullSizeImageUrl && (
        <div
          className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm"
          onClick={() => setFullSizeImageUrl(null)}
        >
          <div
            className="relative max-w-5xl max-h-[90vh] w-full h-full flex items-center justify-center"
            onClick={(e) => e.stopPropagation()}
          >
            <button
              onClick={() => setFullSizeImageUrl(null)}
              className="absolute -top-12 right-0 md:-right-12 text-white/70 hover:text-white bg-black/50 hover:bg-black p-2 rounded-full backdrop-blur-md transition-all"
            >
              <LuX className="w-6 h-6" />
            </button>
            <img
              src={fullSizeImageUrl}
              alt="Full size document"
              className="max-w-full max-h-full object-contain rounded-xl shadow-2xl"
            />
          </div>
        </div>
      )}
    </div>
  );
};

// Helpers
const formatDocType = (type: string) => {
  switch (type) {
    case "NationalIdFront": return "بطاقة الرقم القومي (أمامي)";
    case "NationalIdBack": return "بطاقة الرقم القومي (خلفي)";
    case "BarAssociationCardFront": return "كارنيه النقابة (أمامي)";
    case "BarAssociationCardBack": return "كارنيه النقابة (خلفي)";
    case "SelfieWithId": return "صورة شخصية وانت ممسك بالبطاقة";
    case "OfficialProfilePicture": return "صورة شخصية رسمية لصفحتك";
    default: return type;
  }
};

const DocStatusBadge = ({ status }: { status: string }) => {
  switch (status) {
    case "Verified":
      return <span className="text-[10px] bg-green-100 text-green-700 px-2 py-0.5 rounded font-bold">مقبول</span>;
    case "Rejected":
      return <span className="text-[10px] bg-red-100 text-red-700 px-2 py-0.5 rounded font-bold">مرفوض</span>;
    default:
      return <span className="text-[10px] bg-amber-100 text-amber-700 px-2 py-0.5 rounded font-bold">قيد المراجعة</span>;
  }
};
