import React from 'react';
import { MdAutorenew, MdError, MdFolderOpen } from 'react-icons/md';
import type { CaseListItemDto } from '../types/case.types';
import { CaseCard } from './CaseCard/CaseCard';

interface CasesListProps {
  cases: CaseListItemDto[];
  loading: boolean;
  error: string | null;
  onRefresh: () => void;
}

export const CasesList: React.FC<CasesListProps> = ({ cases, loading, error, onRefresh }) => {
  if (loading) {
    return (
      <div className="flex-1 w-full flex flex-col bg-surface-container-lowest border border-outline-variant rounded-xl shadow-sm overflow-hidden">
        <div className="p-12 flex justify-center items-center h-full">
          <div className="flex flex-col items-center gap-4 text-on-surface-variant">
            <MdAutorenew className="animate-spin text-5xl" />
            <p className="font-label-sm">جاري تحميل القضايا...</p>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex-1 w-full flex flex-col bg-surface-container-lowest border border-outline-variant rounded-xl shadow-sm overflow-hidden">
        <div className="p-12 flex justify-center items-center h-full">
          <div className="flex flex-col items-center gap-4 text-error">
            <MdError className="text-5xl" />
            <p className="font-label-sm">{error}</p>
            <button 
              onClick={onRefresh}
              className="mt-4 bg-surface-container-low text-on-surface-variant border border-outline-variant px-4 py-2 rounded-lg font-label-sm hover:bg-surface-container-highest transition-colors"
            >
              إعادة المحاولة
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (cases.length === 0) {
    return (
      <div className="flex-1 w-full flex flex-col bg-surface-container-lowest border border-outline-variant rounded-xl shadow-sm overflow-hidden">
        <div className="p-12 flex justify-center items-center h-full">
          <div className="flex flex-col items-center gap-4 text-on-surface-variant">
            <MdFolderOpen className="text-5xl" />
            <p className="font-headline-md">لا توجد قضايا حالياً</p>
            <p className="font-body-md text-center max-w-md">
              لم تقم برفع أي قضايا بعد. يمكنك البدء برفع قضيتك الأولى للبحث عن أفضل المحامين لتمثيلك.
            </p>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex-1 w-full flex flex-col bg-gray-50 border border-gray-200 rounded-xl shadow-sm overflow-hidden">
      <div className="p-4 border-b border-gray-200 bg-white flex justify-between items-center rounded-t-xl">
        <h2 className="text-xl font-bold text-gray-900">قائمة القضايا</h2>
        <span className="bg-gray-900 text-white text-xs font-medium px-3 py-1 rounded-full">
          إجمالي: {cases.length} {cases.length === 1 ? 'قضية' : 'قضايا'}
        </span>
      </div>
      
      <div className="flex-1 overflow-y-auto custom-scrollbar p-4 space-y-3">
        <div className="flex flex-col gap-4">
          {cases.map((caseItem) => (
            <CaseCard 
              key={caseItem.id} 
              caseItem={caseItem} 
              onRefresh={onRefresh} 
            />
          ))}
        </div>
      </div>
    </div>
  );
};
