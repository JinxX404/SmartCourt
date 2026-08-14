export interface ApiResponse<T> {
  success: boolean;
  data: T;
  message: string | null;
  errors: any | null;
  statusCode: number;
}

export interface NotificationDto {
  id: string;
  type: string;
  severity: "Info" | "Warning" | "Error" | "Success";
  title: string;
  body: string;
  actionUrl: string | null;
  data: Record<string, string> | null;
  createdAtUtc: string;
  readAtUtc: string | null;
  expiresAtUtc: string | null;
}

export interface NotificationPageDto {
  items: NotificationDto[];
  hasNextPage: boolean;
  nextCursor: string | null;
  unreadCount: number;
}

export interface UnreadNotificationCountDto {
  unreadCount: number;
}

export interface NotificationReadDto {
  id: string;
  readAtUtc: string;
}

export interface NotificationsReadAllDto {
  readAtUtc: string;
}

export interface GetNotificationsRequest {
  pageSize?: number;
  cursor?: string;
}
