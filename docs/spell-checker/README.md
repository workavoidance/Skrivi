# Skrivi contextual spell checker

This folder is the home for research, prototypes, evaluation material and product thinking for Skrivi's proposed local contextual spell checker.

## Product principle

The working concept is an accessibility-first writing sandbox that lets the user **write first and check later**. The spell checker should identify uncertain words in context and offer **single-word suggestions**, while leaving authorship with the writer. It should not silently rewrite sentences or improve style.

The intended long-term characteristics are:

- local/offline processing
- open source
- free to use
- no account required
- no cloud processing of the user's writing
- dyslexia-first interaction design
- Norwegian support, including Bokmål and eventually Nynorsk
- word-level contextual checking rather than sentence rewriting

## Research

- [Norwegian dyslexia dataset research](NORWEGIAN_DYSLEXIA_DATASET_RESEARCH.md) — deep research into authentic Norwegian spelling-error data, including public corpora and diagnosis-linked research datasets.
- [Benchmark format](BENCHMARK_FORMAT.md) — provenance rules, prediction format and the local validation/scoring workflow.

## Current status

The benchmark contract and evaluator are now in place. They report detection
quality and whether the intended correction appears in the first, first three or
first five suggestions, both overall and separately for each writer group.

The next data step is to import authentic Norwegian examples whose licences or
specific permissions allow local evaluation. No pupil-derived records should be
committed to the public repository unless redistribution is explicitly allowed.
