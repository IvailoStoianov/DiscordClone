const API_BASE_URL = 'https://localhost:7001/api';

// Export API base URL for use in components
export { API_BASE_URL };

// Helper function for API requests
async function fetchWithErrorHandling(url, options = {}) {
  try {
    console.log(`Making request to: ${url}`, options);
    
    const response = await fetch(url, {
      ...options,
      credentials: 'include',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        ...(options.headers || {})
      }
    });

    console.log(`Response status: ${response.status}`);
    
    // Check if there's any content
    const contentType = response.headers.get("content-type");
    console.log(`Content type: ${contentType}`);
    
    if (!response.ok) {
      let errorMessage = `Request failed with status ${response.status}`;
      
      if (contentType && contentType.includes("application/json")) {
        try {
          const errorData = await response.json();
          console.log("Error response data:", errorData);
          errorMessage = errorData.message || errorMessage;
        } catch (parseError) {
          console.error("Error parsing error response:", parseError);
        }
      } else if (contentType && contentType.includes("text")) {
        try {
          const errorText = await response.text();
          console.log("Error response text:", errorText);
          errorMessage = errorText || errorMessage;
        } catch (parseError) {
          console.error("Error reading error response text:", parseError);
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
        const data = await response.json();
        console.log("Response data:", data);
        return data;
      } catch (parseError) {
        console.error("Error parsing JSON response:", parseError);
        throw new Error("Invalid JSON response from server");
      }
    } 
    
    // Return text for non-JSON responses
    const text = await response.text();
    console.log("Response text:", text);
    return text;
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
    console.log(`Attempting to login with username: ${username}, length: ${username.length}`);
    
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
    
    console.log("Login successful, response:", response);
    
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
      console.log("Fetched chats after login:", chats);
      
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
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatRoomId}/users/${username}`, {
    method: 'POST',
    body: JSON.stringify({ username })
  });
}

export async function getMessages(chatRoomId) {
  try {
    // Use the chat/{id} endpoint which should return the chat with messages
    const chat = await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatRoomId}`);
    
    // Log the response to see what we're getting
    console.log("Full chat room response:", chat);
    
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

// This function now serves as both a data fetcher and auth validator
export const loadChatRooms = async () => {
  try {
    const response = await fetch(`${API_BASE_URL}/chat`, {
      method: 'GET',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include', 
    });
    
    // If we get a 401 Unauthorized, the session is invalid
    if (response.status === 401) {
      console.log('Session expired or invalid');
      return false;
    }
    
    if (!response.ok) {
      console.error('Error loading chat rooms:', response.status);
      return false;
    }
    
    // If we got a successful response, the session is valid
    return true;
  } catch (error) {
    console.error('Authentication verification failed:', error);
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

export async function removeUserFromChat(chatRoomId, username) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatRoomId}/users/${username}`, {
    method: 'DELETE'
  });
}

export async function deleteChat(chatId) {
  return await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatId}?chatId=${chatId}`, {
    method: 'DELETE'
  });
}

// Add a function to verify authentication
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
      console.error('Error verifying authentication:', response.status);
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
    console.error('Authentication verification failed:', error);
    return { isAuthenticated: false };
  }
} 