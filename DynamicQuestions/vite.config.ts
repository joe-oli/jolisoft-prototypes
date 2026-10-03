import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'
import { fileURLToPath } from 'node:url'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  build: {
    rolldownOptions: {
      input: {
        host: fileURLToPath(new URL('./index.html', import.meta.url)),
        embedded: fileURLToPath(new URL('./embedded.html', import.meta.url)),
        foundations: fileURLToPath(new URL('./foundations.html', import.meta.url)),
      },
    },
  },
})
