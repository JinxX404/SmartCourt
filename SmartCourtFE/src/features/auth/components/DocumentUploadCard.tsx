import { useRef, useState } from "react";
import { LuX, LuFileImage, LuUpload, LuPencil } from "react-icons/lu";
import { SecureImage } from "../../admin/verifications/components/SecureImage";

interface DocumentUploadCardProps {
  label: string;
  icon: React.ReactNode;
  onFileSelect: (file: File | null) => void;
  selectedFile?: File | null;
  status?: "Pending" | "Verified" | "Rejected" | "Expired" | string;
  rejectionReason?: string | null;
  existingFileName?: string;
  existingImageUrl?: string;
}

const StatusBadge = ({ status }: { status: string }) => {
  switch (status) {
    case "Verified":
      return <span className="bg-green-100 dark:bg-green-500/20 text-green-600 dark:text-green-400 px-2 py-0.5 rounded text-[10px] font-bold">مقبول</span>;
    case "Rejected":
      return <span className="bg-red-100 dark:bg-red-500/20 text-red-600 dark:text-red-400 px-2 py-0.5 rounded text-[10px] font-bold">مرفوض</span>;
    case "Pending":
      return <span className="bg-amber-100 dark:bg-amber-500/20 text-amber-600 dark:text-amber-400 px-2 py-0.5 rounded text-[10px] font-bold">قيد المراجعة</span>;
    case "Expired":
      return <span className="bg-gray-100 dark:bg-gray-500/20 text-gray-600 dark:text-gray-400 px-2 py-0.5 rounded text-[10px] font-bold">منتهي</span>;
    default:
      return null;
  }
};

export const DocumentUploadCard = ({
  label,
  icon,
  onFileSelect,
  selectedFile,
  status,
  rejectionReason,
  existingFileName,
  existingImageUrl
}: DocumentUploadCardProps) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [preview, setPreview] = useState<string | null>(null);

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      onFileSelect(file);
      if (file.type.startsWith("image/")) {
        const url = URL.createObjectURL(file);
        setPreview(url);
      } else {
        setPreview(null);
      }
    }
  };

  const handleRemove = (e: React.MouseEvent) => {
    e.stopPropagation();
    onFileSelect(null);
    setPreview(null);
    if (fileInputRef.current) {
      fileInputRef.current.value = "";
    }
  };

  return (
    <div className="border border-gray-200 dark:border-gray-700 rounded-xl overflow-hidden shadow-sm bg-gray-50 dark:bg-gray-800/50 flex flex-col">
      <input
        type="file"
        ref={fileInputRef}
        onChange={handleFileChange}
        className="hidden"
        accept="image/jpeg, image/png, image/webp, image/heic, image/heif"
      />

      {/* Header Bar */}
      <div className="bg-white dark:bg-[#1a1d23] p-3 border-b border-gray-200 dark:border-gray-700 flex justify-between items-center">
        <span className="font-bold text-xs text-gray-800 dark:text-gray-200">
          {label}
        </span>
        <div className="flex items-center gap-2">
          {status && <StatusBadge status={status} />}
          <button
            type="button"
            onClick={(e) => {
              e.stopPropagation();
              fileInputRef.current?.click();
            }}
            className="flex items-center gap-1 px-2.5 py-1 bg-gold/10 hover:bg-gold/20 text-gold font-bold text-[11px] rounded-lg transition-all cursor-pointer"
            title="تعديل أو تغيير الصورة"
          >
            <LuPencil className="w-3 h-3" />
            <span>تعديل</span>
          </button>
        </div>
      </div>

      {/* Image / Upload Area */}
      <div
        onClick={() => fileInputRef.current?.click()}
        className="bg-black/5 dark:bg-black/20 h-48 relative flex items-center justify-center overflow-hidden p-2 cursor-pointer group hover:bg-black/10 dark:hover:bg-black/30 transition-colors"
      >
        {selectedFile && preview ? (
          <>
            <img src={preview} alt="Preview" className="max-h-full max-w-full object-contain rounded-md" />
            <div className="absolute top-2 right-2 bg-blue-600 text-white px-2.5 py-1 rounded-md text-[10px] font-bold shadow-md z-10">
              صورة جديدة مختارة
            </div>
            <button
              onClick={handleRemove}
              className="absolute top-2 left-2 bg-rose-500 hover:bg-rose-600 text-white rounded-full p-1 shadow-md transition-colors z-10 cursor-pointer"
              title="إلغاء الصورة الجديدة"
            >
              <LuX className="w-4 h-4" />
            </button>
          </>
        ) : selectedFile ? (
          <>
            <div className="flex flex-col items-center text-center">
              <LuFileImage className="w-10 h-10 text-gold mb-2" />
              <p className="text-xs font-bold text-gray-600 dark:text-gray-400 truncate max-w-[200px]" dir="ltr">
                {selectedFile.name}
              </p>
            </div>
            <button
              onClick={handleRemove}
              className="absolute top-2 left-2 bg-rose-500 hover:bg-rose-600 text-white rounded-full p-1 shadow-md transition-colors z-10 cursor-pointer"
              title="إلغاء الصورة الجديدة"
            >
              <LuX className="w-4 h-4" />
            </button>
          </>
        ) : existingImageUrl ? (
          <SecureImage url={existingImageUrl} className="max-h-full max-w-full object-contain rounded-md" />
        ) : (
          <div className="flex flex-col items-center text-center gap-2">
            <div className="text-gray-400 dark:text-gray-500">
              {icon}
            </div>
            <div className="flex items-center gap-1.5 text-gray-400 dark:text-gray-500">
              <LuUpload className="w-4 h-4" />
              <span className="text-xs font-bold">اضغط لرفع الصورة</span>
            </div>
          </div>
        )}

        {/* Hover overlay to change image */}
        <div className="absolute inset-0 bg-black/60 opacity-0 group-hover:opacity-100 transition-opacity flex items-center justify-center pointer-events-none">
          <div className="text-white flex items-center gap-2 bg-white/20 px-4 py-2 rounded-xl font-bold backdrop-blur-sm text-sm">
            <LuPencil className="w-4 h-4" /> {existingImageUrl || selectedFile ? "تغيير الصورة" : "رفع الصورة"}
          </div>
        </div>
      </div>

      {/* Rejection Reason Footer */}
      {status === "Rejected" && rejectionReason && !selectedFile && (
        <div className="bg-red-50 dark:bg-red-900/20 p-2 border-t border-red-100 dark:border-red-900/50">
          <p className="text-[11px] text-red-500 font-bold text-center truncate" title={rejectionReason}>
            سبب الرفض: {rejectionReason}
          </p>
        </div>
      )}
    </div>
  );
};
