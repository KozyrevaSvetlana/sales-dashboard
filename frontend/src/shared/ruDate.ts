// Ввод даты в русском формате дд.мм.гггг. Внутри приложения и в API даты — ISO (yyyy-MM-dd).

export const RU_DATE_PLACEHOLDER = 'дд.мм.гггг';

/** Оставляет только цифры и сам расставляет точки: «24092026» → «24.09.2026». */
export function maskRuDate(raw: string): string {
  const digits = raw.replace(/\D/g, '').slice(0, 8);
  const parts = [digits.slice(0, 2), digits.slice(2, 4), digits.slice(4, 8)].filter(Boolean);
  return parts.join('.');
}

/** «24.09.2026» → «2026-09-24». null — если дата неполная или не существует (31.02, 00.13 и т. п.). */
export function parseRuDate(text: string): string | null {
  const match = /^(\d{2})\.(\d{2})\.(\d{4})$/.exec(text);
  if (!match) return null;

  const [, dd, mm, yyyy] = match;
  const day = Number(dd);
  const month = Number(mm);
  const year = Number(yyyy);
  const date = new Date(Date.UTC(year, month - 1, day));
  const exists = date.getUTCFullYear() === year && date.getUTCMonth() === month - 1 && date.getUTCDate() === day;

  return exists && year >= 1900 ? `${yyyy}-${mm}-${dd}` : null;
}

/** «2026-09-24» → «24.09.2026»; пустая строка — для пустого значения. */
export function formatRuDate(iso: string | undefined): string {
  const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(iso ?? '');
  return match ? `${match[3]}.${match[2]}.${match[1]}` : '';
}
