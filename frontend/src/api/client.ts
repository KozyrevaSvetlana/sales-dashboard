/** Ошибка API с текстом из ProblemDetails, пригодным для показа пользователю. */
export class ApiError extends Error {
  readonly status: number;

  constructor(message: string, status: number) {
    super(message);
    this.name = 'ApiError';
    this.status = status;
  }
}

interface ProblemDetails {
  title?: string;
  detail?: string;
}

export type QueryParams = Record<string, string | undefined>;

export async function getJson<T>(path: string, params: QueryParams = {}, signal?: AbortSignal): Promise<T> {
  const query = new URLSearchParams();
  for (const [key, value] of Object.entries(params)) {
    if (value !== undefined) query.set(key, value);
  }

  let response: Response;
  try {
    response = await fetch(`/api${path}?${query.toString()}`, {
      signal,
      headers: { Accept: 'application/json' },
    });
  } catch (error) {
    if (signal?.aborted) throw error; // отмену пробрасываем как есть — React Query её распознает
    throw new ApiError('Сервер недоступен. Проверьте соединение и попробуйте ещё раз.', 0);
  }

  if (!response.ok) {
    const problem = (await response.json().catch(() => null)) as ProblemDetails | null;
    throw new ApiError(problem?.detail ?? problem?.title ?? `Ошибка сервера (${response.status})`, response.status);
  }

  return (await response.json()) as T;
}
