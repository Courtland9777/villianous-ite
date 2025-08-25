import './app.css';
import { Hand } from './features/hand/hand';
import { Realm } from './features/realm/realm';

export function App() {
  return (
    <main className="p-4">
      <h1 className="text-2xl font-bold">Villainous</h1>
      <Realm
        locations={[
          { id: 'l1', name: 'Location 1' },
          { id: 'l2', name: 'Location 2' },
          { id: 'l3', name: 'Location 3' },
          { id: 'l4', name: 'Location 4' },
        ]}
      />
      <Hand cards={['Card A', 'Card B', 'Card C']} />
    </main>
  );
}
