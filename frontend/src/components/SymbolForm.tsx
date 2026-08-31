import { useEffect, useRef, useState } from "react";
import type { ChangeEvent, FormEvent, KeyboardEvent } from "react";
import { searchSymbols } from "../api/stocksApi";
import type { SymbolSuggestion } from "../types/stock";

const SEARCH_DEBOUNCE_MS = 250;

interface SymbolFormProps {
  value: string;
  onChange: (value: string) => void;
  onSubmit: (symbol: string) => void;
  isLoading: boolean;
}

/**
 * The ticker-entry command bar
 */
export function SymbolForm({ value, onChange, onSubmit, isLoading }: SymbolFormProps) {
  const [suggestions, setSuggestions] = useState<SymbolSuggestion[]>([]);
  const [isOpen, setIsOpen] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);

  const containerRef = useRef<HTMLDivElement>(null);

  const debounceRef = useRef<number | undefined>(undefined);
  const abortRef = useRef<AbortController | null>(null);

  // Cancel any pending if component messes up mid search
  useEffect(() => {
    return () => {
      window.clearTimeout(debounceRef.current);
      abortRef.current?.abort();
    };
  }, []);

  // Closes the dropdown on a click anywhere outside
  useEffect(() => {
    if (!isOpen) {
      return;
    }

    function handleOutsideClick(event: MouseEvent) {
      if (containerRef.current && !containerRef.current.contains(event.target as Node)) {
        setIsOpen(false);
      }
    }

    document.addEventListener("mousedown", handleOutsideClick);
    return () => document.removeEventListener("mousedown", handleOutsideClick);
  }, [isOpen]);

  /**
   * Debounces the autocomplete lookup for `query`, cancelling any previous pending stuff
   */
  function scheduleSearch(query: string) {
    window.clearTimeout(debounceRef.current);
    abortRef.current?.abort();

    const trimmed = query.trim();
    if (trimmed.length === 0) {
      setSuggestions([]);
      setIsOpen(false);
      return;
    }

    debounceRef.current = window.setTimeout(async () => {
      const controller = new AbortController();
      abortRef.current = controller;
      try {
        const results = await searchSymbols(trimmed, controller.signal);

        if (!controller.signal.aborted) {
          setSuggestions(results);
          setIsOpen(results.length > 0);
          setActiveIndex(-1);
        }
      } catch {
      }
    }, SEARCH_DEBOUNCE_MS);
  }

  function handleInputChange(event: ChangeEvent<HTMLInputElement>) {
    const next = event.target.value;
    onChange(next);
    scheduleSearch(next);
  }

  /** Fills the input with the chosen suggestion and searches immediately */
  function selectSuggestion(suggestion: SymbolSuggestion) {
    window.clearTimeout(debounceRef.current);
    abortRef.current?.abort();
    setIsOpen(false);
    setSuggestions([]);
    onChange(suggestion.symbol);
    onSubmit(suggestion.symbol);
  }

  function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (!isOpen || suggestions.length === 0) {
      return;
    }

    if (event.key === "ArrowDown") {
      event.preventDefault();
      setActiveIndex((index) => (index + 1) % suggestions.length);
    } else if (event.key === "ArrowUp") {
      event.preventDefault();
      setActiveIndex((index) => (index - 1 + suggestions.length) % suggestions.length);
    } else if (event.key === "Enter" && activeIndex >= 0) {
      // Only intercept Enter when a suggestion is actually highlighted - otherwise let it fall through to the form's normal
      event.preventDefault();
      selectSuggestion(suggestions[activeIndex]);
    } else if (event.key === "Escape") {
      setIsOpen(false);
    }
  }

  function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setIsOpen(false);
    const trimmed = value.trim().toUpperCase();
    if (trimmed) {
      onSubmit(trimmed);
    }
  }

  return (
    <form className="symbol-form" onSubmit={handleSubmit}>
      <label htmlFor="symbol-input">
        <span className="symbol-form-label-text">Ticker</span>
      </label>
      <div className="symbol-input-wrap" ref={containerRef}>
        <input
          id="symbol-input"
          value={value}
          onChange={handleInputChange}
          onKeyDown={handleKeyDown}
          onFocus={() => {
            if (suggestions.length > 0) {
              setIsOpen(true);
            }
          }}
          placeholder="e.g. TSLA"
          maxLength={10}
          autoComplete="off"
          spellCheck={false}
          role="combobox"
          aria-expanded={isOpen}
          aria-autocomplete="list"
          aria-controls="symbol-suggestions"
        />
        {isOpen && suggestions.length > 0 && (
          <ul className="symbol-suggestions" id="symbol-suggestions" role="listbox">
            {suggestions.map((suggestion, index) => (
              <li
                key={suggestion.symbol}
                role="option"
                aria-selected={index === activeIndex}
                className={index === activeIndex ? "active" : undefined}
                onMouseDown={(event) => {
                  event.preventDefault();
                  selectSuggestion(suggestion);
                }}
                onMouseEnter={() => setActiveIndex(index)}
              >
                <span className="symbol-suggestion-symbol">{suggestion.symbol}</span>
                <span className="symbol-suggestion-name">{suggestion.name}</span>
                <span className="symbol-suggestion-exchange">{suggestion.exchange}</span>
              </li>
            ))}
          </ul>
        )}
      </div>
      <button type="submit" disabled={isLoading}>
        {isLoading ? "Loading…" : "Search"}
      </button>
    </form>
  );
}
