import { useEffect, useRef, useState } from 'react'
import './App.css'
import styles from './HostControls.module.css'
import { LocalAssessmentStore } from './adapters/localAssessmentStore'
import { applicableAnswers, isAnswers, isMessage, type AssessmentRecord, type HostContext, type SaveResult } from './adapters/contracts'
import { assessmentErrors } from './adapters/validation'

const recordId = 'ACME-LOCAL-APPLICATION-001'
const store = new LocalAssessmentStore(window.localStorage)

function App() {
  const frameRef = useRef<HTMLIFrameElement>(null)
  const dirtyRef = useRef(false)
  const [record, setRecord] = useState<AssessmentRecord | null>(null)
  const [status, setStatus] = useState('Waiting for embedded assessment')
  const [canEdit, setCanEdit] = useState(true)
  const [frameVersion, setFrameVersion] = useState(0)
  const [dirty, setDirty] = useState(false)

  useEffect(() => {
    function receiveMessage(event: MessageEvent<unknown>) {
      if (event.source !== frameRef.current?.contentWindow || event.origin !== window.location.origin || !isMessage(event.data)) return
      if (event.data.type === 'dynamic-questions-ready') {
        try {
          const loaded = store.load(recordId)
          setRecord(loaded)
          const context: HostContext = { recordId, user: 'local-user', canEdit, record: loaded }
          frameRef.current?.contentWindow?.postMessage({ type: 'host-context', payload: context }, window.location.origin)
          setStatus('Embedded React app connected')
        } catch (error) { setStatus(error instanceof Error ? error.message : 'Unable to load saved assessment.') }
      }
      if (event.data.type === 'assessment-dirty' && typeof event.data.dirty === 'boolean') {
        dirtyRef.current = event.data.dirty
        setDirty(event.data.dirty)
      }
      if (event.data.type === 'assessment-save' && typeof event.data.requestId === 'string') {
        const result: SaveResult = { type: 'assessment-save-result', requestId: event.data.requestId }
        try {
          const payload = event.data.payload as Record<string, unknown> | null
          if (!payload || payload.recordId !== recordId || !isAnswers(payload.answers) || !['Draft', 'Validated'].includes(String(payload.status))) throw new Error('Invalid assessment save request.')
          const current = store.load(recordId)
          if (!canEdit || current?.status === 'Completed') throw new Error('This assessment is read-only.')
          const answers = applicableAnswers(payload.answers)
          if (payload.status === 'Validated' && assessmentErrors(answers).length) throw new Error('The assessment must pass validation before submission.')
          const now = new Date().toISOString()
          const saved: AssessmentRecord = {
            recordId, answers, status: payload.status as 'Draft' | 'Validated', savedBy: 'local-user', savedAt: now,
            ...(payload.status === 'Validated' ? { submittedBy: 'local-user', submittedAt: now } : {}),
          }
          result.record = store.save(saved)
          setRecord(saved)
          setStatus(saved.status === 'Validated' ? 'Host received submitted JSON payload' : 'Host saved draft')
        } catch (error) { result.error = error instanceof Error ? error.message : 'Unable to save assessment.' }
        frameRef.current?.contentWindow?.postMessage(result, window.location.origin)
      }
    }
    window.addEventListener('message', receiveMessage)
    return () => window.removeEventListener('message', receiveMessage)
  }, [canEdit, frameVersion])

  function reload(nextCanEdit = canEdit) {
    if (dirtyRef.current && !window.confirm('Discard unsaved changes and reload the saved assessment?')) return
    dirtyRef.current = false
    setDirty(false)
    setCanEdit(nextCanEdit)
    setFrameVersion((version) => version + 1)
  }

  function complete() {
    if (dirty || !canEdit) return
    try {
      const current = store.load(recordId)
      if (current?.status !== 'Validated') return
      store.save({ ...current, status: 'Completed', savedAt: new Date().toISOString(), savedBy: 'local-user' })
      reload()
    } catch (error) { setStatus(error instanceof Error ? error.message : 'Unable to complete assessment.') }
  }

  return (
    <main className="host-shell">
      <header className="host-header"><div><span className="host-kicker">Fake CRM host</span><h1>ACME application record</h1></div><span className="host-status"><span className="status-dot" />{status}</span></header>
      <section className="host-context"><span>Record</span><strong>{recordId}</strong><span>Web resource: DynamicQuestions</span></section>
      <section className={styles.controls} aria-label="Host record controls">
        <label>Access <select aria-label="Access" value={canEdit ? 'edit' : 'read'} onChange={(event) => reload(event.target.value === 'edit')}><option value="edit">Editable</option><option value="read">Read-only viewer</option></select></label>
        <button type="button" onClick={() => reload()}>Reload saved assessment</button>
        <button type="button" onClick={complete} disabled={!canEdit || dirty || record?.status !== 'Validated'}>Mark completed</button>
        <span>{record?.status ?? 'Draft'} · {dirty ? 'Unsaved changes' : 'Saved'}</span>
        <a href="/foundations.html">RJSF foundations</a>
      </section>
      <div className="host-frame-wrap"><iframe key={frameVersion} ref={frameRef} title="Embedded DynamicQuestions assessment" src="/embedded.html" /></div>
      <section className="payload-panel"><div><span className="host-kicker">Host boundary</span><h2>Saved payload</h2></div><pre>{record ? JSON.stringify(record, null, 2) : 'Save a draft or submit the assessment to see the JSON received by the host.'}</pre></section>
    </main>
  )
}

export default App
