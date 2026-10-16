import { useState } from 'react'
import type { FormEvent } from 'react'
import { Minus, Plus, X } from 'lucide-react'
import type { CreateWord } from '../api'

interface AddWordDialogProps {
  onClose: () => void
  onSubmit: (word: CreateWord) => Promise<void>
}

function AddWordDialog({ onClose, onSubmit }: AddWordDialogProps) {
  const [internalId, setInternalId] = useState('1')
  const [word, setWord] = useState('')
  const [language, setLanguage] = useState('en')
  const [etymologies, setEtymologies] = useState([{ definitions: '' }])
  const [error, setError] = useState('')
  const [isSaving, setIsSaving] = useState(false)

  async function submit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault()
    const normalizedEtymologies = etymologies.map((etymology) => ({
      definitions: etymology.definitions
        .split(/\r?\n/)
        .map((definition) => definition.trim())
        .filter(Boolean),
    }))
    if (normalizedEtymologies.some((etymology) => etymology.definitions.length === 0)) {
      setError('Add at least one definition to every etymology.')
      return
    }

    setIsSaving(true)
    setError('')
    try {
      await onSubmit({
        internalId: Number(internalId),
        word: word.trim(),
        language: language.trim(),
        etymologies: normalizedEtymologies,
      })
    } catch (submitError) {
      setError(submitError instanceof Error ? submitError.message : 'Could not add this word.')
    } finally {
      setIsSaving(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 grid place-items-center bg-[rgba(19,35,29,0.42)] p-5 backdrop-blur-[5px] animate-[fade-in_140ms_ease-out_both]" onMouseDown={(event) => {
      if (event.target === event.currentTarget) onClose()
    }}>
      <section aria-labelledby="add-word-title" aria-modal="true" className="max-h-[min(90vh,720px)] w-full max-w-[490px] overflow-y-auto rounded-[13px] border border-white/80 bg-paper p-[21px] shadow-[0_24px_80px_rgba(17,40,31,0.24)] animate-[dialog-enter_180ms_ease-out_both] sm:p-[26px]" role="dialog">
        <div className="mb-[22px] flex items-start justify-between">
          <div>
            <span className="text-[9px] font-extrabold tracking-[1.25px] text-coral">NEW ENTRY</span>
            <h2 className="mt-1 font-serif text-[29px] font-[550] text-ink" id="add-word-title">Add a word</h2>
          </div>
          <button aria-label="Close dialog" className="grid size-9 place-items-center rounded-[7px] border-0 bg-transparent text-muted hover:bg-[#edf1ec] hover:text-ink" onClick={onClose} type="button">
            <X size={19} />
          </button>
        </div>
        <form className="grid gap-[15px]" onSubmit={submit}>
          <label className="grid gap-[7px] text-xs font-bold text-[#46574f]">
            <span className="flex items-baseline justify-between">Internal ID</span>
            <input className="h-[41px] w-full rounded-[7px] border border-[#d6e0d8] bg-white px-[11px] py-[10px] text-[13px] text-ink focus:border-[#83a191] focus:outline-none focus:shadow-[0_0_0_3px_rgba(36,92,79,0.1)]" min="0" required step="1" type="number" value={internalId} onChange={(event) => setInternalId(event.currentTarget.value)} />
          </label>
          <label className="grid gap-[7px] text-xs font-bold text-[#46574f]">
            <span className="flex items-baseline justify-between">Word</span>
            <input className="h-[41px] w-full rounded-[7px] border border-[#d6e0d8] bg-white px-[11px] py-[10px] text-[13px] text-ink focus:border-[#83a191] focus:outline-none focus:shadow-[0_0_0_3px_rgba(36,92,79,0.1)]" autoFocus maxLength={200} required value={word} onChange={(event) => setWord(event.currentTarget.value)} />
          </label>
          <div className="grid grid-cols-[0.8fr_1fr_1fr] gap-[10px] max-sm:grid-cols-2">
            <label className="col-span-full grid gap-[7px] text-xs font-bold text-[#46574f] sm:col-span-1">
              <span className="flex items-baseline justify-between">Language</span>
              <input className="h-[41px] w-full rounded-[7px] border border-[#d6e0d8] bg-white px-[11px] py-[10px] text-[13px] text-ink focus:border-[#83a191] focus:outline-none focus:shadow-[0_0_0_3px_rgba(36,92,79,0.1)]" maxLength={32} required value={language} onChange={(event) => setLanguage(event.currentTarget.value)} />
            </label>
          </div>
          <div className="grid gap-3">
            {etymologies.map((etymology, etymologyIndex) => (
              <fieldset className="relative m-0 min-w-0 rounded-lg border border-[#d6e0d8] p-3" key={etymologyIndex}>
                <legend className="px-[5px] text-[11px] font-extrabold text-forest">Etymology {etymologyIndex + 1}</legend>
                {etymologies.length > 1 && (
                  <button
                    aria-label={`Remove etymology ${etymologyIndex + 1}`}
                    className="absolute right-2 top-[5px] grid size-[27px] place-items-center rounded-[5px] border-0 bg-transparent text-muted hover:bg-[#f8ebe8] hover:text-danger"
                    onClick={() => setEtymologies((current) => current.filter((_, index) => index !== etymologyIndex))}
                    type="button"
                  >
                    <Minus size={15} />
                  </button>
                )}
                <label className="mt-0.5 grid gap-[7px] text-xs font-bold text-[#46574f]">
                  <span className="flex items-baseline justify-between">Definitions <small className="text-[9px] tracking-[0.8px] text-[#92a097]">ONE PER LINE</small></span>
                  <textarea
                    className="min-h-[104px] w-full resize-y rounded-[7px] border border-[#d6e0d8] bg-white px-[11px] py-[10px] text-[13px] leading-[1.5] text-ink focus:border-[#83a191] focus:outline-none focus:shadow-[0_0_0_3px_rgba(36,92,79,0.1)]"
                    required
                    rows={3}
                    value={etymology.definitions}
                    onChange={(event) => {
                      const definitions = event.currentTarget.value
                      setEtymologies((current) => current.map((item, index) =>
                        index === etymologyIndex ? { ...item, definitions } : item))
                    }}
                  />
                </label>
              </fieldset>
            ))}
          </div>
          <button
            className="inline-flex justify-self-start items-center gap-1.5 border-0 bg-transparent py-[5px] text-xs font-extrabold text-forest hover:text-forest-dark"
            onClick={() => setEtymologies((current) => [...current, { definitions: '' }])}
            type="button"
          >
            <Plus size={15} /> Add etymology
          </button>
          {error && <p className="-mt-[3px] text-xs text-danger" role="alert">{error}</p>}
          <div className="flex justify-end gap-[9px] pt-1.5">
            <button className="min-h-[42px] rounded-lg border border-[#d5dfd7] bg-transparent px-[15px] text-xs font-bold text-[#4d5d55] hover:bg-[#eff3ee]" onClick={onClose} type="button">Cancel</button>
            <button className="min-h-[42px] rounded-[9px] border border-forest bg-forest px-[17px] text-xs font-bold text-white transition-[background,transform] hover:bg-forest-dark active:translate-y-px disabled:cursor-wait disabled:opacity-70" disabled={isSaving} type="submit">
              {isSaving ? 'Adding…' : 'Add entry'}
            </button>
          </div>
        </form>
      </section>
    </div>
  )
}

export default AddWordDialog