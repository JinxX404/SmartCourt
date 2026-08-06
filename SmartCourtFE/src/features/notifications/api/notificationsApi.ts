import { apiClient } from '../../../api/apiClient';

export interface NotificationDto {
  id: string;
  title: string;
  message: string;
  isRead: boolean;
  createdAt: string;
}

export interface GetMyNotificationsResponseDto {
  notifications: NotificationDto[];
}

export const NotificationsApi = {
  getNotifications: async () => {
    const response = await apiClient.get('/api/notifications');
    return response.data;
  },

  markAsRead: async (notificationId: string) => {
    const response = await apiClient.patch(`/api/notifications/${notificationId}/read`);
    return response.data;
  }
};
