# Merging & Branching Standards

For a typical project, the branch structure looks like this:  

* **Protected branches:** `main`, `dev`  
* **Feature branches:** created from `dev` and tied to a ticket in *YouTrack*  
* Changes flow through: `feature → dev → main`  

:::info
All protected branches are **protected**. Commits to these branches should only happen via merge requests.
:::

Since feature branches can diverge from `dev` over time (e.g., someone renames a file you’re editing), long-lived branches can make final merges more complex. It’s recommended to **rebase your feature branch regularly onto `dev`** if it has been open for more than one (minor) version. 

---

## Merge Strategy

There are three main ways to integrate changes:  

1. **Merge** – preserves all commits in the branch. Use this when moving changes **upwards** in the workflow (feature → dev → main).  
2. **Rebase** – re-applies your commits on top of another branch. Use this when moving changes **downwards** (e.g., updating your feature branch with the latest `dev`).  
3. **Squash** – combines all commits into a single commit. Use **only when merging into `dev`** to keep history clean.

---

## Workflow Guidelines

* **Feature → Dev:**  
  * Always merge.  
  * All changes going into `dev` must be **reviewed**, focusing on code quality and implementation correctness.  
  * Run testing pipeline containing unit and integration tests.
  * Squash commits when merging to `dev` to maintain a clean history.  

* **Dev → Main:**  
  * Merge as the final step once all features are stable.  

* **Keeping branches up to date:**  
  * Regularly **rebase feature branches onto `dev`** to avoid large conflicts.  
  * Avoid mixing merge and rebase on the same feature branch unnecessarily.  

---

This workflow follows **GitFlow principles**, adapted to ensure code is reviewed, history is clean, and merges are predictable.