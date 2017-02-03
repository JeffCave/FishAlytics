#setup couchdb
mkdir -p /var/run/couchdb
chown couchdb:couchdb /var/run/couchdb 

#setup couchapp
couchapp --version 1>&2 2>/dev/null || {
    echo "No CouchApp detected. Installing dependencies"
    apt-get -qq update
    #apt-get install couchapp -y
    apt-get -qq -y install python-dev
    pip -qqq install couchapp
    
    apt-get -y install iptables 
    echo "Dependencies installed"
} 

#execute couchdb
iptables -t nat -A PREROUTING -p tcp --dport 8080 -j REDIRECT --to-ports 5984
su couchdb -c /usr/bin/couchdb &
#monitor for couch app changes
# ???