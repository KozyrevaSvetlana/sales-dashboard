import type { ReactNode } from 'react';
import type { UseQueryResult } from '@tanstack/react-query';
import { EmptyState, ErrorState } from './States';

interface AsyncBlockProps<T> {
  query: UseQueryResult<T, Error>;
  /** Что показать при первой загрузке — скелетон той же формы, что и контент. */
  skeleton: ReactNode;
  /** Когда данные есть, но показывать нечего (например, нет продаж за период). */
  isEmpty?: (data: T) => boolean;
  emptyMessage?: string;
  children: (data: T) => ReactNode;
}

/**
 * Одна реализация всех обязательных состояний блока: первая загрузка, обновление при смене периода,
 * ошибка с повтором, пустые данные. Каждый блок дашборда просто оборачивается в AsyncBlock —
 * так не бывает «вечного спиннера» или внезапно пустого блока без объяснения.
 */
export function AsyncBlock<T>({
  query,
  skeleton,
  isEmpty,
  emptyMessage = 'За выбранный период данных нет',
  children,
}: AsyncBlockProps<T>) {
  if (query.isPending) {
    return <div aria-busy="true">{skeleton}</div>;
  }

  if (query.isError) {
    return <ErrorState message={query.error.message} onRetry={() => void query.refetch()} />;
  }

  const { data } = query;
  if (isEmpty?.(data)) {
    return <EmptyState message={emptyMessage} />;
  }

  // При смене периода показываем предыдущие данные приглушёнными, а не мигаем скелетоном.
  const isRefreshing = query.isFetching && query.isPlaceholderData;
  return (
    <div className={isRefreshing ? 'async-block is-refreshing' : 'async-block'} aria-busy={isRefreshing}>
      {children(data)}
    </div>
  );
}
