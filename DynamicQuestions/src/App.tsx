import { useEffect, useRef, useState } from 'react'
import './App.css'

type HostMessage = { type: string; payload?: unknown }

function App() {
  const frameRef = useRef<HTMLIFrameElement>(null)
  const [lastPayload, setLastPayload] = useState<unknown>(null)
  const [status, setStatus] = useState('Waiting for embedded assessment')

  useEffect(() => {
    function receiveMessage(event: MessageEvent<HostMessage>) {
      if (event.source !== frameRef.current?.contentWindow) return
      if (event.data.type === 'dynamic-questions-ready') {
        setStatus('Embedded React app connected')
        frameRef.current?.contentWindow?.postMessage({
          type: 'host-context',
          payload: { recordId: 'ACME-LOCAL-APPLICATION-001', mode: 'draft', user: 'local-user' },
        }, window.location.origin)
      }
      if (event.data.type === 'assessment-submitted') {
        setLastPayload(event.data.payload)
        setStatus('Host received submitted JSON payload')
      }
    }
    window.addEventListener('message', receiveMessage)
    return () => window.removeEventListener('message', receiveMessage)
  }, [])

  return (
    <main className="host-shell">
      <header className="host-header">
        <div><span className="host-kicker">Fake CRM host</span><h1>ACME application record</h1></div>
        <span className="host-status"><span className="status-dot" />{status}</span>
      </header>
      <section className="host-context"><span>Record</span><strong>ACME-LOCAL-APPLICATION-001</strong><span>Web resource: DynamicQuestions</span></section>
      <div className="host-frame-wrap"><iframe ref={frameRef} title="Embedded DynamicQuestions assessment" src="/embedded.html" /></div>
      <section className="payload-panel">
        <div><span className="host-kicker">Host boundary</span><h2>Submitted payload</h2></div>
        <pre>{lastPayload ? JSON.stringify(lastPayload, null, 2) : 'Submit the embedded wizard to see the JSON crossing window.postMessage.'}</pre>
      </section>
    </main>
  )
}

export default App
