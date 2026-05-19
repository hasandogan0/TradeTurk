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
      },
      screens: {
        xs: '375px'
      },
      animation: {
        fadeIn: 'fadeIn 0.4s ease-out both',
        slideUp: 'slideUp 0.5s ease-out both'
      },
      keyframes: {
        fadeIn: {
          from: { opacity: '0', transform: 'translateY(8px)' },
          to: { opacity: '1', transform: 'translateY(0)' }
        },
        slideUp: {
          from: { opacity: '0', transform: 'translateY(20px)' },
          to: { opacity: '1', transform: 'translateY(0)' }
        }
      }
    }
  },
  plugins: []
} satisfies Config;
