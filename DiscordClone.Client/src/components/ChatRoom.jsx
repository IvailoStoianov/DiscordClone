import { useState, useEffect } from 'react'
import '../styles/ChatRoom.css'
import { getAllChats, getMessages, getChatRoomMembers, postMessage, createChatRoom, addUserToChat, removeUserFromChat } from '../services/api'
import Toast from './Toast'
import CreateChatModal from './CreateChatModal'
import AddUserModal from './AddUserModal'
import MembersModal from './MembersModal'

function ChatRoom({ userData, onLogout }) {
  const [chatRooms, setChatRooms] = useState([])
  const [messages, setMessages] = useState([])
  const [newMessage, setNewMessage] = useState('')
  const [selectedRoom, setSelectedRoom] = useState(null)
  const [toast, setToast] = useState({ message: '', type: 'error' })
  const [isLoading, setIsLoading] = useState(false)
  const [roomMembers, setRoomMembers] = useState([])
  const [isCreateModalOpen, setIsCreateModalOpen] = useState(false)
  const [isAddUserModalOpen, setIsAddUserModalOpen] = useState(false)
  const [selectedRoomForUser, setSelectedRoomForUser] = useState(null)
  const [isMembersModalOpen, setIsMembersModalOpen] = useState(false)

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

  const handleCreateChat = async (name) => {
    try {
      setIsLoading(true);
      await createChatRoom(name);
      await loadChatRooms(); // Reload the chat rooms list
      showToast('Chat room created successfully', 'success');
    } catch (error) {
      console.error('Failed to create chat room:', error);
      showToast('Failed to create chat room');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddUser = async (username) => {
    if (!selectedRoomForUser) return;
    
    try {
      setIsLoading(true);
      await addUserToChat(selectedRoomForUser.id, username);
      await loadMembers(selectedRoomForUser.id); // Reload members list
      showToast('User added successfully', 'success');
    } catch (error) {
      console.error('Failed to add user:', error);
      showToast('Failed to add user');
    } finally {
      setIsLoading(false);
    }
  };

  const handleAddUserClick = (room) => {
    setSelectedRoomForUser(room);
    setIsAddUserModalOpen(true);
  };

  const handleRemoveUser = async (username) => {
    if (!selectedRoom) return;
    
    try {
      setIsLoading(true);
      await removeUserFromChat(selectedRoom.id, username);
      await loadMembers(selectedRoom.id); // Reload members list
      showToast(`User ${username} removed from chat`, 'success');
    } catch (error) {
      console.error('Failed to remove user:', error);
      showToast('Failed to remove user');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="chat-container">
      {/* Sidebar */}
      <div className="sidebar">
        <div className="create-room">
          <button onClick={() => setIsCreateModalOpen(true)}>
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
                  onClick={() => handleAddUserClick(room)}
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
              onClick={() => setIsMembersModalOpen(true)}
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
                messages.map((message, index) => {
                  // Check if this message is from the current user
                  const isCurrentUser = String(message.userId) === String(userData?.id);
                  
                  // Format the timestamp
                  let formattedTime = 'Unknown time';
                  if (message.timestamp) {
                    try {
                      const date = new Date(message.timestamp);
                      formattedTime = date.toLocaleString();
                    } catch (error) {
                      console.error("Error parsing timestamp:", error);
                    }
                  } else if (message.formattedTimestamp) {
                    formattedTime = message.formattedTimestamp;
                  }
                  
                  return (
                    <div 
                      key={message.id} 
                      className={`message ${isCurrentUser ? 'message-self' : 'message-other'}`}
                    >
                      {!isCurrentUser && (
                        <img 
                          src="/src/assets/logo.png" 
                          alt="User Avatar" 
                          className="user-avatar" 
                        />
                      )}
                      <div className="message-content">
                        <div className="message-header">
                          <span className="username">
                            {isCurrentUser ? 'You' : message.userName}
                          </span>
                          <span className="timestamp">{formattedTime}</span>
                        </div>
                        <div className="message-text">{message.content}</div>
                      </div>
                      {isCurrentUser && (
                        <img 
                          src="/src/assets/logo.png" 
                          alt="Your Avatar" 
                          className="user-avatar" 
                        />
                      )}
                    </div>
                  );
                })
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

      <CreateChatModal
        isOpen={isCreateModalOpen}
        onClose={() => setIsCreateModalOpen(false)}
        onCreateChat={handleCreateChat}
      />

      <AddUserModal
        isOpen={isAddUserModalOpen}
        onClose={() => setIsAddUserModalOpen(false)}
        onAddUser={handleAddUser}
        roomName={selectedRoomForUser?.name || ''}
      />

      <MembersModal
        isOpen={isMembersModalOpen}
        onClose={() => setIsMembersModalOpen(false)}
        members={roomMembers}
        currentUserId={userData?.id}
        currentUsername={userData?.username}
        roomName={selectedRoom?.name || ''}
        onRemoveUser={handleRemoveUser}
      />
    </div>
  );
}

export default ChatRoom; 