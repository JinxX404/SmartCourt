import { useRef, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { LuCheck, LuLoader, LuBellOff } from "react-icons/lu";
import { NotificationItem } from "./NotificationItem";
import {
  useNotificationsQuery,
  useMarkNotificationRead,
  useMarkAllNotificationsRead,
} from "../hooks/useNotificationsQueries";
import type { NotificationDto } from "../types";

interface NotificationDropdownProps {
  onClose: () => void;
}

export function NotificationDropdown({ onClose }: NotificationDropdownProps) {
  const dropdownRef = useRef<HTMLDivElement>(null);
  const navigate = useNavigate();
  const { data, isLoading, fetchNextPage, hasNextPage, isFetchingNextPage } =
    useNotificationsQuery();
  const { mutate: markRead } = useMarkNotificationRead();
  const { mutate: markAllRead, isPending: isMarkingAll } = useMarkAllNotificationsRead();

  // Close dropdown when clicking outside
  useEffect(() => {
    function handleClickOutside(event: MouseEvent) {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        onClose();
      }
    }
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, [onClose]);

  const handleNotificationClick = (notification: NotificationDto) => {
    if (!notification.readAtUtc) {
      markRead(notification.id);
    }
    onClose();
    if (notification.actionUrl) {
      navigate(notification.actionUrl);
    }
  };

  const notifications = data?.pages.flatMap((p) => p.items) || [];

  return (
    <div
      ref={dropdownRef}
      className="absolute left-0 md:left-auto md:right-0 top-full mt-2 w-80 sm:w-96 bg-white dark:bg-gray-800 rounded-xl shadow-2xl border border-gray-100 dark:border-gray-700 overflow-hidden z-[100] flex flex-col max-h-[85vh] origin-top-right"
    >
      <div className="p-4 border-b border-gray-100 dark:border-gray-700 flex items-center justify-between bg-gray-50 dark:bg-gray-800/50">
        <h3 className="font-bold text-gray-900 dark:text-white text-lg">الإشعارات</h3>
        <button
          onClick={() => markAllRead()}
          disabled={isMarkingAll || notifications.length === 0}
          className="text-xs font-semibold text-gold hover:text-gold/80 flex items-center gap-1 disabled:opacity-50"
        >
          <LuCheck className="w-4 h-4" />
          تحديد الكل كمقروء
        </button>
      </div>

      <div className="flex-1 overflow-y-auto">
        {isLoading ? (
          <div className="p-8 flex justify-center">
            <LuLoader className="w-6 h-6 text-gold animate-spin" />
          </div>
        ) : notifications.length > 0 ? (
          <>
            {notifications.map((notification) => (
              <NotificationItem
                key={notification.id}
                notification={notification}
                onClick={handleNotificationClick}
              />
            ))}
            {hasNextPage && (
              <button
                onClick={() => fetchNextPage()}
                disabled={isFetchingNextPage}
                className="w-full p-3 text-sm font-semibold text-gray-500 hover:bg-gray-50 dark:hover:bg-gray-700 transition-colors"
              >
                {isFetchingNextPage ? "جاري التحميل..." : "عرض المزيد"}
              </button>
            )}
          </>
        ) : (
          <div className="p-8 flex flex-col items-center justify-center text-gray-500 dark:text-gray-400">
            <LuBellOff className="w-12 h-12 mb-3 opacity-20" />
            <p className="font-semibold">لا توجد إشعارات</p>
            <p className="text-sm">أنت على اطلاع بكل جديد</p>
          </div>
        )}
      </div>

      <div className="p-3 border-t border-gray-100 dark:border-gray-700 bg-gray-50 dark:bg-gray-800/50 text-center">
        <button
          onClick={onClose}
          className="text-sm font-semibold text-gray-500 hover:text-gray-700 dark:hover:text-gray-300"
        >
          إغلاق
        </button>
      </div>
    </div>
  );
}
