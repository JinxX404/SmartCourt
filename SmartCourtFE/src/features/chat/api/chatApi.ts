import { apiClient } from '../../../api/apiClient';
import type {
  ChatApiResponse,
  ChatConversationDetailDto,
  ChatConversationPageDto,
  ChatMessageDto,
  ChatMessagePageDto,
  ListConversationsParams,
  SendChatMessageRequest,
} from '../types/chat.types';

export class ChatApi {
  /**
   * GET /api/chat/conversations
   * Lists all conversations for the authenticated user.
   */
  static async listConversations(
    params: ListConversationsParams = {},
  ): Promise<ChatApiResponse<ChatConversationPageDto>> {
    const urlParams = new URLSearchParams();
    if (params.search) urlParams.append('search', params.search);
    if (params.page) urlParams.append('page', params.page.toString());
    if (params.pageSize) urlParams.append('pageSize', params.pageSize.toString());

    const response = await apiClient.get<ChatApiResponse<ChatConversationPageDto>>(
      `/api/chat/conversations?${urlParams.toString()}`,
    );
    return response.data;
  }

  /**
   * GET /api/chat/conversations/{conversationId}
   * Gets a single conversation's details.
   */
  static async getConversation(
    conversationId: string,
  ): Promise<ChatApiResponse<ChatConversationDetailDto>> {
    const response = await apiClient.get<ChatApiResponse<ChatConversationDetailDto>>(
      `/api/chat/conversations/${conversationId}`,
    );
    return response.data;
  }

  /**
   * GET /api/chat/conversations/{conversationId}/messages
   * Gets paginated messages for a conversation.
   */
  static async getMessages(
    conversationId: string,
    page = 1,
    pageSize = 50,
  ): Promise<ChatApiResponse<ChatMessagePageDto>> {
    const response = await apiClient.get<ChatApiResponse<ChatMessagePageDto>>(
      `/api/chat/conversations/${conversationId}/messages?page=${page}&pageSize=${pageSize}`,
    );
    return response.data;
  }

  /**
   * POST /api/chat/conversations/{conversationId}/messages
   * Sends a message via REST (fallback when SignalR is not connected).
   */
  static async sendMessage(
    conversationId: string,
    body: SendChatMessageRequest,
  ): Promise<ChatApiResponse<ChatMessageDto>> {
    const response = await apiClient.post<ChatApiResponse<ChatMessageDto>>(
      `/api/chat/conversations/${conversationId}/messages`,
      body,
    );
    return response.data;
  }
  /**
   * POST /api/chat/conversations/{conversationId}/attachments
   * Sends attachments via REST. SignalR will broadcast the resulting message.
   */
  static async sendAttachments(
    conversationId: string,
    caption: string,
    files: File[],
  ): Promise<ChatApiResponse<ChatMessageDto>> {
    const formData = new FormData();
    if (caption.trim()) formData.append('caption', caption.trim());
    files.forEach((file) => formData.append('files', file));

    const response = await apiClient.post<ChatApiResponse<ChatMessageDto>>(
      `/api/chat/conversations/${conversationId}/attachments`,
      formData,
      {
        headers: {
          'Content-Type': 'multipart/form-data',
        },
      }
    );
    return response.data;
  }

  /**
   * GET attachment blob
   * Downloads an attachment securely using the stored downloadUrl.
   */
  static async downloadAttachment(downloadUrl: string): Promise<Blob> {
    const response = await apiClient.get<Blob>(downloadUrl, {
      responseType: 'blob',
    });
    return response.data;
  }
}
