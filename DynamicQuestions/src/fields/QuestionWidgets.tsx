import type { WidgetProps } from '@rjsf/utils'

function WidgetErrors({ rawErrors }: Pick<WidgetProps, 'rawErrors'>) {
  if (!rawErrors?.length) return null
  return <div className="question-errors">{rawErrors.map((error) => <div key={error}>{error}</div>)}</div>
}

export function TextQuestionWidget({ id, value, onBlur, onChange, onFocus, placeholder, readonly, disabled, rawErrors }: WidgetProps) {
  return <><input id={id} className="form-control" type="text" value={value ?? ''} placeholder={placeholder} readOnly={readonly} disabled={disabled} onBlur={() => onBlur(id, value)} onChange={(event) => onChange(event.target.value)} onFocus={() => onFocus(id, value)} /><WidgetErrors rawErrors={rawErrors} /></>
}

export function TextareaQuestionWidget({ id, value, onBlur, onChange, onFocus, placeholder, readonly, disabled, rawErrors }: WidgetProps) {
  return <><textarea id={id} className="form-control" rows={4} value={value ?? ''} placeholder={placeholder} readOnly={readonly} disabled={disabled} onBlur={() => onBlur(id, value)} onChange={(event) => onChange(event.target.value)} onFocus={() => onFocus(id, value)} /><WidgetErrors rawErrors={rawErrors} /></>
}

export function CheckboxQuestionWidget({ id, value, onBlur, onChange, onFocus, readonly, disabled, label, rawErrors }: WidgetProps) {
  return <div className="form-check"><input id={id} className="form-check-input" type="checkbox" checked={Boolean(value)} disabled={readonly || disabled} onBlur={() => onBlur(id, value)} onChange={(event) => onChange(event.target.checked)} onFocus={() => onFocus(id, value)} /><label className="form-check-label" htmlFor={id}>{label}</label><WidgetErrors rawErrors={rawErrors} /></div>
}

export function RadioQuestionWidget({ id, value, onBlur, onChange, onFocus, readonly, disabled, options, rawErrors }: WidgetProps) {
  return <><div className="question-options">{options.enumOptions?.map((option) => { const optionValue = String(option.value); const optionId = `${id}-${optionValue}`; return <div className="form-check" key={optionValue}><input id={optionId} className="form-check-input" type="radio" name={id} value={optionValue} checked={value === option.value} disabled={readonly || disabled} onBlur={() => onBlur(id, value)} onChange={() => onChange(option.value)} onFocus={() => onFocus(id, value)} /><label className="form-check-label" htmlFor={optionId}>{option.label}</label></div> })}</div><WidgetErrors rawErrors={rawErrors} /></>
}

export function SelectQuestionWidget({ id, value, onBlur, onChange, onFocus, readonly, disabled, options, rawErrors }: WidgetProps) {
  return <><select id={id} className="form-select" value={value ?? ''} disabled={readonly || disabled} onBlur={() => onBlur(id, value)} onChange={(event) => onChange(event.target.value)} onFocus={() => onFocus(id, value)}><option value="">Select an option</option>{options.enumOptions?.map((option) => <option key={String(option.value)} value={String(option.value)}>{option.label}</option>)}</select><WidgetErrors rawErrors={rawErrors} /></>
}

export function DateQuestionWidget({ id, value, onBlur, onChange, onFocus, readonly, disabled, rawErrors }: WidgetProps) {
  return <><input id={id} className="form-control" type="date" value={value ?? ''} readOnly={readonly} disabled={disabled} onBlur={() => onBlur(id, value)} onChange={(event) => onChange(event.target.value)} onFocus={() => onFocus(id, value)} /><WidgetErrors rawErrors={rawErrors} /></>
}
