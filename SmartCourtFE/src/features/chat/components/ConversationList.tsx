import { MdSearch } from 'react-icons/md';
import type { ChatConversationListItemDto } from '../types/chat.types';
import { ConversationItem } from './ConversationItem';

interface ConversationListProps {
  conversations: ChatConversationListItemDto[];
  activeId: string | null;
  search: string;
  onSearchChange: (val: string) => void;
  onSelect: (conversation: ChatConversationListItemDto) => void;
  isLoading: boolean;
}

export const ConversationList = ({
  conversations,
  activeId,
  search,
  onSearchChange,
  onSelect,
  isLoading,
}: ConversationListProps) => {
  return (
    <aside className="w-full lg:w-80 shrink-0 bg-white dark:bg-[#1a1d23] border border-gray-200 dark:border-gray-800 rounded-2xl flex flex-col overflow-hidden shadow-sm">
      {/* Header */}
      <div className="p-4 border-b border-gray-100 dark:border-gray-800 space-y-3">
        <h2 className="text-lg font-bold text-gray-900 dark:text-white">المحادثات</h2>
        <div className="relative">
          <MdSearch className="absolute right-3 top-1/2 -translate-y-1/2 text-gray-400 text-lg pointer-events-none" />
          <input
            type="text"
            value={search}
            onChange={(e) => onSearchChange(e.target.value)}
            placeholder="ابحث عن محامٍ أو قضية..."
            className="w-full pr-10 pl-4 py-2.5 rounded-xl bg-gray-50 dark:bg-[#121620] border border-gray-200 dark:border-gray-700 text-sm text-gray-900 dark:text-white placeholder:text-gray-400 focus:outline-none focus:ring-2 focus:ring-[#c5a059]/50 focus:border-[#c5a059] transition-all"
            dir="rtl"
          />
        </div>
      </div>

      {/* List */}
      <div className="flex-1 overflow-y-auto">
        {isLoading && (
          <div className="space-y-2 p-3">
            {Array.from({ length: 4 }).map((_, i) => (
              <div key={i} className="h-16 rounded-xl bg-gray-100 dark:bg-gray-800 animate-pulse" />
            ))}
          </div>
        )}

        {!isLoading && conversations.length === 0 && (
          <div className="p-6 text-center text-sm text-gray-400">
            لا توجد محادثات بعد.
          </div>
        )}

        {!isLoading &&
          conversations.map((conv) => (
            <ConversationItem
              key={conv.id}
              conversation={conv}
              isActive={conv.id === activeId}
              onClick={() => onSelect(conv)}
            />
          ))}
      </div>
    </aside>
  );
};
