### 🧑‍🏫 Spellcheck

A spell check runs on every push to the repository. The spellcheck workflow settings can be configured in [`.github/workflows/spellcheck.yaml`](.github/workflows/spellcheck.yaml).

The [Code Spell Checker][cspell] plugin for VSCode is recommended to help you catch typos before you commit them. If you need add a word to the dictionary or ignore a certain path, you can edit the project's `cspell.json` file.

You can also words to the local `cspell.json` file from VSCode by hovering over a misspelled word and selecting `Quick Fix...` and then `Add "{word}" to config: cspell.json`.

![Fix Spelling](spelling_fix.png)
