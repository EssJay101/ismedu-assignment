export interface ApiError {
  status: number;
  message: string;
  errors?: Record<string, string[]> | null;
  traceId?: string | null;
}