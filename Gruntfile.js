
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
    'couch-compile': {
      app: {
        files: {
          'bin/alldata.json': 'couchapp/alldata',
          'bin/licenses.json': 'couchapp/licenses',
          //'bin/triggerjob.json' : 'couchapp/triggerjob/*',
          'bin/trips.json' : 'couchapp/trips',
        }
      }
    },
    'couch-push': {
        //options: {user: 'karin',pass: 'secure'},
        dev: {
          files: [
            {dest:'http://localhost:8080/fish', src:'bin/alldata.json'},
            {dest:'http://localhost:8080/fish', src:'bin/licenses.json'},
            //{dest:'http://localhost:8080/fish', src:'bin/triggerjob.json'},
            {dest:'http://localhost:8080/fish', src:'bin/trips.json'}
          ]
        },
        prod:{
          files: [
            {dest:'http://localhost:8080/fish', src:'bin/alldata.json'},
            {dest:'http://localhost:8080/fish', src:'bin/licenses.json'},
            //{dest:'http://localhost:8080/fish', src:'bin/triggerjob.json'},
            {dest:'http://localhost:8080/fish', src:'bin/trips.json'}
          ]
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
  grunt.loadNpmTasks('grunt-shell');
  grunt.loadNpmTasks('grunt-mocha-test');
  
  grunt.registerTask('default', ['build']);
  grunt.registerTask('test', ['mochaTest']);
  grunt.registerTask('config',[
    'checkDependencies',
    'couch-configure',
    ]);
  grunt.registerTask('build',[
    'checkDependencies',
    'jshint',
    'couch',
    ]);
  
};


