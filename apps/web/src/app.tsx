import './app.css';
import { useEffect } from 'react';
import { Hand } from './features/hand/hand';
import { Realm } from './features/realm/realm';
import { PromptModal } from './features/prompt/prompt-modal';
import { usePromptsStore } from './stores/prompts.store';

export function App() {
  const showPrompt = usePromptsStore((s) => s.showPrompt);

  useEffect(() => {
    showPrompt({ id: 'p1', message: 'Sample prompt' });
  }, [showPrompt]);

  return (
    <main className="p-4">
      <h1 className="text-2xl font-bold">Villainous</h1>
      <Realm
        locations={[
          {
            id: 'l1',
            name: 'Location 1',
            spots: [
              { id: 'l1s1', label: 'Gain 1 Power' },
              { id: 'l1s2', label: 'Play a Card' },
            ],
          },
          {
            id: 'l2',
            name: 'Location 2',
            spots: [
              { id: 'l2s1', label: 'Fate' },
              { id: 'l2s2', label: 'Move an Item' },
            ],
          },
          {
            id: 'l3',
            name: 'Location 3',
            spots: [
              { id: 'l3s1', label: 'Gain 2 Power' },
              { id: 'l3s2', label: 'Vanquish' },
            ],
          },
          {
            id: 'l4',
            name: 'Location 4',
            spots: [
              { id: 'l4s1', label: 'Discard' },
              { id: 'l4s2', label: 'Play a Card' },
            ],
          },
        ]}
      />
      <Hand cards={['Card A', 'Card B', 'Card C']} />
      <PromptModal />
    </main>
  );
}
