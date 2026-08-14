import React, { useCallback, useRef } from 'react';
import { LuCloudUpload, LuFile, LuTrash2 } from 'react-icons/lu';

interface FileUploadZoneProps {
  files: File[];
  onChange: (files: File[]) => void;
  maxFiles?: number;
  maxSizeMB?: number;
  accept?: string;
}

export const FileUploadZone: React.FC<FileUploadZoneProps> = ({
  files,
  onChange,
  maxFiles = 10,
  maxSizeMB = 20,
  accept = '.pdf,.doc,.docx'
}) => {
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFiles = (newFiles: FileList | File[]) => {
    const validFiles = Array.from(newFiles).filter(file => {
      // Check file type
      const fileExt = '.' + file.name.split('.').pop()?.toLowerCase();
      const isAcceptedType = accept.split(',').map(a => a.trim().toLowerCase()).includes(fileExt);
      
      // Filter out files that exceed max size or have invalid types
      return isAcceptedType && file.size <= maxSizeMB * 1024 * 1024;
    });

    onChange([...files, ...validFiles].slice(0, maxFiles));
  };

  const handleDrop = useCallback((e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      handleFiles(e.dataTransfer.files);
    }
  }, [files, onChange, maxFiles, maxSizeMB]);

  const handleDragOver = (e: React.DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    e.stopPropagation();
  };

  const removeFile = (indexToRemove: number) => {
    onChange(files.filter((_, index) => index !== indexToRemove));
  };

  return (
    <div className="space-y-4">
      <div 
        className="border-2 border-dashed border-gray-300 dark:border-gray-700 rounded-xl p-8 bg-gray-50 dark:bg-[#1a1d23] flex flex-col items-center justify-center text-center hover:border-gold hover:bg-gray-100 dark:hover:bg-gray-800 transition-all cursor-pointer group"
        onDrop={handleDrop}
        onDragOver={handleDragOver}
        onClick={() => fileInputRef.current?.click()}
      >
        <LuCloudUpload className="w-10 h-10 text-gray-400 group-hover:text-gold mb-4" />
        <h3 className="font-semibold text-sm text-gray-700 dark:text-gray-200 mb-2">اسحب وأفلت الملفات هنا (يمكنك اختيار ملفات متعددة)</h3>
        <p className="text-xs text-gray-500 dark:text-gray-400 mb-4">أو</p>
        <button 
          className="bg-white dark:bg-[#121620] border border-gold text-gold px-6 py-2 rounded-lg text-sm font-semibold hover:bg-gold hover:text-white transition-colors" 
          type="button"
          onClick={(e) => {
             e.stopPropagation();
             fileInputRef.current?.click();
          }}
        >
          استعراض الملفات
        </button>
        <p className="text-xs text-gray-500 dark:text-gray-400 mt-4">
          الصيغ المدعومة: {accept}. الحد الأقصى: {maxSizeMB}MB
        </p>
        <input 
          type="file" 
          multiple 
          ref={fileInputRef} 
          className="hidden" 
          accept={accept}
          onChange={(e) => {
            if (e.target.files) {
              handleFiles(e.target.files);
            }
            // reset input so same files can be selected again if needed
            e.target.value = '';
          }} 
        />
      </div>

      {files.length > 0 && (
        <div className="space-y-3">
          {files.map((file, idx) => (
            <div key={idx} className="flex items-center justify-between bg-gray-50 dark:bg-[#1a1d23] p-3 rounded-lg border border-gray-200 dark:border-gray-700">
              <div className="flex items-center gap-3">
                <LuFile className="w-5 h-5 text-gold" />
                <span className="text-sm text-gray-700 dark:text-gray-200">{file.name}</span>
                <span className="text-xs text-gray-400">({(file.size / (1024 * 1024)).toFixed(2)} MB)</span>
              </div>
              <button 
                className="text-gray-400 hover:text-red-500 transition-colors" 
                type="button"
                onClick={(e) => {
                  e.stopPropagation();
                  removeFile(idx);
                }}
              >
                <LuTrash2 className="w-5 h-5" />
              </button>
            </div>
          ))}
        </div>
      )}
    </div>
  );
};
