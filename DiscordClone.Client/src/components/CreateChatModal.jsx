import React, { useState } from 'react';
import '../styles/CreateChatModal.css';

function CreateChatModal({ isOpen, onClose, onCreateChat }) {
  const [chatName, setChatName] = useState('');

  const handleSubmit = (e) => {
    e.preventDefault();
    if (chatName.trim()) {
      onCreateChat(chatName);
      setChatName('');
      onClose();
    }
  };

  if (!isOpen) return null;

  return (
    <div className="modal-overlay">
      <div className="modal-content">
        <h2>Create New Chat Room</h2>
        <form onSubmit={handleSubmit}>
          <input
            type="text"
            value={chatName}
            onChange={(e) => setChatName(e.target.value)}
            placeholder="Enter chat room name"
            required
          />
          <div className="modal-buttons">
            <button type="button" onClick={onClose}>Cancel</button>
            <button type="submit">Create</button>
          </div>
        </form>
      </div>
    </div>
  );
}

export default CreateChatModal; 