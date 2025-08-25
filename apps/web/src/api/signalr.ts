import { HubConnection, HubConnectionBuilder } from '@microsoft/signalr';

export function createHubConnection(url: string): HubConnection {
  return new HubConnectionBuilder().withUrl(url).withAutomaticReconnect().build();
}
