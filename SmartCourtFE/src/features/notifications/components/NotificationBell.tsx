import { useState } from "react";
import { IoIosNotifications } from "react-icons/io";
import { NotificationDropdown } from "./NotificationDropdown";
import { useUnreadNotificationsCount } from "../hooks/useNotificationsQueries";

export function NotificationBell() {
  const [isOpen, setIsOpen] = useState(false);
  const { data } = useUnreadNotificationsCount();

  const unreadCount = data?.unreadCount || 0;

  return (
    <div className="relative">
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="text-gray-400 hover:text-white transition-colors cursor-pointer relative"
        title="الإشعارات"
      >
        <IoIosNotifications className="w-7 h-7" />
        {unreadCount > 0 && (
          <span className="absolute -top-1 -right-1 flex items-center justify-center min-w-[18px] h-[18px] px-1 bg-red-500 rounded-full border-2 border-[#121620] text-[10px] font-bold text-white animate-pulse">
            {unreadCount > 99 ? '99+' : unreadCount}
          </span>
        )}
      </button>

      {isOpen && <NotificationDropdown onClose={() => setIsOpen(false)} />}
    </div>
  );
}
