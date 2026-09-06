# Create local repo

```bash

git init

git add .gitignore

git commit -m "Add repository gitignore"

git add .

git commit -m "init scaffold"
```

# Assume local repo exists, wire it to GitHub

```bash
git remote add origin https://github.com/joe-oli/jolisoft-prototypes.git

git push -u origin main
```

The `-u` flag in step 3 sets the upstream tracking, so git knows where `main` lives from that point on.


That's it. After this, in future just push or pull:

```bash
git push

git pull
```


# Quick sanity check - to verify the remote is wired

```bash
git remote -v
```

Should show `origin` pointing at your GitHub URL for both fetch and push.

---

Taking snapshots
================
git branch snapshot/01_baseline

git push origin snapshot/01_baseline
