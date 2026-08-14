import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useMutation, useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import {
  MdCheck,
  MdClose,
  MdBlock,
  MdSwapHoriz,
  MdDescription,
  MdChat,
  MdHistory,
} from 'react-icons/md';
import { ProposalApi } from '../api/proposalApi';
import type { ProposalDetailDto, ProposalAction } from '../types/proposal.types';

interface ActionModalProps {
  title: string;
  action: 'Reject' | 'Cancel' | 'TerminateProposal';
  isOpen: boolean;
  onClose: () => void;
  onSubmit: (reason: string) => void;
  isPending: boolean;
}

const ActionModal = ({ title, isOpen, onClose, onSubmit, isPending }: ActionModalProps) => {
  const [reason, setReason] = useState('');
  if (!isOpen) return null;

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!reason.trim()) return;
    onSubmit(reason.trim());
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4" onClick={(e) => e.target === e.currentTarget && onClose()}>
      <div className="w-full max-w-md bg-white dark:bg-[#1a1d23] rounded-3xl p-6 border border-gray-200 dark:border-gray-700 shadow-xl">
        <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-4">{title}</h3>
        <form onSubmit={handleSubmit} className="space-y-4">
          <textarea
            value={reason}
            onChange={(e) => setReason(e.target.value)}
            disabled={isPending}
            maxLength={1000}
            placeholder="السبب..."
            rows={4}
            className="w-full resize-none rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-[#121620] text-gray-900 dark:text-white px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-[#c5a059]"
          />
          <div className="flex gap-3 justify-end">
            <button type="button" onClick={onClose} disabled={isPending} className="px-4 py-2 rounded-xl text-sm font-medium text-gray-600 hover:bg-gray-100 disabled:opacity-50">إلغاء</button>
            <button type="submit" disabled={isPending || !reason.trim()} className="px-5 py-2 rounded-xl bg-[#c5a059] hover:bg-[#b08d4a] text-white text-sm font-bold disabled:opacity-50">تأكيد</button>
          </div>
        </form>
      </div>
    </div>
  );
};

interface ProposalDetailProps {
  proposal: ProposalDetailDto;
}

