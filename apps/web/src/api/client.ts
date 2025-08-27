import { problemDetailsSchema, type ProblemDetails } from './problem-details';

export class ApiError extends Error {
  public problem: ProblemDetails;
  constructor(problem: ProblemDetails) {
    super(problem.title ?? 'Request failed');
    this.problem = problem;
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
