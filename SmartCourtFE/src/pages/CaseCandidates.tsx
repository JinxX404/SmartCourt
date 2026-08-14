import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";
import { CasesApi } from "../features/cases/api/casesApi";
import { Loader } from "../components/Loader";
import { MdArrowBack, MdPerson, MdStar } from "react-icons/md";
import { SendProposalModal } from "../features/proposal/components/SendProposalModal";

interface SelectedLawyer {
  lawyerUserId: string;
  lawyerName: string;
}

export const CaseCandidates = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const [selectedLawyer, setSelectedLawyer] = useState<SelectedLawyer | null>(null);

  const { data, isLoading, error } = useQuery({
    queryKey: ["case", "candidates", id],
    queryFn: () => CasesApi.getRecommendations(id!),
    enabled: !!id,
  });

  if (isLoading) {
    return <Loader />;
  }

  if (error) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50">
        <h2 className="text-xl font-bold text-gray-800 mb-4">حدث خطأ أثناء تحميل ترشيحات المحامين</h2>
        <button
          onClick={() => navigate("/dashboard?tab=cases")}
          className="px-4 py-2 bg-[#c5a059] text-white rounded-lg hover:bg-[#b08d4a] transition-colors"
        >
          العودة للقضايا
        </button>
      </div>
    );
  }

  const candidates = data?.data?.recommendations || data?.recommendations || [];

  return (<div className="min-h-screen bg-[#f4f5f8] dark:bg-[#0d1017] text-[#121620] p-4 sm:p-8 flex flex-col">
    <div className="max-w-4xl w-full mx-auto space-y-6">

      <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div className="flex items-center gap-4">
          <button
            onClick={() => navigate("/dashboard?tab=cases")}
            className="w-10 h-10 flex items-center justify-center rounded-full bg-white dark:bg-[#1a1d23] border border-gray-200 dark:border-gray-800 text-gray-600 dark:text-gray-400 hover:text-[#c5a059] dark:hover:text-[#c5a059] transition-colors"
          >
            <MdArrowBack className="text-xl" />
          </button>
          <div>
            <h1 className="text-2xl font-bold text-gray-900 dark:text-white">الترشيحات للقضية</h1>
            <p className="text-sm text-gray-500 dark:text-gray-400 mt-1">
              رقم القضية: #{id?.substring(0, 11).toUpperCase()}
            </p>
          </div>
        </div>
      </div>

      {/* Main Content */}
      <div className="bg-white dark:bg-[#1a1d23] rounded-3xl p-6 md:p-8 border border-gray-200/80 dark:border-gray-800 shadow-sm space-y-8">

        <h2 className="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
          <MdPerson className="text-2xl text-[#c5a059]" />
          المحامون المرشحون ({candidates.length || 0})
        </h2>

        {candidates && candidates.length > 0 ? (
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {candidates.map((lawyer: any, index: number) => (
              <div key={lawyer.lawyerId || lawyer.id || index} className="p-5 rounded-2xl border border-gray-200 dark:border-gray-800 bg-gray-50 dark:bg-[#121620] flex flex-col gap-4">
                <div className="flex items-center gap-4">
                  <div className="w-14 h-14 rounded-full bg-gray-200 dark:bg-gray-700 flex items-center justify-center text-gray-500">
                    {lawyer.imageUrl ? (
                      <img src={lawyer.imageUrl} alt={lawyer.lawyerName || lawyer.name} className="w-full h-full rounded-full object-cover" />
                    ) : (
                      <MdPerson className="text-3xl" />
                    )}
                  </div>
                  <div>
                    <h3 className="font-bold text-gray-900 dark:text-white">{lawyer.lawyerName || lawyer.name || 'محامي'}</h3>
                    <div className="flex items-center gap-1 text-sm text-amber-500 mt-1">
                      <MdStar />
                      <span className="font-medium">
                        {lawyer.ratingScore ? (lawyer.ratingScore * 5).toFixed(1) : lawyer.rating || 'جديد'}
                      </span>
                      {lawyer.totalScore && (
                        <span className="ml-2 text-xs bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-200 px-2 py-0.5 rounded-full mr-2">
                          نسبة التطابق: {(lawyer.totalScore * 100).toFixed(0)}%
                        </span>
                      )}
                    </div>
                  </div>
                </div>

                {lawyer.explanation && (
                  <p className="text-sm text-gray-600 dark:text-gray-400 mt-2 mb-2 leading-relaxed line-clamp-3">
                    {lawyer.explanation}
                  </p>
                )}

                <button
                  id={`select-lawyer-${lawyer.lawyerId || lawyer.id || index}`}
                  onClick={() =>
                    setSelectedLawyer({
                      lawyerUserId: lawyer.lawyerId || lawyer.id,
                      lawyerName: lawyer.lawyerName || lawyer.name || 'محامي',
                    })
                  }
                  className="w-full py-2 mt-auto bg-[#c5a059] hover:bg-[#b08d4a] text-white rounded-lg text-sm font-bold transition-colors"
                >
                  اختيار المحامي
                </button>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-12 text-gray-500">
            لا توجد ترشيحات متاحة حالياً.
          </div>
        )}

      </div>
    </div>

    {selectedLawyer && id && (
      <SendProposalModal
        caseId={id}
        lawyerUserId={selectedLawyer.lawyerUserId}
        lawyerName={selectedLawyer.lawyerName}
        onClose={() => setSelectedLawyer(null)}
        onSuccess={() => {
          setSelectedLawyer(null);
          navigate(`/dashboard/cases/${id}`);
        }}
      />
    )}
  </div>
  );
};
