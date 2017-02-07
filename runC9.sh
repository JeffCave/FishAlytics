# setup couchdb
mkdir -p /var/run/couchdb
chown couchdb:couchdb /var/run/couchdb 

# setup couchapp
couchapp --version 1>&2 2>/dev/null || {
    echo "No CouchApp detected. Installing dependencies"
    {
        apt-get -qq update
        #apt-get install couchapp -y
        apt-get -qq -y install python-dev iptables
        pip -qqq install couchapp
    } 1>&2 2> /dev/null
    echo "Dependencies installed"
} 

# execute couchdb
#iptables -t nat -A PREROUTING -p tcp --dport 8080 -j REDIRECT --to-ports 5984
su couchdb -c /usr/bin/couchdb &
couchlocal=$(head -n 3 couch.log | tail -n 2 | cut -d ' ' -f 8)
echo $curport
curl -X PUT $couchlocal/_config/httpd/bind_address -d '"0.0.0.0"'
curl -X PUT $couchlocal/_config/httpd/port -d '"8080"'
#monitor for couch app changes
# ???