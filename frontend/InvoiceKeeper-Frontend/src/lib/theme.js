const THEME_KEY = 'ik-theme';
const ACCENT_KEY = 'ik-accent';

export const THEMES = [
  { id: 'obsidian', label: 'Обсидиан', color: '#ff3b3b' },
  { id: 'noctra', label: 'Ноктра', color: '#7c5cff' },
  { id: 'emerald', label: 'Изумруд', color: '#2dd4bf' },
  { id: 'amber', label: 'Янтарь', color: '#f59e0b' },
  { id: 'monolith', label: 'Монолит', color: '#dfe3ea' }
];

const ACCENTS = {
  crimson: { label: 'Алый', value: '#ff3b3b', soft: 'rgba(255, 59, 59, 0.16)', glow: 'rgba(255, 59, 59, 0.34)', alt: '#ff6b6b' },
  violet: { label: 'Фиолет', value: '#7c5cff', soft: 'rgba(124, 92, 255, 0.18)', glow: 'rgba(124, 92, 255, 0.36)', alt: '#a88cff' },
  teal: { label: 'Бирюза', value: '#2dd4bf', soft: 'rgba(45, 212, 191, 0.18)', glow: 'rgba(45, 212, 191, 0.34)', alt: '#5eead4' },
  amber: { label: 'Янтарь', value: '#f59e0b', soft: 'rgba(245, 158, 11, 0.18)', glow: 'rgba(245, 158, 11, 0.34)', alt: '#fbbf24' },
  lime: { label: 'Лайм', value: '#a3e635', soft: 'rgba(163, 230, 53, 0.18)', glow: 'rgba(163, 230, 53, 0.32)', alt: '#bef264' },
  ice: { label: 'Лёд', value: '#7dd3fc', soft: 'rgba(125, 211, 252, 0.18)', glow: 'rgba(125, 211, 252, 0.34)', alt: '#a5e4fd' }
};

export const ACCENTS_LIST = Object.entries(ACCENTS).map(([id, cfg]) => ({ id, ...cfg }));

export function applyTheme(themeId) {
  const theme = THEMES.some((t) => t.id === themeId) ? themeId : 'obsidian';
  document.documentElement.dataset.theme = theme;
  try {
    localStorage.setItem(THEME_KEY, theme);
  } catch {
    /* приватный режим */
  }
}

export function applyAccent(accentId) {
  const accent = ACCENTS[accentId] || ACCENTS.crimson;
  const root = document.documentElement.style;
  root.setProperty('--accent', accent.value);
  root.setProperty('--accent-2', accent.alt);
  root.setProperty('--accent-soft', accent.soft);
  root.setProperty('--accent-glow', accent.glow);
  try {
    localStorage.setItem(ACCENT_KEY, accentId in ACCENTS ? accentId : 'crimson');
  } catch {
    /* приватный режим */
  }
}

export function initialTheme() {
  try {
    const saved = localStorage.getItem(THEME_KEY);
    if (saved && THEMES.some((t) => t.id === saved)) return saved;
    const accent = localStorage.getItem(ACCENT_KEY);
    const match = THEMES.find((t) => t.id === accent);
    return match ? match.id : 'obsidian';
  } catch {
    return 'obsidian';
  }
}

export function initialAccent(themeId) {
  try {
    const saved = localStorage.getItem(ACCENT_KEY);
    if (saved && ACCENTS[saved]) return saved;
  } catch {
    /* приватный режим */
  }
  const byTheme = { obsidian: 'crimson', noctra: 'violet', emerald: 'teal', amber: 'amber', monolith: 'ice' };
  return byTheme[themeId] || 'crimson';
}
