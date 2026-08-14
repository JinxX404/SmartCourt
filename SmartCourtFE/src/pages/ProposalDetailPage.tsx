import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { Loader } from '../components/Loader';
import { ProposalApi } from '../features/proposal/api/proposalApi';
import { ProposalDetail } from '../features/proposal/components/ProposalDetail';
import { MdArrowBack } from 'react-icons/md';

export const ProposalDetailPage = () => {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  const { data, isLoading, error } = useQuery({
    queryKey: ['proposal', id],
    queryFn: () => ProposalApi.getProposal(id!),
    enabled: !!id,
  });

  if (isLoading) return <Loader />;

  if (error || !data?.data) {
    return (
      <div className="min-h-screen flex flex-col items-center justify-center bg-gray-50 dark:bg-[#0d1017] gap-4">
        <h2 className="text-xl font-bold text-gray-800 dark:text-white">
          تعذّر تحميل تفاصيل العرض
        </h2>
        <p className="text-sm text-gray-500 dark:text-gray-400">
          قد لا تكون مشاركاً في هذا العرض أو أن العرض غير موجود.
        </p>
        <button
          onClick={() => navigate(-1)}
          className="flex items-center gap-2 px-5 py-2.5 bg-[#c5a059] text-white rounded-xl hover:bg-[#b08d4a] transition-colors text-sm font-medium"
        >
          <MdArrowBack />
          رجوع
        </button>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-[#f4f5f8] dark:bg-[#0d1017] p-4 sm:p-8">
      <div className="max-w-4xl mx-auto mb-6 flex items-center gap-4">
        <button
          onClick={() => navigate(-1)}
          className="w-10 h-10 flex items-center justify-center rounded-full bg-white dark:bg-[#1a1d23] border border-gray-200 dark:border-gray-800 text-gray-600 dark:text-gray-400 hover:text-[#c5a059] transition-colors"
        >
          <MdArrowBack className="text-xl" />
        </button>
        <h1 className="text-2xl font-bold text-gray-900 dark:text-white">تفاصيل العرض</h1>
      </div>
      <ProposalDetail proposal={data.data} />
    </div>
  );
};
