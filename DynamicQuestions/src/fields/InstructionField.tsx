import type { FieldProps } from '@rjsf/utils'

export function InstructionField({ schema }: FieldProps) {
  return <aside className="instruction-content" aria-label="Assessment guidance"><strong>Before you continue</strong><p>{schema.description ?? schema.title}</p></aside>
}
