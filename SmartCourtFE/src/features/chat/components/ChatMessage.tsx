import { useState } from 'react';
import { MdInsertDriveFile, MdDownload } from 'react-icons/md';
import toast from 'react-hot-toast';
import { ChatApi } from '../api/chatApi';
import type { ChatMessageDto, ChatAttachmentDto } from '../types/chat.types';

interface ChatMessageProps {
  message: ChatMessageDto;
}

function formatTime(isoDate: string): string {
  return new Date(isoDate).toLocaleTimeString('ar-EG', {
    hour: '2-digit',
    minute: '2-digit',
  });
}

function formatBytes(bytes: number, decimals = 2) {
  if (!+bytes) return '0 Bytes';
  const k = 1024;
  const dm = decimals < 0 ? 0 : decimals;
  const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));
  return `${parseFloat((bytes / Math.pow(k, i)).toFixed(dm))} ${sizes[i]}`;
}

export const ChatMessage = ({ message }: ChatMessageProps) => {
  const [downloadingIds, setDownloadingIds] = useState<Set<string>>(new Set());
  const isSystem = message.type === 'System';

  const handleDownload = async (attachment: ChatAttachmentDto) => {
    if (downloadingIds.has(attachment.id)) return;
    
    setDownloadingIds(prev => new Set(prev).add(attachment.id));
    let objectUrl: string | null = null;
    try {
      const blob = await ChatApi.downloadAttachment(attachment.downloadUrl);
      objectUrl = URL.createObjectURL(blob);
      
      const link = document.createElement('a');
      link.href = objectUrl;
      link.download = attachment.fileName;
      document.body.appendChild(link);
      link.click();
      document.body.removeChild(link);
    } catch (err) {
      toast.error('تعذر تنزيل الملف، قد يكون غير متاح.');
    } finally {
      if (objectUrl) {
        setTimeout(() => URL.revokeObjectURL(objectUrl!), 1000);
      }
      setDownloadingIds(prev => {
        const next = new Set(prev);
        next.delete(attachment.id);
        return next;
      });
    }
  };

  if (isSystem) {
    return (
      <div className="flex justify-center my-2">
        <span className="text-xs text-gray-500 dark:text-gray-400 bg-gray-100 dark:bg-gray-800/70 px-4 py-1.5 rounded-full border border-gray-200 dark:border-gray-700">
          {message.content}
        </span>
      </div>
    );
  }

  const hasAttachments = message.attachments && message.attachments.length > 0;

  return (
    <div
      className={`flex items-end gap-2 max-w-[80%] ${
        message.isMine ? 'self-end flex-row-reverse' : 'self-start'
      }`}
    >
      {/* Message bubble */}
      <div
        className={`relative px-4 py-3 rounded-2xl text-sm leading-relaxed shadow-sm ${
          message.isMine
            ? 'bg-[#1a1d23] dark:bg-[#c5a059] text-white dark:text-[#1a1d23] rounded-bl-sm'
            : 'bg-white dark:bg-[#1a1d23] text-gray-900 dark:text-white border border-gray-200 dark:border-gray-700 rounded-br-sm'
        }`}
      >
        {/* Sender name (shown for received messages in group context) */}
        {!message.isMine && message.senderName && (
          <p className="text-[11px] font-semibold text-[#c5a059] mb-1">{message.senderName}</p>
        )}
        
        {message.content && (
          <p className="whitespace-pre-wrap break-words">{message.content}</p>
        )}

        {hasAttachments && (
          <div className={`flex flex-col gap-2 ${message.content ? 'mt-3' : ''}`}>
            {message.attachments!.map((att) => {
              const isDownloading = downloadingIds.has(att.id);
              return (
                <div 
                  key={att.id} 
                  className={`flex items-center gap-3 p-2 rounded-lg border ${
                    message.isMine 
                      ? 'bg-black/10 dark:bg-white/10 border-transparent' 
                      : 'bg-gray-50 dark:bg-[#121620] border-gray-200 dark:border-gray-700'
                  }`}
                >
                  <div className="w-8 h-8 rounded bg-[#c5a059]/20 text-[#c5a059] flex items-center justify-center shrink-0">
                    <MdInsertDriveFile className="text-lg" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-xs font-medium truncate" dir="ltr" title={att.fileName}>{att.fileName}</p>
                    <p className={`text-[10px] ${message.isMine ? 'text-white/70 dark:text-[#1a1d23]/70' : 'text-gray-500'}`}>
                      {formatBytes(att.sizeInBytes)}
                    </p>
                  </div>
                  <button 
                    onClick={() => handleDownload(att)}
                    disabled={isDownloading}
                    className={`w-8 h-8 rounded-full flex items-center justify-center transition-colors ${
                      message.isMine 
                        ? 'hover:bg-black/10 dark:hover:bg-white/10 text-white dark:text-[#1a1d23]' 
                        : 'hover:bg-gray-200 dark:hover:bg-gray-800 text-gray-600 dark:text-gray-300'
                    }`}
                  >
                    {isDownloading ? (
                       <svg className="animate-spin h-4 w-4" viewBox="0 0 24 24" fill="none">
                         <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                         <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v8H4z" />
                       </svg>
                    ) : (
                      <MdDownload className="text-lg" />
                    )}
                  </button>
                </div>
              );
            })}
          </div>
        )}

        <span
          className={`block mt-1 text-[10px] select-none ${
            message.isMine ? 'text-white/60 dark:text-[#1a1d23]/60 text-left' : 'text-gray-400 text-right'
          }`}
        >
          {formatTime(message.createdAt)}
        </span>
      </div>
    </div>
  );
};
