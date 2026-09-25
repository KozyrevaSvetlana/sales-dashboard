import { useEffect, useRef, useState } from 'react';

/** Ширина контейнера для адаптивного SVG-графика (обновляется при изменении размера окна). */
export function useElementWidth<T extends HTMLElement>(fallback = 800) {
  const ref = useRef<T>(null);
  const [width, setWidth] = useState(fallback);

  useEffect(() => {
    const element = ref.current;
    if (!element || typeof ResizeObserver === 'undefined') return;

    const observer = new ResizeObserver(([entry]) => {
      if (entry) setWidth(Math.max(320, Math.round(entry.contentRect.width)));
    });
    observer.observe(element);
    return () => observer.disconnect();
  }, []);

  return [ref, width] as const;
}
