import { useEffect } from 'react';
import '../styles/Toast.css';

function Toast({ message, type = 'error', onClose, duration = 5000 }) {
  useEffect(() => {
    if (message) {
      const timer = setTimeout(() => {
        onClose();
      }, duration);
      
      return () => clearTimeout(timer);
    }
  }, [message, duration, onClose]);

  if (!message) return null;

  return (
    <div className={`toast-container ${type}`}>
      <div className="toast-content">
        <span className="toast-message">{message}</span>
        <button className="toast-close" onClick={onClose}>×</button>
      </div>
    </div>
  );
}

export default Toast; 