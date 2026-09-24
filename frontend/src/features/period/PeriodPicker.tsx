import { useState } from 'react';
import { PERIOD_PRESETS, type Period } from '../../shared/period';

interface PeriodPickerProps {
  value: Period;
  onChange: (period: Period) => void;
}

export function PeriodPicker({ value, onChange }: PeriodPickerProps) {
  const [customFrom, setCustomFrom] = useState(value.from ?? '');
  const [customTo, setCustomTo] = useState(value.to ?? '');
  const [showCustom, setShowCustom] = useState(value.preset === 'Custom');

  const customInvalid = !customFrom || !customTo || customTo < customFrom;

  return (
    <div className="period-picker">
      <div className="segmented" role="radiogroup" aria-label="Период">
        {PERIOD_PRESETS.map((preset) => {
          const active = preset.value === 'Custom' ? showCustom : !showCustom && value.preset === preset.value;
          return (
            <button
              key={preset.value}
              type="button"
              role="radio"
              aria-checked={active}
              className={active ? 'segmented__item is-active' : 'segmented__item'}
              onClick={() => {
                if (preset.value === 'Custom') {
                  setShowCustom(true);
                  return;
                }
                setShowCustom(false);
                onChange({ preset: preset.value });
              }}
            >
              {preset.label}
            </button>
          );
        })}
      </div>

      {showCustom && (
        <form
          className="custom-range"
          onSubmit={(event) => {
            event.preventDefault();
            if (!customInvalid) onChange({ preset: 'Custom', from: customFrom, to: customTo });
          }}
        >
          <input type="date" aria-label="С" value={customFrom} max={customTo || undefined} onChange={(e) => setCustomFrom(e.target.value)} />
          <span aria-hidden="true">—</span>
          <input type="date" aria-label="По" value={customTo} min={customFrom || undefined} onChange={(e) => setCustomTo(e.target.value)} />
          <button type="submit" className="button button--primary" disabled={customInvalid}>
            Применить
          </button>
        </form>
      )}
    </div>
  );
}
