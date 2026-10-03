import { isMessage, isRecord, type AssessmentRecord, type HostContext, type SaveRequest } from './contracts'
import type { Answers } from '../schemas/assessment'

export interface AssessmentAdapter {
  save(answers: Answers, status: 'Draft' | 'Validated'): Promise<AssessmentRecord>
}

export class IframeAssessmentAdapter implements AssessmentAdapter {
  private readonly context: HostContext
  constructor(context: HostContext) { this.context = context }

  save(answers: Answers, status: 'Draft' | 'Validated') {
    return new Promise<AssessmentRecord>((resolve, reject) => {
      const requestId = crypto.randomUUID()
      const cleanup = () => { window.removeEventListener('message', receive); window.clearTimeout(timeout) }
      const receive = (event: MessageEvent<unknown>) => {
        if (event.source !== window.parent || event.origin !== window.location.origin || !isMessage(event.data)) return
        if (event.data.type !== 'assessment-save-result' || event.data.requestId !== requestId) return
        cleanup()
        if (typeof event.data.error === 'string') reject(new Error(event.data.error))
        else if (isRecord(event.data.record) && event.data.record.recordId === this.context.recordId) resolve(event.data.record)
        else reject(new Error('The host returned an invalid save result.'))
      }
      const timeout = window.setTimeout(() => { cleanup(); reject(new Error('The host did not acknowledge the save. Try again.')) }, 10000)
      window.addEventListener('message', receive)
      const request: SaveRequest = { type: 'assessment-save', requestId, payload: { recordId: this.context.recordId, answers, status } }
      window.parent.postMessage(request, window.location.origin)
    })
  }
}
