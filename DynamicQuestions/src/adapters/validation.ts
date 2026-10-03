import validator from '@rjsf/validator-ajv8'
import { assessmentSchema, eligibilitySchema, type Answers } from '../schemas/assessment'

export function assessmentErrors(answers: Answers): string[] {
  const eligibility = validator.validateFormData(answers, eligibilitySchema).errors
  const assessment = answers.eligible === 'Yes' ? validator.validateFormData(answers, assessmentSchema).errors : []
  return [...eligibility, ...assessment].map((error) => error.stack)
}
