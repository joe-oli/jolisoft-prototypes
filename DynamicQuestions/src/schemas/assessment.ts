import type { JSONSchema7 } from 'json-schema'

export type Answers = { eligible?: string; projectName?: string; hasRisk?: boolean; riskLevel?: string; targetDate?: string; notes?: string }

export const eligibilitySchema: JSONSchema7 = { type: 'object', required: ['eligible'], properties: { eligible: { type: 'string', title: 'Is this application eligible?', enum: ['Yes', 'No'] } } }
export const eligibilityUiSchema = { eligible: { 'ui:widget': 'RadioQuestionWidget' } }

export const assessmentSchema: JSONSchema7 = {
  type: 'object', required: ['projectName', 'riskLevel'], properties: {
    assessmentGuidance: { type: 'null', title: 'Assessment guidance', description: 'Record the proposed project, choose its risk level, and include any information a reviewer needs.' },
    projectName: { type: 'string', title: 'Project name', minLength: 3 },
    hasRisk: { type: 'boolean', title: 'Does this application need a risk review?' },
    riskLevel: { type: 'string', title: 'Risk level', enum: ['Low', 'Medium', 'High'] },
    targetDate: { type: 'string', title: 'Target decision date', format: 'date' },
    notes: { type: 'string', title: 'Assessment notes' },
  },
}

export const assessmentUiSchema = {
  assessmentGuidance: { 'ui:field': 'InstructionField' },
  projectName: { 'ui:widget': 'TextQuestionWidget', 'ui:placeholder': 'e.g. Regional export pilot' },
  hasRisk: { 'ui:widget': 'CheckboxQuestionWidget', 'ui:options': { label: false } },
  riskLevel: { 'ui:widget': 'SelectQuestionWidget' },
  targetDate: { 'ui:widget': 'DateQuestionWidget' },
  notes: { 'ui:widget': 'TextareaQuestionWidget', 'ui:placeholder': 'Add reviewer context or supporting details.' },
}
