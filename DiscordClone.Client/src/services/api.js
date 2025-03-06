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
      let errorMessage = `Request failed with status ${response.status}`;
      
      if (contentType && contentType.includes("application/json")) {
        try {
          const errorData = await response.json();
          errorMessage = errorData.message || errorMessage;
        } catch (parseError) {
          // Silent failure on parse error, use default message
        }
      } else if (contentType && contentType.includes("text")) {
        try {
          const errorText = await response.text();
          errorMessage = errorText || errorMessage;
        } catch (parseError) {
          // Silent failure on parse error, use default message
        }
      }
      
      throw new Error(errorMessage);
    }

    // Handle empty responses
    if (response.status === 204 || response.headers.get("content-length") === "0") {
      return null;
    }

    // Only try to parse as JSON if it's actually JSON
    if (contentType && contentType.includes("application/json")) {
      try {
        return await response.json();
      } catch (parseError) {
        throw new Error("Invalid JSON response from server");
      }
    } 
    
    // Return text for non-JSON responses
    return await response.text();
  } catch (error) {
    throw error;
  }
}

// Session management
export function checkLocalSession() {
  try {
    const session = localStorage.getItem('userSession');
    return session ? JSON.parse(session) : null;
  } catch (error) {
    localStorage.removeItem('userSession');
    return null;
  }
}

// Auth functions
export async function loginUser(username) {
  try {
    // Validate username length according to server requirements (3-50 characters)
    if (username.length < 3) {
      throw new Error("Username must be at least 3 characters long");
    }
    
    if (username.length > 50) {
      throw new Error("Username cannot exceed 50 characters");
    }
    
    // Call login endpoint which sets the authentication cookie
    const response = await fetchWithErrorHandling(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      body: JSON.stringify({ username })
    });
    
    // If the server returned user data directly, use it
    if (response && response.id) {
      const userData = {
        username: response.username,
        id: response.id,
        isLoggedIn: true
      };
      
      localStorage.setItem('userSession', JSON.stringify(userData));
      return userData;
    }
    
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

// Verify user's authentication status
export async function verifyAuthentication() {
  try {
    const response = await fetch(`${API_BASE_URL}/auth/verify`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
    });
    
    if (response.status === 401) {
      return { isAuthenticated: false };
    }
    
    if (!response.ok) {
      return { isAuthenticated: false };
    }
    
    const data = await response.json();
    return { 
      isAuthenticated: true,
      userData: {
        id: data.id,
        username: data.username,
        isLoggedIn: true
      }
    };
  } catch (error) {
    return { isAuthenticated: false };
  }
}

// Chat functions
export async function getAllChats() {
  try {
    return await fetchWithErrorHandling(`${API_BASE_URL}/chat`);
  } catch (error) {
    return [];
  }
}

export async function createChatRoom(name) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat`, {
    method: 'POST',
    body: JSON.stringify({ name })
  });
}

export async function addUserToChat(chatRoomId, username) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatRoomId}/users/${username}`, {
    method: 'POST'
  });
}

export async function removeUserFromChat(chatRoomId, username) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatRoomId}/users/${username}`, {
    method: 'DELETE'
  });
}

export async function getMessages(chatRoomId) {
  try {
    const chat = await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatRoomId}`);
    return chat && chat.messages ? chat.messages : [];
  } catch (error) {
    return [];
  }
}

export async function getChatRoomMembers(chatRoomId) {
  try {
    return await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatRoomId}/members`);
  } catch (error) {
    return [];
  }
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
    // Silent failure for session read error
  }
  
  // Make sure chatRoomId is a valid string format
  const chatId = typeof chatRoomId === 'object' ? chatRoomId.id : chatRoomId;
  
  // Include the userId in the message payload
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat/message`, {
    method: 'POST',
    body: JSON.stringify({
      chatRoomId: chatId,
      content: content,
      userId: userId || '00000000-0000-0000-0000-000000000000' // Using default GUID as fallback
    })
  });
}

export async function deleteMessage(messageId) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat/message/${messageId}`, {
    method: 'DELETE'
  });
}

export async function deleteChat(chatId) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatId}`, {
    method: 'DELETE'
  });
} 