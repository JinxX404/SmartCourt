import { useQuery, useInfiniteQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  getNotifications,
  getUnreadNotificationsCount,
  markNotificationRead,
  markAllNotificationsRead,
} from "../api/notificationsApi";

export const NOTIFICATIONS_KEYS = {
  all: ["notifications"] as const,
  lists: () => [...NOTIFICATIONS_KEYS.all, "list"] as const,
  unreadCount: () => [...NOTIFICATIONS_KEYS.all, "unreadCount"] as const,
};

export const useNotificationsQuery = () => {
  return useInfiniteQuery({
    queryKey: NOTIFICATIONS_KEYS.lists(),
    queryFn: ({ pageParam }) => getNotifications({ cursor: pageParam }),
    initialPageParam: undefined as string | undefined,
    getNextPageParam: (lastPage) =>
      lastPage.hasNextPage ? lastPage.nextCursor : undefined,
  });
};

export const useUnreadNotificationsCount = (enabled: boolean = true) => {
  return useQuery({
    queryKey: NOTIFICATIONS_KEYS.unreadCount(),
    queryFn: getUnreadNotificationsCount,
    enabled,
    refetchInterval: 30_000, // Poll every 30s as fallback if SignalR misses events
  });
};

export const useMarkNotificationRead = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: markNotificationRead,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: NOTIFICATIONS_KEYS.unreadCount() });
      queryClient.invalidateQueries({ queryKey: NOTIFICATIONS_KEYS.lists() });
    },
  });
};

export const useMarkAllNotificationsRead = () => {
  const queryClient = useQueryClient();

  return useMutation({
    mutationFn: markAllNotificationsRead,
    onSuccess: () => {
      queryClient.setQueryData(NOTIFICATIONS_KEYS.unreadCount(), { unreadCount: 0 });
      queryClient.invalidateQueries({ queryKey: NOTIFICATIONS_KEYS.lists() });
    },
  });
};
