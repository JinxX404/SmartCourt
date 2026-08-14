import { MdPerson } from 'react-icons/md';
import type { ChatConversationListItemDto } from '../types/chat.types';
import { useAuthStore } from '../../auth/store/useAuthStore';

interface ConversationItemProps {
  conversation: ChatConversationListItemDto;
  isActive: boolean;
  onClick: () => void;
}

function formatRelativeTime(isoDate: string | null): string {
  if (!isoDate) return '';
  const now = Date.now();
  const diff = now - new Date(isoDate).getTime();
  const mins = Math.floor(diff / 60000);
  if (mins < 1) return 'الآن';
  if (mins < 60) return `منذ ${mins} د`;
  const hours = Math.floor(mins / 60);
  if (hours < 24) return `منذ ${hours} س`;
  const days = Math.floor(hours / 24);
  if (days === 1) return 'أمس';
  return `منذ ${days} يوم`;
}

export const ConversationItem = ({ conversation, isActive, onClick }: ConversationItemProps) => {
  const { user } = useAuthStore();

  // Determine the other party's name based on current user role
  const otherParty =
    user?.role === 'Lawyer' ? conversation.client : conversation.lawyer;

  const lastMsgPreview = conversation.lastMessage?.content ?? conversation.caseTitle;
  const relTime = formatRelativeTime(conversation.lastMessageAt ?? conversation.updatedAt);
  const isClosed = conversation.status === 'Closed';

  return (
    <button
      onClick={onClick}
      className={`w-full text-right px-4 py-3 flex items-start gap-3 border-b border-gray-100 dark:border-gray-800 transition-colors ${
        isActive
          ? 'bg-[#c5a059]/10 border-r-4 border-r-[#c5a059]'
          : 'hover:bg-gray-50 dark:hover:bg-gray-800/50'
      }`}
    >
      {/* Avatar placeholder */}
      <div className="relative shrink-0 mt-0.5">
        <div className="w-11 h-11 rounded-full bg-gray-200 dark:bg-gray-700 flex items-center justify-center text-gray-500">
          <MdPerson className="text-2xl" />
        </div>
        {!isClosed && (
          <span className="absolute bottom-0 right-0 w-3 h-3 bg-green-500 border-2 border-white dark:border-[#1a1d23] rounded-full" />
        )}
      </div>

      {/* Content */}
      <div className="flex-1 min-w-0">
        <div className="flex items-baseline justify-between gap-2">
          <h3 className="text-sm font-bold text-gray-900 dark:text-white truncate">{otherParty.name}</h3>
          <span className="text-[11px] text-gray-400 shrink-0">{relTime}</span>
        </div>
        <p className="text-xs text-gray-400 dark:text-gray-500 mt-0.5 truncate">{lastMsgPreview}</p>
        {isClosed && (
          <span className="text-[10px] text-orange-500 mt-0.5 block">محادثة مغلقة (للقراءة فقط)</span>
        )}
      </div>
    </button>
  );
};
