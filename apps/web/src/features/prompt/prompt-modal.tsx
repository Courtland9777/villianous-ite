import { useEffect, useRef } from 'react';
import { usePromptsStore } from '../../stores/prompts.store';

export function PromptModal() {
  const prompt = usePromptsStore((s) => s.prompt);
  const clear = usePromptsStore((s) => s.clearPrompt);
  const closeRef = useRef<HTMLButtonElement>(null);

  useEffect(() => {
    if (prompt) {
      closeRef.current?.focus();
    }
  }, [prompt]);

  if (!prompt) return null;

  const handleKeyDown = (e: React.KeyboardEvent<HTMLDivElement>) => {
    if (e.key === 'Escape') {
      clear();
    }

    if (e.key === 'Tab') {
      e.preventDefault();
      closeRef.current?.focus();
    }
  };

  return (
    <div
      role="dialog"
      aria-modal="true"
      aria-labelledby="prompt-message"
      className="fixed inset-0 flex items-center justify-center bg-black/50"
      onKeyDown={handleKeyDown}
    >
      <div className="rounded bg-white p-4">
        <p id="prompt-message" className="mb-4">
          {prompt.message}
        </p>
        <button
          ref={closeRef}
          type="button"
          className="rounded border px-2 py-1"
          aria-label="Close prompt"
          onClick={clear}
        >
          Close
        </button>
      </div>
    </div>
  );
}
