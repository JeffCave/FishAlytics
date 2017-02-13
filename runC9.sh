workspace=$(find ~ -maxdepth 1 -name 'workspace' -type d)

# setup couchdb
mkdir -p /var/run/couchdb
chown couchdb:couchdb /var/run/couchdb 

# setup couchapp
couchapp --version 1>&2 2>/dev/null || {
    echo "No CouchApp detected. Installing dependencies"
    {
        npm install grunt
        apt-get -qq update
        #apt-get install couchapp -y
        #apt-get -qq -y install python-dev iptables
        #pip -qqq install couchapp
        #git clone https://github.com/JeffCave/couch-daemon-triggerjob.git
        
    } 1>&2 2> /dev/null
    echo "Dependencies installed"
} 

# execute couchdb
su couchdb -c /usr/bin/couchdb &
couchlocal=$(head -n 3 couch.log | tail -n 2 | cut -d ' ' -f 8)
curl -X PUT $couchlocal/_config/httpd/bind_address -d '"0.0.0.0"'
curl -X PUT $couchlocal/_config/httpd/port -d '"8080"'
curl -X PUT $couchlocal/_config/os_daemons/triggerjob -d '"node $workspace/triggerjob-daemon/triggerjob.js"'
#monitor for couch app changes
# ???