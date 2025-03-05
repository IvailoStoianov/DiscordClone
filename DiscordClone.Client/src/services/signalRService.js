import * as signalR from '@microsoft/signalr';

// Store the connection globally
let connection = null;

// Initialize SignalR connection
export async function initializeSignalRConnection() {
  try {
    // Create a new connection if one doesn't exist
    if (!connection) {
      connection = new signalR.HubConnectionBuilder()
        .withUrl('https://localhost:7001/chatHub', {
          withCredentials: true
        })
        .withAutomaticReconnect()
        .configureLogging(signalR.LogLevel.Information)
        .build();

      // Set up reconnect logic
      connection.onreconnecting(error => {
        console.log('Reconnecting to SignalR hub...', error);
      });

      connection.onreconnected(connectionId => {
        console.log('Reconnected to SignalR hub with connectionId:', connectionId);
      });

      connection.onclose(error => {
        console.log('SignalR connection closed:', error);
      });
    }

    // Start the connection if it's not already connected
    if (connection.state !== signalR.HubConnectionState.Connected) {
      await connection.start();
      console.log('SignalR connection established');
    }

    return connection;
  } catch (error) {
    console.error('Error establishing SignalR connection:', error);
    throw error;
  }
}

// Join a chat room
export async function joinChatRoom(roomId) {
  if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
    console.error('Cannot join room: SignalR not connected');
    return false;
  }

  try {
    await connection.invoke('JoinChatRoom', roomId.toString());
    console.log(`Joined chat room: ${roomId}`);
    return true;
  } catch (error) {
    console.error(`Error joining chat room ${roomId}:`, error);
    return false;
  }
}

// Leave a chat room
export async function leaveChatRoom(roomId) {
  if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
    console.error('Cannot leave room: SignalR not connected');
    return false;
  }

  try {
    await connection.invoke('LeaveChatRoom', roomId.toString());
    console.log(`Left chat room: ${roomId}`);
    return true;
  } catch (error) {
    console.error(`Error leaving chat room ${roomId}:`, error);
    return false;
  }
}

// Set up event handler for receiving messages - fix case to match server exactly
export function onReceiveMessage(callback) {
  if (!connection) {
    console.error('Cannot set up message handler: SignalR not connected');
    return;
  }

  // Make sure event name exactly matches what the server broadcasts (case-sensitive)
  connection.on('ReceiveMessage', callback);
}

// Remove event handler for receiving messages
export function offReceiveMessage() {
  if (!connection) {
    console.error('Cannot remove message handler: SignalR not connected');
    return;
  }

  // Match the exact case used in onReceiveMessage
  connection.off('ReceiveMessage');
}

// Set up event handler for message deletion
export function onMessageDeleted(callback) {
  if (!connection) {
    console.error('Cannot set up message deleted handler: SignalR not connected');
    return;
  }

  // Make sure event name exactly matches what the server broadcasts (case-sensitive)
  connection.on('MessageDeleted', callback);
}

// Remove event handler for message deletion
export function offMessageDeleted() {
  if (!connection) {
    console.error('Cannot remove message deleted handler: SignalR not connected');
    return;
  }

  // Match the exact case used in onMessageDeleted
  connection.off('MessageDeleted');
}

// Close SignalR connection
export function closeConnection() {
  if (connection) {
    connection.stop();
    connection = null;
    console.log('SignalR connection closed');
  }
}

// Get connection state for debugging
export const getConnectionState = () => {
  if (!connection) return 'No connection';
  return signalR.HubConnectionState[connection.state];
};

// Export the connection for external access if needed
export const getConnection = () => connection; 