import { create } from 'zustand';

interface Prompt {
  id: string;
  message: string;
}

interface PromptsState {
  prompt: Prompt | null;
  showPrompt: (prompt: Prompt) => void;
  clearPrompt: () => void;
}

export const usePromptsStore = create<PromptsState>((set) => ({
  prompt: null,
  showPrompt: (prompt) => set({ prompt }),
  clearPrompt: () => set({ prompt: null }),
}));
