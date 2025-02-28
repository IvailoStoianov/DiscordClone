import { useState, useEffect } from 'react'
import './App.css'
import ChatRoom from './components/ChatRoom'
import { loginUser, logoutUser, checkLocalSession } from './services/api'
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
    try {
      const session = checkLocalSession();
      if (session) {
        setIsLoggedIn(true);
        setUserData(session);
        setUsername(session.username || '');
      }
    } catch (error) {
      console.error("Error checking local session:", error);
      // Clear any potentially corrupted data
      localStorage.removeItem('userSession');
    } finally {
      setIsLoading(false);
    }
  }, []);

  const showToast = (message, type = 'error') => {
    setToast({ message, type });
  };

  const clearToast = () => {
    setToast({ message: '', type: 'error' });
  };

  const handleJoin = async () => {
    if (username.trim()) {
      try {
        const userData = await loginUser(username.trim());
        setIsLoggedIn(true);
        setUserData(userData);
        setError('');
      } catch (error) {
        setError(error.message || 'Login failed');
        showToast(error.message || 'Login failed', 'error');
      }
    }
  };

  const handleLogout = async () => {
    try {
      await logoutUser();
    } catch (error) {
      console.error('Logout error:', error);
      showToast('Logout failed, but session cleared locally', 'info');
    } finally {
      // Always logout locally even if API call fails
      setIsLoggedIn(false);
      setUserData(null);
      setUsername('');
      setError('');
    }
  };

  if (isLoading) {
    return <div className="welcome-container">Loading...</div>;
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
          placeholder="Enter your username"
          value={username}
          onChange={(e) => setUsername(e.target.value)}
          onKeyPress={(e) => e.key === 'Enter' && handleJoin()}
        />
        <button onClick={handleJoin}>Join</button>
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
