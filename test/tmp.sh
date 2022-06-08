
#!/bin/bash

cd ~/D/Dev/Uni
rm -rf dir
gsi init dir
cd dir

echo a > a
mkdir dir1
echo c > dir1/c
echo t > dir1/t
gsi tmp dir1/t
gsi commit -m v01
gsi branch -c b2

echo cmaster > dir1/c
gsi commit -m v02

gsi switch b2
echo tb2 > dir1/t
gsi commit -m v03

gsi switch master
gsi merge b2

gsi clean-history dir1/t


