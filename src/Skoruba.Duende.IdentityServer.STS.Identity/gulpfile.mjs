import gulp from 'gulp';
import debug from 'gulp-debug';
import concat from 'gulp-concat';
import uglify from 'gulp-uglify';
import dartSass from 'sass';
import gulpSass from 'gulp-sass';
import minifyCSS from 'gulp-clean-css';
import {deleteSync} from 'del';
import npmdist from 'gulp-npm-dist';
import rename from 'gulp-rename';
import fs from 'fs';
import path from 'path';

const sass = gulpSass(dartSass);

const distFolder = './wwwroot/dist/';
const jsFolder = `${distFolder}js/`;
const cssFolder = `${distFolder}css/`;
const fontsFolder = `${distFolder}fonts/`;
const webfontsFolder = `${distFolder}webfonts/`;
const cssThemeFolder = `${distFolder}css/themes/`;

const paths = {
	base: {
		base: {
			dir: './'
		},
		node: {
			dir: './node_modules'
		}
	},
	src: {
		base: {
			dir: './wwwroot/',
			files: './wwwroot/**/*'
		},
		libs: {
			dir: './wwwroot/libs'
		}
	}
}

async function processClean() {
	return deleteSync(`${distFolder}**`, { force: true });
}

function copyNodeLibs() {
	return gulp
		.src(npmdist({ copyUnminified: true, buffer: false, excludes: ['**/*.{eot,otf,ttf,woff,woff2}'] }), { base: paths.base.node.dir })
		.pipe(rename(function (path) {
			path.dirname = path.dirname.replace(/\/dist/, '').replace(/\\dist/, '');
		}))
		.pipe(gulp.dest(paths.src.libs.dir))
		.pipe(debug({title: 'Copy file:'}));
}

function copyNodeOpenIconicLibFonts(done) {
	const src = paths.base.node.dir + '/open-iconic/font/fonts/';
	const dest = paths.src.libs.dir + '/open-iconic/font/fonts';

	// Verificar si la carpeta de destino existe, si no, crearla
	if (!fs.existsSync(dest)) {
		fs.mkdirSync(dest, { recursive: true });
	}

	// Leer los archivos de la carpeta de origen
	fs.readdirSync(src).forEach(file => {
		const ext = path.extname(file);
		if (['.eot', '.otf', '.ttf', '.woff', '.woff2'].includes(ext)) {
			// Copiar archivo al destino
			fs.copyFileSync(path.join(src, file), path.join(dest, file));
			console.log(`Copied font: ${file}`);
		}
	});

	// Llamar a `done` para indicar que la tarea ha terminado
	done();
}

function copyNodeAwesomeLibFonts(done) {
	const src = paths.base.node.dir + '/@fortawesome/fontawesome-free/webfonts';
	const dest = paths.src.libs.dir + '/@fortawesome/fontawesome-free/webfonts';

	// Verificar si la carpeta de destino existe, si no, crearla
	if (!fs.existsSync(dest)) {
		fs.mkdirSync(dest, { recursive: true });
	}

	// Leer los archivos de la carpeta de origen
	fs.readdirSync(src).forEach(file => {
		const ext = path.extname(file);
		if (['.eot', '.otf', '.ttf', '.woff', '.woff2'].includes(ext)) {
			// Copiar archivo al destino
			fs.copyFileSync(path.join(src, file), path.join(dest, file));
			console.log(`Copied font: ${file}`);
		}
	});

	// Llamar a `done` para indicar que la tarea ha terminado
	done();
}

