interface ErrorStateProps {
  message: string;
  onRetry: () => void;
}

export function ErrorState({ message, onRetry }: ErrorStateProps) {
  return (
    <div className="state state--error" role="alert">
      <p className="state__title">Не удалось загрузить данные</p>
      <p className="state__text">{message}</p>
      <button type="button" className="button" onClick={onRetry}>
        Повторить
      </button>
    </div>
  );
}

export function EmptyState({ message }: { message: string }) {
  return (
    <div className="state state--empty">
      <p className="state__title">Пока пусто</p>
      <p className="state__text">{message}</p>
    </div>
  );
}

export function Skeleton({ height = 16, width = '100%' }: { height?: number; width?: number | string }) {
  return <span className="skeleton" style={{ height, width }} />;
}
