import type { Answers } from '../schemas/assessment'

export type AssessmentStatus = 'Draft' | 'Validated' | 'Completed'
export type AssessmentRecord = {
  recordId: string
  status: AssessmentStatus
  answers: Answers
  savedBy: string
  savedAt: string
  submittedBy?: string
  submittedAt?: string
}
export type HostContext = {
  recordId: string
  user: string
  canEdit: boolean
  record: AssessmentRecord | null
}
export type SaveRequest = {
  type: 'assessment-save'
  requestId: string
  payload: { recordId: string; answers: Answers; status: 'Draft' | 'Validated' }
}
export type SaveResult = {
  type: 'assessment-save-result'
  requestId: string
  record?: AssessmentRecord
  error?: string
}

export function isMessage(value: unknown): value is Record<string, unknown> & { type: string } {
  return typeof value === 'object' && value !== null && 'type' in value && typeof value.type === 'string'
}

export function isAnswers(value: unknown): value is Answers {
  if (typeof value !== 'object' || value === null || Array.isArray(value)) return false
  return Object.entries(value).every(([key, answer]) => key === 'hasRisk'
    ? typeof answer === 'boolean'
    : ['eligible', 'projectName', 'riskLevel', 'targetDate', 'notes'].includes(key) && typeof answer === 'string')
}

export function isRecord(value: unknown): value is AssessmentRecord {
  if (typeof value !== 'object' || value === null) return false
  const record = value as Partial<AssessmentRecord>
  return typeof record.recordId === 'string' && ['Draft', 'Validated', 'Completed'].includes(record.status ?? '')
    && isAnswers(record.answers) && typeof record.savedBy === 'string' && typeof record.savedAt === 'string'
}

export function applicableAnswers(answers: Answers): Answers {
  if (answers.eligible === 'No') return { eligible: 'No' }
  return Object.fromEntries(Object.entries(answers).filter(([key, value]) =>
    ['eligible', 'projectName', 'hasRisk', 'riskLevel', 'targetDate', 'notes'].includes(key) && value !== undefined))
}
