import { BrowserRouter, Route, Routes } from 'react-router-dom'
import DictionaryPage from './pages/DictionaryPage'
import WordPage from './pages/WordPage'

function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route element={<DictionaryPage />} path="/" />
        <Route element={<WordPage />} path="/word/:word" />
      </Routes>
    </BrowserRouter>
  )
}

export default App
