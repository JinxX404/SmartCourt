import React, { useEffect } from 'react';
import { useQuery } from '@tanstack/react-query';
import { CasesApi } from '../../api/casesApi';
import { Loader } from '../../../../components/Loader';
import { 
  MdCheckCircle, 
  MdWarning, 
  MdLightbulbOutline, 
  MdInfoOutline, 
  MdOutlineDocumentScanner 
} from 'react-icons/md';
import type { ReviewPointType, ReviewPointDto } from '../../types/case.types';

interface CaseReviewAnalysisProps {
  caseId: string;
}

const typeOrder: Record<ReviewPointType, number> = {
  Strength: 0,
  Weakness: 1,
  Suggestion: 2,
  MissingCaseInfo: 3,
  MissingCaseDoc: 4,
};

const getReviewPointConfig = (type: ReviewPointType) => {
  switch (type) {
    case 'Strength':
      return {
        icon: <MdCheckCircle className="text-xl text-emerald-500" />,
        bgColor: 'bg-emerald-50 dark:bg-emerald-500/10',
        borderColor: 'border-emerald-200 dark:border-emerald-500/20',
        titleColor: 'text-emerald-700 dark:text-emerald-400',
        title: 'نقطة قوة'
      };
    case 'Weakness':
      return {
        icon: <MdWarning className="text-xl text-red-500" />,
        bgColor: 'bg-red-50 dark:bg-red-500/10',
        borderColor: 'border-red-200 dark:border-red-500/20',
        titleColor: 'text-red-700 dark:text-red-400',
        title: 'نقطة ضعف'
      };
    case 'Suggestion':
      return {
        icon: <MdLightbulbOutline className="text-xl text-blue-500" />,
        bgColor: 'bg-blue-50 dark:bg-blue-500/10',
        borderColor: 'border-blue-200 dark:border-blue-500/20',
        titleColor: 'text-blue-700 dark:text-blue-400',
        title: 'اقتراح'
      };
    case 'MissingCaseInfo':
      return {
        icon: <MdInfoOutline className="text-xl text-amber-500" />,
        bgColor: 'bg-amber-50 dark:bg-amber-500/10',
        borderColor: 'border-amber-200 dark:border-amber-500/20',
        titleColor: 'text-amber-700 dark:text-amber-400',
        title: 'معلومات ناقصة'
      };
    case 'MissingCaseDoc':
      return {
        icon: <MdOutlineDocumentScanner className="text-xl text-orange-500" />,
        bgColor: 'bg-orange-50 dark:bg-orange-500/10',
        borderColor: 'border-orange-200 dark:border-orange-500/20',
        titleColor: 'text-orange-700 dark:text-orange-400',
        title: 'مستندات ناقصة'
      };
    default:
      return {
        icon: <MdInfoOutline className="text-xl text-gray-500" />,
        bgColor: 'bg-gray-50 dark:bg-gray-500/10',
        borderColor: 'border-gray-200 dark:border-gray-500/20',
        titleColor: 'text-gray-700 dark:text-gray-400',
        title: 'ملاحظة'
      };
  }
};

export const CaseReviewAnalysis: React.FC<CaseReviewAnalysisProps> = ({ caseId }) => {
  const { data: response, isLoading, error } = useQuery({
    queryKey: ['caseReview', caseId],
    queryFn: () => CasesApi.getLatestReview(caseId),
    enabled: !!caseId,
  });

  useEffect(() => {
    if (!isLoading && response?.success && window.location.hash === '#report') {
      setTimeout(() => {
        const el = document.getElementById('report');
        if (el) {
          el.scrollIntoView({ behavior: 'smooth' });
        }
      }, 100);
    }
  }, [isLoading, response]);

  if (isLoading) {
    return (
      <div className="flex justify-center p-8 border border-gray-200/80 dark:border-gray-800 rounded-3xl bg-white dark:bg-[#1a1d23] shadow-sm mt-8">
        <Loader />
      </div>
    );
  }

  // If there's an error (like 404), or no success, don't show anything (or show a fallback)
  if (error || !response?.success || !response.data) {
    return null; 
  }

  const reviewData = response.data;
  const reviewPoints = [...(reviewData.reviewPoints || [])].sort(
    (a, b) => (typeOrder[a.type] ?? 99) - (typeOrder[b.type] ?? 99)
  );

  if (reviewPoints.length === 0) {
    return null;
  }

  return (
    <div id="report" className="bg-white dark:bg-[#1a1d23] rounded-3xl p-6 md:p-8 border border-gray-200/80 dark:border-gray-800 shadow-sm space-y-6 mt-8">
      <div>
        <h2 className="text-xl font-bold text-gray-900 dark:text-white flex items-center gap-2">
          <MdLightbulbOutline className="text-2xl text-[#c5a059]" />
          التحليل الآلي للقضية
        </h2>
        <p className="text-sm text-gray-500 dark:text-gray-400 mt-2">
          هذا التحليل تم إنشاؤه آلياً بواسطة المساعد الذكي لمساعدتك في تقييم وتجهيز القضية.
        </p>
      </div>

      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {reviewPoints.map((point: ReviewPointDto) => {
          const config = getReviewPointConfig(point.type);
          
          return (
            <div 
              key={point.id} 
              className={`p-4 rounded-2xl border ${config.bgColor} ${config.borderColor} flex flex-col gap-2`}
            >
              <div className="flex items-center gap-2">
                {config.icon}
                <span className={`text-sm font-bold ${config.titleColor}`}>
                  {config.title}
                </span>
              </div>
              <p className="text-sm text-gray-700 dark:text-gray-300 leading-relaxed">
                {point.description}
              </p>
            </div>
          );
        })}
      </div>
    </div>
  );
};
