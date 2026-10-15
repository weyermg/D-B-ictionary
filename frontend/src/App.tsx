import { useEffect, useState } from 'react'
import type { FormEvent } from 'react'
import { BookOpenText, LoaderCircle, Plus, Search, X } from 'lucide-react'
import { addWord, searchWords } from './api'
import type { CreateWord, Word } from './api'
import './dictionary.css'

function App() {
  const [searchText, setSearchText] = useState('')
  const [searchRevision, setSearchRevision] = useState(0)
  const [results, setResults] = useState<Word[]>([])
  const [searchState, setSearchState] = useState<'idle' | 'loading' | 'ready' | 'empty' | 'error'>('idle')
  const [errorMessage, setErrorMessage] = useState('')
  const [isAddOpen, setIsAddOpen] = useState(false)

  useEffect(() => {
    const query = searchText.trim()
    if (!query) {
      setResults([])
      setSearchState('idle')
      setErrorMessage('')
      return
    }

    const controller = new AbortController()
    const timer = window.setTimeout(async () => {
      setSearchState('loading')
      setErrorMessage('')

      try {
        const words = await searchWords(query, controller.signal)
        setResults(words)
        setSearchState(words.length > 0 ? 'ready' : 'empty')
      } catch (error) {
        if (controller.signal.aborted) return
        setResults([])
        setSearchState('error')
        setErrorMessage(error instanceof Error ? error.message : 'Search failed.')
      }
    }, 280)

    return () => {
      window.clearTimeout(timer)
      controller.abort()
    }
  }, [searchText, searchRevision])

  async function handleAdd(word: CreateWord) {
    await addWord(word)
    setIsAddOpen(false)
    setSearchText(word.word)
    setSearchRevision((revision) => revision + 1)
  }

  return (
    <main className="app-shell">
      <header className="topbar">
        <a className="wordmark" href="/" aria-label="Dictionary home">
          <span className="wordmark-icon"><BookOpenText size={19} strokeWidth={1.8} /></span>
          <span>D/B</span>
        </a>
        <div className="topbar-meta">
          <span className="connection-dot" />
          <span>ENGLISH INDEX</span>
        </div>
      </header>

      <section className="search-area" aria-label="Dictionary search">
        <div className="search-row">
          <div className="search-box">
            {searchState === 'loading'
              ? <LoaderCircle className="search-icon spinning" size={20} aria-hidden="true" />
              : <Search className="search-icon" size={20} aria-hidden="true" />}
            <input
              aria-label="Search words and definitions"
              autoComplete="off"
              placeholder="Search words and definitions"
              type="search"
              value={searchText}
              onChange={(event) => setSearchText(event.currentTarget.value)}
            />
            <kbd className="search-hint">⌘ K</kbd>
          </div>
          <button className="add-button" type="button" onClick={() => setIsAddOpen(true)}>
            <Plus size={18} aria-hidden="true" />
            <span>Add word</span>
          </button>
        </div>
      </section>

      <section className="results-area" aria-live="polite">
        {searchState === 'idle' && <div className="quiet-rule" />}
        {searchState === 'empty' && <p className="state-message">No matches found.</p>}
        {searchState === 'error' && <p className="state-message error-message">{errorMessage}</p>}
        {results.length > 0 && (
          <>
            <div className="results-heading">
              <span>ENTRIES</span>
              <span>{results.length.toString().padStart(2, '0')}</span>
            </div>
            <ol className="results-list">
              {results.map((word, index) => (
                <li className="result-row" key={word.id} style={{ animationDelay: `${Math.min(index, 8) * 35}ms` }}>
                  <span className="result-number">{(index + 1).toString().padStart(2, '0')}</span>
                  <article className="result-content">
                    <div className="result-title-row">
                      <h2>{word.word}</h2>
                      <span className="language-tag">{word.language}</span>
                    </div>
                    <ol className="definitions">
                      {word.definitions.map((definition, definitionIndex) => (
                        <li key={`${word.id}-${definitionIndex}`}>{definition}</li>
                      ))}
                    </ol>
                    <div className="entry-meta">
                      <span>ETYMOLOGY {word.etymologyIndex}</span>
                      <span>DEFINITION {word.definitionIndex}</span>
                    </div>
                  </article>
                </li>
              ))}
            </ol>
          </>
        )}
      </section>

      {isAddOpen && <AddWordDialog onClose={() => setIsAddOpen(false)} onSubmit={handleAdd} />}
    </main>
  )
}

interface AddWordDialogProps {
  onClose: () => void
  onSubmit: (word: CreateWord) => Promise<void>
}

function AddWordDialog({ onClose, onSubmit }: AddWordDialogProps) {
  const [word, setWord] = useState('')
  const [language, setLanguage] = useState('en')
  const [etymologyIndex, setEtymologyIndex] = useState('1')
  const [definitionIndex, setDefinitionIndex] = useState('1')
  const [definitions, setDefinitions] = useState('')
  const [error, setError] = useState('')
  const [isSaving, setIsSaving] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const definitionList = definitions
      .split(/\r?\n/)
      .map((definition) => definition.trim())
      .filter(Boolean)

    if (definitionList.length === 0) {
      setError('Add at least one definition.')
      return
    }

    setIsSaving(true)
    setError('')
    try {
      await onSubmit({
        word: word.trim(),
        language: language.trim(),
        etymologyIndex: Number(etymologyIndex),
        definitionIndex: Number(definitionIndex),
        definitions: definitionList,
      })
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : 'Could not add this word.')
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="modal-backdrop" onMouseDown={(event) => {
      if (event.target === event.currentTarget) onClose()
    }}>
      <section aria-labelledby="add-word-title" aria-modal="true" className="add-dialog" role="dialog">
        <div className="dialog-heading">
          <div>
            <span className="eyebrow">NEW ENTRY</span>
            <h2 id="add-word-title">Add a word</h2>
          </div>
          <button aria-label="Close dialog" className="icon-button" onClick={onClose} type="button">
            <X size={19} />
          </button>
        </div>
        <form onSubmit={submit}>
          <label>
            <span>Word</span>
            <input autoFocus maxLength={200} required value={word} onChange={(event) => setWord(event.currentTarget.value)} />
          </label>
          <div className="form-grid">
            <label>
              <span>Language</span>
              <input maxLength={32} required value={language} onChange={(event) => setLanguage(event.currentTarget.value)} />
            </label>
            <label>
              <span>Etymology index</span>
              <input min="0" required step="1" type="number" value={etymologyIndex} onChange={(event) => setEtymologyIndex(event.currentTarget.value)} />
            </label>
            <label>
              <span>Definition index</span>
              <input min="0" required step="1" type="number" value={definitionIndex} onChange={(event) => setDefinitionIndex(event.currentTarget.value)} />
            </label>
          </div>
          <label>
            <span>Definitions <small>ONE PER LINE</small></span>
            <textarea required rows={4} value={definitions} onChange={(event) => setDefinitions(event.currentTarget.value)} />
          </label>
          {error && <p className="form-error" role="alert">{error}</p>}
          <div className="dialog-actions">
            <button className="cancel-button" onClick={onClose} type="button">Cancel</button>
            <button className="submit-button" disabled={isSaving} type="submit">
              {isSaving ? 'Adding…' : 'Add entry'}
            </button>
          </div>
        </form>
      </section>
    </div>
  )
}

export default App
