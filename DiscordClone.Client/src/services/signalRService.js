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
        .configureLogging(signalR.LogLevel.Error) // Only log errors, not info
        .build();
    }

    // Start the connection if it's not already connected
    if (connection.state !== signalR.HubConnectionState.Connected) {
      await connection.start();
    }

    return connection;
  } catch (error) {
    throw new Error(`SignalR connection failed: ${error.message}`);
  }
}

// Join a chat room
export async function joinChatRoom(roomId) {
  if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
    return false;
  }

  try {
    await connection.invoke('JoinChatRoom', roomId.toString());
    return true;
  } catch (error) {
    return false;
  }
}

// Leave a chat room
export async function leaveChatRoom(roomId) {
  if (!connection || connection.state !== signalR.HubConnectionState.Connected) {
    return false;
  }

  try {
    await connection.invoke('LeaveChatRoom', roomId.toString());
    return true;
  } catch (error) {
    return false;
  }
}

// Set up event handler for receiving messages
export function onReceiveMessage(callback) {
  if (!connection) return;
  connection.on('ReceiveMessage', callback);
}

// Remove event handler for receiving messages
export function offReceiveMessage() {
  if (!connection) return;
  connection.off('ReceiveMessage');
}

// Set up event handler for message deletion
export function onMessageDeleted(callback) {
  if (!connection) return;
  connection.on('MessageDeleted', callback);
}

// Remove event handler for message deletion
export function offMessageDeleted() {
  if (!connection) return;
  connection.off('MessageDeleted');
}

// Set up event handler for when user is added to a chat room
export function onUserAddedToChat(callback) {
  if (!connection) return;
  connection.on('UserAddedToChat', callback);
}

// Remove event handler for when user is added to a chat room
export function offUserAddedToChat() {
  if (!connection) return;
  connection.off('UserAddedToChat');
}

// Set up event handler for when user is removed from a chat room
export function onUserRemovedFromChat(callback) {
  if (!connection) return;
  connection.on('UserRemovedFromChat', callback);
}

// Remove event handler for when user is removed from a chat room
export function offUserRemovedFromChat() {
  if (!connection) return;
  connection.off('UserRemovedFromChat');
}

// Set up event handler for when a user joins a chat room
export function onUserJoinedRoom(callback) {
  if (!connection) return;
  connection.on('UserJoinedRoom', callback);
}

// Remove event handler for when a user joins a chat room
export function offUserJoinedRoom() {
  if (!connection) return;
  connection.off('UserJoinedRoom');
}

// Set up event handler for when a user leaves a chat room
export function onUserLeftRoom(callback) {
  if (!connection) return;
  connection.on('UserLeftRoom', callback);
}

// Remove event handler for when a user leaves a chat room
export function offUserLeftRoom() {
  if (!connection) return;
  connection.off('UserLeftRoom');
}

// Close SignalR connection
export function closeConnection() {
  if (connection) {
    connection.stop();
    connection = null;
  }
} 