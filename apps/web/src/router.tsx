import { createBrowserRouter } from 'react-router-dom';
import { App } from './app.tsx';

export const router = createBrowserRouter([
  { path: '/', element: <App /> },
]);
