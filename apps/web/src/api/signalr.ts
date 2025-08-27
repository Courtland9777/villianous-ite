import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';
import { problemDetailsSchema } from './problem-details';
import { usePromptsStore } from '../stores/prompts.store';

export function createHubConnection(url: string): HubConnection {
  const connection = new HubConnectionBuilder()
    .withUrl(url)
    .withAutomaticReconnect()
    .build();

  connection.on('CommandRejected', (problem: unknown) => {
    const details = problemDetailsSchema.parse(problem);
    const parts = [details.title ?? 'Command rejected'];
    if (details.code) parts.push(`[${details.code}]`);
    if (details.traceId) parts.push(`(Trace ID: ${details.traceId})`);
    const message = parts.join(' ');
    usePromptsStore
      .getState()
      .showPrompt({ id: details.traceId ?? crypto.randomUUID(), message });
  });

  return connection;
}
