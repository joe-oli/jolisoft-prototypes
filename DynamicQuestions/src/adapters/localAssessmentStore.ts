import { isRecord, type AssessmentRecord } from './contracts'

export interface AssessmentStore {
  load(recordId: string): AssessmentRecord | null
  save(record: AssessmentRecord): AssessmentRecord
}

// Only the host owns storage. The iframe accesses it through its message adapter.
export class LocalAssessmentStore implements AssessmentStore {
  private readonly storage: Storage
  constructor(storage: Storage) { this.storage = storage }

  load(recordId: string) {
    const raw = this.storage.getItem(`jolisoft.assessment.v1.${recordId}`)
    if (!raw) return null
    const record: unknown = JSON.parse(raw)
    if (!isRecord(record) || record.recordId !== recordId) throw new Error('The saved assessment is invalid.')
    return record
  }

  save(record: AssessmentRecord) {
    this.storage.setItem(`jolisoft.assessment.v1.${record.recordId}`, JSON.stringify(record))
    return record
  }
}
