import { create } from 'zustand';

interface MatchState {
  matchId: string | null;
  setMatchId: (id: string) => void;
}

export const useMatchStore = create<MatchState>((set) => ({
  matchId: null,
  setMatchId: (matchId) => set({ matchId }),
}));
