import { useEffect, useRef, useState } from 'react';
import { RU_DATE_PLACEHOLDER, formatRuDate, maskRuDate, parseRuDate } from '../ruDate';

interface DateInputProps {
  /** Дата в ISO (yyyy-MM-dd) или '' — пока дата не введена полностью. */
  value: string;
  onChange: (isoDate: string) => void;
  label: string;
  min?: string;
  max?: string;
}

/**
 * Поле даты в русском формате дд.мм.гггг.
 *
 * Нативный <input type="date"> показывает формат по языку браузера (у англоязычного Chrome — mm/dd/yyyy),
 * и изменить это со страницы нельзя. Поэтому ввод — обычное текстовое поле с маской,
 * а кнопка календаря открывает тот же встроенный календарь браузера (скрытый input type="date").
 */
export function DateInput({ value, onChange, label, min, max }: DateInputProps) {
  const [text, setText] = useState(formatRuDate(value));
  const pickerRef = useRef<HTMLInputElement>(null);

  // Дату выбрали в календаре (или сбросили снаружи) — показываем её в поле.
  // Пока пользователь печатает неполную дату, value = '' — текст не трогаем.
  useEffect(() => {
    if (value && value !== parseRuDate(text)) setText(formatRuDate(value));
  }, [value]);

  const parsed = parseRuDate(text);
  const outOfRange = parsed !== null && ((min !== undefined && parsed < min) || (max !== undefined && parsed > max));
  const invalid = (text.length === 10 && parsed === null) || outOfRange;

  const openCalendar = () => {
    try {
      pickerRef.current?.showPicker();
    } catch {
      // Старые браузеры без showPicker: остаётся ручной ввод.
    }
  };

  return (
    <span className={invalid ? 'date-input is-invalid' : 'date-input'}>
      <input
        type="text"
        inputMode="numeric"
        autoComplete="off"
        placeholder={RU_DATE_PLACEHOLDER}
        aria-label={label}
        aria-invalid={invalid}
        maxLength={10}
        value={text}
        onChange={(event) => {
          const masked = maskRuDate(event.target.value);
          setText(masked);
          onChange(parseRuDate(masked) ?? '');
        }}
      />
      <button type="button" className="date-input__calendar" aria-label={`${label}: выбрать в календаре`} onClick={openCalendar}>
        <svg width="16" height="16" viewBox="0 0 24 24" aria-hidden="true">
          <path
            fill="currentColor"
            d="M7 2v2H5a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6a2 2 0 0 0-2-2h-2V2h-2v2H9V2H7zm-2 7h14v10H5V9z"
          />
        </svg>
      </button>
      <input
        ref={pickerRef}
        type="date"
        className="date-input__native"
        tabIndex={-1}
        aria-hidden="true"
        value={value}
        min={min}
        max={max}
        onChange={(event) => onChange(event.target.value)}
      />
    </span>
  );
}
