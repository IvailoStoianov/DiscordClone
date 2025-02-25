const API_BASE_URL = 'https://localhost:7001/api';

// Export API base URL for use in components
export { API_BASE_URL };

const defaultOptions = {
    credentials: 'include',
    headers: {
        'Content-Type': 'application/json'
    }
};

// Add error handling wrapper
const fetchWithErrorHandling = async (url, options = {}) => {
  try {
    const response = await fetch(url, {
      ...options,
      credentials: 'include', // Include cookies if using sessions
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json',
        ...(options.headers || {})
      }
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }

    return await response.json();
  } catch (error) {
    console.error('API request failed:', error);
    throw error;
  }
};

export async function loginUser(username) {
  try {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      credentials: 'include',
      body: JSON.stringify({ username }),
    });
    
    if (!response.ok) {
      throw new Error('Login failed');
    }
    
    // Store username in localStorage on successful login
    localStorage.setItem('username', username);
    
    // Return a basic user object
    return {
      username,
      isLoggedIn: true
    };
  } catch (error) {
    console.error('Login error:', error);
    throw error;
  }
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

// Instead of getUserSession, we'll check localStorage
export function checkLocalSession() {
  const username = localStorage.getItem('username');
  return username ? { username, isLoggedIn: true } : null;
}

export const clearUserSession = () => {
  localStorage.removeItem('userSession');
};

// Add logout API call
export async function logoutUser() {
  try {
    const response = await fetch(`${API_BASE_URL}/auth/logout`, {
      method: 'POST',
      credentials: 'include',
    });

    if (!response.ok) {
      throw new Error('Logout failed');
    }

    // Clear stored username on logout
    localStorage.removeItem('username');
    return true;
  } catch (error) {
    console.error('Logout error:', error);
    throw error;
  }
}

export const getAllChats = async () => {
  try {
    const response = await fetchWithErrorHandling(`${API_BASE_URL}/chat`);
    return response;
  } catch (error) {
    console.error('Failed to fetch chats:', error);
    throw error;
  }
};

export const createChat = async (chatName) => {
  try {
    const response = await fetchWithErrorHandling(`${API_BASE_URL}/chat`, {
      method: 'POST',
      body: JSON.stringify({
        name: chatName
      })
    });
    return response;
  } catch (error) {
    console.error('Failed to create chat:', error);
    throw error;
  }
};

export const postMessage = async (message, chatRoomId, userId) => {
  try {
    const response = await fetchWithErrorHandling(`${API_BASE_URL}/chat/message`, {
      method: 'POST',
      body: JSON.stringify({
        content: message,
        chatRoomId: chatRoomId,
        userId: userId
      })
    });
    return response;
  } catch (error) {
    console.error('Failed to post message:', error);
    throw error;
  }
};

// Add function to get chat room members
export async function getChatRoomMembers(chatRoomId) {
  try {
    const response = await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatRoomId}/members`, {
      method: 'GET'
    });
    return response;
  } catch (error) {
    console.error('Failed to fetch chat room members:', error);
    throw error;
  }
}

// Add function to get messages for a chat room
export async function getMessages(chatRoomId) {
  try {
    const response = await fetchWithErrorHandling(`${API_BASE_URL}/chat/${chatRoomId}/messages`, {
      method: 'GET'
    });
    return response;
  } catch (error) {
    console.error('Failed to fetch messages:', error);
    throw error;
  }
} 