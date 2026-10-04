import styles from './WPFToolsPanel.module.css'

export default function WPFToolsPanel() {
  return (
    <section className={`container ${styles.panel}`} id="wpf-tools" aria-labelledby="wpf-tools-title">
      <p className="eyebrow">Windows desktop example</p>
      <h2 id="wpf-tools-title">Try the query workbench.</h2>
      <p>Run this command from the repository root to open WPFTools:</p>
      <pre className={styles.command}><code>{'dotnet run --project .\\WPFTools\\Jolisoft.WPFTools\\Jolisoft.WPFTools.csproj'}</code></pre>
      <p>Toggle <strong>Active records only</strong>, <strong>Unexpired or no expiry</strong>, and <strong>Join active organisation</strong>, then click <strong>Run SDK query</strong>. The results use fictional records and a fixed reference date of 3 October 2026 UTC.</p>
      <p>The desktop example runs independently of this catalog, the API, and SQL Server. Its local service supports filters and one organisation lookup inner join, including aliased columns. A retrieve-all loop fetches two rows per page; the desktop status shows row and page counts. The fixed state choices appear inline and supply the results grid's State labels.</p>
      <p className={styles.reference}>Run guide: HOW_TO_RUN.md, section 5. Query scope and checks: WPFTools/README.md.</p>
      <p>Open <strong>SDK LINQ…</strong> for a separate typed-query window with assessment filters and an organisation join. Its trace shows the actual SDK-generated query predicates and join details.</p>
      <p>Open <strong>Retry scenarios…</strong> in the desktop workbench for a separate window showing temporary-failure recovery, retry exhaustion, and a query error that stops immediately.</p>
    </section>
  )
}
