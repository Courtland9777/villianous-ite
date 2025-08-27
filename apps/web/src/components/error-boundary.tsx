import { Component, type ReactNode } from 'react';
import { ApiError } from '../api/client';

interface Props {
  children: ReactNode;
}

interface State {
  error: Error | null;
}

export class ErrorBoundary extends Component<Props, State> {
  state: State = { error: null };

  static getDerivedStateFromError(error: Error): State {
    return { error };
  }

  render() {
    const error = this.state.error;
    if (error) {
      const problem = error instanceof ApiError ? error.problem : undefined;
      return (
        <div role="alert" className="bg-red-100 text-red-800 p-4">
          <p>{problem?.title ?? error.message}</p>
          {problem?.code && <p className="text-xs">Code: {problem.code}</p>}
          {problem?.traceId && (
            <p className="text-xs">Trace ID: {problem.traceId}</p>
          )}
        </div>
      );
    }

    return this.props.children;
  }
}
