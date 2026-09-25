import { useCallback, useLayoutEffect, useRef } from 'react';
import { prefersReducedMotion } from './motion';

type Key = string | number;

/**
 * FLIP-анимация перестановки строк списка (First, Last, Invert, Play):
 * запоминаем положение строк до обновления, после рендера сдвигаем каждую туда,
 * где она была, и плавно возвращаем на новое место. Без сторонних библиотек.
 *
 * Использование: `const flipRef = useFlipList(ids)` и `<tr ref={flipRef(id)}>`.
 */
export function useFlipList<T extends HTMLElement>(keys: readonly Key[]) {
  const nodes = useRef(new Map<Key, T>());
  const previousTops = useRef(new Map<Key, number>());
  const order = keys.join('|');

  useLayoutEffect(() => {
    const reduceMotion = prefersReducedMotion();
    const currentTops = new Map<Key, number>();

    nodes.current.forEach((element, key) => {
      const top = element.getBoundingClientRect().top;
      currentTops.set(key, top);

      const previousTop = previousTops.current.get(key);
      if (reduceMotion || previousTop === undefined || previousTop === top) return;

      element.style.transition = 'none';
      element.style.transform = `translateY(${previousTop - top}px)`;
      requestAnimationFrame(() => {
        element.style.transition = 'transform 400ms cubic-bezier(0.2, 0, 0, 1)';
        element.style.transform = '';
      });
    });

    previousTops.current = currentTops;
  }, [order]);

  return useCallback(
    (key: Key) => (element: T | null) => {
      if (element) nodes.current.set(key, element);
      else nodes.current.delete(key);
    },
    [],
  );
}
