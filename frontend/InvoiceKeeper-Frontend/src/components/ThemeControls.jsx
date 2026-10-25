import { useEffect, useState } from 'react';
import { ACCENTS_LIST, THEMES, applyAccent, applyTheme, initialAccent, initialTheme } from '../lib/theme.js';

export default function ThemeControls() {
  const [theme, setTheme] = useState(initialTheme);
  const [accent, setAccent] = useState(() => initialAccent(initialTheme()));

  useEffect(() => {
    applyTheme(theme);
  }, [theme]);

  useEffect(() => {
    applyAccent(accent);
  }, [accent]);

  return (
    <div className="settings" role="group" aria-label="Оформление">
      <div className="settings__group">
        <span className="theme-switch__label">Тема</span>
        {THEMES.map((item) => (
          <button
            key={item.id}
            type="button"
            className={`theme-chip ${theme === item.id ? 'is-active' : ''}`}
            style={{ background: item.color }}
            title={item.label}
            aria-label={`Тема ${item.label}`}
            aria-pressed={theme === item.id}
            onClick={() => setTheme(item.id)}
          />
        ))}
      </div>

      <span className="settings__divider" aria-hidden="true" />

      <div className="settings__group settings__group--accent">
        <span className="theme-switch__label">Акцент</span>
        {ACCENTS_LIST.map((item) => (
          <button
            key={item.id}
            type="button"
            className={`accent-dot ${accent === item.id ? 'is-active' : ''}`}
            style={{ background: item.value }}
            title={item.label}
            aria-label={`Акцент ${item.label}`}
            aria-pressed={accent === item.id}
            onClick={() => setAccent(item.id)}
          />
        ))}
      </div>
    </div>
  );
}
