import { ArrowLeft, BookOpenText } from 'lucide-react'
import { Link, useParams } from 'react-router-dom'

function WordPage() {
  const { word = '' } = useParams()

  return (
    <main className="mx-auto min-h-screen w-[calc(100%-2rem)] max-w-[1040px] animate-[page-enter_420ms_ease-out_both] sm:w-[calc(100%-3rem)]">
      <header className="flex h-[62px] items-center justify-between border-b border-line sm:h-[74px]">
        <Link className="inline-flex items-center gap-2.5 text-sm font-extrabold text-ink no-underline" to="/" aria-label="Dictionary home">
          <span className="grid size-8 place-items-center rounded-[9px_9px_9px_3px] bg-forest text-white"><BookOpenText size={19} strokeWidth={1.8} /></span>
          <span>D/B</span>
        </Link>
      </header>
      <section className="pb-[72px] pt-7 sm:pt-[58px]">
        <Link className="inline-flex items-center gap-[7px] text-[13px] font-bold text-forest no-underline hover:text-forest-dark hover:underline hover:underline-offset-4" to="/"><ArrowLeft size={16} aria-hidden="true" /> Dictionary</Link>
        <h1 className="mt-[26px] font-serif text-5xl font-[550] leading-[1.1] text-ink">{word}</h1>
      </section>
    </main>
  )
}

export default WordPage