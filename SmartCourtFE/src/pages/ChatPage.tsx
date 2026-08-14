import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { useQuery } from '@tanstack/react-query';
import { ChatApi } from '../features/chat/api/chatApi';
import { ConversationList } from '../features/chat/components/ConversationList';
import { ChatThread } from '../features/chat/components/ChatThread';
import type { ChatConversationListItemDto } from '../features/chat/types/chat.types';
import { MdChatBubbleOutline } from 'react-icons/md';

/**
 * ChatPage
 * Route: /dashboard/chat  (shows all conversations, no thread selected)
 *        /dashboard/chat/:conversationId  (opens a specific thread)
 *
 * The conversationId is optional — when navigated from a proposal action
 * the URL already carries the ID, so we auto-open the thread.
 */
export const ChatPage = () => {
  const { conversationId: routeConvId } = useParams<{ conversationId?: string }>();
  const navigate = useNavigate();

  const [search, setSearch] = useState('');
  const [activeConv, setActiveConv] = useState<ChatConversationListItemDto | null>(null);

  // ── Fetch conversation list ─────────────────────────────────────────────────
  const { data, isLoading } = useQuery({
    queryKey: ['chat-conversations', search],
    queryFn: () => ChatApi.listConversations({ search: search || undefined, pageSize: 50 }),
    refetchInterval: 30_000, // poll every 30 s as SignalR only covers open thread
  });

  const conversations: ChatConversationListItemDto[] = data?.data?.items ?? [];

  // ── Auto-open conversation from URL param ───────────────────────────────────
  useEffect(() => {
    if (routeConvId && conversations.length > 0) {
      const found = conversations.find((c) => c.id === routeConvId);
      if (found) setActiveConv(found);
    }
  }, [routeConvId, conversations]);

  // ── Navigate on selection ───────────────────────────────────────────────────
  const handleSelect = (conv: ChatConversationListItemDto) => {
    setActiveConv(conv);
    navigate(`/dashboard/chat/${conv.id}`, { replace: true });
  };

  const handleBack = () => {
    setActiveConv(null);
    navigate('/dashboard/chat', { replace: true });
  };

  const currentId = activeConv?.id ?? routeConvId;

  return (
    <main className="flex-1 w-full p-4 sm:p-6 overflow-hidden flex flex-col min-w-0">
      <div className="w-full flex-1 flex gap-4 min-h-0 h-full">

        {/* Left / Conversation list — hidden on mobile when a thread is open */}
        <div className={`${currentId ? 'hidden lg:flex' : 'flex'} flex-col w-full lg:w-80 shrink-0`}>
          <ConversationList
            conversations={conversations}
            activeId={currentId ?? null}
            search={search}
            onSearchChange={(val) => setSearch(val)}
            onSelect={handleSelect}
            isLoading={isLoading}
          />
        </div>

        {/* Right / Active thread — hidden on mobile when no thread is selected */}
        {currentId ? (
          <div className="flex flex-col flex-1 min-w-0">
            <ChatThread
              conversationId={currentId}
              onBack={handleBack}
            />
          </div>
        ) : (
          /* Empty state on desktop */
          <div className="hidden lg:flex flex-1 items-center justify-center bg-white dark:bg-[#1a1d23] rounded-2xl border border-gray-200 dark:border-gray-800 shadow-sm">
            <div className="text-center text-gray-400">
              <MdChatBubbleOutline className="text-6xl mx-auto mb-3 opacity-30" />
              <p className="text-sm">اختر محادثة لعرضها</p>
            </div>
          </div>
        )}

      </div>
    </main>
  );
};
