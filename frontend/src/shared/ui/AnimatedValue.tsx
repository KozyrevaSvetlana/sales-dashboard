import { useCountUp } from '../hooks/useCountUp';

interface AnimatedValueProps {
  value: number | null;
  format: (value: number | null) => string;
}

/** Число, которое плавно меняется при смене периода. Для скринридеров — сразу итоговое значение. */
export function AnimatedValue({ value, format }: AnimatedValueProps) {
  const animated = useCountUp(value);
  return (
    <>
      <span aria-hidden="true">{format(animated)}</span>
      <span className="visually-hidden">{format(value)}</span>
    </>
  );
}
