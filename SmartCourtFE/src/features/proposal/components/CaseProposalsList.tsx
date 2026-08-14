import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { MdInbox, MdFilterList } from 'react-icons/md';
import { ProposalApi } from '../api/proposalApi';
import { ProposalCard } from './ProposalCard';
import type { ProposalStatus } from '../types/proposal.types';

const STATUS_OPTIONS: { value: '' | ProposalStatus; label: string }[] = [
  { value: '', label: 'الكل (قيد الانتظار افتراضياً)' },
  { value: 'Pending', label: 'قيد الانتظار' },
  { value: 'Accepted', label: 'مقبول' },
  { value: 'Rejected', label: 'مرفوض' },
  { value: 'Cancelled', label: 'ملغي' },
  { value: 'Expired', label: 'منتهي الصلاحية' },
  { value: 'Terminated', label: 'منهى' },
  { value: 'Superseded', label: 'مُستبدَل' },
];

const PAGE_SIZE = 4;

interface CaseProposalsListProps {
  caseId: string;
}

export const CaseProposalsList = ({ caseId }: CaseProposalsListProps) => {
  const [statusFilter, setStatusFilter] = useState<'' | ProposalStatus>('');
  const [page, setPage] = useState(1);

  const { data, isLoading, isError } = useQuery({
    queryKey: ['case-proposals', caseId, statusFilter, page],
    queryFn: () =>
      ProposalApi.getCaseProposals(caseId, {
        statuses: statusFilter ? [statusFilter] : undefined,
        page,
        pageSize: PAGE_SIZE,
      }),
  });

  const proposals = data?.data?.items ?? [];
  const totalCount = data?.data?.totalCount ?? 0;
  const hasNextPage = data?.data?.hasNextPage ?? false;
  const totalPages = Math.ceil(totalCount / PAGE_SIZE);

  const handleStatusChange = (val: '' | ProposalStatus) => {
    setStatusFilter(val);
    setPage(1);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <h3 className="text-lg font-bold text-gray-900 dark:text-white">عروض هذه القضية</h3>
        <div className="relative shrink-0">
          <MdFilterList className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 text-lg pointer-events-none" />
          <select
            value={statusFilter}
            onChange={(e) => handleStatusChange(e.target.value as '' | ProposalStatus)}
            className="appearance-none pr-9 pl-4 py-2 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-[#1a1d23] text-gray-700 dark:text-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-[#c5a059]/50 focus:border-[#c5a059] transition-all cursor-pointer"
            dir="rtl"
          >
            {STATUS_OPTIONS.map((opt) => (
              <option key={opt.value} value={opt.value}>
                {opt.label}
              </option>
            ))}
          </select>
        </div>
      </div>

      {isLoading && (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {Array.from({ length: 2 }).map((_, i) => (
            <div key={i} className="h-44 rounded-2xl bg-gray-100 dark:bg-gray-800/50 animate-pulse" />
          ))}
        </div>
      )}

      {isError && (
        <div className="text-center py-6 text-red-500 text-sm">حدث خطأ أثناء تحميل العروض.</div>
      )}

      {!isLoading && !isError && proposals.length === 0 && (
        <div className="text-center py-8 text-gray-400 bg-gray-50 dark:bg-[#121620] rounded-2xl border border-dashed border-gray-200 dark:border-gray-800">
          <MdInbox className="text-3xl mx-auto mb-2 opacity-40" />
          <p className="text-sm">لا توجد عروض مرسلة بعد.</p>
        </div>
      )}

      {!isLoading && !isError && proposals.length > 0 && (
        <div className="grid grid-cols-1 sm:grid-cols-2 gap-4">
          {proposals.map((proposal) => (
            <ProposalCard key={proposal.id} proposal={proposal} viewAs="client" />
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-2 pt-2">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page <= 1}
            className="px-3 py-1.5 rounded-lg border border-gray-200 dark:border-gray-700 text-sm font-medium text-gray-600 hover:bg-gray-50 disabled:opacity-40 transition-colors"
          >
            السابق
          </button>
          <span className="text-sm text-gray-500">{page} / {totalPages}</span>
          <button
            onClick={() => setPage((p) => p + 1)}
            disabled={!hasNextPage}
            className="px-3 py-1.5 rounded-lg border border-gray-200 dark:border-gray-700 text-sm font-medium text-gray-600 hover:bg-gray-50 disabled:opacity-40 transition-colors"
          >
            التالي
          </button>
        </div>
      )}
    </div>
  );
};
