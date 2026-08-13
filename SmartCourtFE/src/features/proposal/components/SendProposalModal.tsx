import { useState, useEffect } from 'react';
import { useMutation, useQuery } from '@tanstack/react-query';
import { MdClose, MdSend, MdWarning, MdCheckCircle, MdLock } from 'react-icons/md';
import toast from 'react-hot-toast';
import { ProposalApi } from '../api/proposalApi';

interface SendProposalModalProps {
  caseId: string;
  lawyerUserId: string;
  lawyerName: string;
  onClose: () => void;
  onSuccess: () => void;
}

export const SendProposalModal = ({
  caseId,
  lawyerUserId,
  lawyerName,
  onClose,
  onSuccess,
}: SendProposalModalProps) => {
  const [message, setMessage] = useState('');
  const MAX_LENGTH = 2000;

  // ── Availability check ──────────────────────────────────────────────────────
  const { data: availabilityRes, isLoading: isCheckingAvailability } = useQuery({
    queryKey: ['proposal-availability', caseId],
    queryFn: () => ProposalApi.checkAvailability(caseId),
    retry: false,
  });

  const availability = availabilityRes?.data;

  // ── Submit mutation ─────────────────────────────────────────────────────────
  const { mutate: sendProposal, isPending: isSending } = useMutation({
    mutationFn: (body: { legalCaseId: string; lawyerUserId: string; message: string }) =>
      ProposalApi.createProposal(body),
    onSuccess: () => {
      toast.success('تم إرسال العرض بنجاح!');
      onSuccess();
    },
    onError: (err: any) => {
      const serverMsg =
        err?.response?.data?.message ||
        err?.response?.data?.errors?.[0] ||
        'حدث خطأ أثناء إرسال العرض.';
      toast.error(serverMsg);
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!message.trim()) return;
    sendProposal({ legalCaseId: caseId, lawyerUserId, message: message.trim() });
  };

  // Close on Escape key
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
    };
    document.addEventListener('keydown', handleKeyDown);
    return () => document.removeEventListener('keydown', handleKeyDown);
  }, [onClose]);

  const canSend = availability?.canSendProposal ?? false;
  const isFormDisabled = !canSend || isSending || isCheckingAvailability;

  return (
    <div
      className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4"
      onClick={(e) => e.target === e.currentTarget && onClose()}
    >
      <div
        className="relative w-full max-w-lg bg-white dark:bg-[#1a1d23] rounded-3xl shadow-2xl border border-gray-200 dark:border-gray-700 overflow-hidden"
        role="dialog"
      >
        <div className="h-1 w-full bg-gradient-to-r from-[#c5a059] via-[#e0c080] to-[#c5a059]" />

        <div className="flex items-center justify-between px-6 pt-5 pb-4 border-b border-gray-100 dark:border-gray-800">
          <div>
            <h2 className="text-lg font-bold text-gray-900 dark:text-white">إرسال عرض للمحامي</h2>
            <p className="text-sm text-[#c5a059] font-medium mt-0.5">{lawyerName}</p>
          </div>
          <button
            onClick={onClose}
            className="w-9 h-9 flex items-center justify-center rounded-full text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
          >
            <MdClose className="text-xl" />
          </button>
        </div>

        <div className="px-6 py-5 space-y-4">
          {isCheckingAvailability && (
            <div className="flex items-center gap-2 text-sm text-gray-500 dark:text-gray-400">
              <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none">
                <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
              </svg>
              <span>جاري التحقق من توفر الفتحات...</span>
            </div>
          )}

          {!isCheckingAvailability && availability && (
            <div
              className={`flex items-start gap-3 rounded-xl px-4 py-3 text-sm ${
                canSend
                  ? 'bg-green-50 dark:bg-green-950/40 text-green-700 dark:text-green-300 border border-green-200 dark:border-green-800'
                  : 'bg-red-50 dark:bg-red-950/40 text-red-700 dark:text-red-300 border border-red-200 dark:border-red-800'
              }`}
            >
              {canSend ? (
                <MdCheckCircle className="text-lg mt-0.5 shrink-0" />
              ) : availability.availableProposalSlots === 0 ? (
                <MdLock className="text-lg mt-0.5 shrink-0" />
              ) : (
                <MdWarning className="text-lg mt-0.5 shrink-0" />
              )}
              <span>
                {canSend
                  ? `يمكنك إرسال عرض. الفتحات المتاحة: ${availability.availableProposalSlots} من ${availability.proposalLimit}`
                  : availability.availableProposalSlots === 0
                  ? 'تم الوصول إلى الحد الأقصى للعروض النشطة (5 عروض).'
                  : 'هذه القضية غير مؤهلة لإرسال عروض في الوقت الحالي.'}
              </span>
            </div>
          )}

          <form onSubmit={handleSubmit} id="send-proposal-form" className="space-y-2">
            <label className="block text-sm font-semibold text-gray-700 dark:text-gray-300">
              رسالتك للمحامي
              <span className="text-red-500 ml-1">*</span>
            </label>
            <textarea
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              disabled={isFormDisabled}
              placeholder="اكتب رسالتك هنا… صف قضيتك وما تحتاجه من المحامي."
              rows={6}
              maxLength={MAX_LENGTH}
              className="w-full resize-none rounded-xl border border-gray-200 dark:border-gray-700 bg-gray-50 dark:bg-[#121620] text-gray-900 dark:text-white placeholder:text-gray-400 px-4 py-3 text-sm focus:outline-none focus:ring-2 focus:ring-[#c5a059]/60 focus:border-[#c5a059] transition-all disabled:opacity-50 disabled:cursor-not-allowed"
              dir="rtl"
            />
            <div className="flex justify-between text-xs text-gray-400">
              <span>الحد الأقصى: {MAX_LENGTH} حرف</span>
              <span className={message.length > MAX_LENGTH * 0.9 ? 'text-red-500 font-medium' : ''}>
                {message.length} / {MAX_LENGTH}
              </span>
            </div>
          </form>
        </div>

        <div className="px-6 pb-6 flex flex-col-reverse sm:flex-row gap-3 justify-end">
          <button
            type="button"
            onClick={onClose}
            disabled={isSending}
            className="px-5 py-2.5 rounded-xl border border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 text-sm font-medium hover:bg-gray-50 dark:hover:bg-gray-800 transition-colors disabled:opacity-50"
          >
            إلغاء
          </button>
          <button
            type="submit"
            form="send-proposal-form"
            disabled={isFormDisabled || !message.trim()}
            className="flex items-center gap-2 px-6 py-2.5 rounded-xl bg-[#c5a059] hover:bg-[#b08d4a] text-white text-sm font-bold transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isSending ? (
              <>
                <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                </svg>
                <span>جاري الإرسال...</span>
              </>
            ) : (
              <>
                <MdSend className="text-base" />
                <span>إرسال العرض</span>
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
};
