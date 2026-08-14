import * as signalR from '@microsoft/signalr';
import type { ChatMessageDto, SendChatMessageRequest } from '../types/chat.types';

let connection: signalR.HubConnection | null = null;

function buildConnection(): signalR.HubConnection {
  const baseUrl = import.meta.env.DEV ? '' : 'http://localhost:5049';
  return new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/chat`, {
      withCredentials: true,
      // Allow all transports so WebSockets are tried first but SSE/long-poll work as fallback
      transport:
        signalR.HttpTransportType.WebSockets |
        signalR.HttpTransportType.ServerSentEvents |
        signalR.HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();
}

function getConnection(): signalR.HubConnection {
  if (!connection || connection.state === signalR.HubConnectionState.Disconnected) {
    // If old connection exists and is in a bad state, discard it
    if (connection) {
      connection.off('ReceiveMessage');
    }
    connection = buildConnection();
  }
  return connection;
}

export const chatHub = {
  /** Start the SignalR connection (idempotent). */
  async start(): Promise<void> {
    const conn = getConnection();
    if (
      conn.state === signalR.HubConnectionState.Disconnected
    ) {
      await conn.start();
    }
  },

  /** Stop and destroy the connection. */
  async stop(): Promise<void> {
    if (connection) {
      connection.off('ReceiveMessage');
      if (connection.state !== signalR.HubConnectionState.Disconnected) {
        await connection.stop();
      }
      connection = null;
    }
  },

  /** Join a conversation group to receive real-time messages. */
  async joinConversation(conversationId: string): Promise<void> {
    await getConnection().invoke('JoinConversation', conversationId);
  },

  /** Leave a conversation group. */
  async leaveConversation(conversationId: string): Promise<void> {
    try {
      await getConnection().invoke('LeaveConversation', conversationId);
    } catch {
      // Connection may already be gone on unmount — ignore
    }
  },

  /**
   * Send a message via SignalR. Returns the persisted ChatMessageDto.
   */
  async sendMessage(
    conversationId: string,
    request: SendChatMessageRequest,
  ): Promise<ChatMessageDto> {
    return getConnection().invoke<ChatMessageDto>('SendMessage', conversationId, request);
  },

  /**
   * Register a handler for incoming messages.
   * Uses the 'ReceiveMessage' event as broadcast by ChatHub.
   * Returns an unsubscribe function.
   */
  onMessage(handler: (message: ChatMessageDto) => void): () => void {
    const conn = getConnection();
    conn.on('ReceiveMessage', handler);
    return () => conn.off('ReceiveMessage', handler);
  },

  get state(): signalR.HubConnectionState {
    return connection?.state ?? signalR.HubConnectionState.Disconnected;
  },
};
