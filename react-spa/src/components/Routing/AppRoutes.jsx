import { Routes, Route, Navigate } from 'react-router-dom';

function ProtectedRoute({ children }) {
    // Fetch authentication status here...
    const isLoggedIn = true; 

    return isLoggedIn ? children : <Navigate to="/login" />;
}

const AppRoutes = () => {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />
      <Route path="/main" element={<ProtectedRoute><Main /></ProtectedRoute>} />
    </Routes>
  );
}

export default AppRoutes;