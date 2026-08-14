import { useParams, useNavigate } from "react-router-dom";
import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { toast } from "react-hot-toast";
import { CasesApi } from "../features/cases/api/casesApi";
import { Loader } from "../components/Loader";
import { MdArrowBack, MdDescription, MdLocationOn } from "react-icons/md";
import { CaseStatusBadge } from "../features/cases/components/CaseCard/CaseStatusBadge";
import { CaseReviewAnalysis } from "../features/cases/components/CaseReview/CaseReviewAnalysis";
import { CaseProposalsList } from "../features/proposal/components/CaseProposalsList";
export const CaseDetails = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [downloadingDocId, setDownloadingDocId] = useState<string | null>(null);

  const { data, isLoading, error } = useQuery({
    queryKey: ["case", id],
    queryFn: () => CasesApi.getCaseById(id!),
    enabled: !!id,
  });

  if (isLoading) {
    return <Loader />;
  }

  if (error || !data) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50">
        <h2 className="text-xl font-bold text-gray-800 mb-4">حدث خطأ أثناء تحميل تفاصيل القضية</h2>
        <button 
          onClick={() => navigate("/dashboard?tab=cases")}
          className="px-4 py-2 bg-[#c5a059] text-white rounded-lg hover:bg-[#b08d4a] transition-colors"
        >
          العودة للقضايا
        </button>
      </div>
    );
  }

  const caseData = (data as any).data || data;

  const handleDocumentClick = async (docId: string, docName: string) => {
    try {
      setDownloadingDocId(docId);
      const result = await CasesApi.downloadDocument(caseData.id, docId);
      const url = window.URL.createObjectURL(result.data);
      
      // If it's a PDF, we can open it in a new tab
      if (result.contentType === 'application/pdf') {
        window.open(url, '_blank');
      } else {
        // Otherwise download it
        const link = document.createElement('a');
        link.href = url;
        link.download = result.fileName || docName;
        document.body.appendChild(link);
        link.click();
        document.body.removeChild(link);
      }
      
      setTimeout(() => window.URL.revokeObjectURL(url), 10000);
    } catch (error) {
      console.error("Failed to download document:", error);
      toast.error("حدث خطأ أثناء تحميل المستند");
    } finally {
      setDownloadingDocId(null);
    }
  };

  const formattedDate = new Intl.DateTimeFormat('ar-EG', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
    hour: '2-digit',
    minute: '2-digit'
  }).format(new Date(caseData.createdAt));

  return (
    <div className="min-h-screen bg-[#f4f5f8] dark:bg-[#0d1017] text-[#121620] p-4 sm:p-8 flex flex-col">
      <div className="max-w-4xl w-full mx-auto space-y-6">
        
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div className="flex items-center gap-4">
            <button 
              onClick={() => navigate("/dashboard?tab=cases")}
              className="w-10 h-10 flex items-center justify-center rounded-full bg-white dark:bg-[#1a1d23] border border-gray-200 dark:border-gray-800 text-gray-600 dark:text-gray-400 hover:text-[#c5a059] dark:hover:text-[#c5a059] transition-colors"
            >
              <MdArrowBack className="text-xl" />
            </button>
            <div>
              <h1 className="text-2xl font-bold text-gray-900 dark:text-white">تفاصيل القضية</h1>
              <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
                رقم القضية: #{caseData.id?.substring(0, 11).toUpperCase()}
              </p>
            </div>
          </div>
          <CaseStatusBadge status={caseData.status} />
        </div>

        {/* Main Content */}
        <div className="bg-white dark:bg-[#1a1d23] rounded-3xl p-6 md:p-8 border border-gray-200/80 dark:border-gray-800 shadow-sm space-y-8">
          
          <div>
            <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-2">
              {caseData.title || 'قضية بدون عنوان'}
            </h2>
            <div className="flex flex-wrap items-center gap-4 text-sm text-gray-500 dark:text-gray-400">
              <span className="flex items-center gap-1.5">
                <MdLocationOn className="text-lg" />
                {caseData.governorate} - {caseData.city}
              </span>
              <span>•</span>
              <span>تاريخ الإنشاء: {formattedDate}</span>
            </div>
          </div>

          <div>
            <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-3">وصف القضية</h3>
            <p className="text-gray-700 dark:text-gray-300 leading-relaxed bg-gray-50 dark:bg-gray-800/50 p-4 rounded-2xl whitespace-pre-wrap border border-gray-100 dark:border-gray-800">
              {caseData.description || 'لا يوجد وصف متاح'}
            </p>
          </div>

          <div>
            <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-4 flex items-center gap-2">
              <MdDescription className="text-xl text-[#c5a059]" />
              المستندات المرفقة ({caseData.documents?.length || 0})
            </h3>
            
            {caseData.documents && caseData.documents.length > 0 ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
                {caseData.documents.map((doc: any, index: number) => (
                  <div key={doc.id || index} className="flex items-center justify-between p-4 rounded-xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-gray-800/30">
                    <div className="flex items-center gap-3 overflow-hidden">
                      <div className="w-10 h-10 rounded-lg bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 flex items-center justify-center shrink-0">
                        <MdDescription className="text-xl" />
                      </div>
                      <span className="text-sm font-medium text-gray-700 dark:text-gray-300 truncate" title={doc.fileName}>
                        {doc.fileName || 'مستند بدون اسم'}
                      </span>
                    </div>
                    {doc.id && (
                      <button 
                        onClick={() => handleDocumentClick(doc.id, doc.fileName || 'مستند')}
                        disabled={downloadingDocId === doc.id}
                        className="text-xs font-bold text-[#c5a059] hover:text-[#b08d4a] whitespace-nowrap px-3 py-1.5 bg-[#c5a059]/10 rounded-lg disabled:opacity-50 transition-all cursor-pointer"
                      >
                        {downloadingDocId === doc.id ? 'جاري التحميل...' : 'عرض / تحميل'}
                      </button>
                    )}
                  </div>
                ))}
              </div>
            ) : (
              <p className="text-sm text-gray-500">لا توجد مستندات مرفقة</p>
            )}
          </div>

        </div>
        
        {/* AI Review Analysis */}
        {caseData.status === 'Reviewed' && (
          <CaseReviewAnalysis caseId={caseData.id} />
        )}

        {/* Proposals List (Client) */}
        {id && (
          <div className="bg-white dark:bg-[#1a1d23] rounded-3xl p-6 md:p-8 border border-gray-200/80 dark:border-gray-800 shadow-sm mt-6">
            <CaseProposalsList caseId={id} />
          </div>
        )}

      </div>
    </div>
  );
};
