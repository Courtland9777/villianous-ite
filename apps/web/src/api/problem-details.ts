import { z } from 'zod';

export const problemDetailsSchema = z.object({
  type: z.string().optional(),
  title: z.string().optional(),
  status: z.number().optional(),
  detail: z.string().optional(),
  instance: z.string().optional(),
  code: z.string().optional(),
  traceId: z.string().optional(),
});

export type ProblemDetails = z.infer<typeof problemDetailsSchema>;
