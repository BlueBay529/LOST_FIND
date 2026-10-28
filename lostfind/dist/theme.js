(() => {
  'use strict';

  const storageKey = 'lostfind_theme';
  const root = document.documentElement;
  const systemTheme = window.matchMedia('(prefers-color-scheme: dark)');
  let preference = null;
  try {
    const saved = localStorage.getItem(storageKey);
    if (saved === 'light' || saved === 'dark') preference = saved;
  } catch {
    // 저장 공간을 사용할 수 없어도 현재 페이지에서는 전환할 수 있습니다.
  }

  function applyTheme(theme) {
    root.dataset.theme = theme;
    root.style.colorScheme = theme;
    const dark = theme === 'dark';
    const browserColor = document.querySelector('meta[name="theme-color"]');
    if (browserColor) browserColor.content = dark ? '#202937' : '#2563eb';
    document.querySelectorAll('[data-theme-toggle]').forEach(button => {
      const label = dark ? '라이트 모드로 전환' : '다크 모드로 전환';
      button.setAttribute('aria-label', label);
      button.setAttribute('aria-pressed', String(dark));
      button.title = label;
      const icon = button.querySelector('[data-theme-icon]');
      if (icon) icon.textContent = dark ? '☀' : '☾';
    });
  }

  // 본문을 그리기 전에 저장된 테마를 적용해 밝은 화면이 깜빡이지 않게 합니다.
  applyTheme(preference || (systemTheme.matches ? 'dark' : 'light'));

  document.addEventListener('DOMContentLoaded', () => {
    applyTheme(root.dataset.theme);
    document.querySelectorAll('[data-theme-toggle]').forEach(button => {
      button.addEventListener('click', () => {
        preference = root.dataset.theme === 'dark' ? 'light' : 'dark';
        try {
          localStorage.setItem(storageKey, preference);
        } catch {
          // 브라우저 저장 제한이 있으면 이번 방문 동안만 선택을 유지합니다.
        }
        applyTheme(preference);
      });
    });
  });

  systemTheme.addEventListener('change', event => {
    if (!preference) applyTheme(event.matches ? 'dark' : 'light');
  });
  window.addEventListener('storage', event => {
    if (event.key !== storageKey && event.key !== null) return;
    preference = event.newValue === 'light' || event.newValue === 'dark' ? event.newValue : null;
    applyTheme(preference || (systemTheme.matches ? 'dark' : 'light'));
  });
})();
