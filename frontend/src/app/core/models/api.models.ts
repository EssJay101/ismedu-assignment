export interface ApiError {
  status: number;
  message: string;
  errors?: Record<string, string[]> | null;
  traceId?: string | null;
}

export interface PagedResult<T> {
  items: T[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
}