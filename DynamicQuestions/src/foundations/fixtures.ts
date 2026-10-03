import type { JSONSchema7 } from 'json-schema'
import type { UiSchema } from '@rjsf/utils'

export const fixtureSchema: JSONSchema7 = {
  type: 'object', title: 'Project proposal', required: ['projectName', 'riskLevel'], properties: {
    guidance: { type: 'null', title: 'Instructions', description: 'Edit the schema and presentation independently, then compare saving a draft with validating it.' },
    projectName: { type: 'string', title: 'Project name', minLength: 3 },
    riskLevel: { type: 'string', title: 'Risk level', enum: ['Low', 'Medium', 'High'] },
    hasRisk: { type: 'boolean', title: 'Needs risk review' },
    targetDate: { type: 'string', title: 'Target date', format: 'date' },
    notes: { type: 'string', title: 'Notes' },
  },
}
export const fixtureUiSchema: UiSchema = {
  guidance: { 'ui:field': 'InstructionField' },
  projectName: { 'ui:widget': 'TextQuestionWidget' },
  riskLevel: { 'ui:widget': 'SelectQuestionWidget' },
  hasRisk: { 'ui:widget': 'CheckboxQuestionWidget' },
  targetDate: { 'ui:widget': 'DateQuestionWidget' },
  notes: { 'ui:widget': 'TextareaQuestionWidget' },
}