function copyLibScriptsToDist() {
	return gulp
		.src([
			'./wwwroot/libs/jquery/jquery.js',
			'./wwwroot/libs/jquery/jquery.min.js',

			'./wwwroot/libs/jquery-validation/jquery.validate.js',
			'./wwwroot/libs/jquery-validation/jquery.validate.min.js',

			'./wwwroot/libs/jquery-validation-unobtrusive/jquery.validate.unobtrusive.js',
			'./wwwroot/libs/jquery-validation-unobtrusive/jquery.validate.unobtrusive.min.js',

			'./wwwroot/libs/@popperjs/core/umd/popper.js',
			'./wwwroot/libs/@popperjs/core/umd/popper.min.js',

            './wwwroot/libs/bootstrap/js/bootstrap.js',
			'./wwwroot/libs/bootstrap/js/bootstrap.min.js',

            './wwwroot/libs/cookieconsent/build/cookieconsent.min.js',

			'./wwwroot/libs/holderjs/holder.js',
			'./wwwroot/libs/holderjs/holder.min.js'
		])
		.pipe(gulp.dest(jsFolder))
		.pipe(debug({title: 'Copied file:'}));
}

function copyAppScriptsToDist() {
	return gulp
		.src([
			'./Scripts/App/components/Menu.js',
			'./Scripts/App/components/Language.js',
			'./Scripts/App/components/Theme.js',
			'./Scripts/App/components/CookieConsentHandler.js',

			'./Scripts/App/pages/account_login.js',
			'./Scripts/App/pages/account_register.js',
			'./Scripts/App/pages/account_forgotpassword.js'
		])
		.pipe(gulp.dest(jsFolder))
		.pipe(debug({ title: 'Copied file:' }));
}

function copyLibFontsToDist(done) {
	const src = paths.src.libs.dir + '/open-iconic/font/fonts';
	const dest = fontsFolder;

	// Verificar si la carpeta de destino existe, si no, crearla
	if (!fs.existsSync(dest)) {
		fs.mkdirSync(dest, { recursive: true });
	}

	// Leer los archivos de la carpeta de origen
	fs.readdirSync(src).forEach(file => {
		//// Copiar archivo al destino
		fs.copyFileSync(path.join(src, file), path.join(dest, file));
		console.log(`Copied font: ${file}`);
		//}
	});

	// Llamar a `done` para indicar que la tarea ha terminado
	done();
}

function copyLibWebfontsToDist(done) {
	const src = paths.src.libs.dir + '/@fortawesome/fontawesome-free/webfonts';
	const dest = webfontsFolder;

	// Verificar si la carpeta de destino existe, si no, crearla
	if (!fs.existsSync(dest)) {
		fs.mkdirSync(dest, { recursive: true });
	}

	// Leer los archivos de la carpeta de origen
	fs.readdirSync(src).forEach(file => {
		//// Copiar archivo al destino
		fs.copyFileSync(path.join(src, file), path.join(dest, file));
		console.log(`Copied font: ${file}`);
		//}
	});

	// Llamar a `done` para indicar que la tarea ha terminado
	done();
}

function copyLibCssToDist() {
	return gulp
		.src([
			'./wwwroot/libs/bootstrap/css/bootstrap.css',
			'./wwwroot/libs/bootstrap/css/bootstrap.min.css',

			'./wwwroot/libs/open-iconic/font/css/open-iconic-bootstrap.css',
			'./wwwroot/libs/open-iconic/font/css/open-iconic-bootstrap.min.css',

			'./wwwroot/libs/@fortawesome/fontawesome-free/css/all.css',
			'./wwwroot/libs/@fortawesome/fontawesome-free/css/all.min.css',

			'./wwwroot/libs/cookieconsent/build/cookieconsent.min.css'
		])
		.pipe(rename(function (path) {
			// Solo renombramos los archivos all.css y all.min.css
			if (path.basename === 'all' || path.basename === 'all.min') {
				path.basename = 'fontawesome-' + path.basename;
			}
			return path;
		}))
		.pipe(gulp.dest(cssFolder))
		.pipe(debug({ title: 'Copied file:' }));
}

function copyLibThemesToDist() {
	return gulp
		.src('wwwroot/libs/bootswatch/**/bootstrap.min.css')
		.pipe(gulp.dest(cssThemeFolder))
		.pipe(debug({ title: 'Copied file:' }));
}

