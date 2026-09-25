import { useEffect, useRef, useState } from 'react';
import { prefersReducedMotion } from './motion';

const easeOutCubic = (t: number) => 1 - (1 - t) ** 3;

/**
 * Плавно «докручивает» число до нового значения (при первом показе — от нуля).
 * null не анимируется: «нет значения» показывается сразу.
 */
export function useCountUp(target: number | null, durationMs = 700): number | null {
  const [value, setValue] = useState<number | null>(target === null ? null : 0);
  const currentRef = useRef<number | null>(target === null ? null : 0);

  useEffect(() => {
    if (target === null || prefersReducedMotion()) {
      currentRef.current = target;
      setValue(target);
      return;
    }

    const from = currentRef.current ?? 0;
    if (from === target) {
      setValue(target);
      return;
    }

    let frame = 0;
    const startedAt = performance.now();
    const tick = (now: number) => {
      const progress = Math.min(1, (now - startedAt) / durationMs);
      const next = from + (target - from) * easeOutCubic(progress);
      currentRef.current = next;
      setValue(next);
      if (progress < 1) frame = requestAnimationFrame(tick);
    };
    frame = requestAnimationFrame(tick);

    return () => cancelAnimationFrame(frame);
  }, [target, durationMs]);

  return value;
}
