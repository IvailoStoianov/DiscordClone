const API_BASE_URL = import.meta.env.DEV 
  ? 'https://localhost:7001'  // Development
  : '/api';                   // Production

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

export const loginUser = async (username) => {
  return fetchWithErrorHandling(`${API_BASE_URL}/api/auth/login`, {
    method: 'POST',
    body: JSON.stringify({ username })
  });
};

export const loadChatRooms = async () => {
  const response = await fetch(`${API_BASE_URL}/api/chat/chatrooms`);
  return response.json();
};

export const saveUserSession = (userData) => {
  localStorage.setItem('userSession', JSON.stringify(userData));
};

export const getUserSession = () => {
  const session = localStorage.getItem('userSession');
  return session ? JSON.parse(session) : null;
};

export const clearUserSession = () => {
  localStorage.removeItem('userSession');
};

// Add logout API call
export const logoutUser = async (username) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/auth/logout`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Accept': 'application/json'
      },
      body: JSON.stringify(username)
    });

    if (!response.ok) {
      throw new Error('Logout failed');
    }

    clearUserSession();
  } catch (error) {
    console.error('Logout error:', error);
    // Still clear the local session even if the API call fails
    clearUserSession();
    throw error;
  }
};

export const getAllChats = async (userId) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/chat?userId=${userId}`);
    if (!response.ok) {
      throw new Error('Failed to fetch chats');
    }
    return await response.json();
  } catch (error) {
    throw error;
  }
};

export const createChat = async (chat, userId) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/chat`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        chat,
        userId
      })
    });
    if (!response.ok) {
      throw new Error('Failed to create chat');
    }
    return await response.json();
  } catch (error) {
    throw error;
  }
};

export const postMessage = async (message, userId) => {
  try {
    const response = await fetch(`${API_BASE_URL}/api/chat/message`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        message,
        userId
      })
    });
    if (!response.ok) {
      throw new Error('Failed to post message');
    }
    return await response.json();
  } catch (error) {
    throw error;
  }
}; 