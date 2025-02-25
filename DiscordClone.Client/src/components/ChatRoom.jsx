import { useState, useEffect } from 'react'
import '../styles/ChatRoom.css'
import logo from '../assets/logo.png'
import { getAllChats, createChat, postMessage, getChatRoomMembers, getMessages } from '../services/api'
import Toast from './Toast'

function ChatRoom({ userData, onLogout }) {
  const [chatRooms, setChatRooms] = useState([])
  const [messages, setMessages] = useState([])
  const [newMessage, setNewMessage] = useState('')
  const [showCreateForm, setShowCreateForm] = useState(false)
  const [showAddUserForm, setShowAddUserForm] = useState(false)
  const [newChatRoomName, setNewChatRoomName] = useState('')
  const [newUser, setNewUser] = useState('')
  const [selectedRoom, setSelectedRoom] = useState(null)
  const [showMembersModal, setShowMembersModal] = useState(false)
  const [editingMessageId, setEditingMessageId] = useState(null)
  const [editMessageText, setEditMessageText] = useState('')
  const [error, setError] = useState(null)
  const [toast, setToast] = useState({ message: '', type: 'error' })
  const [roomMembers, setRoomMembers] = useState([]);
  
  // Mock data for chat members - replace this with actual data from your API
  const [chatMembers] = useState([
    { id: 1, username: 'User1' },
    { id: 2, username: 'User2' },
    { id: 3, username: 'User3' },
  ])

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
    loadChatRooms();
  }, [userData.id]);

  const loadChatRooms = async () => {
    try {
      const chats = await getAllChats(userData.id);
      setChatRooms(chats);
    } catch (error) {
      showToast('Failed to load chats');
      console.error(error);
    }
  };

  const handleCreateRoom = async (e) => {
    e.preventDefault();
    if (!newChatRoomName.trim()) return;

    try {
      const newChat = await createChat(newChatRoomName);
      setChatRooms([...chatRooms, newChat]);
      setNewChatRoomName('');
      setShowCreateForm(false);
      showToast('Chat room created successfully', 'success');
    } catch (error) {
      showToast('Failed to create chat');
      console.error(error);
    }
  };

  const handleSendMessage = async (e) => {
    e.preventDefault();
    if (!newMessage.trim() || !selectedRoom) return;

    try {
      const userSession = JSON.parse(localStorage.getItem('userSession'));
      await postMessage(newMessage, selectedRoom.id, userSession.userId);
      
      // Clear input and refresh messages
      setNewMessage('');
      loadMessages(selectedRoom.id);
    } catch (error) {
      console.error('Failed to send message:', error);
      showToast('Failed to send message');
    }
  };

  const handleAddUser = () => {
    if (newUser.trim()) {
      // Here you would typically make an API call to add the user
      console.log(`Adding user ${newUser} to room ${selectedRoom.id}`)
      setNewUser('')
      setShowAddUserForm(false)
      showToast('User added to chat', 'success');
    }
  }

  const handleEditMessage = (messageId) => {
    const message = messages.find(m => m.id === messageId)
    if (message) {
      setEditingMessageId(messageId)
      setEditMessageText(message.text)
    }
  }

  const handleSaveEdit = (messageId) => {
    setMessages(messages.map(message => 
      message.id === messageId 
        ? { ...message, text: editMessageText }
        : message
    ))
    setEditingMessageId(null)
    setEditMessageText('')
  }

  const handleDeleteMessage = (messageId) => {
    setMessages(messages.filter(message => message.id !== messageId))
  }

  // Add function to load members
  const loadMembers = async (roomId) => {
    if (!roomId) return;
    
    try {
      const members = await getChatRoomMembers(roomId);
      setRoomMembers(members);
    } catch (error) {
      console.error('Failed to load members:', error);
      showToast('Failed to load chat members');
    }
  };
  
  // Update handleRoomSelect to also load members
  const handleRoomSelect = async (room) => {
    setSelectedRoom(room);
    await loadMessages(room.id);
    await loadMembers(room.id);
  };

  const loadMessages = async (roomId) => {
    try {
      // This function isn't implemented yet in your API, so I'll create a stub
      // You'll need to implement getMessages in your api.js file
      const messagesData = await getMessages(roomId);
      setMessages(messagesData);
    } catch (error) {
      console.error('Failed to load messages:', error);
      showToast('Failed to load messages');
    }
  };

  return (
    <div className="chat-container">
      {/* Add Toast component */}
      <Toast 
        message={toast.message}
        type={toast.type}
        onClose={clearToast}
      />
      
      {/* Sidebar with chat rooms */}
      <div className="sidebar">
        <div className="create-room">
          <button onClick={() => setShowCreateForm(true)}>Create Chat Room</button>
        </div>
        
        <div className="rooms-list">
          {chatRooms.map(room => (
            <div 
              key={room.id} 
              className={`room-item ${selectedRoom?.id === room.id ? 'active' : ''}`}
            >
              <div 
                className="room-name"
                onClick={() => handleRoomSelect(room)}
              >
                # {room.name}
              </div>
              <button 
                className="add-user-btn"
                onClick={() => {
                  setSelectedRoom(room);
                  setShowAddUserForm(true);
                }}
              >
                +
              </button>
            </div>
          ))}
        </div>
      </div>

      {/* Main chat area */}
      <div className="chat-area">
        <div className="chat-header">
          <h2>{selectedRoom ? `# ${selectedRoom.name}` : 'Select a room'}</h2>
          <div className="header-buttons">
            <button 
              className="members-btn"
              onClick={() => setShowMembersModal(true)}
              disabled={!selectedRoom}
            >
              Members
            </button>
            <button className="logout-btn" onClick={onLogout}>
              Logout
            </button>
          </div>
        </div>
        
        <div className="messages">
          {messages.map((message, index) => {
            const showUserInfo = index === 0 || 
              messages[index - 1].user.username !== message.user.username;

            return (
              <div key={message.id} className={`message ${showUserInfo ? '' : 'consecutive'}`}>
                {showUserInfo && (
                  <img 
                    src={message.user.avatarUrl} 
                    alt="avatar" 
                    className="user-avatar"
                  />
                )}
                <div className="message-content">
                  {showUserInfo && (
                    <div className="message-header">
                      <span className="username">{message.user.username}</span>
                      <span className="timestamp">{message.timestamp}</span>
                    </div>
                  )}
                  {editingMessageId === message.id ? (
                    <div className="edit-message">
                      <input
                        type="text"
                        value={editMessageText}
                        onChange={(e) => setEditMessageText(e.target.value)}
                        onKeyPress={(e) => e.key === 'Enter' && handleSaveEdit(message.id)}
                      />
                      <div className="edit-buttons">
                        <button onClick={() => handleSaveEdit(message.id)}>Save</button>
                        <button onClick={() => setEditingMessageId(null)}>Cancel</button>
                      </div>
                    </div>
                  ) : (
                    <div className="message-text">
                      {message.text}
                    </div>
                  )}
                </div>
                <div className="message-actions">
                  <button 
                    className="edit-btn"
                    onClick={() => handleEditMessage(message.id)}
                  >
                    Edit
                  </button>
                  <button 
                    className="delete-btn"
                    onClick={() => handleDeleteMessage(message.id)}
                  >
                    Delete
                  </button>
                </div>
              </div>
            )
          })}
        </div>

        <div className="message-input">
          <form onSubmit={handleSendMessage}>
            <input
              type="text"
              placeholder={`Message #${selectedRoom?.name}`}
              value={newMessage}
              onChange={(e) => setNewMessage(e.target.value)}
            />
            <button type="submit">Send</button>
          </form>
        </div>
      </div>

      {/* Create room modal */}
      {showCreateForm && (
        <div className="modal-overlay">
          <div className="modal">
            <h2>Create New Chat Room</h2>
            <input
              type="text"
              placeholder="Chat room name"
              value={newChatRoomName}
              onChange={(e) => setNewChatRoomName(e.target.value)}
            />
            <div className="modal-buttons">
              <button onClick={handleCreateRoom}>Create</button>
              <button onClick={() => setShowCreateForm(false)}>Cancel</button>
            </div>
          </div>
        </div>
      )}

      {/* Add user modal */}
      {showAddUserForm && (
        <div className="modal-overlay">
          <div className="modal">
            <h2>Add User to {selectedRoom?.name}</h2>
            <input
              type="text"
              placeholder="Enter username"
              value={newUser}
              onChange={(e) => setNewUser(e.target.value)}
            />
            <div className="modal-buttons">
              <button onClick={handleAddUser}>Add User</button>
              <button onClick={() => setShowAddUserForm(false)}>Cancel</button>
            </div>
          </div>
        </div>
      )}

      {/* Members Modal */}
      {showMembersModal && (
        <div className="modal-overlay">
          <div className="modal-content">
            <h3>Chat Room Members</h3>
            <div className="members-list">
              {roomMembers && roomMembers.length > 0 ? (
                roomMembers.map(member => (
                  <div key={member.id} className="member-item">
                    <span className="member-username">{member.username}</span>
                  </div>
                ))
              ) : (
                <p>No members found or loading...</p>
              )}
            </div>
            <div className="modal-buttons">
              <button onClick={() => setShowMembersModal(false)}>Close</button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}

export default ChatRoom 