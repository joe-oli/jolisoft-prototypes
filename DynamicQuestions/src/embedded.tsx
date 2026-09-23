import { useEffect, useMemo, useState } from 'react'
import { createRoot } from 'react-dom/client'
import Form from '@rjsf/core'
import validator from '@rjsf/validator-ajv8'
import { Tab, TabList, TabPanel, Tabs } from 'react-tabs'
import 'bootstrap/dist/css/bootstrap.min.css'
import 'react-tabs/style/react-tabs.css'
import './embedded.css'
import { questionFields, questionWidgets } from './fields'
import { assessmentSchema, assessmentUiSchema, eligibilitySchema, eligibilityUiSchema, type Answers } from './schemas/assessment'

type HostContext = { recordId: string; mode: string; user: string }

function EmbeddedApp() {
  const [context, setContext] = useState<HostContext>({ recordId: 'local', mode: 'draft', user: 'local-user' })
  const [answers, setAnswers] = useState<Answers>({})
  const [selectedIndex, setSelectedIndex] = useState(0)
  const [message, setMessage] = useState('Draft answers locally; submit when complete.')

  useEffect(() => {
    function receiveMessage(event: MessageEvent<{ type: string; payload?: HostContext }>) {
      if (event.source !== window.parent || event.origin !== window.location.origin) return
      if (event.data.type === 'host-context' && event.data.payload) setContext(event.data.payload)
    }
    window.addEventListener('message', receiveMessage)
    window.parent.postMessage({ type: 'dynamic-questions-ready' }, window.location.origin)
    return () => window.removeEventListener('message', receiveMessage)
  }, [])

  const finalTabIndex = answers.eligible === 'No' ? 2 : 3
  const tabs = useMemo(() => ['Eligibility', 'Assessment', 'Review'], [])
  const mergeAnswers = (next: Answers) => setAnswers((current) => ({ ...current, ...next }))
  const submit = () => {
    const payload = { recordId: context.recordId, status: 'Validated', answers, submittedBy: context.user, submittedAt: new Date().toISOString() }
    window.parent.postMessage({ type: 'assessment-submitted', payload }, window.location.origin)
    setMessage('Submitted JSON payload to the fake CRM host.')
  }

  return <main className="embedded-app">
    <header className="embedded-header"><div><span className="eyebrow">Embedded React web resource</span><h1>Dynamic assessment</h1></div><span className="mode-badge">{context.mode}</span></header>
    <p className="intro">Record <strong>{context.recordId}</strong> {'·'} answers are held by the embedded app until the host receives the final payload.</p>
    <Tabs selectedIndex={Math.min(selectedIndex, finalTabIndex)} onSelect={(index) => setSelectedIndex(index)}>
      <TabList>{tabs.map((tab, index) => <Tab key={tab} disabled={index === 1 && answers.eligible === 'No'}>{tab}<small>{String(index + 1).padStart(2, '0')}</small></Tab>)}</TabList>
      <TabPanel><Form schema={eligibilitySchema} uiSchema={eligibilityUiSchema} fields={questionFields} widgets={questionWidgets} validator={validator} formData={answers} onChange={(event) => mergeAnswers(event.formData)} onSubmit={() => setSelectedIndex(answers.eligible === 'No' ? 2 : 1)} omitExtraData liveValidate><div className="form-actions"><button className="btn btn-dark" type="submit">Save & Next {'→'}</button></div></Form></TabPanel>
      <TabPanel>{answers.eligible === 'No' ? <div className="skip-note"><h2>Assessment skipped</h2><p>The eligibility answer sends the wizard directly to finalisation.</p><button className="btn btn-dark" onClick={() => setSelectedIndex(2)}>Continue to review {'→'}</button></div> : <Form schema={assessmentSchema} uiSchema={assessmentUiSchema} fields={questionFields} widgets={questionWidgets} validator={validator} formData={answers} onChange={(event) => mergeAnswers(event.formData)} onSubmit={() => setSelectedIndex(2)} omitExtraData liveValidate><div className="form-actions"><button className="btn btn-outline-dark" type="button" onClick={() => setSelectedIndex(0)}>{'←'} Back</button><button className="btn btn-dark" type="submit">Save & Next {'→'}</button></div></Form>}</TabPanel>
      <TabPanel><div className="review"><span className="eyebrow">Ready to submit</span><h2>Review answers</h2><dl>{Object.entries(answers).map(([key, value]) => <div key={key}><dt>{key}</dt><dd>{String(value)}</dd></div>)}</dl><div className="form-actions"><button className="btn btn-outline-dark" onClick={() => setSelectedIndex(answers.eligible === 'No' ? 0 : 1)}>{'←'} Edit</button><button className="btn btn-primary" onClick={submit}>Submit JSON to host {'↗'}</button></div></div></TabPanel>
    </Tabs>
    <p className="save-message">{message}</p>
  </main>
}

createRoot(document.getElementById('root')!).render(<EmbeddedApp />)
