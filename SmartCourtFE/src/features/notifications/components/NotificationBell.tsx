import { useState, useEffect, useRef } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { LuBell, LuCircleCheck } from "react-icons/lu";
import { NotificationsApi, type NotificationDto } from "../api/notificationsApi";
import { useAuthStore } from "../../auth/store/useAuthStore";
import * as signalR from "@microsoft/signalr";
import { toast } from "react-hot-toast";

export const NotificationBell = () => {
  const { user } = useAuthStore();
  const [isOpen, setIsOpen] = useState(false);
  const dropdownRef = useRef<HTMLDivElement>(null);
  const queryClient = useQueryClient();

  // Fetch initial notifications
  const { data: response } = useQuery({
    queryKey: ["notifications"],
    queryFn: NotificationsApi.getNotifications,
    enabled: !!user,
    refetchInterval: 4000,
  });

  const notifications: NotificationDto[] = response?.notifications || [];
  const unreadCount = notifications.filter(n => !n.isRead).length;

  const markAsReadMutation = useMutation({
    mutationFn: NotificationsApi.markAsRead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
    },
  });

  useEffect(() => {
    if (!user) return;

    // Connect to SignalR Hub
    const hubUrl = import.meta.env.DEV 
      ? "/hubs/notifications" 
      : "http://localhost:5049/hubs/notifications";
      
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        withCredentials: true
      })
      .withAutomaticReconnect()
      .build();

    connection.on("ReceiveNotification", (title: string, message: string) => {
      // Invalidate query to fetch new notifications and show toast
      queryClient.invalidateQueries({ queryKey: ["notifications"] });
      toast(`${title}: ${message}`, { icon: "🔔", duration: 5000 });
    });

    connection.start().catch(err => console.error("SignalR Connection Error: ", err));

    return () => {
      connection.stop();
    };
  }, [user, queryClient]);

  // Click outside to close
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      if (dropdownRef.current && !dropdownRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    };
    document.addEventListener("mousedown", handleClickOutside);
    return () => document.removeEventListener("mousedown", handleClickOutside);
  }, []);

  const handleNotificationClick = (notification: NotificationDto) => {
    if (!notification.isRead) {
      markAsReadMutation.mutate(notification.id);
    }
  };

  return (
    <div className="relative" ref={dropdownRef}>
      <button
        onClick={() => setIsOpen(!isOpen)}
        className="relative p-2 rounded-xl text-gray-400 hover:text-white hover:bg-white/5 transition-all focus:outline-none"
      >
        <LuBell className="w-6 h-6" />
        {unreadCount > 0 && (
          <span className="absolute top-1.5 right-1.5 w-2 h-2 bg-red-500 rounded-full animate-pulse shadow-[0_0_8px_rgba(239,68,68,0.8)]"></span>
        )}
      </button>

      {isOpen && (
        <div className="absolute right-0 top-full mt-2 w-72 sm:w-80 bg-white dark:bg-[#1a1d23] border border-gray-200 dark:border-gray-800 rounded-2xl shadow-2xl z-[9999] overflow-hidden flex flex-col">
          <div className="p-4 border-b border-gray-200 dark:border-gray-800 flex justify-between items-center bg-gray-50 dark:bg-[#121620]">
            <h3 className="font-bold text-gray-900 dark:text-white text-sm">الإشعارات</h3>
            {unreadCount > 0 && (
              <span className="text-[10px] bg-red-100 dark:bg-red-900/30 text-red-600 dark:text-red-400 px-2 py-0.5 rounded-full font-bold">
                {unreadCount} جديد
              </span>
            )}
          </div>
          
          <div className="max-h-80 overflow-y-auto">
            {notifications.length === 0 ? (
              <div className="p-8 text-center text-gray-500 dark:text-gray-400 text-xs">
                لا توجد إشعارات حالياً
              </div>
            ) : (
              <div className="flex flex-col">
                {notifications.map((notif) => (
                  <div 
                    key={notif.id}
                    onClick={() => handleNotificationClick(notif)}
                    className={`p-4 border-b border-gray-100 dark:border-gray-800 cursor-pointer transition-colors hover:bg-gray-50 dark:hover:bg-white/5 flex gap-3 ${!notif.isRead ? 'bg-gold/5 dark:bg-gold/5' : ''}`}
                  >
                    <div className="mt-0.5">
                      {notif.title.includes("قبول") ? (
                        <LuCircleCheck className="w-5 h-5 text-green-500" />
                      ) : (
                        <LuBell className={`w-5 h-5 ${!notif.isRead ? 'text-gold' : 'text-gray-400'}`} />
                      )}
                    </div>
                    <div>
                      <h4 className={`text-xs font-bold mb-1 ${!notif.isRead ? 'text-gray-900 dark:text-white' : 'text-gray-600 dark:text-gray-300'}`}>
                        {notif.title}
                      </h4>
                      <p className="text-[11px] text-gray-500 dark:text-gray-400 leading-relaxed">
                        {notif.message}
                      </p>
                      <span className="text-[9px] text-gray-400 dark:text-gray-500 mt-2 block">
                        {new Date(notif.createdAt).toLocaleString("ar-EG")}
                      </span>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      )}
    </div>
  );
};
