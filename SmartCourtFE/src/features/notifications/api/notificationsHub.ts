import * as signalR from '@microsoft/signalr';

let connection: signalR.HubConnection | null = null;
let startPromise: Promise<void> | null = null;

function buildConnection(): signalR.HubConnection {
  const baseUrl = import.meta.env.DEV ? '' : 'http://localhost:5049';
  return new signalR.HubConnectionBuilder()
    .withUrl(`${baseUrl}/hubs/notifications`, {
      withCredentials: true,
      transport:
        signalR.HttpTransportType.WebSockets |
        signalR.HttpTransportType.ServerSentEvents |
        signalR.HttpTransportType.LongPolling,
    })
    .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
    .configureLogging(signalR.LogLevel.Warning)
    .build();
}

/**
 * Always returns the same connection instance.
 * Only creates a new one if none exists at all.
 */
function getOrCreateConnection(): signalR.HubConnection {
  if (!connection) {
    connection = buildConnection();
  }
  return connection;
}

export const notificationsHub = {
  async start(): Promise<void> {
    const conn = getOrCreateConnection();
    if (conn.state === signalR.HubConnectionState.Disconnected) {
      if (!startPromise) {
        startPromise = conn.start()
          .then(() => {
            console.log('[NotificationsHub] Connected successfully');
          })
          .catch((err) => {
            console.error('[NotificationsHub] Failed to start connection', err);
          })
          .finally(() => {
            startPromise = null;
          });
      }
      await startPromise;
    }
  },

  async stop(): Promise<void> {
    if (connection) {
      connection.off('NotificationCreated');
      connection.off('NotificationRead');
      connection.off('NotificationsReadAll');
      if (connection.state !== signalR.HubConnectionState.Disconnected) {
        await connection.stop();
      }
      connection = null;
      startPromise = null;
    }
  },

  onNotificationCreated(handler: (notification: any) => void): () => void {
    const conn = getOrCreateConnection();
    conn.on('NotificationCreated', handler);
    return () => conn.off('NotificationCreated', handler);
  },

  onNotificationRead(handler: (notificationId: string) => void): () => void {
    const conn = getOrCreateConnection();
    conn.on('NotificationRead', handler);
    return () => conn.off('NotificationRead', handler);
  },

  onNotificationsReadAll(handler: () => void): () => void {
    const conn = getOrCreateConnection();
    conn.on('NotificationsReadAll', handler);
    return () => conn.off('NotificationsReadAll', handler);
  },

  get state(): signalR.HubConnectionState {
    return connection?.state ?? signalR.HubConnectionState.Disconnected;
  },
};
