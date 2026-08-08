import React, { useEffect } from "react";
import { LuTriangleAlert, LuInfo, LuShieldAlert, LuCheck, LuX, LuLoader } from "react-icons/lu";

interface ConfirmModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => void;
  title: string;
  description: string;
  confirmText?: string;
  cancelText?: string;
  type?: "warning" | "danger" | "info" | "success";
  isLoading?: boolean;
}

export const ConfirmModal: React.FC<ConfirmModalProps> = ({
  isOpen,
  onClose,
  onConfirm,
  title,
  description,
  confirmText = "تأكيد واستمرار",
  cancelText = "إلغاء",
  type = "warning",
  isLoading = false,
}) => {
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === "Escape" && isOpen && !isLoading) {
        onClose();
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [isOpen, isLoading, onClose]);

  if (!isOpen) return null;

  const renderIcon = () => {
    switch (type) {
      case "danger":
        return (
          <div className="w-14 h-14 rounded-2xl bg-red-500/10 text-red-500 flex items-center justify-center mb-4 ring-8 ring-red-500/5">
            <LuShieldAlert className="w-7 h-7" />
          </div>
        );
      case "success":
        return (
          <div className="w-14 h-14 rounded-2xl bg-green-500/10 text-green-500 flex items-center justify-center mb-4 ring-8 ring-green-500/5">
            <LuCheck className="w-7 h-7" />
          </div>
        );
      case "info":
        return (
          <div className="w-14 h-14 rounded-2xl bg-blue-500/10 text-blue-500 flex items-center justify-center mb-4 ring-8 ring-blue-500/5">
            <LuInfo className="w-7 h-7" />
          </div>
        );
      case "warning":
      default:
        return (
          <div className="w-14 h-14 rounded-2xl bg-amber-500/10 text-amber-500 flex items-center justify-center mb-4 ring-8 ring-amber-500/5">
            <LuTriangleAlert className="w-7 h-7" />
          </div>
        );
    }
  };

  const getConfirmButtonStyles = () => {
    switch (type) {
      case "danger":
        return "bg-red-600 hover:bg-red-700 text-white shadow-red-500/20";
      case "success":
        return "bg-green-600 hover:bg-green-700 text-white shadow-green-500/20";
      case "info":
        return "bg-blue-600 hover:bg-blue-700 text-white shadow-blue-500/20";
      case "warning":
      default:
        return "bg-gold hover:bg-[#b08752] text-white shadow-gold/20";
    }
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/60 backdrop-blur-md animate-fade-in dir-rtl">
      {/* Backdrop click */}
      <div className="absolute inset-0" onClick={() => !isLoading && onClose()} />

      {/* Modal Container */}
      <div className="relative w-full max-w-md bg-white dark:bg-[#1a1d23] rounded-3xl p-6 sm:p-8 border border-gray-200/80 dark:border-gray-800 shadow-2xl z-10 transform transition-all animate-scale-up">
        {/* Close Button */}
        <button
          onClick={onClose}
          disabled={isLoading}
          className="absolute top-4 left-4 p-2 text-gray-400 hover:text-gray-600 dark:hover:text-gray-200 rounded-full hover:bg-gray-100 dark:hover:bg-gray-800 transition-colors disabled:opacity-50 cursor-pointer"
        >
          <LuX className="w-5 h-5" />
        </button>

        <div className="flex flex-col items-center text-center">
          {renderIcon()}

          <h3 className="text-lg font-bold text-gray-900 dark:text-white mb-2">
            {title}
          </h3>

          <p className="text-xs sm:text-sm text-gray-600 dark:text-gray-300 leading-relaxed mb-6">
            {description}
          </p>

          <div className="flex flex-col sm:flex-row items-center gap-3 w-full">
            <button
              onClick={onConfirm}
              disabled={isLoading}
              className={`w-full sm:flex-1 py-3 px-5 rounded-xl font-bold text-xs sm:text-sm shadow-md transition-all flex items-center justify-center gap-2 cursor-pointer ${getConfirmButtonStyles()}`}
            >
              {isLoading ? (
                <>
                  <LuLoader className="w-4 h-4 animate-spin" />
                  <span>جاري التنفيذ...</span>
                </>
              ) : (
                <span>{confirmText}</span>
              )}
            </button>

            <button
              onClick={onClose}
              disabled={isLoading}
              className="w-full sm:flex-1 py-3 px-5 bg-gray-100 dark:bg-gray-800 hover:bg-gray-200 dark:hover:bg-gray-700 text-gray-700 dark:text-gray-300 font-bold text-xs sm:text-sm rounded-xl transition-all disabled:opacity-50 cursor-pointer"
            >
              {cancelText}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
