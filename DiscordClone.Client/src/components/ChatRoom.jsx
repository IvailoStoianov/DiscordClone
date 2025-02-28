import { useState, useEffect } from 'react'
import '../styles/ChatRoom.css'
import { getAllChats, getMessages, getChatRoomMembers, postMessage } from '../services/api'
import Toast from './Toast'

function ChatRoom({ userData, onLogout }) {
  const [chatRooms, setChatRooms] = useState([])
  const [messages, setMessages] = useState([])
  const [newMessage, setNewMessage] = useState('')
  const [selectedRoom, setSelectedRoom] = useState(null)
  const [toast, setToast] = useState({ message: '', type: 'error' })
  const [isLoading, setIsLoading] = useState(false)
  const [roomMembers, setRoomMembers] = useState([])

  // Show toast notification
  const showToast = (message, type = 'error') => {
    setToast({ message, type });
  };

  // Clear toast notification
  const clearToast = () => {
    setToast({ message: '', type: 'error' });
  };

  // Load chat rooms when component mounts
  useEffect(() => {
    if (userData && userData.id) {
      loadChatRooms();
    }
  }, [userData]);

  const loadChatRooms = async () => {
    setIsLoading(true);
    try {
      const chats = await getAllChats(userData.id);
      console.log("Chat rooms loaded:", chats);
      
      // Make sure we always have an array, even if the API returns something else
      setChatRooms(Array.isArray(chats) ? chats : []);
    } catch (error) {
      console.error("Failed to load chats:", error);
      showToast('Failed to load chats');
      setChatRooms([]);
    } finally {
      setIsLoading(false);
    }
  };

  const loadMessages = async (roomId) => {
    if (!roomId) return;
    
    setIsLoading(true);
    try {
      const messagesData = await getMessages(roomId);
      console.log("Messages loaded for room", roomId, ":", messagesData);
      
      // Make sure we're setting an array to state
      setMessages(Array.isArray(messagesData) ? messagesData : []);
    } catch (error) {
      console.error('Failed to load messages:', error);
      showToast('Failed to load messages, but you can still chat');
      setMessages([]);
    } finally {
      setIsLoading(false);
    }
  };

  const loadMembers = async (roomId) => {
    if (!roomId) return;
    
    try {
      const members = await getChatRoomMembers(roomId);
      setRoomMembers(members || []);
    } catch (error) {
      showToast('Failed to load room members');
      console.error(error);
      setRoomMembers([]);
    }
  };

  const handleRoomSelect = async (room) => {
    setSelectedRoom(room);
    await loadMessages(room.id);
    await loadMembers(room.id);
  };

  const handleSendMessage = async (e) => {
    e.preventDefault();
    if (!newMessage.trim() || !selectedRoom) return;

    try {
      setIsLoading(true);
      await postMessage(selectedRoom.id, newMessage);
      
      // Clear the input field
      setNewMessage('');
      
      // Reload messages to show the new message
      await loadMessages(selectedRoom.id);
      
      // Success toast
      showToast('Message sent', 'success');
    } catch (error) {
      console.error('Failed to send message:', error);
      showToast('Failed to send message');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="chat-container">
      {/* Sidebar */}
      <div className="sidebar">
        <div className="create-room">
          <button onClick={() => showToast('Create room feature coming soon', 'info')}>
            Create New Room
          </button>
        </div>
        <div className="rooms-list">
          {isLoading && chatRooms.length === 0 ? (
            <div style={{ padding: '16px', color: '#8e9297' }}>Loading...</div>
          ) : chatRooms.length > 0 ? (
            chatRooms.map(room => (
              <div
                key={room.id}
                className={`room-item ${selectedRoom?.id === room.id ? 'active' : ''}`}
              >
                <div
                  className="room-name"
                  onClick={() => handleRoomSelect(room)}
                >
                  {room.name}
                </div>
                <button 
                  className="add-user-btn"
                  onClick={() => showToast('Add user feature coming soon', 'info')}
                >
                  +
                </button>
              </div>
            ))
          ) : (
            <div style={{ padding: '16px', color: '#8e9297' }}>No rooms available</div>
          )}
        </div>
      </div>

      {/* Chat Area */}
      <div className="chat-area">
        <div className="chat-header">
          <h2>{selectedRoom ? selectedRoom.name : 'Select a room'}</h2>
          <div className="header-buttons">
            <button
              className="members-btn"
              disabled={!selectedRoom}
              onClick={() => showToast(`Room has ${roomMembers.length} members`, 'info')}
            >
              Members
            </button>
            <button className="logout-btn" onClick={onLogout}>
              Logout
            </button>
          </div>
        </div>

        {!selectedRoom ? (
          <div className="no-room-selected">
            <p>Select a room to start chatting</p>
          </div>
        ) : (
          <>
            <div className="messages">
              {isLoading ? (
                <div style={{ padding: '16px', color: '#8e9297', alignSelf: 'center' }}>
                  Loading messages...
                </div>
              ) : messages.length > 0 ? (
                messages.map((message, index) => (
                  <div key={message.id} className="message">
                    <img 
                      src="https://via.placeholder.com/40" 
                      alt="User Avatar" 
                      className="user-avatar" 
                    />
                    <div className="message-content">
                      <div className="message-header">
                        <span className="username">{message.username || 'Unknown User'}</span>
                        <span className="timestamp">{message.formattedTimestamp || 'Unknown time'}</span>
                      </div>
                      <div className="message-text">{message.content}</div>
                    </div>
                  </div>
                ))
              ) : (
                <div style={{ padding: '16px', color: '#8e9297', alignSelf: 'center' }}>
                  No messages yet
                </div>
              )}
            </div>

            <div className="message-input">
              <form onSubmit={handleSendMessage}>
                <input
                  type="text"
                  placeholder={selectedRoom ? `Message #${selectedRoom.name}` : "Select a room first"}
                  value={newMessage}
                  onChange={(e) => setNewMessage(e.target.value)}
                  disabled={!selectedRoom}
                />
                <button type="submit" disabled={!selectedRoom || !newMessage.trim()}>Send</button>
              </form>
            </div>
          </>
        )}
      </div>

      {toast.message && (
        <Toast 
          message={toast.message} 
          type={toast.type} 
          onClose={clearToast} 
        />
      )}
    </div>
  );
}

export default ChatRoom; 