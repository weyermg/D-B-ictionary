export interface Word {
  id: string
  word: string
  language: string
  etymologyIndex: number
  definitionIndex: number
  definitions: string[]
}

export interface CreateWord {
  word: string
  language: string
  etymologyIndex: number
  definitionIndex: number
  definitions: string[]
}

interface ProblemDetails {
  title?: string
  detail?: string
}

async function getErrorMessage(response: Response): Promise<string> {
  try {
    const problem = await response.json() as ProblemDetails
    return problem.detail ?? problem.title ?? `Request failed (${response.status}).`
  } catch {
    return `Request failed (${response.status}).`
  }
}

export async function searchWords(query: string, signal: AbortSignal): Promise<Word[]> {
  const params = new URLSearchParams({ query, language: 'en' })
  const response = await fetch(`/api/words?${params}`, { signal })
  if (!response.ok) throw new Error(await getErrorMessage(response))
  return await response.json() as Word[]
}

export async function addWord(word: CreateWord): Promise<Word> {
  const response = await fetch('/api/words', {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify(word),
  })
  if (!response.ok) throw new Error(await getErrorMessage(response))
  return await response.json() as Word
}