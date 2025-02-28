const API_BASE_URL = 'https://localhost:7001/api';

// Export API base URL for use in components
export { API_BASE_URL };

// Helper function for API requests
async function fetchWithErrorHandling(url, options = {}) {
  try {
    const response = await fetch(url, {
      ...options,
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        ...(options.headers || {})
      }
    });

    // Check if there's any content
    const contentType = response.headers.get("content-type");
    
    if (!response.ok) {
      if (contentType && contentType.includes("application/json")) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `Request failed with status ${response.status}`);
      } else {
        throw new Error(`Request failed with status ${response.status}`);
      }
    }

    // Handle empty responses
    if (response.status === 204 || response.headers.get("content-length") === "0") {
      return null;
    }

    // Only try to parse as JSON if it's actually JSON
    if (contentType && contentType.includes("application/json")) {
      return await response.json();
    } 
    
    // Return text for non-JSON responses
    return await response.text();
  } catch (error) {
    console.error('API request failed:', error);
    throw error;
  }
}

// Session management
export function checkLocalSession() {
  try {
    const session = localStorage.getItem('userSession');
    return session ? JSON.parse(session) : null;
  } catch (error) {
    console.error('Error checking local session:', error);
    localStorage.removeItem('userSession');
    return null;
  }
}

// Auth functions
export async function loginUser(username) {
  try {
    // Call login endpoint which sets the authentication cookie
    await fetchWithErrorHandling(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      body: JSON.stringify({ username })
    });
    
    // After login, try to get all chatrooms to extract user ID 
    try {
      const chats = await fetchWithErrorHandling(`${API_BASE_URL}/chat`);
      // Look for an owner ID in the chats to get a valid user ID
      const userId = chats && chats.length > 0 && chats[0].ownerId ? 
                     chats[0].ownerId : generateUuid();
      
      // Store more complete user data
      const userData = { 
        username,
        id: userId,
        isLoggedIn: true 
      };
      
      localStorage.setItem('userSession', JSON.stringify(userData));
      return userData;
    } catch (error) {
      console.error("Error getting chats after login:", error);
      // Fallback to a basic user object
      const userData = { 
        username,
        id: generateUuid(), // Generate a UUID as fallback
        isLoggedIn: true 
      };
      localStorage.setItem('userSession', JSON.stringify(userData));
      return userData;
    }
  } catch (error) {
    console.error('Login error:', error);
    throw error;
  }
}

// Helper function to generate UUID
function generateUuid() {
  return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function(c) {
    var r = Math.random() * 16 | 0, v = c === 'x' ? r : (r & 0x3 | 0x8);
    return v.toString(16);
  });
}

export async function logoutUser() {
  await fetchWithErrorHandling(`${API_BASE_URL}/auth/logout`, {
    method: 'POST'
  });
  
  localStorage.removeItem('userSession');
}

// Chat functions
export async function getAllChats(userId) {
  try {
    // Try the direct /chat endpoint first
    const response = await fetchWithErrorHandling(`${API_BASE_URL}/chat`);
    return response;
  } catch (error) {
    console.error('Failed to fetch chats:', error);
    // Return empty array instead of throwing to prevent errors in the UI
    return [];
  }
}

export async function createChat(name) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat`, {
    method: 'POST',
    body: JSON.stringify({ name })
  });
}

export async function addUserToChat(chatRoomId, username) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatRoomId}/members`, {
    method: 'POST',
    body: JSON.stringify({ username })
  });
}

export async function getMessages(chatRoomId) {
  try {
    // Use the chat/{id} endpoint which should return the chat with messages
    const chat = await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatRoomId}`);
    
    // Return the messages array from the chat object
    return chat && chat.messages ? chat.messages : [];
  } catch (error) {
    console.error('Failed to fetch messages:', error);
    return [];
  }
}

export async function getChatRoomMembers(chatRoomId) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatRoomId}/members`);
}

export async function postMessage(chatRoomId, content) {
  // Get current user ID from stored session
  let userId = null;
  try {
    const session = localStorage.getItem('userSession');
    if (session) {
      const userData = JSON.parse(session);
      userId = userData.id;
    }
  } catch (error) {
    console.error("Error getting user data from session:", error);
  }
  
  // Make sure chatRoomId is a valid string format
  const chatId = typeof chatRoomId === 'object' ? chatRoomId.id : chatRoomId;
  
  console.log("Posting message with:", {
    chatRoomId: chatId,
    content,
    userId
  });
  
  try {
    // Include the userId in the message payload
    const response = await fetchWithErrorHandling(`${API_BASE_URL}/chat/message`, {
      method: 'POST',
      body: JSON.stringify({
        chatRoomId: chatId,
        content: content,
        userId: userId || '00000000-0000-0000-0000-000000000000' // Using default GUID as fallback
      })
    });
    
    return response;
  } catch (error) {
    console.error("Error posting message:", error);
    throw error;
  }
}

export async function editMessage(messageId, content) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat/messages/${messageId}`, {
    method: 'PUT',
    body: JSON.stringify({ content })
  });
}

export async function deleteMessage(messageId) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat/messages/${messageId}`, {
    method: 'DELETE'
  });
}

export const loadChatRooms = async () => {
  try {
    const response = await fetch(`${API_BASE_URL}/chat`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include', 
    });
    
    if (!response.ok) {
      throw new Error('Login failed');
    }
    
    return true;
  } catch (error) {
    console.error('Login error:', error);
    return false;
  }
};

export const saveUserSession = (userData) => {
  localStorage.setItem('userSession', JSON.stringify(userData));
};

export const clearUserSession = () => {
  localStorage.removeItem('userSession');
};

// Add createChatRoom function
export async function createChatRoom(name) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat`, {
    method: 'POST',
    body: JSON.stringify({ name })
  });
} 