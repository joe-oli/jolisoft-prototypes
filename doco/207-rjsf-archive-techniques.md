# RJSF and AJV techniques beyond DynamicQuestions

Reviewed: 4 October 2026. These examples stand alone after the historical archive is deleted. No historical source files or paths are needed to understand or reuse them.

## 1. Packages and the curated baseline

`@rjsf/core` renders a React form from JSON Schema, applies UI-schema presentation, and coordinates change/submission/error callbacks. `@rjsf/validator-ajv8` supplies the AJV 8 validation implementation passed through the Form validator prop. Rendering, validation and persistence are distinct responsibilities; neither package supplies the application's storage or workflow. See the project's [validation documentation](https://rjsf-team.github.io/react-jsonschema-form/docs/usage/validation/).

The curated package.json ranges are `^6.8.0` for both packages; the installed versions checked for this pass are both 6.8.0. A range is not an exact pin: use the committed lockfile and npm ci for repeatable installation. The online documentation currently describes a newer v6 release, so the code below is checked against this repository's installed types. Types such as RJSFSchema, FieldProps and ErrorListProps come from `@rjsf/utils`; a separate project importing it should declare it directly at a compatible version.

Already runnable in the curated example:

| Technique | Permanent repository location |
| --- | --- |
| Schema/UI-schema editors, trusted preview and error recovery | DynamicQuestions/src/foundations/main.tsx |
| Controlled answers, draft save, form-ref validated save and custom validation | DynamicQuestions/src/foundations/main.tsx |
| Named text/textarea/radio/select/checkbox/date widgets and instruction field | DynamicQuestions/src/fields/ |
| Schema validation outside the rendered form | DynamicQuestions/src/adapters/validation.ts |
| Host handshake, branching wizard, persistence, permissions and completion lifecycle | DynamicQuestions/src/App.tsx, embedded.tsx and adapters/ |

The extra examples below preserve the gaps rather than duplicating that application. No new UI or dependency upgrades were required.

## 2. Complete reference component

Copy this TSX listing into a React project with the stated packages, then render `<RjsfTechniqueReference />`. It combines a structured answer field, multi-select values, optional notes, mandatory consent, zero-safe numeric input and an accessible custom error summary. Instructions remain presentation metadata; answers remain data.

```tsx
import { useState } from 'react';
import Form from '@rjsf/core';
import validator from '@rjsf/validator-ajv8';
import type { ErrorListProps, FieldProps, RJSFSchema, UiSchema, WidgetProps } from '@rjsf/utils';

type Question = { value?: string[]; notes?: string };
type Answers = { skills?: Question; consent?: boolean; budget?: number; code?: string };

export const referenceSchema: RJSFSchema = {
  type: 'object', title: 'ACME training request', required: ['skills', 'consent', 'budget', 'code'],
  properties: {
    skills: {
      type: 'object', title: 'Skills to develop', required: ['value'],
      properties: {
        value: {
          type: 'array', minItems: 1, uniqueItems: true,
          items: { type: 'string', enum: ['Planning', 'Reporting', 'Review'] }
        },
        notes: { type: 'string', maxLength: 3600 }
      }
    },
    consent: { type: 'boolean', title: 'I agree to the training conditions', const: true },
    budget: { type: 'number', title: 'Budget', minimum: 0, maximum: 10000 },
    code: { type: 'string', title: 'Reference code', minLength: 3, maxLength: 12, pattern: '^[A-Z0-9-]+$' }
  }
};

export const referenceUiSchema: UiSchema = {
  skills: { 'ui:field': 'QuestionWithNotes', 'ui:options': { instructionHref: 'https://example.com/training' } },
  budget: { 'ui:widget': 'ZeroSafeNumber' }
};

function QuestionWithNotes(props: FieldProps<Question>) {
  const [showNotes, setShowNotes] = useState(false);
  const data = props.formData ?? {};
  const id = props.fieldPathId.$id;
  const locked = props.readonly || props.disabled;
  const valueSchema = props.schema.properties?.value as RJSFSchema | undefined;
  const items = valueSchema?.items as RJSFSchema | undefined;
  const choices = (items?.enum ?? []).map(String);
  const href = props.uiSchema?.['ui:options']?.instructionHref;
  const notesAllowed = Boolean(props.schema.properties && 'notes' in props.schema.properties);
  const change = (patch: Partial<Question>) =>
    props.onChange({ ...data, ...patch }, props.fieldPathId.path);
  return <fieldset>
    <legend>{props.schema.title}{props.required ? ' *' : ''}</legend>
    <label htmlFor={id}>Choose one or more skills</label>
    <select id={id} multiple size={3} value={data.value ?? []} disabled={locked}
      onChange={event => change({ value: Array.from(event.target.selectedOptions, option => option.value) })}>
      {choices.map(choice => <option key={choice} value={choice}>{choice}</option>)}
    </select>
    {props.errorSchema?.value?.__errors?.map((error, index) => <p role="alert" key={index}>{error}</p>)}
    {typeof href === 'string' && href.startsWith('https://') &&
      <p><a href={href} target="_blank" rel="noopener noreferrer">Training instructions</a></p>}
    {notesAllowed && <>
      <button type="button" aria-expanded={showNotes} aria-controls={`${id}-notes`}
        onClick={() => setShowNotes(current => !current)}>Reviewer notes</button>
      {showNotes && <div id={`${id}-notes`}>
        <label htmlFor={`${id}-notes-input`}>Notes</label>
        <textarea id={`${id}-notes-input`} value={data.notes ?? ''} disabled={props.disabled}
          readOnly={props.readonly} maxLength={3600} onChange={event => change({ notes: event.target.value })}/>
        {props.errorSchema?.notes?.__errors?.map((error, index) => <p role="alert" key={index}>{error}</p>)}
      </div>}
    </>}
  </fieldset>;
}

function ZeroSafeNumber(props: WidgetProps) {
  return <input id={props.id} type="number" step="any" value={props.value ?? ''}
    disabled={props.disabled} readOnly={props.readonly}
    onBlur={() => props.onBlur(props.id, props.value)}
    onFocus={() => props.onFocus(props.id, props.value)}
    onChange={event => props.onChange(event.target.value === '' ? undefined : Number(event.target.value))}/>;
}

function ErrorSummary({ errors }: ErrorListProps) {
  if (!errors.length) return null;
  return <section role="alert" aria-label="Form errors">
    <h2>Correct these answers</h2>
    <ul>{errors.map((error, index) => <li key={index}>{error.stack}</li>)}</ul>
  </section>;
}

export function RjsfTechniqueReference() {
  const [data, setData] = useState<Answers>({});
  const [saved, setSaved] = useState('Nothing submitted yet.');
  return <>
    <Form<any> schema={referenceSchema} uiSchema={referenceUiSchema} validator={validator}
      fields={{ QuestionWithNotes }} widgets={{ ZeroSafeNumber }}
      templates={{ ErrorListTemplate: ErrorSummary }} formData={data}
      onChange={event => setData(event.formData ?? {})}
      onSubmit={event => setSaved(JSON.stringify(event.formData))} noHtml5Validate>
      <button type="submit">Validate example</button>
    </Form>
    <pre aria-label="Validated answers">{saved}</pre>
  </>;
}
```

The form validates on submission and displays the payload; it does not persist it. Named field/widget registrations remain executable React code outside the JSON schemas. The v6 custom-field onChange includes the field path; an old one-argument FieldProps callback and idSchema access should not be copied unchanged. The notes button actually reveals/hides content; it never submits the form. Revealing notes is allowed even when answers are read-only, but editing remains locked.

## 3. Structured questions versus scalar widgets

The preserved pattern stores an answer as `{ value, notes }` instead of just a scalar. A custom field owns the entire object and updates one member with `{ ...data, ...patch }`, retaining the others. The UI schema selects that field with ui:field. A widget, selected with ui:widget, typically renders one property's value; the numeric example uses that simpler contract.

A notes field is shown only when the question schema defines it. Validation messages for nested value and notes are read from the corresponding errorSchema nodes. Rendering only a top-level rawErrors list would miss that structure. A custom field must honor readonly/disabled itself; setting flags on Form does not fix a custom component that ignores them.

The archived question variants also put instruction URLs in answer objects and rendered HTML titles. This preservation separates fixed instruction links into ui:options and renders labels as text. If rich instructions are needed later, define and sanitize that content explicitly rather than directly rendering arbitrary schema HTML. No raw HTML renderer or sanitizer package is introduced here.

## 4. Required does not mean nonempty, true or positive

| Desired rule | Schema expression | Expected failure |
| --- | --- | --- |
| Object must contain an answer property | required: ['value'] on that object | Missing value |
| At least one selected item | minItems: 1 | Empty array |
| No duplicate selections | uniqueItems: true | Repeated item |
| Consent must be accepted | type: 'boolean', const: true, plus required at parent | false or missing |
| Numeric range | minimum: 0, maximum: 10000 | Negative or above limit |
| String shape and size | pattern, minLength, maxLength | Invalid reference code |

The original array and consent experiments established these distinctions. Required is a property-presence rule; false and [] are present values. Parent required plus nested required keeps a structured question from disappearing entirely. Date regex experiments are also a useful caution: a pattern permitting day 31 in every month does not validate a calendar date. The curated date widget already uses format: date with the validator.

Use nullish fallback (`value ?? ''`) instead of truthy fallback (`value || ''`) for numeric inputs so zero remains visible. Clearing the custom number widget emits undefined instead of Number('') = 0. HTML input constraints guide entry; JSON Schema/AJV remains responsible for the actual validation rules. An integer-only question should use type: integer and an appropriate input step.

## 5. Error summaries and validation APIs

The archive includes working ErrorListTemplate registration and older theme/wrapper experiments that did not render errors as intended. Preserve `templates={{ ErrorListTemplate: ErrorSummary }}` rather than replacing the Form element with a custom form wrapper and manually calling its onSubmit. Keep complete nested paths in error.stack; splitting a path at its first dot can lose which answer member failed.

Error text customization and a summary layout are separate responsibilities. The preserved example changes layout, not validator keywords. Custom business validation, draft saves and imperative form-ref submission are already present in the permanent foundations component and are not duplicated here. Official [validation guidance](https://rjsf-team.github.io/react-jsonschema-form/docs/usage/validation/) describes the validator boundary; examples here are checked against installed 6.8.0.

## 6. Schema transformation without mutating fixtures

Historical helpers dynamically marked questions required or made custom fields read-only. Their heuristics depended on defaults and shallow copies. Defaults can be false, zero or empty strings, so truthiness is not a reliable signal that a question is an instruction or optional. Use explicit required names and return a new schema:

```ts
import type { RJSFSchema, UiSchema } from '@rjsf/utils';

export function requireSelectedQuestions(schema: RJSFSchema, names: string[]): RJSFSchema {
  for (const name of names) {
    if (!Object.hasOwn(schema.properties ?? {}, name)) throw new Error(`Unknown question: ${name}`);
  }
  return { ...schema, required: [...new Set([...(schema.required ?? []), ...names])] };
}

export function readonlyQuestions(uiSchema: UiSchema, names: string[]): UiSchema {
  const next = { ...uiSchema };
  for (const name of names) next[name] = { ...(uiSchema[name] ?? {}), 'ui:readonly': true };
  return next;
}
```

These functions handle explicitly named immediate properties only. They do not claim recursive traversal of arbitrary schema arrays, refs or alternatives. Spreading the individual UI nodes avoids mutating the source node through a shallow root copy. For whole-form read-only mode use the curated Form readonly behavior and compliant custom controls.

## 7. Scan coverage and permanent preservation

All seven archive families were traversed for RJSF packages, schema/UI-schema markers and validation/custom-control hooks, with targeted follow-up searches ignoring ignore-files to catch nested sources. The initial candidate search found 126 files: 24 in one foundations family, 43 in one hosted-form family and 59 in its later variants. These are keyword matches, not unique techniques. Dependency/build outputs, minified bundles and files larger than 1 MiB were excluded; ZIPs, screenshots and every line of duplicated variants were not reviewed exhaustively.

Confirmed gaps preserved here: structured value/notes questions; multi-select and checkbox-array schema semantics; mandatory boolean consent; numeric zero/empty handling and range constraints; nested error rendering; ErrorListTemplate registration; explicit immutable required/read-only transformations; and separation of instructional presentation from saved answers. The multi-select schema can also use the built-in `ui:widget: 'checkboxes'` when no structured wrapper is needed; uniqueItems and minItems still belong to the schema.

Conditional dependencies/oneOf, precompiled AJV, custom format registration and server error injection were not confirmed as implemented archive techniques in this pass. They are not added merely because the packages support them. Imported withTheme or an unfinished wrapper is not proof of a working custom theme. Existing curated form editors, widgets, validation and host lifecycle remain the primary runnable examples.

## 8. Verification

The TSX and TypeScript examples are intended to be checked against the repository's installed RJSF 6.8.0 packages. Validate the schema separately with validator.validateFormData to test missing/false consent, empty/duplicate arrays, range limits and valid zero values. Type checking proves API compatibility, not browser accessibility or live host integration. This documentation pass leaves DynamicQuestions runtime code and the historical archive unchanged.

Validation on 4 October 2026: extracted both code listings and passed strict TypeScript checking against installed RJSF 6.8.0. Seven AJV cases passed: valid zero, rejected false consent, empty/duplicate selections, out-of-range budgets and invalid code pattern. The Form generic is explicitly any to accommodate the independently typed nested field and the default validator; schema validation supplies runtime data checks. No browser rendering check was performed. Scratch checks live under ignored node_modules/.jolisoft-reference-check and are not required by the document.
