# Skrivi website

This folder contains the first public-facing Skrivi website.

It is intentionally a simple static site: plain HTML, CSS and a small language-switching script. There is no framework, build step, database, analytics package, account system or external font dependency.

Download links remain measurable without adding browser tracking. Microsoft
Store links use separate `cid` campaign values for the homepage and testing
guide; their results appear in Partner Center's Acquisitions report. GitHub's
release asset API supplies the cumulative `download_count` for each conventional
installer file.

## Design direction

The site follows `docs/BRAND_GUIDELINES.md`:

- Norwegian first, English second
- warm paper-white, charcoal and strong orange
- clean, generous spacing
- product-first rather than documentation-first
- student value and school reassurance given equal weight
- local/private/free/non-generative positioning

## Preview locally

Open `index.html` directly in a browser, or serve this folder with any basic static web server.

## Publishing

`.github/workflows/pages.yml` publishes this folder with GitHub Pages when changes under `website/` are pushed to `main`.

GitHub Pages must be enabled for the repository with **GitHub Actions** selected as the Pages source. Once enabled, the site should be available at the repository's GitHub Pages URL.

A custom domain can be added later without changing the site structure.
