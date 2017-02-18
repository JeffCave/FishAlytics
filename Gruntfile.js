
module.exports = function(grunt) {

  grunt.initConfig({
    checkDependencies: {
        this: {
            options: {
                install: true,
            },
        },
    },
    jshint : {
      files : ['Gruntfile.js', 'couchapp/**/*.js'],
      options : {
        ignores : [
            'couchapp/*/lib/*',
            'couchapp/trips/_attachments/scripts/regression.js',
            'couchapp/trips/_attachments/scripts/leaflet.js',
            'couchapp/triggerjob/**',
          ],
        esversion : 6,
        //strict : 'implied',
        laxcomma : true,
        globals : {
          couch: true
        }
      }
    },
    watch: {
      files: ['<%= jshint.files %>'],
      tasks: ['build']
    },
    exec:{
      config:'echo not implemented',
    },
    'couch-compile': {
      app: {
        files: [
          {dest:'bin/alldata.json',src: 'couchapp/alldata/*'},
          {dest:'bin/licenses.json',src: 'couchapp/licenses/*'},
          //{dest:'bin/triggerjob.json',src: 'couchapp/triggerjob/*'},
          {dest:'bin/trips.json',src: 'couchapp/trips/*'},
        ]
      }
    },
    'couch-push': {
        //options: {user: 'karin',pass: 'secure'},
        c9: {
          files: [
//            {dest:'http://localhost:8080/trips', src:'bin/alldata.json'},
//            {dest:'http://localhost:8080/trips', src:'bin/licenses.json'},
//            {dest:'http://localhost:8080/trips', src:'bin/triggerjob.json'},
//            {dest:'http://localhost:8080/trips', src:'bin/trips.json'}
          ]
        }
    },
    shell: {
      "couch-start": {
        /*
        sudo chmod +w /var/lib/couchdb/*
        sudo chmod +x /var/lib/couchdb
        */
        command: 'couchdb -a ./couch.ini -p ./bin/couchdb/couch.pid -b',
        options: {
          async: true
        }
      },
      "couch-stop": {
        command: 'couchdb -a ./couch.ini -p ./bin/couchdb/couch.pid -d',
        options: {
          async: false
        }
      }
    },
    mochaTest: {
        test: {
          options: {
            reporter: 'spec',
            quiet: false, // Optionally suppress output to standard out (defaults to false) 
            clearRequireCache: false // Optionally clear the require cache before running tests (defaults to false) 
          },
          src: ['test/**/*.js']
        }
    }
  });
  
  
  grunt.loadNpmTasks('grunt-check-dependencies');
  grunt.loadNpmTasks('grunt-contrib-jshint');
  grunt.loadNpmTasks('grunt-contrib-watch');
  grunt.loadNpmTasks('grunt-exec');
  grunt.loadNpmTasks('grunt-couch');
  grunt.loadNpmTasks('grunt-shell-spawn');
  grunt.loadNpmTasks('grunt-mocha-test');
  
  grunt.registerTask('default', ['build']);
  grunt.registerTask('test', ['mochaTest']);
  grunt.registerTask('start',['shell:couch-start']);
  grunt.registerTask('stop',['shell:couch-stop']);
  grunt.registerTask('config',[
    'checkDependencies',
    'couch-configure',
    ]);
  grunt.registerTask('build',[
    'checkDependencies',
    'jshint',
    'couch',
    ]);
  
  /*
  grunt.registerTask('setcouchport',function(){
    const request = require('sync-request');
    const done = this.async();
    var port = 0;
    request('http://localhost:8080', function (error, response, body) {
      grunt.log.writeln('aklsdjf;lksda');
      if (!error && response.statusCode == 200) { 
        port = 8080;
        done();
      }
      else request('http://localhost:5986', function (error, response, body) {
      grunt.log.writeln('aklsdjf;lksda');
        if (!error && response.statusCode == 200) { 
          port = 5984;
          done();
        }
      });
    });
      grunt.log.writeln('Port:'+port);
    return port;
  });
  */
  
};