export const ProposalDetail = ({ proposal }: ProposalDetailProps) => {
  const navigate = useNavigate();
  const queryClient = useQueryClient();
  const [modalState, setModalState] = useState<{ isOpen: boolean; action: 'Reject' | 'Cancel' | 'TerminateProposal' | null; title: string }>({ isOpen: false, action: null, title: '' });

  const invalidateQueries = () => {
    queryClient.invalidateQueries({ queryKey: ['proposal', proposal.id] });
    queryClient.invalidateQueries({ queryKey: ['lawyer-proposals'] });
    queryClient.invalidateQueries({ queryKey: ['case-proposals'] });
  };

  const acceptMutation = useMutation({
    mutationFn: () => ProposalApi.acceptProposal(proposal.id),
    onSuccess: (res) => {
      toast.success('تم قبول العرض');
      invalidateQueries();
      if (res.data.conversationId) {
        navigate(`/dashboard/chat/${res.data.conversationId}`);
      }
    },
    onError: (err: any) => toast.error(err?.response?.data?.message || 'حدث خطأ'),
  });

  const rejectMutation = useMutation({
    mutationFn: (reason: string) => ProposalApi.rejectProposal(proposal.id, { reason }),
    onSuccess: () => { toast.success('تم الرفض'); invalidateQueries(); setModalState({ isOpen: false, action: null, title: '' }); },
    onError: (err: any) => toast.error(err?.response?.data?.message || 'حدث خطأ'),
  });

  const cancelMutation = useMutation({
    mutationFn: (reason: string) => ProposalApi.cancelProposal(proposal.id, { reason }),
    onSuccess: () => { toast.success('تم الإلغاء'); invalidateQueries(); setModalState({ isOpen: false, action: null, title: '' }); },
    onError: (err: any) => toast.error(err?.response?.data?.message || 'حدث خطأ'),
  });

  const terminateMutation = useMutation({
    mutationFn: (reason: string) => ProposalApi.terminateProposal(proposal.id, { reason }),
    onSuccess: () => { toast.success('تم الإنهاء'); invalidateQueries(); setModalState({ isOpen: false, action: null, title: '' }); },
    onError: (err: any) => toast.error(err?.response?.data?.message || 'حدث خطأ'),
  });

  const handleAction = (action: ProposalAction) => {
    switch (action) {
      case 'Accept':
        acceptMutation.mutate();
        break;
      case 'Reject':
        setModalState({ isOpen: true, action: 'Reject', title: 'رفض العرض' });
        break;
      case 'Cancel':
        setModalState({ isOpen: true, action: 'Cancel', title: 'إلغاء العرض' });
        break;
      case 'TerminateProposal':
        setModalState({ isOpen: true, action: 'TerminateProposal', title: 'إنهاء التفاوض' });
        break;
      case 'OpenChat':
      case 'ViewChatHistory':
        if (proposal.conversationId) navigate(`/dashboard/chat/${proposal.conversationId}`);
        break;
      case 'CreateContract':
      case 'ViewContract':
        // TODO: Implemented in contract feature
        toast('ميزة العقود سيتم تفعيلها قريباً', { icon: '🚧' });
        break;
    }
  };

  const renderActionButton = (action: ProposalAction) => {
    const isPending = acceptMutation.isPending || rejectMutation.isPending || cancelMutation.isPending || terminateMutation.isPending;

    switch (action) {
      case 'Accept':
        return (
          <button key={action} onClick={() => handleAction(action)} disabled={isPending} className="flex-1 sm:flex-none flex items-center justify-center gap-2 px-6 py-2.5 rounded-xl bg-green-600 hover:bg-green-700 text-white font-bold transition-colors disabled:opacity-50">
            <MdCheck /> قبول
          </button>
        );
      case 'Reject':
        return (
          <button key={action} onClick={() => handleAction(action)} disabled={isPending} className="flex-1 sm:flex-none flex items-center justify-center gap-2 px-6 py-2.5 rounded-xl bg-red-100 hover:bg-red-200 text-red-700 font-bold transition-colors disabled:opacity-50">
            <MdClose /> رفض
          </button>
        );
      case 'Cancel':
        return (
          <button key={action} onClick={() => handleAction(action)} disabled={isPending} className="flex-1 sm:flex-none flex items-center justify-center gap-2 px-6 py-2.5 rounded-xl border border-gray-300 hover:bg-gray-100 text-gray-700 font-bold transition-colors disabled:opacity-50">
            <MdBlock /> إلغاء
          </button>
        );
      case 'TerminateProposal':
        return (
          <button key={action} onClick={() => handleAction(action)} disabled={isPending} className="flex-1 sm:flex-none flex items-center justify-center gap-2 px-6 py-2.5 rounded-xl bg-purple-100 hover:bg-purple-200 text-purple-700 font-bold transition-colors disabled:opacity-50">
            <MdSwapHoriz /> إنهاء التفاوض
          </button>
        );
      case 'OpenChat':
        return (
          <button key={action} onClick={() => handleAction(action)} className="flex-1 sm:flex-none flex items-center justify-center gap-2 px-6 py-2.5 rounded-xl bg-[#c5a059] hover:bg-[#b08d4a] text-white font-bold transition-colors">
            <MdChat /> فتح المحادثة
          </button>
        );
      case 'ViewChatHistory':
        return (
          <button key={action} onClick={() => handleAction(action)} className="flex-1 sm:flex-none flex items-center justify-center gap-2 px-6 py-2.5 rounded-xl border border-gray-300 hover:bg-gray-100 text-gray-700 font-bold transition-colors">
            <MdHistory /> سجل المحادثة
          </button>
        );
      case 'CreateContract':
        return (
          <button key={action} onClick={() => handleAction(action)} className="flex-1 sm:flex-none flex items-center justify-center gap-2 px-6 py-2.5 rounded-xl bg-blue-600 hover:bg-blue-700 text-white font-bold transition-colors">
            <MdDescription /> إنشاء العقد
          </button>
        );
      case 'ViewContract':
        return (
          <button key={action} onClick={() => handleAction(action)} className="flex-1 sm:flex-none flex items-center justify-center gap-2 px-6 py-2.5 rounded-xl border border-blue-300 hover:bg-blue-50 text-blue-700 font-bold transition-colors">
            <MdDescription /> عرض العقد
          </button>
        );
      default:
        return null;
    }
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      <div className="bg-white dark:bg-[#1a1d23] rounded-3xl p-6 md:p-8 border border-gray-200/80 dark:border-gray-800 shadow-sm space-y-8">
        <div>
          <h2 className="text-xl font-bold text-gray-900 dark:text-white mb-2">{proposal.caseTitle}</h2>
          <div className="flex flex-wrap gap-x-6 gap-y-2 text-sm text-gray-500 dark:text-gray-400 mt-4 border-b border-gray-100 dark:border-gray-800 pb-4">
            <p>الموكل: <span className="font-semibold text-gray-900 dark:text-gray-200">{proposal.clientName}</span></p>
            <p>المحامي: <span className="font-semibold text-gray-900 dark:text-gray-200">{proposal.lawyerName}</span></p>
            <p>حالة العرض: <span className="font-semibold text-[#c5a059]">{proposal.status}</span></p>
          </div>
        </div>

        <div>
          <h3 className="font-bold text-gray-900 dark:text-white mb-3">تفاصيل العرض</h3>
          <div className="p-5 rounded-2xl bg-gray-50 dark:bg-[#121620] border border-gray-100 dark:border-gray-800 text-gray-700 dark:text-gray-300 text-sm leading-relaxed whitespace-pre-wrap">
            {proposal.message}
          </div>
        </div>

        {proposal.decisionReason && (
          <div>
            <h3 className="font-bold text-gray-900 dark:text-white mb-3">سبب القرار</h3>
            <div className="p-5 rounded-2xl bg-gray-50 dark:bg-[#121620] border border-gray-100 dark:border-gray-800 text-gray-700 dark:text-gray-300 text-sm leading-relaxed whitespace-pre-wrap">
              {proposal.decisionReason}
            </div>
          </div>
        )}

        {proposal.permittedActions.length > 0 && (
          <div className="flex flex-wrap gap-3 pt-4 border-t border-gray-100 dark:border-gray-800">
            {proposal.permittedActions.map(renderActionButton)}
          </div>
        )}
      </div>

      <ActionModal
        isOpen={modalState.isOpen}
        title={modalState.title}
        action={modalState.action!}
        isPending={rejectMutation.isPending || cancelMutation.isPending || terminateMutation.isPending}
        onClose={() => setModalState({ isOpen: false, action: null, title: '' })}
        onSubmit={(reason) => {
          if (modalState.action === 'Reject') rejectMutation.mutate(reason);
          else if (modalState.action === 'Cancel') cancelMutation.mutate(reason);
          else if (modalState.action === 'TerminateProposal') terminateMutation.mutate(reason);
        }}
      />
    </div>
  );
};
