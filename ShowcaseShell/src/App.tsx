import './App.css'
import WorkflowPanel from './WorkflowPanel'

type ShowcaseItem = {
  number: string
  title: string
  summary: string
  stack: string[]
  status: string
  tone: string
}

const showcaseItems: ShowcaseItem[] = [
  {
    number: '01',
    title: 'Jolisoft full system',
    summary: 'A deliberately small, multi-tier workflow showing UI, API, middleware, persistence, and local service boundaries.',
    stack: ['React', 'ASP.NET Core', 'SQL Server'],
    status: 'First vertical slice',
    tone: 'teal',
  },
  {
    number: '02',
    title: 'DynamicQuestions',
    summary: 'Schema-driven forms, custom widgets, validation, and the patterns that grew from the CRM question experiments.',
    stack: ['React', 'TypeScript', 'RJSF'],
    status: 'Comparison pending',
    tone: 'coral',
  },
  {
    number: '03',
    title: 'WPFTools',
    summary: 'A focused desktop workbench for generated Dataverse models, metadata inspection, and resilient service access.',
    stack: ['WPF', '.NET', 'Dataverse'],
    status: 'Planned',
    tone: 'gold',
  },
]

function App() {
  return (
    <main className="showcase-shell">
      <nav className="navbar shell-nav" aria-label="Showcase navigation">
        <a className="navbar-brand brand-mark" href="/">
          <span className="brand-mark__dot" aria-hidden="true" />
          Jolisoft / showcase
        </a>
        <div className="shell-nav__links">
          <a href="#systems">Systems</a>
          <a href="#principles">Principles</a>
          <a href="#doco">Doco</a>
        </div>
      </nav>

      <section className="container hero-section">
        <div className="hero-copy">
          <p className="eyebrow">Private technical archive · 2026</p>
          <h1>Useful things,<br /><em>kept alive.</em></h1>
          <p className="hero-lede">
            A small, runnable home for the patterns gathered across years of Jolisoft work: dynamic forms, layered systems, service boundaries, and the tools around them.
          </p>
          <a className="btn btn-dark hero-action" href="#systems">Explore the systems <span aria-hidden="true">↓</span></a>
        </div>
        <div className="hero-note" aria-label="Current implementation status">
          <div className="hero-note__index">01 <span>/ 03</span></div>
          <div className="hero-note__rule" />
          <p>Building a local-first vertical slice across the catalog, API, persistence, and fake platform services.</p>
          <span className="hero-note__status"><span className="status-pulse" /> In progress</span>
        </div>
      </section>

      <section className="container systems-section" id="systems">
        <div className="section-heading">
          <div>
            <p className="eyebrow">The collection</p>
            <h2>Three ways in.</h2>
          </div>
          <p className="section-heading__aside">Each system is intentionally smaller than its source. The point is the technique, not the paperwork.</p>
        </div>
        <div className="system-grid">
          {showcaseItems.map((item) => (
            <article className={`system-card system-card--${item.tone}`} key={item.number}>
              <div className="system-card__top"><span>{item.number}</span><span className="system-card__status">{item.status}</span></div>
              <div>
                <h3>{item.title}</h3>
                <p>{item.summary}</p>
              </div>
              <div className="system-card__footer">
                <div className="tag-list">{item.stack.map((tag) => <span className="badge rounded-pill" key={tag}>{tag}</span>)}</div>
                <button className="icon-button" type="button" aria-label={`Open ${item.title}`} title="Open system">↗</button>
              </div>
            </article>
          ))}
        </div>
      </section>

      <WorkflowPanel />

      <section className="principles-band" id="principles">
        <div className="container principles-grid">
          <p className="eyebrow">Working principles</p>
          <div className="principle"><strong>01</strong><span>Buildable</span><p>Every selected example earns its place by compiling and showing a complete path.</p></div>
          <div className="principle"><strong>02</strong><span>Local-first</span><p>Fakes stand in for cloud platforms so the techniques remain usable offline.</p></div>
          <div className="principle"><strong>03</strong><span>Intentional</span><p>Small schemas and focused boundaries preserve the idea without recreating noise.</p></div>
        </div>
      </section>

      <footer className="container shell-footer" id="doco">
        <span>Jolisoft showcase shell · React / TypeScript / Vite</span>
        <span>doco/101-showcase-plan.md</span>
      </footer>
    </main>
  )
}

export default App
