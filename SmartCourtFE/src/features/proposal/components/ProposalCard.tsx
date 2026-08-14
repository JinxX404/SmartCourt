import { useNavigate } from 'react-router-dom';
import {
  MdAccessTime,
  MdCheckCircle,
  MdCancel,
  MdHourglassEmpty,
  MdBlock,
  MdSwapHoriz,
  MdArrowForward,
} from 'react-icons/md';
import type { ProposalListItem, ProposalStatus } from '../types/proposal.types';

const STATUS_CONFIG: Record<ProposalStatus, { label: string; icon: React.ReactNode; className: string }> = {
  Pending: {
    label: 'قيد الانتظار',
    icon: <MdHourglassEmpty />,
    className: 'bg-amber-100 text-amber-700 dark:bg-amber-950/50 dark:text-amber-300 border-amber-200 dark:border-amber-800',
  },
  Accepted: {
    label: 'مقبول',
    icon: <MdCheckCircle />,
    className: 'bg-green-100 text-green-700 dark:bg-green-950/50 dark:text-green-300 border-green-200 dark:border-green-800',
  },
  Rejected: {
    label: 'مرفوض',
    icon: <MdCancel />,
    className: 'bg-red-100 text-red-700 dark:bg-red-950/50 dark:text-red-300 border-red-200 dark:border-red-800',
  },
  Cancelled: {
    label: 'ملغي',
    icon: <MdBlock />,
    className: 'bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400 border-gray-200 dark:border-gray-700',
  },
  Expired: {
    label: 'منتهي الصلاحية',
    icon: <MdAccessTime />,
    className: 'bg-orange-100 text-orange-700 dark:bg-orange-950/50 dark:text-orange-300 border-orange-200 dark:border-orange-800',
  },
  Terminated: {
    label: 'منهى',
    icon: <MdSwapHoriz />,
    className: 'bg-purple-100 text-purple-700 dark:bg-purple-950/50 dark:text-purple-300 border-purple-200 dark:border-purple-800',
  },
  Superseded: {
    label: 'مُستبدَل',
    icon: <MdSwapHoriz />,
    className: 'bg-slate-100 text-slate-600 dark:bg-slate-800 dark:text-slate-400 border-slate-200 dark:border-slate-700',
  },
};

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString('ar-EG', {
    year: 'numeric',
    month: 'short',
    day: 'numeric',
  });
}

function getExpiryCountdown(expiresAt: string | null): string | null {
  if (!expiresAt) return null;
  const diffMs = new Date(expiresAt).getTime() - Date.now();
  if (diffMs <= 0) return 'انتهت المدة';
  const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
  if (diffHours < 24) return `ينتهي خلال ${diffHours} ساعة`;
  const diffDays = Math.floor(diffHours / 24);
  return `ينتهي خلال ${diffDays} يوم`;
}

interface ProposalCardProps {
  proposal: ProposalListItem;
  viewAs: 'client' | 'lawyer';
}

export const ProposalCard = ({ proposal, viewAs }: ProposalCardProps) => {
  const navigate = useNavigate();
  const statusConfig = STATUS_CONFIG[proposal.status];
  const countdown = proposal.status === 'Pending' ? getExpiryCountdown(proposal.expiresAt) : null;

  const otherPartyLabel = viewAs === 'client' ? 'المحامي' : 'الموكل';
  const otherPartyName = viewAs === 'client' ? proposal.lawyerName : proposal.clientName;

  return (
    <div className="group bg-white dark:bg-[#1a1d23] rounded-2xl border border-gray-200 dark:border-gray-800 p-5 flex flex-col gap-4 shadow-sm hover:shadow-md hover:border-[#c5a059]/40 transition-all duration-200">
      <div className="flex items-start justify-between gap-3">
        <div className="min-w-0">
          <h3 className="font-bold text-gray-900 dark:text-white truncate text-sm">{proposal.caseTitle}</h3>
          <p className="text-xs text-gray-500 dark:text-gray-400 mt-0.5">
            {otherPartyLabel}: <span className="text-gray-700 dark:text-gray-300 font-medium">{otherPartyName}</span>
          </p>
          {proposal.isAssignedLawyer && (
            <span className="inline-block mt-1 text-[10px] bg-blue-100 text-blue-700 dark:bg-blue-900/40 dark:text-blue-300 px-2 py-0.5 rounded border border-blue-200 dark:border-blue-800">
              المحامي الموكل بالقضية
            </span>
          )}
        </div>
        <span className={`flex items-center gap-1.5 px-3 py-1 rounded-full text-[11px] font-semibold shrink-0 border ${statusConfig.className}`}>
          {statusConfig.icon}
          {statusConfig.label}
        </span>
      </div>

      {countdown && (
        <div className="flex items-center gap-1.5 text-xs font-medium text-amber-600 dark:text-amber-400 bg-amber-50 dark:bg-amber-950/30 rounded-lg px-3 py-1.5">
          <MdAccessTime className="shrink-0" />
          {countdown}
        </div>
      )}

      <div className="flex items-center justify-between text-[11px] text-gray-400 border-t border-gray-100 dark:border-gray-800 pt-3">
        <span>أُرسل: {formatDate(proposal.createdAt)}</span>
        {proposal.closedAt && <span>أُغلق: {formatDate(proposal.closedAt)}</span>}
      </div>

      <button
        onClick={() => navigate(`/dashboard/proposals/${proposal.id}`)}
        className="w-full flex items-center justify-center gap-1.5 py-2 rounded-xl border border-gray-200 dark:border-gray-700 hover:bg-gray-50 dark:hover:bg-gray-800 text-gray-700 dark:text-gray-300 text-sm font-medium transition-colors mt-2"
      >
        <span>عرض التفاصيل</span>
        <MdArrowForward className="text-base" />
      </button>
    </div>
  );
};
