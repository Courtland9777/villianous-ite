import { describe, it, expect, vi, afterEach } from 'vitest';
import { createHubConnection } from './signalr';
import { useMatchStore } from '../stores/match.store';

type MockConnection = {
  on: ReturnType<typeof vi.fn>;
  onreconnected: (cb: () => void) => void;
  invoke: ReturnType<typeof vi.fn>;
  _reconnected?: () => void;
};

vi.mock('@microsoft/signalr', () => {
  const connection: MockConnection = {
    on: vi.fn(),
    onreconnected: (cb) => {
      connection._reconnected = cb;
    },
    invoke: vi.fn(),
  };
  return {
    HubConnectionBuilder: class {
      withUrl() {
        return this;
      }
      withAutomaticReconnect() {
        return this;
      }
      build() {
        return connection;
      }
    },
    __connection: connection,
  };
});

import { __connection } from '@microsoft/signalr';

afterEach(() => {
  useMatchStore.setState({ matchId: null });
  vi.clearAllMocks();
});

describe('createHubConnection', () => {
  it('rejoins match after reconnect', async () => {
    useMatchStore.getState().setMatchId('match-1');
    createHubConnection('/hub');
    const handler = (__connection as MockConnection)
      ._reconnected as () => Promise<void>;
    expect(typeof handler).toBe('function');
    await handler();
    expect(__connection.invoke).toHaveBeenCalledWith('JoinMatch', 'match-1');
  });
});
