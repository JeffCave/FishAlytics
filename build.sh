#! /bin/sh

$apps[0] = "alldata"
$apps[1] = "liceses"
$apps[2] = "trips"

cd alldata
couchapp push alldata
cd ..

cd trips
couchapp push trips
cd ..

