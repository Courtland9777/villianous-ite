import { usePromptsStore } from '../../stores/prompts.store';

export function PromptModal() {
  const prompt = usePromptsStore((s) => s.prompt);
  const clear = usePromptsStore((s) => s.clearPrompt);

  if (!prompt) return null;

  return (
    <div
      role="dialog"
      aria-modal="true"
      className="fixed inset-0 flex items-center justify-center bg-black/50"
    >
      <div className="rounded bg-white p-4">
        <p className="mb-4">{prompt.message}</p>
        <button
          type="button"
          className="rounded border px-2 py-1"
          onClick={clear}
        >
          Close
        </button>
      </div>
    </div>
  );
}
