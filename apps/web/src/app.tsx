import './app.css';
import { Hand } from './features/hand/hand';

export function App() {
  return (
    <main className="p-4">
      <h1 className="text-2xl font-bold">Villainous</h1>
      <Hand cards={['Card A', 'Card B', 'Card C']} />
    </main>
  );
}
