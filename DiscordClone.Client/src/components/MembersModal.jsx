import React, { useEffect } from 'react';
import '../styles/MembersModal.css';

function MembersModal({ isOpen, onClose, members, currentUserId, currentUsername, roomName, onRemoveUser }) {
  if (!isOpen) return null;

  // Debug values
  useEffect(() => {
    if (isOpen) {
      console.log("Current user ID:", currentUserId);
      console.log("Current username:", currentUsername);
      console.log("Members:", members);
    }
  }, [isOpen, currentUserId, currentUsername, members]);

  return (
    <div className="modal-overlay">
      <div className="modal-content members-modal">
        <h2>Members in {roomName}</h2>
        <div className="members-list">
          {members.length === 0 ? (
            <div className="no-members">No members found</div>
          ) : (
            members.map(member => {
              // Check if this is the current user by ID or username
              const isCurrentUser = 
                String(member.id) === String(currentUserId) || 
                member.username === currentUsername;
              
              return (
                <div key={member.id} className="member-item">
                  <div className="member-name">
                    {member.username}
                    {isCurrentUser && " (You)"}
                  </div>
                  {/* Only show Remove button for other users */}
                  {!isCurrentUser && (
                    <button 
                      className="remove-user-btn"
                      onClick={() => onRemoveUser(member.username)}
                    >
                      Remove
                    </button>
                  )}
                </div>
              );
            })
          )}
        </div>
        <div className="modal-buttons">
          <button type="button" onClick={onClose}>Close</button>
        </div>
      </div>
    </div>
  );
}

export default MembersModal; 