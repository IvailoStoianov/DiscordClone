import { useState, useEffect } from 'react'
import './App.css'
import ChatRoom from './components/ChatRoom'
import { loginUser, getUserSession, logoutUser } from './services/api'

function App() {
  const [username, setUsername] = useState('')
  const [isLoggedIn, setIsLoggedIn] = useState(false)
  const [error, setError] = useState('')
  const [userData, setUserData] = useState(null)

  // Check for existing session on component mount
  useEffect(() => {
    const session = getUserSession();
    if (session) {
      setIsLoggedIn(true);
      setUserData(session);
      setUsername(session.username);
    }
  }, []);

  const handleJoin = async () => {
    if (username.trim()) {
      try {
        const data = await loginUser(username.trim());
        setUserData(data);
        setIsLoggedIn(true);
        setError('');
      } catch (error) {
        setError(error.message);
      }
    }
  };

  const handleLogout = async () => {
    try {
      await logoutUser(userData.username);
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

  if (isLoggedIn) {
    return <ChatRoom userData={userData} onLogout={handleLogout} />
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
  )
}

export default App
