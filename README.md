# gsi
Git made SImple

Gsi simplifies git usage, being at the same time compatible with it.
Features:
  * direct addition or removal from staging area is replaced with a user file .track
  * branching from non-master branch is forbidden
  * when switching by a commit hash, the current branch is also redirected, losing some history
  * efficient and convenient deletion of temporary files saved in a user file .tmp
  * additional structural diff for JSON files

Full list of commands:
``` bash
gsi init <path>

gsi track <file|dir>
gsi untrack <file|dir>
gsi tmp <file|dir>
gsi commit -m <msg> -i [file1|dir1,...] -e [file2|dir2,...]
gsi clean-history <file|dir> | --all

gsi branch -c [name1,...] -d [name2,...]
gsi switch <name|hash>
gsi merge <name>

gsi status
gsi ls-files
gsi cat-file <hash>
gsi restore <file> --hash <hash>
gsi json-diff <file1|hash1> <file2|hash2> [-Hiea]
```
