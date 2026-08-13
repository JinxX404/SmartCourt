import React, { useRef, useState, type ChangeEvent } from 'react';
import { MdSend, MdAttachFile, MdClose, MdInsertDriveFile } from 'react-icons/md';

interface MessageInputProps {
  onSend: (content: string, files?: File[]) => void;
  disabled?: boolean;
  placeholder?: string;
}

const ALLOWED_TYPES = [
  'application/pdf',
  'application/vnd.openxmlformats-officedocument.wordprocessingml.document',
  'text/plain',
  'image/png',
  'image/jpeg'
];
const MAX_FILES = 5;
const MAX_FILE_SIZE = 10 * 1024 * 1024; // 10MB

export const MessageInput = ({ onSend, disabled = false, placeholder = 'اكتب رسالتك هنا...' }: MessageInputProps) => {
  const [value, setValue] = useState('');
  const [files, setFiles] = useState<File[]>([]);
  const textareaRef = useRef<HTMLTextAreaElement>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleSend = () => {
    const trimmed = value.trim();
    if ((!trimmed && files.length === 0) || disabled) return;
    onSend(trimmed, files.length > 0 ? files : undefined);
    setValue('');
    setFiles([]);
    if (textareaRef.current) {
      textareaRef.current.style.height = 'auto';
    }
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent<HTMLTextAreaElement>) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSend();
    }
  };

  const handleInput = () => {
    const el = textareaRef.current;
    if (!el) return;
    el.style.height = 'auto';
    el.style.height = `${Math.min(el.scrollHeight, 128)}px`;
  };

  const handleFileSelect = (e: ChangeEvent<HTMLInputElement>) => {
    if (!e.target.files) return;
    const selectedFiles = Array.from(e.target.files);
    
    // Filter by allowed type and size
    const validFiles = selectedFiles.filter(file => {
      if (!ALLOWED_TYPES.includes(file.type)) {
        // Fallback checks for extensions if mime type is missing or generic
        const ext = file.name.split('.').pop()?.toLowerCase();
        if (!['pdf', 'docx', 'txt', 'png', 'jpg', 'jpeg'].includes(ext || '')) {
           return false;
        }
      }
      return file.size <= MAX_FILE_SIZE;
    });

    setFiles(prev => {
      const combined = [...prev, ...validFiles];
      return combined.slice(0, MAX_FILES);
    });
    
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  const removeFile = (index: number) => {
    setFiles(prev => prev.filter((_, i) => i !== index));
  };

  return (
    <div className="flex flex-col bg-white dark:bg-[#1a1d23] border-t border-gray-100 dark:border-gray-800">
      {/* File Preview Area */}
      {files.length > 0 && (
        <div className="flex flex-wrap gap-2 p-3 pb-0 border-b border-gray-100 dark:border-gray-800">
          {files.map((file, idx) => (
            <div key={`${file.name}-${idx}`} className="flex items-center gap-1.5 bg-gray-100 dark:bg-gray-800 px-2 py-1 rounded-md text-xs border border-gray-200 dark:border-gray-700 max-w-[200px]">
              <MdInsertDriveFile className="text-gray-500 shrink-0" />
              <span className="truncate text-gray-700 dark:text-gray-300">{file.name}</span>
              <button
                type="button"
                onClick={() => removeFile(idx)}
                disabled={disabled}
                className="text-gray-400 hover:text-red-500 transition-colors ml-1"
              >
                <MdClose />
              </button>
            </div>
          ))}
          {files.length === MAX_FILES && (
            <div className="text-[10px] text-orange-500 self-center">الحد الأقصى 5 ملفات</div>
          )}
        </div>
      )}

      <div className="p-4 flex items-end gap-2">
        <label
          className={`p-2 transition-colors shrink-0 ${
            disabled || files.length >= MAX_FILES
              ? 'text-gray-400 opacity-40 cursor-not-allowed pointer-events-none'
              : 'text-gray-400 hover:text-[#c5a059] cursor-pointer'
          }`}
        >
          <input 
            type="file" 
            ref={fileInputRef} 
            onChange={handleFileSelect} 
            multiple 
            accept=".pdf,.docx,.txt,.png,.jpeg,.jpg" 
            className="hidden" 
            disabled={disabled || files.length >= MAX_FILES}
          />
          <MdAttachFile className="text-xl" />
        </label>

        <div
          className={`flex-1 flex items-end bg-gray-50 dark:bg-[#121620] rounded-xl border transition-all ${
            disabled
              ? 'border-gray-200 dark:border-gray-700 opacity-50'
              : 'border-gray-200 dark:border-gray-700 focus-within:border-[#c5a059] focus-within:ring-2 focus-within:ring-[#c5a059]/30'
          }`}
        >
          <textarea
            ref={textareaRef}
            value={value}
            onChange={(e) => setValue(e.target.value)}
            onKeyDown={handleKeyDown}
            onInput={handleInput}
            disabled={disabled}
            placeholder={files.length > 0 ? "اكتب تعليقاً (اختياري)..." : placeholder}
            rows={1}
            dir="rtl"
            maxLength={2000}
            className="w-full bg-transparent border-none resize-none focus:ring-0 focus:outline-none text-sm text-gray-900 dark:text-white placeholder:text-gray-400 py-3 px-4 max-h-32 disabled:cursor-not-allowed"
            style={{ minHeight: '44px' }}
          />
        </div>

        <button
          type="button"
          onClick={handleSend}
          disabled={disabled || (!value.trim() && files.length === 0)}
          className="w-12 h-12 shrink-0 bg-[#c5a059] hover:bg-[#b08d4a] text-white rounded-full flex items-center justify-center shadow-md transition-all disabled:opacity-40 disabled:cursor-not-allowed active:scale-95"
        >
          <MdSend className="text-xl rtl:-scale-x-100" />
        </button>
      </div>
    </div>
  );
};
