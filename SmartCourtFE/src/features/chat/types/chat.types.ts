export interface ChatParticipantDto {
  userId: string;
  name: string;
  role: 'Client' | 'Lawyer';
}

export interface ChatAttachmentDto {
  id: string;
  fileName: string;
  contentType: string;
  sizeInBytes: number;
  downloadUrl: string;
}

export interface ChatMessageDto {
  id: string;
  conversationId: string;
  senderUserId: string | null;
  senderName: string | null;
  type: 'Text' | 'System' | string;
  content: string;
  systemCode: string | null;
  relatedEntityId: string | null;
  createdAt: string;
  isMine: boolean;
  attachments?: ChatAttachmentDto[];
}

export interface ChatConversationListItemDto {
  id: string;
  proposalId: string;
  legalCaseId: string;
  caseTitle: string;
  client: ChatParticipantDto;
  lawyer: ChatParticipantDto;
  status: 'Open' | 'Closed' | string;
  createdAt: string;
  updatedAt: string;
  lastMessageAt: string | null;
  lastMessage: ChatMessageDto | null;
}

export interface ChatConversationDetailDto extends ChatConversationListItemDto {
  // detail has same fields as list item
}

export interface ChatConversationPageDto {
  items: ChatConversationListItemDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
}

export interface ChatMessagePageDto {
  items: ChatMessageDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  hasNextPage: boolean;
}

export interface SendChatMessageRequest {
  content: string;
}

export interface ChatApiResponse<T> {
  success: boolean;
  statusCode: number;
  message: string | null;
  errors: string[] | null;
  data: T;
}

export interface ListConversationsParams {
  search?: string;
  page?: number;
  pageSize?: number;
}
