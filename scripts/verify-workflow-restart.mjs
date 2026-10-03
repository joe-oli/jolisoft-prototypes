import { spawn } from 'node:child_process'
import { createWriteStream } from 'node:fs'
import { mkdir } from 'node:fs/promises'
import { createRequire } from 'node:module'
import { fileURLToPath } from 'node:url'
import path from 'node:path'
import net from 'node:net'

const root = fileURLToPath(new URL('../', import.meta.url))
const require = createRequire(path.join(root, 'DynamicQuestions/package.json'))
const { chromium, expect } = require('@playwright/test')
if (!process.env.ConnectionStrings__DefaultConnection) throw new Error('Supply the private Linux SQL connection in ConnectionStrings__DefaultConnection.')
await mkdir(path.join(root, 'artifacts/verification'), { recursive: true })
const log = createWriteStream(path.join(root, 'artifacts/verification/workflow-stack.log'))
let api, ui, browser

async function freePort(port) {
  await new Promise((resolve, reject) => {
    const socket = net.connect({ port, host: 'localhost' })
    socket.on('connect', () => { socket.destroy(); reject(new Error(`Port ${port} is already in use. Stop the existing stack before verification.`)) })
    socket.on('error', (error) => error.code === 'ECONNREFUSED' ? resolve() : reject(error))
  })
}
async function ready(url, child) {
  for (let attempt = 0; attempt < 100; attempt++) {
    if (child.exitCode !== null) throw new Error('Verification server exited. Inspect artifacts/verification/workflow-stack.log.')
    try { if ((await fetch(url)).ok) return } catch {}
    await new Promise((resolve) => setTimeout(resolve, 300))
  }
  throw new Error(`Timed out waiting for ${url}`)
}
function start(command, args, cwd) {
  const child = spawn(command, args, { cwd, env: process.env, windowsHide: true, stdio: ['ignore', 'pipe', 'pipe'] })
  child.stdout.pipe(log, { end: false }); child.stderr.pipe(log, { end: false })
  child.on('error', (error) => log.write(error.message))
  return child
}
async function stop(child) {
  if (!child || child.exitCode !== null) return
  await new Promise((resolve) => { child.once('exit', resolve); child.kill() })
}
async function startStack() {
  await freePort(6041); await freePort(6173)
  api = start('dotnet', [path.join(root, 'Jolisoft.Demo.WebAPI/bin/Debug/net10.0/Jolisoft.Demo.WebAPI.dll'), '--urls', 'http://localhost:6041'], path.join(root, 'Jolisoft.Demo.WebAPI'))
  ui = start(process.execPath, [path.join(root, 'ShowcaseShell/node_modules/vite/bin/vite.js'), '--host', 'localhost', '--port', '6173', '--strictPort'], path.join(root, 'ShowcaseShell'))
  await Promise.all([ready('http://localhost:6041/api/workflows', api), ready('http://localhost:6173', ui)])
}

try {
  await startStack()
  browser = await chromium.launch({ channel: 'msedge', headless: true })
  const page = await browser.newPage()
  const title = `Linux restart verification ${new Date().toISOString()}`
  await page.goto('http://localhost:6173')
  await expect(page.getByText('Connected to the local API', { exact: true })).toBeVisible()
  await page.getByLabel('Workflow title').fill(title)
  const creation = page.waitForResponse((response) => response.url().endsWith('/api/workflows') && response.request().method() === 'POST')
  await page.getByRole('button', { name: /Create record/ }).click()
  const response = await creation
  if (response.status() !== 201) throw new Error(`Workflow creation returned ${response.status()}`)
  const created = await response.json()
  await expect(page.getByText(title, { exact: true })).toBeVisible()
  await page.screenshot({ path: path.join(root, 'artifacts/verification/workflow-created.png'), fullPage: true })
  await stop(api); await stop(ui)
  await startStack()
  await page.reload()
  await expect(page.getByText(title, { exact: true })).toBeVisible()
  const records = await (await fetch('http://localhost:6041/api/workflows')).json()
  if (!records.some((record) => record.id === created.id && record.title === title)) throw new Error('Created workflow missing after restart.')
  await page.screenshot({ path: path.join(root, 'artifacts/verification/workflow-after-restart.png'), fullPage: true })
  console.log(`PASS: browser-created workflow ${created.id} survived API and UI restart on Linux SQL Server.`)
} finally {
  await browser?.close(); await stop(api); await stop(ui); log.end()
}
