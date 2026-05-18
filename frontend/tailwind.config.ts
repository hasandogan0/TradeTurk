import type { Config } from 'tailwindcss';

export default {
  content: ['./index.html', './src/**/*.{ts,tsx}'],
  theme: {
    extend: {
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui']
      },
      colors: {
        ink: '#070914',
        panel: '#111827',
        panelSoft: '#172033',
        accent: '#22c55e',
        danger: '#f43f5e',
        line: 'rgba(148, 163, 184, 0.18)'
      }
    }
  },
  plugins: []
} satisfies Config;
