import { http, HttpResponse } from 'msw';
import { describe, expect, it } from 'vitest';
import { server } from '../test-server';
import { fetchJson } from './client';

describe('fetchJson', () => {
  it('throws ApiError with ProblemDetails on error responses', async () => {
    const problem = { status: 400, title: 'Bad Request', traceId: 'abc' };
    server.use(
      http.get('/test', () => HttpResponse.json(problem, { status: 400 })),
    );

    await expect(fetchJson('/test')).rejects.toMatchObject({ problem });
  });
});
