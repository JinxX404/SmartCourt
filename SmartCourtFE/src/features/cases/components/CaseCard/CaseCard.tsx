import React from 'react';
import { MdDescription, MdCalendarToday } from 'react-icons/md';
import { useNavigate } from 'react-router-dom';
import type { CaseListItemDto } from '../../types/case.types';
import { CaseStatusBadge } from './CaseStatusBadge';
import { CaseCardActions } from './CaseCardActions';

interface CaseCardProps {
  caseItem: CaseListItemDto;
  onRefresh: () => void;
}

const getStatusColorClass = (status: string) => {
  switch (status) {
    case 'Submitted':
      return 'bg-blue-500';
    case 'Reviewed':
      return 'bg-indigo-500';
    case 'FinalSubmitted':
    case 'Analyzed':
      return 'bg-legal-gold';
    case 'Matched':
      return 'bg-green-500';
    case 'Assigned':
      return 'bg-amber-500';
    case 'Closed':
      return 'bg-gray-500';
    default:
      return 'bg-gray-300';
  }
};

export const CaseCard: React.FC<CaseCardProps> = ({ caseItem, onRefresh }) => {
  const navigate = useNavigate();
  const isClosed = caseItem.status === 'Closed';

  const formattedDate = new Intl.DateTimeFormat('ar-EG', {
    year: 'numeric',
    month: 'long',
    day: 'numeric',
  }).format(new Date(caseItem.createdAt));

  const handleCardClick = () => {
    navigate(`/dashboard/cases/${caseItem.id}`);
  };

  return (
    <div 
      onClick={handleCardClick}
      className={`bg-white border border-gray-200 rounded-xl p-5 md:p-6 shadow-sm hover:shadow-md transition-all duration-300 group flex flex-col md:flex-row md:items-center justify-between gap-6 relative overflow-hidden cursor-pointer ${isClosed ? 'bg-gray-50 opacity-80' : ''}`}
    >
      <div className={`absolute right-0 top-0 bottom-0 w-1.5 ${getStatusColorClass(caseItem.status)}`}></div>
      
      <div className="flex-1 flex flex-col gap-3">
        <div className="flex items-center gap-3">
          <CaseStatusBadge status={caseItem.status} />
          <span className="text-sm font-medium text-gray-400">
            #{caseItem.id.substring(0, 11).toUpperCase()}
          </span>
        </div>
        
        <h3 className="text-xl font-bold text-gray-900 group-hover:text-legal-gold transition-colors">
          {caseItem.title || 'قضية بدون عنوان'}
        </h3>
        
        <div className="flex items-center gap-6 text-sm text-gray-500">
          <span className="flex items-center gap-1.5">
            <MdDescription className="text-lg" /> 
            {caseItem.documentCount || 0} مستندات
          </span>
          <span className="flex items-center gap-1.5">
            <MdCalendarToday className="text-lg" /> 
            تم الإنشاء: {formattedDate}
          </span>
        </div>
      </div>

      <CaseCardActions id={caseItem.id} status={caseItem.status} onRefresh={onRefresh} />
    </div>
  );
};
