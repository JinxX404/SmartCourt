import { useState } from 'react';
import { useQuery } from '@tanstack/react-query';
import { MdSearch, MdInbox, MdFilterList } from 'react-icons/md';
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

const PAGE_SIZE = 10;

export const LawyerProposalsList = () => {
  const [statusFilter, setStatusFilter] = useState<'' | ProposalStatus>('');
  const [search, setSearch] = useState('');
  const [searchInput, setSearchInput] = useState('');
  const [page, setPage] = useState(1);

  const { data, isLoading, isError } = useQuery({
    queryKey: ['lawyer-proposals', statusFilter, search, page],
    queryFn: () =>
      ProposalApi.getLawyerProposals({
        statuses: statusFilter ? [statusFilter] : undefined,
        search: search || undefined,
        page,
        pageSize: PAGE_SIZE,
      }),
  });

  const proposals = data?.data?.items ?? [];
  const totalCount = data?.data?.totalCount ?? 0;
  const hasNextPage = data?.data?.hasNextPage ?? false;
  const totalPages = Math.ceil(totalCount / PAGE_SIZE);

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    setSearch(searchInput);
    setPage(1);
  };

  const handleStatusChange = (val: '' | ProposalStatus) => {
    setStatusFilter(val);
    setPage(1);
  };

  return (
    <div className="space-y-6">
      <div className="flex flex-col sm:flex-row gap-3">
        <form onSubmit={handleSearch} className="flex-1 flex gap-2">
          <div className="relative flex-1">
            <MdSearch className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 text-lg pointer-events-none" />
            <input
              type="text"
              value={searchInput}
              onChange={(e) => setSearchInput(e.target.value)}
              placeholder="ابحث بعنوان القضية أو اسم الموكل..."
              maxLength={100}
              className="w-full pr-10 pl-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-[#1a1d23] text-gray-900 dark:text-white text-sm placeholder:text-gray-400 focus:outline-none focus:ring-2 focus:ring-[#c5a059]/50 focus:border-[#c5a059] transition-all"
              dir="rtl"
            />
          </div>
          <button
            type="submit"
            className="px-4 py-2.5 rounded-xl bg-[#c5a059] hover:bg-[#b08d4a] text-white text-sm font-medium transition-colors"
          >
            بحث
          </button>
        </form>

        <div className="relative shrink-0">
          <MdFilterList className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 text-lg pointer-events-none" />
          <select
            value={statusFilter}
            onChange={(e) => handleStatusChange(e.target.value as '' | ProposalStatus)}
            className="appearance-none pr-9 pl-4 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 bg-white dark:bg-[#1a1d23] text-gray-700 dark:text-gray-300 text-sm focus:outline-none focus:ring-2 focus:ring-[#c5a059]/50 focus:border-[#c5a059] transition-all cursor-pointer"
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

      {!isLoading && !isError && (
        <p className="text-sm text-gray-500 dark:text-gray-400">
          {totalCount > 0 ? (
            <>إجمالي النتائج: <span className="font-semibold text-gray-800 dark:text-gray-200">{totalCount}</span> عرض</>
          ) : 'لا توجد عروض مطابقة.'}
        </p>
      )}

      {isLoading && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {Array.from({ length: 3 }).map((_, i) => (
            <div key={i} className="h-44 rounded-2xl bg-gray-100 dark:bg-gray-800/50 animate-pulse" />
          ))}
        </div>
      )}

      {isError && (
        <div className="text-center py-12 text-red-500">حدث خطأ أثناء تحميل العروض. يرجى المحاولة مرة أخرى.</div>
      )}

      {!isLoading && !isError && proposals.length === 0 && (
        <div className="text-center py-16 text-gray-400">
          <MdInbox className="text-5xl mx-auto mb-3 opacity-40" />
          <p className="text-sm">لا توجد عروض.</p>
        </div>
      )}

      {!isLoading && !isError && proposals.length > 0 && (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
          {proposals.map((proposal) => (
            <ProposalCard key={proposal.id} proposal={proposal} viewAs="lawyer" />
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-2 pt-2">
          <button
            onClick={() => setPage((p) => Math.max(1, p - 1))}
            disabled={page <= 1}
            className="px-4 py-2 rounded-xl border border-gray-200 dark:border-gray-700 text-sm font-medium text-gray-600 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            السابق
          </button>
          <span className="text-sm text-gray-500 min-w-[80px] text-center">{page} / {totalPages}</span>
          <button
            onClick={() => setPage((p) => p + 1)}
            disabled={!hasNextPage}
            className="px-4 py-2 rounded-xl border border-gray-200 dark:border-gray-700 text-sm font-medium text-gray-600 hover:bg-gray-50 disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
          >
            التالي
          </button>
        </div>
      )}
    </div>
  );
};
