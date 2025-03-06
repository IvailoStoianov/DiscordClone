import { useState, useEffect, useRef } from 'react'
import '../styles/ChatRoom.css'
import { getAllChats, getMessages, getChatRoomMembers, postMessage, createChatRoom, addUserToChat, removeUserFromChat, deleteChat, deleteMessage } from '../services/api'
import Toast from './Toast'
import CreateChatModal from './CreateChatModal'
import AddUserModal from './AddUserModal'
import MembersModal from './MembersModal'
import { 
  initializeSignalRConnection, 
  joinChatRoom, 
  leaveChatRoom, 
  onReceiveMessage,
  onMessageDeleted,
  offReceiveMessage,
  offMessageDeleted,
  onUserAddedToChat,
  offUserAddedToChat,
  onUserRemovedFromChat,
  offUserRemovedFromChat,
  onUserJoinedRoom,
  offUserJoinedRoom,
  onUserLeftRoom,
  offUserLeftRoom,
  closeConnection
} from '../services/signalRService'

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
  const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false)
  const [roomToDelete, setRoomToDelete] = useState(null)
  const [messageToDelete, setMessageToDelete] = useState(null)
  const [isDeleteMessageConfirmOpen, setIsDeleteMessageConfirmOpen] = useState(false)
  const [signalRConnected, setSignalRConnected] = useState(false)
  const [shouldScrollToBottom, setShouldScrollToBottom] = useState(false)
  
  // Reference for messages container to scroll only when needed
  const messagesContainerRef = useRef(null);
  
  // Function to scroll to bottom of messages
  const scrollToBottom = () => {
    if (messagesContainerRef.current) {
      const container = messagesContainerRef.current;
      container.scrollTop = container.scrollHeight;
    }
  };

  // Effect to scroll to bottom when needed
  useEffect(() => {
    if (shouldScrollToBottom) {
      scrollToBottom();
      setShouldScrollToBottom(false);
    }
  }, [shouldScrollToBottom]);
  
  // Clean up SignalR connection on component unmount
  useEffect(() => {
    return () => {
      closeConnection();
    };
  }, []);

  // Initialize SignalR connection when component mounts or userData changes
  useEffect(() => {
    let isMounted = true;
    
    const connectToSignalR = async () => {
      try {
        const connection = await initializeSignalRConnection();
        
        if (connection && isMounted) {
          setSignalRConnected(true);
        }
      } catch (error) {
        showToast('Failed to connect to real-time service', 'error');
      }
    };

    if (userData?.id) {
      connectToSignalR();
    }

    // Clean up function
    return () => {
      isMounted = false;
      offReceiveMessage();
      offMessageDeleted();
    };
  }, [userData]);

  // Set up SignalR message handlers when room changes
  useEffect(() => {
    if (!selectedRoom || !signalRConnected) return;
    
    const setupSignalRHandlers = async () => {
      try {
        // Join the chat room
        await joinChatRoom(selectedRoom.id);
        
        // Handle incoming messages
        onReceiveMessage((message) => {
          setMessages(prevMessages => {
            // Just make sure it's not a duplicate by ID
            const exists = prevMessages.some(m => m.id === message.id);
            if (exists) return prevMessages;
            
            // If we're at the bottom, schedule a scroll
            if (messagesContainerRef.current) {
              const container = messagesContainerRef.current;
              const atBottom = container.scrollHeight - container.clientHeight <= container.scrollTop + 50;
              if (atBottom) {
                setTimeout(scrollToBottom, 50);
              }
            }
            
            return [...prevMessages, message];
          });
        });

        // Handle deleted messages
        onMessageDeleted((messageId) => {
          setMessages(prevMessages => 
            prevMessages.filter(message => message.id !== messageId)
          );
        });
        
        // Handle user joining room (notification for existing members)
        onUserJoinedRoom((data) => {
          showToast(`${data.username} joined the chat`, 'info');
          
          // Reload room members to reflect the change
          loadMembers(selectedRoom.id);
        });
        
        // Handle user leaving room (notification for remaining members)
        onUserLeftRoom((data) => {
          showToast(`${data.username} left the chat`, 'info');
          
          // Reload room members to reflect the change
          loadMembers(selectedRoom.id);
        });
      } catch (error) {
        showToast('Error connecting to chat room', 'error');
      }
    };

    setupSignalRHandlers();

    // Clean up when changing rooms or unmounting
    return () => {
      if (selectedRoom) {
        leaveChatRoom(selectedRoom.id).catch(() => {
          // Silent failure on leaving room is acceptable
        });
      }
      offReceiveMessage();
      offMessageDeleted();
      offUserJoinedRoom();
      offUserLeftRoom();
    };
  }, [selectedRoom, signalRConnected]);

  // Set up SignalR user events that need to be handled globally (not tied to specific rooms)
  useEffect(() => {
    if (!signalRConnected) return;
    
    // Handle when the current user is added to a new chat room
    onUserAddedToChat((chatRoom) => {
      // Add the new chat room to the list if it's not already there
      setChatRooms(prevRooms => {
        const exists = prevRooms.some(room => room.id === chatRoom.id);
        if (exists) return prevRooms;
        
        showToast(`You were added to chat room: ${chatRoom.name}`, 'success');
        return [...prevRooms, chatRoom];
      });
    });
    
    // Handle when the current user is removed from a chat room
    onUserRemovedFromChat((chatRoomId) => {
      // Remove the chat room from the list
      setChatRooms(prevRooms => {
        const updatedRooms = prevRooms.filter(room => room.id !== chatRoomId);
        
        // If the removed room was selected, clear the selection
        if (selectedRoom && selectedRoom.id === chatRoomId) {
          setSelectedRoom(null);
          setMessages([]);
          showToast('You were removed from the active chat room', 'warning');
        } else {
          showToast('You were removed from a chat room', 'info');
        }
        
        return updatedRooms;
      });
    });
    
    return () => {
      offUserAddedToChat();
      offUserRemovedFromChat();
    };
  }, [signalRConnected, selectedRoom]);

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
      const chats = await getAllChats();
      
      // Make sure we always have an array, even if the API returns something else
      setChatRooms(Array.isArray(chats) ? chats : []);
    } catch (error) {
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
      
      // Make sure we're setting an array to state
      setMessages(Array.isArray(messagesData) ? messagesData : []);
      
      // Schedule scroll to bottom after messages load
      setShouldScrollToBottom(true);
      
    } catch (error) {
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
      setRoomMembers([]);
    }
  };

  const handleRoomSelect = async (room) => {
    // If we're already in a room, leave it first
    if (selectedRoom) {
      await leaveChatRoom(selectedRoom.id);
    }
    
    setSelectedRoom(room);
    await loadMessages(room.id);
    await loadMembers(room.id);
    
    // Join the new room via SignalR (this is also handled in the useEffect)
    if (signalRConnected) {
      await joinChatRoom(room.id);
    }
  };

  const handleSendMessage = async (e) => {
    e.preventDefault();
    
    if (!newMessage.trim() || !selectedRoom) return;

    try {
      setIsLoading(true);
      
      // Store the message content before clearing input
      const messageContent = newMessage;
      
      // Clear the input field immediately
      setNewMessage('');
      
      // Send the message to the server - the UI will update via SignalR
      await postMessage(selectedRoom.id, messageContent);
      
    } catch (error) {
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
      showToast('Failed to remove user');
    } finally {
      setIsLoading(false);
    }
  };

  const handleDeleteChat = async () => {
    if (!roomToDelete) return;
    
    try {
      setIsLoading(true);
      await deleteChat(roomToDelete.id);
      
      // Remove the chat from the list
      setChatRooms(prevRooms => prevRooms.filter(room => room.id !== roomToDelete.id));
      
      // If the deleted room was selected, clear the selection
      if (selectedRoom && selectedRoom.id === roomToDelete.id) {
        setSelectedRoom(null);
        setMessages([]);
      }
      
      showToast('Chat room deleted successfully', 'success');
    } catch (error) {
      showToast('Failed to delete chat room');
    } finally {
      setIsLoading(false);
      setIsDeleteConfirmOpen(false);
      setRoomToDelete(null);
    }
  };

  const handleDeleteClick = (e, room) => {
    e.stopPropagation(); // Prevent selecting the room when clicking delete
    setRoomToDelete(room);
    setIsDeleteConfirmOpen(true);
  };

  const handleDeleteMessage = async (messageId) => {
    setIsLoading(true);
    try {
      await deleteMessage(messageId);
      // No need to update the messages array locally - SignalR will handle it
      showToast('Message deleted successfully', 'success');
    } catch (error) {
      showToast('Failed to delete message');
    } finally {
      setIsLoading(false);
      setIsDeleteMessageConfirmOpen(false);
      setMessageToDelete(null);
    }
  };

  const handleDeleteMessageClick = (message) => {
    setMessageToDelete(message);
    setIsDeleteMessageConfirmOpen(true);
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
                <div className="room-actions">
                  <button 
                    className="add-user-btn"
                    onClick={() => handleAddUserClick(room)}
                    title="Add User"
                  >
                    +
                  </button>
                  <button 
                    className="delete-room-btn"
                    onClick={(e) => handleDeleteClick(e, room)}
                    title="Delete Room"
                  >
                    ×
                  </button>
                </div>
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
            <div 
              className="messages" 
              ref={messagesContainerRef}
            >
              {isLoading ? (
                <div style={{ padding: '16px', color: '#8e9297', alignSelf: 'center' }}>
                  Loading messages...
                </div>
              ) : messages.length > 0 ? (
                messages.map((message) => {
                  // Format the timestamp
                  let formattedTime = 'Unknown time';
                  if (message.timestamp) {
                    try {
                      const date = new Date(message.timestamp);
                      formattedTime = date.toLocaleString();
                    } catch (error) {
                      // Silent fail, use fallback
                    }
                  } else if (message.formattedTimestamp) {
                    formattedTime = message.formattedTimestamp;
                  }
                  
                  return (
                    <div 
                      key={message.id} 
                      className="message"
                    >
                      <img 
                        src="/src/assets/logo.png" 
                        alt="User Avatar" 
                        className="user-avatar" 
                      />
                      <div className="message-content">
                        <div className="message-header">
                          <span className="username">
                            {message.userName}
                          </span>
                          <span className="timestamp">{formattedTime}</span>
                        </div>
                        <div className="message-text">{message.content}</div>
                      </div>
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

      {/* Delete Chat Confirmation Modal */}
      {isDeleteConfirmOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <h2>Delete Chat Room</h2>
            <p>Are you sure you want to delete "{roomToDelete?.name}"?</p>
            <p className="warning-text">This action cannot be undone.</p>
            <div className="modal-buttons">
              <button 
                className="cancel-btn" 
                onClick={() => {
                  setIsDeleteConfirmOpen(false);
                  setRoomToDelete(null);
                }}
              >
                Cancel
              </button>
              <button 
                className="delete-btn" 
                onClick={handleDeleteChat}
                disabled={isLoading}
              >
                {isLoading ? 'Deleting...' : 'Delete'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Delete Message Confirmation Modal */}
      {isDeleteMessageConfirmOpen && (
        <div className="modal-overlay">
          <div className="modal-content">
            <h2>Delete Message</h2>
            <p>Are you sure you want to delete this message?</p>
            <div className="message-preview">"{messageToDelete?.content}"</div>
            <p className="warning-text">This action cannot be undone.</p>
            <div className="modal-buttons">
              <button 
                className="cancel-btn" 
                onClick={() => {
                  setIsDeleteMessageConfirmOpen(false);
                  setMessageToDelete(null);
                }}
              >
                Cancel
              </button>
              <button 
                className="delete-btn" 
                onClick={() => handleDeleteMessage(messageToDelete.id)}
                disabled={isLoading}
              >
                {isLoading ? 'Deleting...' : 'Delete'}
              </button>
            </div>
          </div>
        </div>
      )}

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