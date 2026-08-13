import React from 'react';
import { MdPendingActions, MdFactCheck, MdOutlineSync, MdCheckCircle, MdEngineering, MdLock, MdInfo } from 'react-icons/md';
import type { CaseStatus } from '../../types/case.types';

interface CaseStatusBadgeProps {
  status: CaseStatus;
}

export const CaseStatusBadge: React.FC<CaseStatusBadgeProps> = ({ status }) => {
  switch (status) {
    case 'Submitted':
      return (
        <span className="bg-blue-50 text-blue-600 font-medium text-xs px-3 py-1.5 rounded-full flex items-center gap-1.5 w-fit">
          <MdPendingActions className="text-sm" />
          تم التقديم
        </span>
      );
    case 'Reviewed':
      return (
        <span className="bg-indigo-50 text-indigo-600 font-medium text-xs px-3 py-1.5 rounded-full flex items-center gap-1.5 w-fit">
          <MdFactCheck className="text-sm" />
          تم التدقيق
        </span>
      );
    case 'FinalSubmitted':
    case 'Analyzed':
      return (
        <span className="bg-amber-50 text-amber-600 font-medium text-xs px-3 py-1.5 rounded-full flex items-center gap-1.5 w-fit">
          <MdOutlineSync className="animate-spin text-sm" />
          جاري التجهيز
        </span>
      );
    case 'Matched':
      return (
        <span className="bg-green-50 text-green-600 font-medium text-xs px-3 py-1.5 rounded-full flex items-center gap-1.5 w-fit">
          <MdCheckCircle className="text-sm" />
          تمت المطابقة
        </span>
      );
    case 'Assigned':
      return (
        <span className="bg-orange-50 text-orange-600 font-medium text-xs px-3 py-1.5 rounded-full flex items-center gap-1.5 w-fit">
          <MdEngineering className="text-sm" />
          قيد المتابعة
        </span>
      );
    case 'Closed':
      return (
        <span className="bg-gray-100 text-gray-500 font-medium text-xs px-3 py-1.5 rounded-full flex items-center gap-1.5 w-fit">
          <MdLock className="text-sm" />
          مغلقة
        </span>
      );
    default:
      return (
        <span className="bg-gray-100 text-gray-600 font-medium text-xs px-3 py-1.5 rounded-full flex items-center gap-1.5 w-fit">
          <MdInfo className="text-sm" />
          {status}
        </span>
      );
  }
};
