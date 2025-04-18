import './App.css';
import { Router } from 'react-router-dom';
import AppRoutes from './components/Routing/AppRoutes';

function App() {    
  return (
    <Router>
      <AppRoutes />
    </Router>
  );
}

export default App
