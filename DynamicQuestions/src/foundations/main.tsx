import { Component, useRef, useState, type ReactNode } from 'react'
import { createRoot } from 'react-dom/client'
import Form from '@rjsf/core'
import validator from '@rjsf/validator-ajv8'
import type { FormValidation, UiSchema } from '@rjsf/utils'
import type { JSONSchema7 } from 'json-schema'
import 'bootstrap/dist/css/bootstrap.min.css'
import { questionFields, questionWidgets } from '../fields'
import { fixtureSchema, fixtureUiSchema } from './fixtures'
import styles from './Foundations.module.css'

const draftKey = 'jolisoft.foundations.v1'
type FormData = Record<string, unknown>

class PreviewBoundary extends Component<{ children: ReactNode }, { error: string | null }> {
  state: { error: string | null } = { error: null }
  static getDerivedStateFromError(error: Error) { return { error: error.message } }
  render() { return this.state.error ? <p role="alert">Preview could not render: {this.state.error}. Correct the UI schema and apply again.</p> : this.props.children }
}

export function Foundations() {
  const [schema, setSchema] = useState<JSONSchema7>(fixtureSchema)
  const [uiSchema, setUiSchema] = useState<UiSchema>(fixtureUiSchema)
  const [schemaText, setSchemaText] = useState(JSON.stringify(fixtureSchema, null, 2))
  const [uiText, setUiText] = useState(JSON.stringify(fixtureUiSchema, null, 2))
  const [data, setData] = useState<FormData>({})
  const [message, setMessage] = useState('Apply both editors to update the preview.')
  const [version, setVersion] = useState(0)
  const formRef = useRef<Form<FormData>>(null)

  function apply() {
    try {
      const nextSchema: JSONSchema7 = JSON.parse(schemaText)
      const nextUi: UiSchema = JSON.parse(uiText)
      if (!nextSchema || typeof nextSchema !== 'object' || Array.isArray(nextSchema) || nextSchema.type !== 'object') throw new Error('Use an object JSON schema for this example.')
      if (!nextUi || typeof nextUi !== 'object' || Array.isArray(nextUi)) throw new Error('UI schema must be a JSON object.')
      const check = validator.rawValidation(nextSchema, {})
      if (check.validationError) throw check.validationError
      setSchema(nextSchema)
      setUiSchema(nextUi)
      setVersion((current) => current + 1)
      setMessage('Schema and UI schema applied. Answers retained for comparison.')
    } catch (error) { setMessage(error instanceof Error ? error.message : 'Invalid schema JSON.') }
  }

  function save(validated: boolean) {
    try {
      localStorage.setItem(draftKey, JSON.stringify({ schema, uiSchema, data, status: validated ? 'Validated' : 'Draft' }))
      setMessage(validated ? 'Validated and saved locally.' : 'Draft saved locally without validation.')
    } catch { setMessage('Local storage is unavailable. Answers have not been saved.') }
  }

  function load() {
    try {
      const raw = localStorage.getItem(draftKey)
      if (!raw) { setMessage('No saved foundations draft.'); return }
      const saved = JSON.parse(raw)
      if (!saved.schema || saved.schema.type !== 'object' || !saved.uiSchema || !saved.data || typeof saved.data !== 'object' || Array.isArray(saved.data)) throw new Error('Saved draft is invalid.')
      const check = validator.rawValidation(saved.schema, {})
      if (check.validationError) throw check.validationError
      setSchema(saved.schema); setUiSchema(saved.uiSchema); setData(saved.data)
      setSchemaText(JSON.stringify(saved.schema, null, 2)); setUiText(JSON.stringify(saved.uiSchema, null, 2))
      setVersion((current) => current + 1)
      setMessage('Saved foundations draft loaded.')
    } catch { setMessage('The saved foundations draft could not be loaded.') }
  }

  function customValidate(formData: FormData | undefined, errors: FormValidation<FormData>) {
    if (formData?.riskLevel === 'High' && (typeof formData.notes !== 'string' || formData.notes.trim().length < 10)) {
      errors.notes?.addError('High risk proposals need at least 10 characters of reviewer notes.')
    }
    return errors
  }

  return <main className={styles.page}>
    <a href="/">Back to hosted assessment</a><h1>RJSF foundations</h1>
    <p>A separate schema workbench preserving the editor, local fixtures, custom controls, and manual validation techniques from the historical foundations project.</p>
    <div className={styles.layout}>
      <section className={styles.editor} aria-label="Schema editors"><h2>Schema editors</h2>
        <label>JSON schema<textarea aria-label="JSON schema" spellCheck={false} value={schemaText} onChange={(event) => setSchemaText(event.target.value)} /></label>
        <label>UI schema<textarea aria-label="UI schema" spellCheck={false} value={uiText} onChange={(event) => setUiText(event.target.value)} /></label>
        <div className={styles.actions}><button className="btn btn-dark" onClick={apply}>Apply schemas</button><button className="btn btn-outline-dark" onClick={() => { setSchemaText(JSON.stringify(fixtureSchema, null, 2)); setUiText(JSON.stringify(fixtureUiSchema, null, 2)); setMessage('Fixture restored in editors. Apply to update the preview.') }}>Restore fixture</button></div>
      </section>
      <section className={styles.preview} aria-label="Form preview"><h2>Form preview</h2>
        <PreviewBoundary key={version}><Form<FormData> ref={formRef} schema={schema} uiSchema={uiSchema} fields={questionFields} widgets={questionWidgets} validator={validator} customValidate={customValidate} formData={data} onChange={(event) => setData(event.formData ?? {})} onSubmit={() => save(true)} onError={() => setMessage('Validation failed. Correct the highlighted fields.')} noHtml5Validate><div /></Form></PreviewBoundary>
        <div className={styles.actions}><button className="btn btn-outline-dark" onClick={() => save(false)}>Save draft</button><button className="btn btn-primary" onClick={() => formRef.current?.submit()}>Validate & Save</button><button className="btn btn-outline-dark" onClick={load}>Load saved draft</button></div>
        <p>Custom rule: high risk proposals require reviewer notes of at least 10 characters.</p><pre>{JSON.stringify(data, null, 2)}</pre>
      </section>
    </div><p role="status">{message}</p>
  </main>
}

createRoot(document.getElementById('root')!).render(<Foundations />)
