import js from '@eslint/js'
import globals from 'globals'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import tseslint from 'typescript-eslint'
import boundaries from 'eslint-plugin-boundaries'
import { defineConfig, globalIgnores } from 'eslint/config'

export default defineConfig([
  globalIgnores(['dist']),
  {
    files: ['**/*.{ts,tsx}'],
    extends: [
      js.configs.recommended,
      tseslint.configs.recommended,
      reactHooks.configs.flat.recommended,
    ],
    plugins: {
      'react-refresh': reactRefresh,
      'boundaries': boundaries,
    },
    languageOptions: {
      globals: globals.browser,
    },
    settings: {
      'boundaries/include': ['src/**/*'],
      'boundaries/elements': [
        {
          mode: 'full',
          type: 'shared',
          pattern: [
            'src/components/**/*', // Global UI components
            'src/services/**/*',   // API calls (replaces server/drizzle)
            'src/hooks/**/*',      // Global hooks
            'src/context/**/*',    // Global state (like AuthProvider)
            'src/utils/**/*',      // Helper functions
            'src/assets/**/*'      // Images/CSS
          ]
        },
        {
          mode: 'full',
          type: 'feature',
          capture: ['featureName'],
          pattern: ['src/features/*/**/*']
        },
        {
          mode: 'full',
          type: 'pages', // Replaces Next.js 'app'
          capture: ['_', 'fileName'],
          pattern: ['src/pages/**/*']
        },
        {
          mode: 'full',
          type: 'neverImport',
          pattern: ['src/*'] // Blocks importing from main.tsx or App.tsx
        }
      ]
    },
    rules: {
      'react-refresh/only-export-components': [
        'warn',
        { allowConstantExport: true },
      ],
      'boundaries/no-unknown': ['error'],
      'boundaries/no-unknown-files': ['error'],
      'boundaries/element-types': [
        'error',
        {
          default: 'disallow',
          rules: [
            {
              // Shared files can only import other shared files
              from: ['shared'],
              allow: ['shared']
            },
            {
              // A Feature can import shared files, and files within its OWN feature
              from: ['feature'],
              allow: [
                'shared',
                ['feature', { featureName: '${from.featureName}' }]
              ]
            },
            {
              // Pages and root files can import shared files and features
              from: ['pages', 'neverImport'],
              allow: ['shared', 'feature']
            },
            {
              // Allow pages to import their specific CSS modules if needed
              from: ['pages'],
              allow: [['pages', { fileName: '*.css' }]]
            }
          ]
        }
      ]
    }
  },
])