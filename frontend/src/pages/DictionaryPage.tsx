import { useEffect, useState } from 'react'
import { BookOpenText, LoaderCircle, Plus, Search } from 'lucide-react'
import { Link } from 'react-router-dom'
import AddWordDialog from '../components/AddWordDialog'
import { addWord, searchWords } from '../api'
import type { CreateWord, Word } from '../api'

function DictionaryPage() {
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
    <main className="mx-auto min-h-screen w-[calc(100%-2rem)] max-w-[1040px] animate-[page-enter_420ms_ease-out_both] sm:w-[calc(100%-3rem)]">
      <header className="flex h-[62px] items-center justify-between border-b border-line sm:h-[74px]">
        <Link className="inline-flex items-center gap-2.5 text-sm font-extrabold text-ink no-underline" to="/" aria-label="Dictionary home">
          <span className="grid size-8 place-items-center rounded-[9px_9px_9px_3px] bg-forest text-white"><BookOpenText size={19} strokeWidth={1.8} /></span>
          <span>D/B</span>
        </Link>
      </header>
      <section className="pb-7 pt-7 sm:pt-[58px]" aria-label="Dictionary search">
        <div className="grid grid-cols-1 gap-3 sm:grid-cols-[minmax(0,1fr)_auto]">
          <div className="flex h-14 min-w-0 items-center gap-2.5 rounded-[11px] border border-[#d5dfd7] bg-field px-[13px] shadow-[0_8px_28px_rgba(34,65,53,0.055)] transition-[border-color,box-shadow] focus-within:border-[#8eaa9b] focus-within:shadow-[0_0_0_4px_rgba(36,92,79,0.09),0_8px_28px_rgba(34,65,53,0.06)] sm:h-[62px] sm:gap-3.5 sm:px-[18px]">
            {searchState === 'loading'
              ? <LoaderCircle className="shrink-0 animate-spin text-forest" size={20} aria-hidden="true" />
              : <Search className="shrink-0 text-forest" size={20} aria-hidden="true" />}
            <input
              className="min-w-0 w-full border-0 bg-transparent text-sm text-ink outline-none placeholder:text-[#94a099] focus-visible:outline-none sm:text-base"
              aria-label="Search words and definitions"
              autoComplete="off"
              placeholder="Search words and definitions"
              type="search"
              value={searchText}
              onChange={(event) => setSearchText(event.currentTarget.value)}
            />
          </div>
          <button className="inline-flex min-h-11 items-center justify-center gap-2 rounded-[9px] border border-forest bg-forest px-[17px] text-[13px] font-bold text-white transition-[background,transform] hover:bg-forest-dark active:translate-y-px sm:min-h-12" type="button" onClick={() => setIsAddOpen(true)}>
            <Plus className="text-[#f3d38b]" size={18} aria-hidden="true" />
            <span>Add word</span>
          </button>
        </div>
      </section>
      <section className="pb-[72px] pt-[10px]" aria-live="polite">
        {searchState === 'idle' && <div className="h-px bg-gradient-to-r from-line to-transparent" />}
        {searchState === 'empty' && <p className="py-[26px] text-[13px] text-muted">No matches found.</p>}
        {searchState === 'error' && <p className="py-[26px] text-[13px] text-danger">{errorMessage}</p>}
        {results.length > 0 && (
          <>
            <div className="flex justify-between border-b border-line pb-3 pt-[17px] text-[10px] font-extrabold tracking-[1.25px] text-muted">
              <span>ENTRIES</span>
              <span>{results.length.toString().padStart(2, '0')}</span>
            </div>
            <ol className="m-0 list-none p-0">
              {results.map((word, index) => (
                <li className="grid grid-cols-[30px_minmax(0,1fr)] gap-1 border-b border-line pb-5 pt-[18px] animate-[row-enter_300ms_ease_both] sm:grid-cols-[52px_minmax(0,1fr)] sm:gap-[10px] sm:pb-6 sm:pt-[22px]" key={word.documentId} style={{ animationDelay: `${Math.min(index, 8) * 35}ms` }}>
                  <span className="pt-1 text-[11px] tabular-nums text-[#9aa69e]">{(index + 1).toString().padStart(2, '0')}</span>
                  <article className="min-w-0">
                    <div className="flex flex-wrap items-baseline gap-3">
                      <h2 className="m-0 font-serif text-[29px] font-[550] leading-[1.05] text-ink sm:text-[32px]"><Link className="text-inherit decoration-coral underline-offset-4 no-underline hover:underline" to={`/word/${encodeURIComponent(word.word)}`}>{word.word}</Link></h2>
                      <span className="rounded-[4px] bg-[#e4eee7] px-[7px] py-[3px] text-[10px] font-extrabold uppercase text-[#2e6c5e]">{word.language}</span>
                    </div>
                    {word.etymologies.map((etymology, etymologyIndex) => (
                      <section className="mt-3 first:mt-0" key={`${word.documentId}-${etymologyIndex}`}>
                        <div className="text-[9px] font-extrabold tracking-[0.9px] text-muted">ETYMOLOGY {etymologyIndex + 1}</div>
                        <ol className="mb-3.5 mt-3 grid list-decimal gap-1.5 pl-5 text-[#495b53]">
                          {etymology.definitions.map((definition, definitionIndex) => (
                            <li className="pl-[3px] text-[13px] leading-[1.65] marker:font-serif marker:text-coral sm:text-sm" key={`${word.documentId}-${etymologyIndex}-${definitionIndex}`}>{definition}</li>
                          ))}
                        </ol>
                      </section>
                    ))}
                    <div className="flex gap-4 text-[9px] font-extrabold tracking-[0.8px] text-muted">
                      <span>INTERNAL ID {word.internalId}</span>
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

export default DictionaryPage