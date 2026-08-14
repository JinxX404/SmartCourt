import { useEffect, useRef, useState, useCallback } from 'react';
import { useQueryClient } from '@tanstack/react-query';
import toast from 'react-hot-toast';
import { MdArrowBack, MdMoreVert, MdPerson, MdLock } from 'react-icons/md';
import { ChatApi } from '../api/chatApi';
import { chatHub } from '../api/chatHub';
import { ChatMessage } from './ChatMessage';
import { MessageInput } from './MessageInput';
import type { ChatConversationDetailDto, ChatMessageDto } from '../types/chat.types';
import { useAuthStore } from '../../auth/store/useAuthStore';

interface ChatThreadProps {
  conversationId: string;
  onBack?: () => void;
}

function formatDateLabel(isoDate: string): string {
  const date = new Date(isoDate);
  const today = new Date();
  const yesterday = new Date(today);
  yesterday.setDate(today.getDate() - 1);
  if (date.toDateString() === today.toDateString()) return 'اليوم';
  if (date.toDateString() === yesterday.toDateString()) return 'أمس';
  return date.toLocaleDateString('ar-EG', { year: 'numeric', month: 'long', day: 'numeric' });
}

export const ChatThread = ({ conversationId, onBack }: ChatThreadProps) => {
  const { user } = useAuthStore();
  const queryClient = useQueryClient();

  // ── State ────────────────────────────────────────────────────────────────────
  const [messages, setMessages] = useState<ChatMessageDto[]>([]);
  const [conversation, setConversation] = useState<ChatConversationDetailDto | undefined>();
  const [isLoadingInitial, setIsLoadingInitial] = useState(true);
  const [isLoadingOlder, setIsLoadingOlder] = useState(false);
  const [isSending, setIsSending] = useState(false);
  const [hubConnected, setHubConnected] = useState(false);
  const [currentPage, setCurrentPage] = useState(1);
  const [hasMorePages, setHasMorePages] = useState(false);

  // ── Refs ─────────────────────────────────────────────────────────────────────
  const messagesBottomRef = useRef<HTMLDivElement>(null);
  const messagesContainerRef = useRef<HTMLDivElement>(null);
  const seenIds = useRef<Set<string>>(new Set());

  /** Append a message only if not already in state (dedup). */
  const addMessage = useCallback((msg: ChatMessageDto) => {
    // Backend may broadcast `isMine` relative to sender, recalculate relative to current user
    const fixedMsg = {
      ...msg,
      isMine: msg.senderUserId === user?.id
    };
    if (seenIds.current.has(fixedMsg.id)) return;
    seenIds.current.add(fixedMsg.id);
    setMessages((prev) => [...prev, fixedMsg]);
  }, [user?.id]);

  // ── Initial load: conversation + first page of messages ─────────────────────
  useEffect(() => {
    let cancelled = false;

    const load = async () => {
      setIsLoadingInitial(true);
      seenIds.current.clear();
      setMessages([]);
      setCurrentPage(1);
      setHasMorePages(false);

      try {
        const [convRes, msgRes] = await Promise.all([
          ChatApi.getConversation(conversationId),
          // page=1 = newest messages; we reverse them to show oldest-first
          ChatApi.getMessages(conversationId, 1, 50),
        ]);

        if (cancelled) return;

        if (convRes.data) setConversation(convRes.data);

        if (msgRes.data?.items) {
          const ordered = [...msgRes.data.items]; // API already returns ordered oldest-first for the requested page
          ordered.forEach((m) => {
            m.isMine = m.senderUserId === user?.id;
            seenIds.current.add(m.id);
          });
          setMessages(ordered);
          setHasMorePages(msgRes.data.hasNextPage);
          setCurrentPage(1);
        }
      } catch {
        // ignore — user will see empty state
      } finally {
        if (!cancelled) setIsLoadingInitial(false);
      }
    };

    load();
    return () => { cancelled = true; };
  }, [conversationId]);

  // ── Auto-scroll to bottom on initial load and new messages ──────────────────
  const isFirstScroll = useRef(true);
  useEffect(() => {
    if (isLoadingInitial) return;
    if (isFirstScroll.current) {
      // Instant jump on first render
      messagesBottomRef.current?.scrollIntoView({ behavior: 'instant' as ScrollBehavior });
      isFirstScroll.current = false;
    } else {
      // Smooth scroll only when a new message arrives (not on older-page prepend)
      if (!isLoadingOlder) {
        messagesBottomRef.current?.scrollIntoView({ behavior: 'smooth' });
      }
    }
  }, [messages, isLoadingInitial, isLoadingOlder]);

  // ── Load older messages (scroll-up pagination) ───────────────────────────────
  const loadOlderMessages = useCallback(async () => {
    if (isLoadingOlder || !hasMorePages) return;

    const nextPage = currentPage + 1;
    setIsLoadingOlder(true);

    // Remember scroll position before prepending
    const container = messagesContainerRef.current;
    const prevScrollHeight = container?.scrollHeight ?? 0;

    try {
      const res = await ChatApi.getMessages(conversationId, nextPage, 50);
      if (res.data?.items) {
        const older = [...res.data.items]; // API already returns ordered oldest-first
        const newMsgs = older.filter((m) => !seenIds.current.has(m.id)).map(m => ({
          ...m,
          isMine: m.senderUserId === user?.id
        }));
        newMsgs.forEach((m) => seenIds.current.add(m.id));

        setMessages((prev) => [...newMsgs, ...prev]);
        setHasMorePages(res.data.hasNextPage);
        setCurrentPage(nextPage);

        // Restore scroll position so the user stays at the same message
        requestAnimationFrame(() => {
          if (container) {
            container.scrollTop = container.scrollHeight - prevScrollHeight;
          }
        });
      }
    } catch {
      // ignore
    } finally {
      setIsLoadingOlder(false);
    }
  }, [conversationId, currentPage, hasMorePages, isLoadingOlder]);

  // ── Scroll listener for load-older trigger ───────────────────────────────────
  useEffect(() => {
    const container = messagesContainerRef.current;
    if (!container) return;

    const handleScroll = () => {
      if (container.scrollTop < 60 && hasMorePages && !isLoadingOlder) {
        loadOlderMessages();
      }
    };

    container.addEventListener('scroll', handleScroll, { passive: true });
    return () => container.removeEventListener('scroll', handleScroll);
  }, [hasMorePages, isLoadingOlder, loadOlderMessages]);

  // ── SignalR connection ───────────────────────────────────────────────────────
  useEffect(() => {
    let unsubscribe: (() => void) | null = null;

    (async () => {
      try {
        await chatHub.start();
        await chatHub.joinConversation(conversationId);
        setHubConnected(true);

        unsubscribe = chatHub.onMessage((msg) => {
          const incomingId = msg.conversationId || (msg as any).ConversationId;
          if (incomingId && incomingId.toLowerCase() === conversationId.toLowerCase()) {
            addMessage(msg);
            // Refresh conversation list preview
            queryClient.invalidateQueries({ queryKey: ['chat-conversations'] });
          }
        });
      } catch {
        setHubConnected(false);
      }
    })();

    return () => {
      unsubscribe?.();
      chatHub.leaveConversation(conversationId).catch(() => {});
    };
  }, [conversationId, addMessage, queryClient]);

  // ── Send message ─────────────────────────────────────────────────────────────
  const handleSend = useCallback(
    async (content: string, files?: File[]) => {
      if ((!content.trim() && (!files || files.length === 0)) || isSending) return;
      setIsSending(true);

      try {
        let newMsg: ChatMessageDto;

        if (files && files.length > 0) {
          // Documents are uploaded with REST, not through SignalR
          const res = await ChatApi.sendAttachments(conversationId, content, files);
          newMsg = res.data;
        } else {
          if (hubConnected) {
            newMsg = await chatHub.sendMessage(conversationId, { content });
          } else {
            const res = await ChatApi.sendMessage(conversationId, { content });
            newMsg = res.data;
          }
        }

        addMessage(newMsg);
        queryClient.invalidateQueries({ queryKey: ['chat-conversations'] });
      } catch (err: any) {
        const serverMsg = err?.response?.data?.message || 'حدث خطأ أثناء إرسال الرسالة أو المرفقات.';
        toast.error(serverMsg);
      } finally {
        setIsSending(false);
      }
    },
    [conversationId, hubConnected, isSending, addMessage, queryClient],
  );

  // ── Derived ──────────────────────────────────────────────────────────────────
  const isClosed = conversation?.status === 'Closed';
  const otherParty =
    user?.role === 'Lawyer' ? conversation?.client : conversation?.lawyer;

  // ── Render ───────────────────────────────────────────────────────────────────
  return (
    <section className="flex-1 bg-white dark:bg-[#1a1d23] border border-gray-200 dark:border-gray-800 rounded-2xl flex flex-col overflow-hidden shadow-sm">

      {/* Thread Header */}
      <div className="px-4 py-3 border-b border-gray-100 dark:border-gray-800 bg-white dark:bg-[#1a1d23] flex items-center justify-between gap-3 shrink-0">
        <div className="flex items-center gap-3">
          {onBack && (
            <button
              onClick={onBack}
              className="lg:hidden w-8 h-8 flex items-center justify-center rounded-full text-gray-500 hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors"
            >
              <MdArrowBack className="text-xl" />
            </button>
          )}
          <div className="w-10 h-10 rounded-full bg-gray-200 dark:bg-gray-700 flex items-center justify-center text-gray-500 shrink-0">
            <MdPerson className="text-2xl" />
          </div>
          <div>
            {isLoadingInitial ? (
              <div className="h-4 w-32 bg-gray-200 dark:bg-gray-700 rounded animate-pulse" />
            ) : (
              <>
                <h2 className="text-sm font-bold text-gray-900 dark:text-white">
                  {otherParty?.name ?? '...'}
                </h2>
                <p className="text-xs text-[#c5a059]">{conversation?.caseTitle}</p>
              </>
            )}
          </div>
        </div>

        <div className="flex items-center gap-2">
          {/* Live indicator */}
          {hubConnected && !isClosed && (
            <span className="flex items-center gap-1.5 text-[11px] text-green-600 dark:text-green-400">
              <span className="w-2 h-2 rounded-full bg-green-500 animate-pulse inline-block" />
              مباشر
            </span>
          )}
          {isClosed && (
            <span className="flex items-center gap-1 text-[11px] bg-orange-100 dark:bg-orange-900/30 text-orange-600 dark:text-orange-400 px-2 py-1 rounded-full">
              <MdLock className="text-sm" />
              مغلقة
            </span>
          )}
          <button className="p-1.5 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 rounded-full hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors">
            <MdMoreVert className="text-xl" />
          </button>
        </div>
      </div>

      {/* Messages container */}
      <div
        ref={messagesContainerRef}
        className="flex-1 overflow-y-auto p-4 flex flex-col gap-3"
        style={{
          backgroundImage: 'radial-gradient(circle, rgba(197,160,89,0.04) 1px, transparent 1px)',
          backgroundSize: '20px 20px',
        }}
      >
        {/* Load-older spinner at the top */}
        {isLoadingOlder && (
          <div className="flex justify-center py-2">
            <svg className="animate-spin h-5 w-5 text-[#c5a059]" viewBox="0 0 24 24" fill="none">
              <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
              <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
            </svg>
          </div>
        )}

        {/* "No more messages" hint */}
        {!isLoadingInitial && !hasMorePages && messages.length > 0 && (
          <div className="flex justify-center py-1">
            <span className="text-[11px] text-gray-400">بداية المحادثة</span>
          </div>
        )}

        {/* Initial loading skeleton */}
        {isLoadingInitial && (
          <div className="space-y-4 py-4">
            {Array.from({ length: 5 }).map((_, i) => (
              <div key={i} className={`flex gap-2 ${i % 2 === 0 ? '' : 'flex-row-reverse'}`}>
                <div className="w-8 h-8 rounded-full bg-gray-200 dark:bg-gray-700 animate-pulse shrink-0" />
                <div className={`h-10 rounded-2xl bg-gray-200 dark:bg-gray-700 animate-pulse ${i % 2 === 0 ? 'w-3/5' : 'w-2/5'}`} />
              </div>
            ))}
          </div>
        )}

        {/* Empty state */}
        {!isLoadingInitial && messages.length === 0 && (
          <div className="flex-1 flex items-center justify-center text-gray-400 text-sm">
            لا توجد رسائل بعد. ابدأ المحادثة!
          </div>
        )}

        {/* Messages: oldest at top, newest at bottom */}
        {!isLoadingInitial &&
          messages.map((msg, idx) => {
            const showDateLabel =
              idx === 0 ||
              formatDateLabel(msg.createdAt) !== formatDateLabel(messages[idx - 1].createdAt);

            return (
              <div key={msg.id} className="contents">
                {showDateLabel && (
                  <div className="flex justify-center my-1">
                    <span className="text-[11px] text-gray-400 bg-gray-100 dark:bg-gray-800/70 px-3 py-1 rounded-full">
                      {formatDateLabel(msg.createdAt)}
                    </span>
                  </div>
                )}
                <ChatMessage message={msg} />
              </div>
            );
          })}

        {/* Anchor for auto-scroll-to-bottom */}
        <div ref={messagesBottomRef} />
      </div>

      {/* Input / closed banner */}
      {isClosed ? (
        <div className="px-4 py-3 bg-orange-50 dark:bg-orange-900/20 border-t border-orange-200 dark:border-orange-800 text-center text-sm text-orange-600 dark:text-orange-400 shrink-0">
          <MdLock className="inline ml-1" />
          هذه المحادثة مغلقة. يمكنك قراءة الرسائل السابقة فقط.
        </div>
      ) : (
        <MessageInput onSend={handleSend} disabled={isSending} />
      )}
    </section>
  );
};
