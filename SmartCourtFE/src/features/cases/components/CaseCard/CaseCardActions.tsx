import React, { useState } from 'react';
import { MdOutlineSync, MdAutoAwesome, MdAnalytics, MdAutorenew, MdGroup, MdChat, MdVisibility } from 'react-icons/md';
import { useNavigate } from 'react-router-dom';
import type { CaseStatus } from '../../types/case.types';
import { CasesApi } from '../../api/casesApi';
import toast from 'react-hot-toast';

interface CaseCardActionsProps {
  id: string;
  status: CaseStatus;
  onRefresh: () => void;
}

export const CaseCardActions: React.FC<CaseCardActionsProps> = ({ id, status, onRefresh }) => {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [isFinalizing, setIsFinalizing] = useState(false);

  const handleAiReview = async (e: React.MouseEvent) => {
    e.stopPropagation();
    try {
      setLoading(true);
      await CasesApi.triggerAiReview(id);
      toast.success('تم إرسال القضية للتدقيق بنجاح');
      onRefresh();
    } catch (error) {
      toast.error('حدث خطأ أثناء إرسال القضية للتدقيق');
    } finally {
      setLoading(false);
    }
  };

  const handleFinalize = async (e: React.MouseEvent) => {
    e.stopPropagation();
    try {
      setIsFinalizing(true);
      await CasesApi.finalizeCase(id);
      toast.success('تم الاعتماد والبحث عن محامٍ بنجاح');
      onRefresh();
    } catch (error) {
      toast.error('حدث خطأ أثناء اعتماد القضية');
      setIsFinalizing(false);
    }
  };

  const handleViewReport = (e: React.MouseEvent) => {
    e.stopPropagation();
    // Navigate to report view section
    navigate(`/dashboard/cases/${id}#report`);
  };

  const handleViewCandidates = (e: React.MouseEvent) => {
    e.stopPropagation();
    navigate(`/dashboard/cases/${id}/candidates`);
  };

  const handleChat = (e: React.MouseEvent) => {
    e.stopPropagation();
    // TODO: add chat endpoint call
    navigate(`/dashboard/cases/${id}/chat`);
  };

  const handleViewDetails = (e: React.MouseEvent) => {
    e.stopPropagation();
    navigate(`/dashboard/cases/${id}`);
  };

  if (isFinalizing || status === 'FinalSubmitted' || status === 'Analyzed') {
    return (
      <div className="flex flex-col sm:flex-row gap-3 shrink-0">
        <div className="bg-gray-50 text-gray-400 border border-gray-200 text-sm font-medium px-5 py-2.5 rounded-lg flex items-center justify-center gap-2 whitespace-nowrap cursor-not-allowed">
          <MdAutorenew className="animate-spin text-lg" />
          جاري تحليل القضية وترشيح المحامين...
        </div>
      </div>
    );
  }

  if (status === 'Submitted') {
    return (
      <div className="flex flex-col sm:flex-row gap-3 shrink-0">
        <button 
          onClick={handleAiReview}
          disabled={loading}
          className="bg-amber-50 text-amber-700 border border-amber-200 hover:bg-amber-100 text-sm font-medium px-5 py-2.5 rounded-lg transition-colors flex items-center justify-center gap-2 whitespace-nowrap disabled:opacity-50"
        >
          {loading ? (
            <MdOutlineSync className="animate-spin text-lg" />
          ) : (
            <MdAutoAwesome className="text-lg" />
          )}
          تدقيق بالذكاء الاصطناعي
        </button>
        <button 
          onClick={handleFinalize}
          disabled={isFinalizing}
          className="bg-white text-gray-600 border border-gray-300 hover:bg-gray-50 text-sm font-medium px-5 py-2.5 rounded-lg transition-colors flex items-center justify-center whitespace-nowrap hidden md:flex disabled:opacity-50"
        >
          اعتماد والبحث عن محامٍ
        </button>
      </div>
    );
  }

  if (status === 'Reviewed') {
    return (
      <div className="flex flex-col sm:flex-row gap-3 shrink-0">
        <button 
          onClick={handleViewReport}
          disabled={loading}
          className="bg-[#c5a059] text-white hover:bg-[#b08d4a] text-sm font-medium px-5 py-2.5 rounded-lg transition-colors flex items-center justify-center gap-2 whitespace-nowrap disabled:opacity-50"
        >
          <MdAnalytics className="text-lg" />
          عرض تقرير التدقيق
        </button>
        <button 
          onClick={handleFinalize}
          disabled={isFinalizing}
          className="bg-white text-gray-600 border border-gray-300 hover:bg-gray-50 text-sm font-medium px-5 py-2.5 rounded-lg transition-colors flex items-center justify-center whitespace-nowrap hidden md:flex disabled:opacity-50"
        >
          اعتماد والبحث عن محامٍ
        </button>
      </div>
    );
  }

  if (status === 'Matched') {
    return (
      <div className="flex flex-col sm:flex-row gap-3 shrink-0">
        <button 
          onClick={handleViewCandidates}
          className="bg-[#c5a059] text-white hover:bg-[#b08d4a] text-sm font-medium px-5 py-2.5 rounded-lg transition-colors flex items-center justify-center gap-2 whitespace-nowrap"
        >
          <MdGroup className="text-lg" />
          عرض الترشيحات
        </button>
      </div>
    );
  }

  if (status === 'Assigned') {
    return (
      <div className="flex flex-col sm:flex-row gap-3 shrink-0">
        <button 
          onClick={handleChat}
          className="bg-[#c5a059] text-white hover:bg-[#b08d4a] text-sm font-medium px-5 py-2.5 rounded-lg transition-colors flex items-center justify-center gap-2 whitespace-nowrap"
        >
          <MdChat className="text-lg" />
          مراسلة المحامي
        </button>
      </div>
    );
  }

  if (status === 'Closed') {
    return (
      <div className="flex flex-col sm:flex-row gap-3 shrink-0">
        <button 
          onClick={handleViewDetails}
          className="bg-white text-gray-500 border border-gray-200 hover:bg-gray-50 text-sm font-medium px-5 py-2.5 rounded-lg transition-colors flex items-center justify-center gap-2 whitespace-nowrap"
        >
          <MdVisibility className="text-lg" />
          عرض تفاصيل القضية
        </button>
      </div>
    );
  }

  return null;
};
