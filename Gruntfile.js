module.exports = function(grunt) {
	const fs = require('fs');

	grunt.initConfig({});

	grunt.config.merge({
		target: (function(){
			var targ = '';
			try {
				targ = JSON.parse(process.env.deployment);
			}
			catch (e) {
				targ = {
					port: process.env.PORT || 5964,
					hostname: 'localhost',
					db:'fish',
					protocol:'http',
				};
			}
			targ.url = targ.protocol + '://' + targ.hostname + ':' + targ.port + '/';
			return targ;
		})(),
		isProd : (process.env.isProd == 'true')
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
					'couchapp/**/lib/**/*.js',
					'couchapp/_users/_auth/*',
					'couchapp/fish/trips/_attachments/intro/**',
					'couchapp/fish/trips/_attachments/scripts/regression.js',
					'couchapp/fish/trips/_attachments/scripts/leaflet.js',
					'couchapp/**/trigger/**',
				],
				esversion: 6,
				evil:true,
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
					'bin/fish/alldata.json': 'couchapp/fish/alldata',
					'bin/fish/licenses.json': 'couchapp/fish/licenses',
					'bin/fish/trips.json': 'couchapp/fish/trips',
					'bin/_users/triggerjob.json' : 'couchapp/fish/triggerjob',
					'bin/_users/oauth.json': 'couchapp/_users/oauth',
					'bin/_users/_auth.json': 'couchapp/_users/_auth',
				}
			}
		},
		'couch-push': {
			app: {
				options: grunt.config.process('<%= target %>'),
				files: (()=>{
					var f = {};
					f[grunt.config.get('target').url + 'fish'] = [
							'bin/fish/alldata.json',
							'bin/fish/licenses.json',
							'bin/fish/trips.json',
						];
					
					f[grunt.config.get('target').url + '_users'] = [
							'bin/_users/_auth.json',
							'bin/_users/oauth.json',
							'bin/_users/triggerjob.json',
						];
					return f;
				})()
			}
		},
		mochaTest: {
			test: {
				options: {
					//reporter: 'spec',
					reporter: 'landing',
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
	grunt.registerTask('build', ['deploy']);
	grunt.registerTask('deploy', [
		'checkDependencies',
		'jshint',
		'couch-compile',
		'secretkeys',
		'couch-push'
	]);
	
	grunt.task.registerTask('secretkeys', 'Replace various keys', function() {
		var oauth;
		//grunt.log.write('HERE:'+ JSON.stringify(JSON.parse(process.env.oauthKeys),null,4) + '\n');
		try{
			oauth = JSON.parse(process.env.oauthKeys).oauth;
		}
		catch(e){
			oauth = {google:{}};
		}
		var replaces = {
			'239959269801-rc9sbujsr5gv4gm43ecsavjk6s149ug7.apps.googleusercontent.com':oauth.google.client_id || '{**GOOGLECLIENTID**}',
			'QyYKQRBx7HuKI-q11oJnkK-d':oauth.google.client_secret || '{**GOOGLESECRETKEY**}',
			//'../../_session': (grunt.config.get('isProd') ? '../' : '') + '../../_session',
			//'../../_users': (grunt.config.get('isProd') ? '../' : '') + '../../_users',
		};
		const child = require('child_process');
		grunt.file.expand('bin/**/*.json').forEach(function(file) {
			grunt.log.write(`${file} \n`);
			for(var key in replaces){
				var cmd = 'sed -i s~{{orig}}~{{new}}~g {{file}}'
					.replace(/{{file}}/g,file)
					.replace(/{{orig}}/g,key.replace(/~/g,'\\~'))
					.replace(/{{new}}/g,replaces[key].replace(/~/g,'\\~'))
					;
				grunt.log.write(` - ${key} \n`);
				//grunt.log.write(` ${cmd} \n`);
				child.execSync(cmd);
			}
		});
	});

};
