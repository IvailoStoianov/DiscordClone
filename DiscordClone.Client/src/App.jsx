import { useState, useEffect } from 'react'
import './App.css'
import ChatRoom from './components/ChatRoom'
import { loginUser, logoutUser, checkLocalSession, verifyAuthentication } from './services/api'
import Toast from './components/Toast'

function App() {
  const [username, setUsername] = useState('')
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const [error, setError] = useState('')
  const [userData, setUserData] = useState(null)
  const [isLoading, setIsLoading] = useState(true)
  const [toast, setToast] = useState({ message: '', type: 'error' })

  // Check for existing session on component mount
  useEffect(() => {
    const verifySession = async () => {
      try {
        // First check local storage for faster initial load
        const localSession = checkLocalSession();
        
        // Verify with server if session is valid
        const { isAuthenticated, userData: serverUserData } = await verifyAuthentication();
        
        if (isAuthenticated) {
          // If server says we're authenticated, use that data
          setIsLoggedIn(true);
          
          // If we have server data, use it; otherwise fall back to local session
          if (serverUserData) {
            setUserData(serverUserData);
            setUsername(serverUserData.username || '');
            // Update local storage with latest server data
            localStorage.setItem('userSession', JSON.stringify(serverUserData));
          } else if (localSession) {
            setUserData(localSession);
            setUsername(localSession.username || '');
          }
        } else {
          // Not authenticated, clear local storage
          localStorage.removeItem('userSession');
          console.log("Not authenticated, showing login screen");
        }
      } catch (error) {
        console.error("Error checking session:", error);
        // Clear any potentially corrupted data
        localStorage.removeItem('userSession');
      } finally {
        setIsLoading(false);
      }
    };

    // Set a timeout to ensure loading state doesn't get stuck
    const loadingTimeout = setTimeout(() => {
      if (isLoading) {
        console.log("Loading timeout - forcing login screen");
        setIsLoading(false);
      }
    }, 5000); // 5 second timeout

    verifySession();
    
    // Clear the timeout on cleanup
    return () => clearTimeout(loadingTimeout);
  }, []);

  const showToast = (message, type = 'error') => {
    setToast({ message, type });
  };

  const clearToast = () => {
    setToast({ message: '', type: 'error' });
  };

  const handleJoin = async () => {
    if (!username.trim()) {
      setError('Please enter a username');
      showToast('Please enter a username', 'error');
      return;
    }
    
    if (username.trim().length < 3) {
      setError('Username must be at least 3 characters long');
      showToast('Username must be at least 3 characters long', 'error');
      return;
    }
    
    if (username.trim().length > 50) {
      setError('Username cannot exceed 50 characters');
      showToast('Username cannot exceed 50 characters', 'error');
      return;
    }
    
    try {
      setIsLoading(true);
      setError('');
      
      const userData = await loginUser(username.trim());
      
      if (userData) {
        setIsLoggedIn(true);
        setUserData(userData);
        showToast(`Welcome, ${username}!`, 'success');
      } else {
        throw new Error('Login failed - no user data returned');
      }
    } catch (error) {
      console.error('Login error in handleJoin:', error);
      setError(error.message || 'Login failed');
      showToast(error.message || 'Login failed', 'error');
      setIsLoggedIn(false);
      setUserData(null);
    } finally {
      setIsLoading(false);
    }
  };

  const handleLogout = async () => {
    try {
      setIsLoading(true);
      await logoutUser();
      showToast('Logged out successfully', 'success');
    } catch (error) {
      console.error('Logout error:', error);
      showToast('Logout failed, but session cleared locally', 'info');
    } finally {
      // Always logout locally even if API call fails
      setIsLoggedIn(false);
      setUserData(null);
      setUsername('');
      setError('');
      localStorage.removeItem('userSession');
      setIsLoading(false);
    }
  };

  if (isLoading) {
    return (
      <div className="welcome-container">
        <div className="loading-spinner"></div>
        <p>Connecting to server...</p>
      </div>
    );
  }

  if (isLoggedIn && userData) {
    return (
      <>
        <ChatRoom userData={userData} onLogout={handleLogout} />
        {toast.message && (
          <Toast 
            message={toast.message} 
            type={toast.type} 
            onClose={clearToast} 
          />
        )}
      </>
    );
  }

  return (
    <div className="welcome-container">
      <h1>Welcome to DiscordClone</h1>
      <div className="join-form">
        <input
          type="text"
          placeholder="Enter your username (3-50 characters)"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          onKeyPress={(e) => e.key === 'Enter' && handleJoin()}
          disabled={isLoading}
        />
        <button 
          onClick={handleJoin}
          disabled={isLoading}
        >
          {isLoading ? 'Connecting...' : 'Join'}
        </button>
        {error && <div className="error-message">{error}</div>}
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

export default App;
