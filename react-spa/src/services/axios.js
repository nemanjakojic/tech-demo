import axios from 'axios';

// Create an Axios instance
const api = axios.create({
  baseURL: 'http://localhost:5162/api', // base URL for all requests (place it to a configuration file)
  withCredentials: true,
  headers: {
    'Content-Type': 'application/json',
  }
});

export default api;
