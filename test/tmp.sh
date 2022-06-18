
#!/bin/bash

cd ~/D/Dev/Uni
rm -rf dir
gsi init dir
cd dir

echo OK > f1
echo "The cake is a lie" > t1
mkdir dir1
echo "Yeah, the cake is a lie" > dir1/t2
gsi tmp t1 dir1/t2
gsi commit -m v01
gsi branch -c b2

echo "When life gives you lemons, make lemonade" >> t1
gsi commit -m v02

gsi switch b2
echo "When life gives you lemons, don't make lemonade" >> t1
gsi commit -m v03

gsi switch master
gsi merge b2

# fix merg confl
# gsi commit
# gsi clean-history --all
# gsi cat-file <hash>

