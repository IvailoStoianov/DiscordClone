// Get the authentication token from localStorage
export const getAuthToken = () => {
  return localStorage.getItem('token');
};

// Store the authentication token in localStorage
export const setAuthToken = (token) => {
  localStorage.setItem('token', token);
};

// Remove the authentication token from localStorage
export const removeAuthToken = () => {
  localStorage.removeItem('token');
}; 