# gsi
Git made SImple

Gsi simplifies git usage, being at the same time compatible with it.
Features:
  * direct addition or removal from staging area is replaced with a user file .track (explore [docs](https://github.com/volokh0x/gsi/blob/master/docs/track.txt))
  * branching from non-master branch is forbidden
  * when switching by commit hash, current branch is also redirected, losing some history 

Full list of commands:
``` bash
gsi init <path>

gsi track <path>
gsi untrack <path>
gsi commit -m <msg> -i [path1,...] -e [path2,...]

gsi create-branch <name>
gsi switch <name/hash>
gsi merge <name>

gsi status
gsi ls-files
gsi ls-branches
gsi cat-file <hash>
```