function minifyAppScripts() {
    return gulp
        .src([
            './wwwroot/dist/js/Menu.js',
            './wwwroot/dist/js/Language.js',
			'./wwwroot/dist/js/Theme.js',
            './wwwroot/dist/js/CookieConsentHandler.js',

			'./wwwroot/dist/js/account_login.js',
			'./wwwroot/dist/js/account_register.js',
			'./wwwroot/dist/js/account_forgotpassword.js'
        ])
		.pipe(uglify())
		.pipe(rename({ suffix: '.min' })) 
		.pipe(gulp.dest(jsFolder))
		.pipe(debug({ title: 'Minified file:' }));
}

function bundleScripts() {
	return gulp
		.src([
			'./wwwroot/dist/js/jquery.js',
			'./wwwroot/dist/js/jquery.validate.js',
			'./wwwroot/dist/js/jquery.validate.unobtrusive.js',
			'./wwwroot/dist/js/popper.js',
			'./wwwroot/dist/js/bootstrap.js',
			'./wwwroot/dist/js/cookieconsent.min.js',
			'./wwwroot/dist/js/holder.js',
			'./wwwroot/dist/js/Menu.js',
			'./wwwroot/dist/js/Language.js',
			'./wwwroot/dist/js/Theme.js',
			'./wwwroot/dist/js/CookieConsentHandler.js'
		])
		.pipe(concat('bundle.min.js'))
		.pipe(uglify())
		.pipe(gulp.dest(jsFolder))
		.pipe(debug({ title: 'Bundled scripts:' }));
}

function bundleLibStyles() {
	return gulp
		.src([
			'./wwwroot/dist/css/bootstrap.css',
			'./wwwroot/dist/css/open-iconic-bootstrap.css',
			'./wwwroot/dist/css/fontawesome-all.css',
			'./wwwroot/dist/css/cookieconsent.min.css'
		])
		.pipe(minifyCSS())
		.pipe(concat('bundle.min.css'))
		.pipe(gulp.dest(cssFolder))
		.pipe(debug({ title: 'Bundled styles:' }));
}

function bundleAppSass() {
	return gulp
		.src('Styles/web.scss')
		.pipe(sass())
		.on('error', sass.logError)
		.pipe(gulp.dest(cssFolder))
		.pipe(debug({ title: 'Bundled app sass:' }));
}

function bundelAppSassMin() {
	return gulp
		.src('Styles/web.scss')
		.pipe(sass())
		.on('error', sass.logError)
		.pipe(minifyCSS())
		.pipe(concat('web.min.css'))
		.pipe(gulp.dest(cssFolder))
		.pipe(debug({ title: 'Bundled app sass minified:' }));
}

var initDist = gulp.series(copyAppScriptsToDist, copyLibScriptsToDist, copyLibCssToDist, copyLibFontsToDist, copyLibWebfontsToDist, copyLibThemesToDist);
var bundleStyles = gulp.series(bundleLibStyles, bundleAppSass, bundelAppSassMin);
var build = gulp.series(minifyAppScripts, bundleStyles, bundleScripts);

gulp.task('clean', processClean);
gulp.task('copy-node-dist', gulp.series(copyNodeLibs, copyNodeAwesomeLibFonts, copyNodeOpenIconicLibFonts));
gulp.task('init-dist', initDist);
gulp.task('build', build);
gulp.task('default', build);

gulp.task('styles', bundleStyles);
gulp.task('sass', bundleAppSass);
gulp.task('sass:min', bundelAppSassMin);
gulp.task('fonts', gulp.series(copyLibFontsToDist, copyLibWebfontsToDist));
gulp.task('scripts', bundleScripts);

// watch
function processWatch() {
	gulp.watch([
		'Styles/**/*.scss',
		'Scripts/**/*.js'
	], gulp.series(copyAppScriptsToDist, minifyAppScripts, bundleScripts, bundleStyles));
}
gulp.task('watch', processWatch);

export default copyNodeLibs;