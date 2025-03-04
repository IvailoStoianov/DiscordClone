import React, { useState } from 'react';
import '../styles/AddUserModal.css';

function AddUserModal({ isOpen, onClose, onAddUser, roomName }) {
  const [username, setUsername] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    if (username.trim()) {
      onAddUser(username);
      setUsername('');
      onClose();
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2>Add User to {roomName}</h2>
        <form onSubmit={handleSubmit}>
          <input
            type="text"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            placeholder="Enter username"
            required
          />
          <div className="modal-buttons">
            <button type="button" onClick={onClose}>Cancel</button>
            <button type="submit">Add User</button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default AddUserModal; 