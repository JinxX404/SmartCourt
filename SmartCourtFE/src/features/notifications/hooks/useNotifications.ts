import { useEffect } from 'react';
import { notificationsHub } from '../api/notificationsHub';
import type { UnreadNotificationCountDto } from '../types';
import { useQueryClient } from '@tanstack/react-query';
import { NOTIFICATIONS_KEYS } from './useNotificationsQueries';

/**
 * A global hook to initialize the notifications hub and attach global handlers.
 * Handlers are registered BEFORE starting the connection to avoid race conditions.
 */
export function useNotifications() {
  const queryClient = useQueryClient();

  useEffect(() => {
    // Register handlers FIRST, then start connection.
    // This ensures no event is missed between start() resolving and handlers being attached.
    const unsubCreated = notificationsHub.onNotificationCreated(() => {
      console.log('[Notifications] SignalR: NotificationCreated received');
      // Optimistically increment unread count
      queryClient.setQueryData<UnreadNotificationCountDto | undefined>(
        NOTIFICATIONS_KEYS.unreadCount(),
        (old) => {
          const newCount = old ? old.unreadCount + 1 : 1;
          console.log('[Notifications] Unread count:', old?.unreadCount, '->', newCount);
          return { unreadCount: newCount };
        }
      );
      // Also refetch from server to ensure accuracy
      queryClient.invalidateQueries({ queryKey: NOTIFICATIONS_KEYS.unreadCount() });
      queryClient.invalidateQueries({ queryKey: NOTIFICATIONS_KEYS.lists() });
    });

    const unsubRead = notificationsHub.onNotificationRead(() => {
      queryClient.invalidateQueries({ queryKey: NOTIFICATIONS_KEYS.unreadCount() });
      queryClient.invalidateQueries({ queryKey: NOTIFICATIONS_KEYS.lists() });
    });

    const unsubReadAll = notificationsHub.onNotificationsReadAll(() => {
      queryClient.setQueryData(NOTIFICATIONS_KEYS.unreadCount(), { unreadCount: 0 });
      queryClient.invalidateQueries({ queryKey: NOTIFICATIONS_KEYS.lists() });
    });

    // NOW start the connection
    notificationsHub.start();

    return () => {
      unsubCreated();
      unsubRead();
      unsubReadAll();
    };
  }, [queryClient]);

  return {
    state: notificationsHub.state,
    hub: notificationsHub,
  };
}
