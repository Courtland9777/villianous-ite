import { problemDetailsSchema, type ProblemDetails } from './problem-details';

export class ApiError extends Error {
  constructor(public problem: ProblemDetails) {
    super(problem.title ?? 'Request failed');
  }
}

export async function fetchJson<T>(input: RequestInfo, init?: RequestInit): Promise<T> {
  const response = await fetch(input, init);
  if (!response.ok) {
    let problem: ProblemDetails = { status: response.status };
    try {
      const data = await response.json();
      problem = problemDetailsSchema.parse(data);
    } catch {
      // ignore parse failures
    }
    throw new ApiError(problem);
  }
  return (await response.json()) as T;
}
