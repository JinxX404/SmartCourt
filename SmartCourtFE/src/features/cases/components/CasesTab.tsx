import React, { useState, useEffect, useCallback } from 'react';
import type { CaseListItemDto } from '../types/case.types';
import { CasesApi } from '../api/casesApi';
import { CasesHeader } from './CasesHeader';
import { CasesList } from './CasesList';

export const CasesTab: React.FC = () => {
  const [cases, setCases] = useState<CaseListItemDto[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  const fetchCases = useCallback(async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await CasesApi.fetchCasesList();
      if (response.success) {
        // Map the response data properly just in case it doesn't perfectly match
        // Or if the backend returns CaseDto instead of CaseListItemDto, we can adapt here
        // Assuming response.data is the array of cases we need
        setCases(response.data || []);
      } else {
        setError(response.message || 'حدث خطأ أثناء تحميل القضايا');
      }
    } catch (err: any) {
      setError(err?.response?.data?.message || 'حدث خطأ في الاتصال بالخادم');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    fetchCases();
  }, [fetchCases]);

  return (
    <div className="flex-1 overflow-y-auto custom-scrollbar p-6 md:p-8 h-full bg-background">
      <div className="max-w-[1400px] mx-auto h-full flex flex-col">
        <CasesHeader />
        <CasesList 
          cases={cases} 
          loading={loading} 
          error={error} 
          onRefresh={fetchCases} 
        />
      </div>
    </div>
  );
};
