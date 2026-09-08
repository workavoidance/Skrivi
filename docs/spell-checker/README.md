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

## Current status

The first engineering step is a small proof of concept using a tiny local language model. Before treating model performance as meaningful, we need a benchmark containing authentic Norwegian spelling errors with clear provenance. The research document above records the best sources found so far and a practical route toward a 1,000+ pair dyslexia-focused evaluation set.
