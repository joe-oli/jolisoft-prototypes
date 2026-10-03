import { useEffect, useState } from 'react'
import { createRoot } from 'react-dom/client'
import Form from '@rjsf/core'
import validator from '@rjsf/validator-ajv8'
import { Tab, TabList, TabPanel, Tabs } from 'react-tabs'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'react-tabs/style/react-tabs.css'
import './embedded.css'
import { questionFields, questionWidgets } from './fields'
import { assessmentSchema, assessmentUiSchema, eligibilitySchema, eligibilityUiSchema, type Answers } from './schemas/assessment'
import { applicableAnswers, isMessage, isRecord, type HostContext, type AssessmentRecord } from './adapters/contracts'
import { IframeAssessmentAdapter } from './adapters/iframeAssessmentAdapter'
import { assessmentErrors } from './adapters/validation'

export function EmbeddedApp() {
  const [context, setContext] = useState<HostContext | null>(null)
  const [record, setRecord] = useState<AssessmentRecord | null>(null)
  const [answers, setAnswers] = useState<Answers>({})
  const [savedAnswers, setSavedAnswers] = useState('{}')
  const [selectedIndex, setSelectedIndex] = useState(0)
  const [busy, setBusy] = useState(false)
  const [message, setMessage] = useState('Waiting for the host to load this record.')
  const dirty = JSON.stringify(applicableAnswers(answers)) !== savedAnswers
  const readOnly = !context?.canEdit || record?.status === 'Completed'

  useEffect(() => {
    function receiveMessage(event: MessageEvent<unknown>) {
      if (event.source !== window.parent || event.origin !== window.location.origin || !isMessage(event.data)) return
      if (event.data.type !== 'host-context' || typeof event.data.payload !== 'object' || event.data.payload === null) return
      const next = event.data.payload as HostContext
      if (typeof next.recordId !== 'string' || typeof next.user !== 'string' || typeof next.canEdit !== 'boolean') return
      if (next.record !== null && (!isRecord(next.record) || next.record.recordId !== next.recordId)) return
      setContext(next)
      setRecord(next.record)
      const loaded = next.record?.answers ?? {}
      setAnswers(loaded)
      setSavedAnswers(JSON.stringify(applicableAnswers(loaded)))
      setSelectedIndex(next.record?.status === 'Completed' ? 2 : 0)
      setMessage(next.record ? 'Loaded saved assessment from the host.' : 'New draft. Save your answers when ready.')
    }
    window.addEventListener('message', receiveMessage)
    window.parent.postMessage({ type: 'dynamic-questions-ready' }, window.location.origin)
    return () => window.removeEventListener('message', receiveMessage)
  }, [])

  useEffect(() => {
    window.parent.postMessage({ type: 'assessment-dirty', dirty }, window.location.origin)
    const warn = (event: BeforeUnloadEvent) => { if (dirty) { event.preventDefault(); event.returnValue = '' } }
    window.addEventListener('beforeunload', warn)
    return () => window.removeEventListener('beforeunload', warn)
  }, [dirty])

  const mergeSection = (next: Answers | undefined, section: 'eligibility' | 'assessment') => {
    if (readOnly || busy) return
    const keys = section === 'eligibility' ? ['eligible'] : ['projectName', 'hasRisk', 'riskLevel', 'targetDate', 'notes']
    setAnswers((current) => {
      const merged = Object.fromEntries(Object.entries(current).filter(([key]) => !keys.includes(key)))
      for (const [key, value] of Object.entries(next ?? {})) if (keys.includes(key) && value !== undefined) merged[key] = value
      return applicableAnswers(merged)
    })
  }

  async function save(status: 'Draft' | 'Validated', nextTab?: number) {
    if (!context || readOnly || busy) return
    const snapshot = applicableAnswers(answers)
    if (status === 'Validated') {
      const errors = assessmentErrors(snapshot)
      if (errors.length) { setMessage(`Please complete the assessment: ${errors.join('; ')}`); return }
    }
    setBusy(true)
    try {
      const saved = await new IframeAssessmentAdapter(context).save(snapshot, status)
      setRecord(saved)
      setSavedAnswers(JSON.stringify(applicableAnswers(saved.answers)))
      setMessage(status === 'Validated' ? 'Validated assessment saved and submitted to the host.' : 'Draft saved by the host.')
      if (nextTab !== undefined) setSelectedIndex(nextTab)
    } catch (error) {
      setMessage(error instanceof Error ? error.message : 'Save failed. Your changes remain unsaved.')
    } finally { setBusy(false) }
  }

  if (!context) return <main className="embedded-app"><p role="status">{message}</p></main>

  return <main className="embedded-app">
    <header className="embedded-header"><div><span className="eyebrow">Embedded React web resource</span><h1>Dynamic assessment</h1></div><span className="mode-badge">{record?.status ?? 'Draft'}{readOnly ? ' / Read-only' : ''}</span></header>
    <p className="intro">Record <strong>{context.recordId}</strong> · {dirty ? 'Unsaved changes' : 'All changes saved'}{busy ? ' · Saving…' : ''}</p>
    <Tabs selectedIndex={selectedIndex} onSelect={(index) => { if (busy) return false; setSelectedIndex(index); return true }}>
      <TabList>{['Eligibility', 'Assessment', 'Review'].map((tab, index) => <Tab key={tab} disabled={busy || (index === 1 && answers.eligible === 'No')}>{tab}<small>{String(index + 1).padStart(2, '0')}</small></Tab>)}</TabList>
      <TabPanel><Form schema={eligibilitySchema} uiSchema={eligibilityUiSchema} fields={questionFields} widgets={questionWidgets} validator={validator} formData={answers} readonly={readOnly} disabled={busy} onChange={(event) => mergeSection(event.formData, 'eligibility')} onSubmit={() => void save('Draft', answers.eligible === 'No' ? 2 : 1)} noHtml5Validate liveValidate>
        <div className="form-actions">{readOnly ? <button className="btn btn-dark" type="button" onClick={() => setSelectedIndex(answers.eligible === 'No' ? 2 : 1)}>Next</button> : <button className="btn btn-dark" type="submit" disabled={busy}>Save & Next</button>}</div>
      </Form></TabPanel>
      <TabPanel><Form schema={assessmentSchema} uiSchema={assessmentUiSchema} fields={questionFields} widgets={questionWidgets} validator={validator} formData={answers} readonly={readOnly} disabled={busy} onChange={(event) => mergeSection(event.formData, 'assessment')} onSubmit={() => void save('Draft', 2)} noHtml5Validate liveValidate>
        <div className="form-actions"><button className="btn btn-outline-dark" type="button" disabled={busy} onClick={() => setSelectedIndex(0)}>Back</button>{readOnly ? <button className="btn btn-dark" type="button" onClick={() => setSelectedIndex(2)}>Next</button> : <button className="btn btn-dark" type="submit" disabled={busy}>Save & Next</button>}</div>
      </Form></TabPanel>
      <TabPanel><div className="review"><span className="eyebrow">Assessment summary</span><h2>Review answers</h2><dl>{Object.entries(applicableAnswers(answers)).map(([key, value]) => <div key={key}><dt>{key}</dt><dd>{String(value)}</dd></div>)}</dl><div className="form-actions"><button className="btn btn-outline-dark" disabled={busy} onClick={() => setSelectedIndex(answers.eligible === 'No' ? 0 : 1)}>{readOnly ? 'View answers' : 'Edit'}</button>{!readOnly && <button className="btn btn-primary" disabled={busy} onClick={() => void save('Validated')}>Submit JSON to host</button>}</div></div></TabPanel>
    </Tabs>
    {!readOnly && <button className="btn btn-outline-secondary mt-3" disabled={busy || !dirty} onClick={() => void save('Draft')}>Save draft</button>}
    <p className="save-message" role="status">{message}</p>
  </main>
}

createRoot(document.getElementById('root')!).render(<EmbeddedApp />)
