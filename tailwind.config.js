module.exports = {
  darkMode: 'class',
  content: [
    './src/Server/Pages/**/*.razor',
    './src/Server/Components/**/*.razor',
    './src/Server/Pages/Shared/**/*.razor',
    './src/Server/wwwroot/**/*.html'
  ],
  theme: {
    fontFamily: {
      sans: ['Inter', 'sans-serif'],
    },
    extend: {
      colors: {
        primary: '#3B82F6',
        'primary-600': '#2563EB',
        success: '#10B981',
        warning: '#F59E0B',
        danger: '#EF4444',
        neutral: {
          50: '#F8FAFC',
          100: '#F1F5F9',
          200: '#E2E8F0',
          300: '#CBD5E1',
          400: '#94A3B8',
          500: '#64748B',
          600: '#475569',
          700: '#334155',
          800: '#1E293B',
          900: '#0F172A',
        },
      },
      boxShadow: {
        sm: '0 1px 2px 0 rgba(0,0,0,0.05)',
        md: '0 4px 6px -1px rgba(0,0,0,0.1)',
      },
      borderRadius: {
        xl: '1rem',
        '2xl': '1.5rem',
      },
    },
  },
  plugins: [],
}
