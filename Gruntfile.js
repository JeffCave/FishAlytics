module.exports = function(grunt) {

	grunt.initConfig({});
	grunt.config.merge({
		localTarget: {
			url: 'http://localhost:8080/fish'
		}
	});
	
	grunt.config.merge({
		prodTarget: (function(){
			try {
				return JSON.parse(process.env.deployProd);
			}
			catch (e) {
				return grunt.config.get('localTarget');
			}
		})(),
	});
	grunt.config.merge({
		checkDependencies: {
			this: {
				options: {
					install: true,
				},
			},
		},
		jshint: {
			files: ['Gruntfile.js', 'couchapp/**/*.js'],
			options: {
				ignores: [
					'couchapp/*/lib/*',
					'couchapp/trips/_attachments/scripts/regression.js',
					'couchapp/trips/_attachments/scripts/leaflet.js',
					'couchapp/triggerjob/**',
				],
				esversion: 6,
				//strict : 'implied',
				laxcomma: true,
				globals: {
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
					'bin/trips.json': 'couchapp/trips',
				}
			}
		},
		'couch-push': {
			dev: {
				files: [
						'alldata.json',
						'licenses.json',
						//'triggerjob.json',
						'trips.json'
					].map(function(d){
						return {dest:'http://localhost:8080/fish',src:'bin/'+d};
					})
			},
			git: {
				files: [
						'alldata.json',
						'licenses.json',
						//'triggerjob.json',
						'trips.json'
					].map(function(d){
						return {dest:'http://localhost:5984/fish',src:'bin/'+d};
					})
			},
			prod: {
				options: grunt.config.process('<%= prodTarget %>'),
				files: [
						'alldata.json',
						'licenses.json',
						//'triggerjob.json',
						'trips.json'
					].map(function(d){
						return {dest:grunt.config.get('prodTarget').url,src:'bin/'+d};
					})
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
	grunt.registerTask('config', [
		'checkDependencies'
	]);
	grunt.registerTask('build', [
		'checkDependencies',
		'jshint',
		'couch',
	]);

};
