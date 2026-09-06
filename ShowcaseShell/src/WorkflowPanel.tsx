import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import styles from './WorkflowPanel.module.css'

type Workflow = {
  id: string
  title: string
  status: string
  createdBy: string
  createdAt: string
  platformReference: string
}

function WorkflowPanel() {
  const [workflows, setWorkflows] = useState<Workflow[]>([])
  const [title, setTitle] = useState('')
  const [createdBy, setCreatedBy] = useState('local-user')
  const [message, setMessage] = useState('Loading local records...')

  useEffect(() => {
    void loadWorkflows()
  }, [])

  async function loadWorkflows() {
    try {
      const response = await fetch('/api/workflows')
      if (!response.ok) throw new Error('API unavailable')
      setWorkflows(await response.json() as Workflow[])
      setMessage('Connected to the local API')
    } catch {
      setMessage('Start the API on port 6041 to use this workflow')
    }
  }

  async function createWorkflow(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    if (!title.trim() || !createdBy.trim()) return

    const response = await fetch('/api/workflows', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ title, createdBy }),
    })

    if (response.ok) {
      setTitle('')
      setMessage('Workflow created through the API and fake ACME boundary')
      await loadWorkflows()
    } else {
      setMessage('The API rejected the workflow')
    }
  }

  return (
    <section className={`container ${styles.panel}`} aria-labelledby="workflow-title">
      <div className={styles.heading}>
        <div>
          <p className="eyebrow">Live vertical slice</p>
          <h2 id="workflow-title">Try the workflow.</h2>
        </div>
        <p>{message}</p>
      </div>
      <form className={styles.form} onSubmit={createWorkflow}>
        <label>
          Workflow title
          <input value={title} onChange={(event) => setTitle(event.target.value)} placeholder="e.g. Grant application" />
        </label>
        <label>
          Created by
          <input value={createdBy} onChange={(event) => setCreatedBy(event.target.value)} />
        </label>
        <button className="btn btn-dark" type="submit">Create record <span aria-hidden="true">↗</span></button>
      </form>
      <div className={styles.records}>
        {workflows.length === 0 ? <p className={styles.empty}>No local records yet.</p> : workflows.map((workflow) => (
          <div className={styles.record} key={workflow.id}>
            <span>{workflow.title}</span>
            <small>{workflow.status} · {workflow.platformReference}</small>
          </div>
        ))}
      </div>
    </section>
  )
}

export default WorkflowPanel