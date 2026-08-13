import { apiClient } from "../../../api/apiClient";
import type {
  ApiResponse,
  NotificationPageDto,
  UnreadNotificationCountDto,
  NotificationDto,
  NotificationsReadAllDto,
  GetNotificationsRequest,
} from "../types";

export const getNotifications = async (
  request: GetNotificationsRequest
): Promise<NotificationPageDto> => {
  const { data } = await apiClient.get<ApiResponse<NotificationPageDto>>(
    "/api/notifications",
    { params: request }
  );
  return data.data;
};

export const getUnreadNotificationsCount =
  async (): Promise<UnreadNotificationCountDto> => {
    const { data } = await apiClient.get<ApiResponse<UnreadNotificationCountDto>>(
      "/api/notifications/unread-count"
    );
    return data.data;
  };

export const markNotificationRead = async (
  notificationId: string
): Promise<NotificationDto> => {
  const { data } = await apiClient.patch<ApiResponse<NotificationDto>>(
    `/api/notifications/${notificationId}/read`
  );
  return data.data;
};

export const markAllNotificationsRead =
  async (): Promise<NotificationsReadAllDto> => {
    const { data } = await apiClient.patch<ApiResponse<NotificationsReadAllDto>>(
      "/api/notifications/read-all"
    );
    return data.data;
  };
