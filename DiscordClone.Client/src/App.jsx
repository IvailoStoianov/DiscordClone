import { useState, useEffect } from 'react'
import './App.css'
import ChatRoom from './components/ChatRoom'
import { loginUser, logoutUser, checkLocalSession } from './services/api'

function App() {
  const [username, setUsername] = useState('')
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const [error, setError] = useState('')
  const [userData, setUserData] = useState(null)
  const [isLoading, setIsLoading] = useState(true)

  // Check for existing session on component mount
  useEffect(() => {
    const session = checkLocalSession();
    if (session) {
      setIsLoggedIn(true);
      setUserData(session);
      setUsername(session.username);
    }
    setIsLoading(false);
  }, []);

  const handleJoin = async () => {
    if (username.trim()) {
      try {
        const userData = await loginUser(username.trim());
        setIsLoggedIn(true);
        setUserData(userData);
        setError('');
      } catch (error) {
        setError(error.message || 'Login failed');
      }
    }
  };

  const handleLogout = async () => {
    try {
      await logoutUser();
      setIsLoggedIn(false);
      setUserData(null);
      setUsername('');
      setError('');
    } catch (error) {
      console.error('Logout error:', error);
      // Still logout locally even if the API call fails
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
    return <ChatRoom userData={userData} onLogout={handleLogout} />;
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
    </div>
  );
}

export default App;
