/**
 * @module FlexClient/core
 * @author Bogdan Nazar [me@bogdan-nazar.ru]
 * @version 3.2.0 (15.12.2016 16:53 +0400)
 * @description Ядро клиента Core
 * @copyright 2003-2016 Bogdan Nazar
 * @license MIT & FlexEngine License
 * 		http://www.opensource.org/licenses/mit-license.php
 * 		http://www.flexengin.ru/docs/license
 */

/**
 * Примеры и документация: http://www.flexengin.ru/docs/client/core/core-js
 *
 * Терминология:
 *
 * 	Приложение:
 *		клиентская система, состоящая из клиентского ядра, расширений и модулей
 *
 *	Ядро:
 * 		клиентское дополнение основного серверного системного модуля "ядро" (core)
 *
 * 	Расширение:
 * 		клиентское дополнение вспомогательного серверного системного модуля (lib, msgr, render и др.)
 *
 * 	Модуль:
 * 		клиентское дополнение пользовательского серверного модуля
 *
 *
 * Стиль записи кода:
 * 	названия публичных свойств начинаются на $
 * 	названия приватных свойств начинаются на $_
 * 	названия приватных методов начинаются на _
 *
 * Старт приложения происходит в два этапа в inline-скрипте на странице:
 * 	1) Создание экземпляра
 *
 * Схема выполнения расширения:
 *	1) Загрузка:
 *		а) удаление глобального указателя на расширение
 * 		б) регистрация расширения
 * 		в) получение набора публичных методов .extend_()
 *	2) Получение зависимостей: вызов метода .deps()
 * 	3) Инициализация, ядро передает в функцию инициализации:
 * 		а) конфиг расширения
 * 		б) клон объекта $ (различные нужные указатели и сокращения)
 * 		в) список расширений (зависимости)
 *
 * Схема выполнения модуля:
 *	1) Регистрация в ядре.
 * 		Третьим аргументом при регистрации (выполнении функции .module(name, constructor, sections))
 * 		модуль должен передать в ядро список всех режимов выполнения (секций),
 * 		являющихся по сути HTML id элементов. Появление этих элементов на странице
 * 		будет инициировать создание экземпляра модуля в привязке к конкретной
 * 		секции (режим выполнения).
 *	2) Создание экземпляра.
 * 		На данном этапе, в результате выполнения конcтруктора, модуль должен
 * 		создать публичный метод .deps() для получения ядром списка зависимостей
 * 		(требуемых расширения ядра) и метод .start() для его последующего запуска
 * 	3) Выполнение.
 * 		Запуская выполнение модуля ядро передает в функцию выполнения
 * 		список найденных зависимостей (требуемых расширений)
 */

(function(){

var $ = Object.create(null, {
	ce: {
		enumerable: true,
		value: function(tag, attrs) {
			var el = $.d.createElement(tag);
			if (typeof attrs == "object" && attrs) {
				for (var c in attrs) {
					if (typeof attrs.hasOwnProperty == "function") {
						if (!attrs.hasOwnProperty(c)) continue;
					}
					if (attrs[c] && (typeof attrs[c] == "object")) {
						//используется для задания, например, стиля style
						if (el[c] && (typeof el[c] == "object"))
						for (var c1 in attrs[c]) {
							if (typeof attrs[c].hasOwnProperty == "function") {
								if (!attrs[c].hasOwnProperty(c1)) continue;
							}
							el[c][c1] = attrs[c][c1];
						}
					} else {
						el[c] = attrs[c];
					}
				}
			}
			return el;
		}
	},
	d: {
		enumerable: true,
		value: document
	},
	ge: {
		enumerable: true,
		value: function(id) {
			return $.d.getElementById(id);
		}
	},
	w: {
		enumerable: true,
		value: window
	}
});

var __name_this = "core",
	__name_script = __name_this + ".js";

(function(){
var core = function(client) {
	/* ------- приватные свойства ------ */
	var $_appName				=	client.cfg.appName || "FlexEngine",
		$_bind					=	(typeof Function.prototype.bind != "function"),
		$_classes				=	[],
		$_classesMap			=	{},
		$_client				=	client,
		$_config				=	client.cfg,
		$_console				=	!!window.console,
		$_debug					=	true,
		$_deps					=	[
			"Auth",
			"Lib",
			"Lightbox",
			"Msgr",
			"Render"
		],
		$_dirs					=	{
			admin:					$_config.dirs.admin || "admin",
			core:					$_config.dirs.core || "core",
			modules:				$_config.dirs.classes || "classes",
			require:				$_config.dirs.require || "require",
			root:					$_config.dirs.root || "/",
			source:					"",
			templates:				$_config.dirs.templates || "templates",
			userdata:				$_config.dirs.data || "data"
		},
		$_exts					=	[],//расширения
		$_extsMap				=	{},//карта быстрого доступа к расширениям
		$_export				=	[
			"action",
			"console",
			"dir",
			"lang",
			"silentReqBuild"
		],
		$_form					=	{
			action:					"",
			target:					""
		},
		$_historyCbs			=	{},
		$_html5					=	((!!(history.pushState && history.state !== "undefined")) && ("classList" in document.createElement("i"))),
		$_init					=	{
			data:					{},
			err:					false,
			ival:					300,
			ivalObj:				null,
			readyId:				$_config.initPoint || "core-client-init",
			tm:						300,
			tmObj:					null,
			tryMax:					200
		},
		$_lang					=	$_config.lang || "ru-Ru",
		$_mods					=	{},//зарегистрированные модули
		$_modsStack				=	[],//зарегистрированные модули в виде стека (оптимизация для быстрого перебора)
		$_modsWorker			=	{
			ival:					false,
			tm:						400,
			wait:					150//60 сек
		},
		$_name					=	__name_this,
		$_page					=	$_config._exts.Render.page,
		$_silentQueue			=	0,//индекс последней очереди
		$_silentReqs			=	[],//стек тихих запросов
		$_silentReqsDone		=	[],//стек отработанных тихих запросов
		$_silentXReqs			=	[],

		//расширения
		$_Auth					=	null,
		$_Lib					=	null,
		$_Lightbox				=	null,
		$_Msgr					=	null,
		$_Render				=	null;
	/* --------------------------------- */


	/* -------- приватные методы ------- */
	var _action = function(action, path, query, target, seed) {
		return $_exts[$_extsMap.Render]._.actionSend(action, path, query, target, seed);
	},

	/**
	* Вывод сообщения в консоль
	*
	* @param msg - текст сообщения
	* @param {boolean} crit - флаг критичости, вывод диалогом
	*/
	_console = function(msg, crit) {
		if ($_console) window.console.log(msg);
		if (crit && $_Render) $_Render.dlgAlert(msg);
	},

	/**
	* Возвращает названия директорий сервера по
	* их строковым идентификаторам
	*/
	_dir = function(name) {
		if (typeof $_dirs[name] != "undefined") return $_dirs[name];
		else return "";
	},

	/**
	* Функция расширяет указанный объект
	* на набор методов ядра/расширения/модуля
	* который задан реестром $_export
	*/
	_export = function(obj) {
		if (!($_export instanceof Array)) return obj || {};
		if ((typeof obj != "object") || !obj) obj = {};
		var func,//название текущего метода
			l = $_export.length;
		for (; l--;) {
			func = $_export[l];
			try {
				obj[func] = eval("[function() {\
					return _" + func + ".apply(null, arguments);\
				}][0]");
			} catch(e) {
				_console(__name_script + " > " + $_name + "#_export(): > Export failed, function: [" + func + "]!");
				_console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
			}
			if ($_bind) obj[func].bind = $_Lib.bind;
		}
		return obj;
	},

	/**
	* Загрузка расширений
	*/
	_extensionsLoad = function() {
		//получаем API расширений
		var ext,	//текущее расширение
			extRec,	//инфо-запись по текущему расширению
			l = $_deps.length,
			name;	//идентификатор текущего расширения
		//сохраняем указатель на себя
		//(для передачи указателя другим расширениям и модулям)
		extRec = {
			_: this,//указатель на экземпляр
			deps: [],//зависимости
			export_: null,
			name: "Core"
		};
		//сохраняем в стек расширений
		$_exts.push(extRec);
		$_extsMap[extRec.name] = $_exts.length - 1;
		//перебираем остальные расширения
		for (; l--;) {
			name = $_deps[l];
			if ($_extsMap[name]) {
				_console(__name_script + " > " + $_name + "#_extensionsLoad(): > Core extension [" + name + "] is already loaded!");
				continue;
			}
			if (!window["FlexClient" + name]) {
				_console(__name_script + " > " + $_name + "#_extensionsLoad(): > Fatal error: core extension [" + name + "] is not loaded!");
				return false;
			}
			try {
				//получаем указатель и импортируем методы
				eval("ext = window.FlexClient" + name);
				eval("delete window.FlexClient" + name);
				extRec = {
					_: ext,//указатель на экземпляр
					deps: [],//зависимости
					export_: null,
					name: name,
				};
				if (typeof ext.export_ == "function") {
					//сохраним для последующего клонирования методов модулям
					extRec.export_ = ext.export_(Object.create(null));
					//сохраняем локально
					eval("$_" + name + " = extRec.export_;");
				}
				//сохраняем в стек расширений
				$_exts.push(extRec);
				$_extsMap[extRec.name] = $_exts.length - 1;
			} catch(e) {
				_console(__name_script + " > " + $_name + "#_extensionsLoad(): > Fatal error: core extension [" + $_deps[l] + "] is not created!");
				_console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
				return false;
			}
		}
		//сформируем список собственных экспортируемых методов (методы ядра)
		$_exts[0].export_ = _export(Object.create(null));
		return true;
	},

	/**
	* Удаляет запись из истории навигации страницы
	*
	* @param {Object} e
	*/
	_historyPop = function(e) {
		if (e && e.state && e.state.flexapp) {
			document.title = e.state.title;
			if (typeof $_historyCbs[e.state.key] != "undefined") {
				var params = $_historyCbs[e.state.key];
				if (typeof params.callback != "undefined") {
					var func = "";
					try {
						if (typeof params.callback == "function") params.callback(params);
						else if (typeof params.callback == "string") {
							func = params.callback + (params.callback.indexOf("()") == -1 ? "()" : "") + ";";
							eval(func);
						}
					} catch(e) {
						_console(__name_script + " > " + $_name + "#_historyPop(): Ошибка выполнения callback-функции" + (func ? (" [" + func + "]") : "") + "!");
						_console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
					}
				}
			}
		} else document.title = $page.title;
	},

	/**
	* Записывает в историю навигации страницы новую запись
	*
	* @param {string} uri
	* @param {string} title
	* @param {params} params
	*/
	_historyWrite = function(uri, title, params) {
		if (!$_html5) return;
		if ((typeof uri != "string") || (!uri)) uri = "";
		if ((typeof title != "string") || (!title)) title = document.title;
		else document.title = title;
		var pars;
		if ((typeof params != "object") || !params) {
			pars = {key: $_Lib.seed(), name: "", replace: false, replaceAny: false, callback: false};
		} else {
			pars = {key: $_Lib.seed()};
			if ((typeof params.name != "string") || (!params.name)) pars.name = "";
			else pars.name = params.name;
			if ((typeof params.data != "object") || (!params.data)) pars.data = false;
			else pars.data = params.data;
			if (typeof params.replace != "boolean") pars.replace = false;
			else pars.replace = params.replace;
			if (typeof params.replaceAny != "boolean") pars.replaceAny = false;
			else pars.replaceAny = params.replaceAny;
			if (typeof params.callback != "function") pars.callback = false;
			else pars.callback = params.callback;
		}
		$_historyCbs[pars.key] = pars;
		var state = {};
		state.flexapp = true;
		var u = $_page.url;
		state.url = "//"+ u.host + (u.port ? (":" + u.port) : "") + (uri.indexOf("/") == 0 ? "" : "/") + (uri ? uri : (u.segments.join("/")));
		state.title = title;
		state.key = pars.key;
		if (pars.replace) {
			if (pars.replaceAny || (!history.state) || (
				history.state &&
				(typeof history.state.key != "undefined") &&
				(typeof $_historyCbs[history.state.key] != "undefined") &&
				(typeof $_historyCbs[history.state.key].name == "string") &&
				($_historyCbs[history.state.key].name === pars.name)
			)) {
				history.replaceState(state, state.title, state.url);
				return;
			}
		}
		history.pushState(state, state.title, state.url);
	},

	/**
	* Инициализация
	*/
	_init = function() {
		$.h = $.d.querySelector("HEAD");
		$.b = $.d.querySelector("BODY");
		//получаем зависимости для расширений
		var deps,
			ext,
			l = $_exts.length;
		for (; l--;) {
			ext = $_exts[l];
			if (typeof ext._.deps == "function")
			try {
				deps = ext._.deps();
				if (!(deps instanceof Array)) {
					_console(__name_script + " > " + $_name + "#_init(): Fatal error: extension's runtime method [" + ext.name + ".deps()] returned unexpected data!");
					return false;
				}
				ext.deps = deps;
			} catch(e) {
				_console(__name_script + " > " + $_name + "#_init(): Fatal error: extension's runtime method [" + ext.name + ".deps()] returned with the error!");
				_console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
				return false;
			}
		}
		//инициализируем расширения
		var args,
			c = 0,
			l1,
			res;//вложенный цикл
		l = $_exts.length;
		for (; c < l; c++) {
			ext = $_exts[c];
			if (typeof ext._.init == "function") {
				args = [];
				//первый аргумент - config
				args.push($_config._exts[ext.name] || null);
				//второй аргумент - $
				args.push($_Lib.clone($, [$.h, $.b, $.d, $.w]));
				//третий аргумент - объект со списком расширений,
				//его сначала формируем
				deps = Object.create(null);
				if (ext.deps) {
					l1 = ext.deps.length;
					for (; l1--;) deps[ext.deps[l1]] = $_Lib.clone($_exts[$_extsMap[ext.deps[l1]]].export_);
				}
				args.push(deps);
				//инициализируем расширение...
				try {
					res = ext._.init.apply(ext._, args);
				} catch(e) {
					_console(__name_script + " > " + $_name + "#_init(): Fatal error: extension [" + ext.name + "] inited with the error!");
					_console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
					return false;
				}
				if (res === false) return false;
			}
		}
		//парсим URL
		$_page.url = $_Lib.urlParse();
		return true;
	},

	/**
	* Возвращает текущий язык приложения
	*/
	_lang = function() {
		return $_lang;
	},

	/**
	* Регистрация модуля в ядре
	*
	* @param {string} name - название класса модуля
	* @param {Function|string} constructor - конструктор модуля
	* @param {Array} sections - массив со списком секций (HTML id)
	*/
	_module = function(name, constructor, sections) {
		//проверяем валидность названия класса модуля
		if ((typeof name != "string") || !name) {
			_console(__name_script + " > " + $_name + "#_module(): Can not process module with the not valid classname (not a string or empty), [got type: " + typeof name + "]!");
			return;
		}
		//если второй аргумент не является конструктором,
		//а является строкой, то трактуем constructor как HTML id
		//и пытаемся сразу создать экземпляр
		//(отложенное создание дополнительных экземпляров)
		//
		//проверяем валидность конструктора
		if (typeof constructor != "function") {
			if ((typeof name != "string") || !name) {
				_console(__name_script + " > " + $_name + "#_module(): Can not process module with the not valid classname (not a string or empty), [got type: " + typeof name + "]!");
				return;
			}
			sections = [constructor];
			constructor = null;
		}
		var reg;//регистрационная запись модуля
		if (!$_mods[name]) {
			reg = {
				constructor: constructor,
				id: $_modsStack.length,
				name: name,
				sections: {},
				sectionsStack: []
			};
			$_mods[name] = reg;
			$_modsStack.unshift(reg);
		} else {
			reg = $_mods[name];
		}
		//проверяем секции
		if (!(sections instanceof Array) || !sections.length) sections = [];
		//также ищем список секций в клиентском конфиге модуля
		var cfg = $_config._mods[reg.name];
		if (cfg && (typeof cfg == "object") && (cfg.sectionsOnInit instanceof Array)) {
			sections = sections.concat(cfg.sectionsOnInit);
		}
		if (!sections.length) {
			_console(__name_script + " > " + $_name + "#_module(): Warning: got not valid list of sections for module [" + name + "]!");
		}
		//запускаем таймер ожидания секций модулей
		if (_moduleSections(reg, sections)) _modulesWorker();
	},

	/**
	* Проверка и сохранения секций (режимов выполнения) модуля
	*
	* @param {Object} reg - регистрационная запись модуля
	* @param {Array} sections - секции модуля
	*
	* @returns {Number}
	*/
	_moduleSections = function(reg, sections) {
		//перебираем секции, сохраняем только валидные
		var added = 0,
			l = sections.length;
		for (; l--;) {
			if ((typeof sections[l] != "string") || !sections[l]) {
				_console(__name_script + " > " + $_name + "#_moduleSections(): Invalid sections passed for module [" + reg.name + "], [got type: " + typeof sections[l] + "]!");
			} else {
				//добавляем секцию только в том случае,
				//если она не была уже добавлена ранее
				if (typeof reg.sections[sections[l]] == "undefined") {
					added++;
					reg.sections[sections[l]] = {
						id: sections[l],
						inst: false,
						wait: 0
					};
					reg.sectionsStack.unshift(reg.sections[sections[l]]);
				}
			}
		}
		return added;
	},

	/**
	* Воркер обработки модулей.
	* Ожидает появления на странице задеклалированных
	* модулями элементов (секций) и при их появлении
	* создает экземпляр модуля в привязке к найденной секции
	* (режим выполнения модуля)
	*/
	_modulesWorker = function() {
		var args,
			deps,
			el,
			ext,
			l = $_modsStack.length,
			l1,
			reg,
			s,
			sl,
			wait = 0;
		//перебираем все модули
		for (; l--;) {
			reg = $_modsStack[l];
			s = reg.sectionsStack;
			sl = s.length;
			//перебираем все секции модуля (экзепляры)
			for (; sl--;) {
				//если секция еще не была обработана ранее
				//(не был создан экземпляр модуля),
				//то обрабатываем...
				if (!s[sl].inst && (typeof reg.constructor == "function")) {
					//проверяем, доступен ли требуемый DOM элемент
					el = $.ge(reg.name + "-" + s[sl].id);
					if (el) {
						//создаем экземпляр
						try {
							s[sl].inst = new reg.constructor(el);
						} catch(e) {
							s[sl].inst = 1;//чтоб при следующих обходах модуль пропускался
							_console(__name_script + " > " + $_name + "#_modulesWorker(): Error while instantiating the module [" + reg.name + "]!");
							_console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
						}
						//получаем зависимости
						if (s[sl].inst !== 1) {
							try {
								s[sl].deps = s[sl].inst.deps();
							} catch(e) {
								s[sl].inst = 1;//чтоб при следующих обходах модуль пропускался
								_console(__name_script + " > " + $_name + "#_modulesWorker(): Error while aquiring deps of the module [" + reg.name + "]!");
								_console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
							}
						}
						//стартуем
						if (s[sl].inst !== 1) {
							if (typeof s[sl].inst.start == "function") {
								args = [];
								//первый аргумент - config
								args.push($_config._mods[reg.name] || null);
								//второй аргумент - $
								args.push($_Lib.clone($, [$.h, $.b, $.d, $.w]));
								//третий аргумент - объект со списком расширений,
								//его сначала формируем
								deps = Object.create(null);
								if (s[sl].deps && (s[sl].deps instanceof Array)) {
									l1 = s[sl].deps.length;
									for (; l1--;) {
										ext = s[sl].deps[l1];
										if (typeof $_extsMap[ext] == "number")
											deps[ext] = $_Lib.clone($_exts[$_extsMap[ext]].export_);
									}
								}
								args.push(deps);
								//выполняем .start()...
								try {
									s[sl].inst.start.apply(s[sl].inst, args);
								} catch(e) {
									s[sl].inst = 1;//чтоб при следующих обходах модуль пропускался
									_console(__name_script + " > " + $_name + "#_modulesWorker(): Error while starting the module [" + reg.name + "]!");
									_console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
								}
							}
						}
					} else {
						if (s[sl].wait <= $_modsWorker.wait) {
							s[sl].wait++;
							wait++;
						} else {
							_console(__name_script + " > " + $_name + "#_modulesWorker(): Wait timeout for the section [" + s[sl].id + "] of module [" + reg.name + "].");
						}
					}
				}
			}
		}
		if (wait) {
			$_modsWorker.ival = $.w.setTimeout(_modulesWorker, $_modsWorker.tm);
		} else $_modsWorker.ival = false;
	},

	/**
	* Выполнение фонового ("тихого") запроса
	*
	* @param {Object} req - объект типа "запрос", см. _silentReqBuild()
	*
	* @returns {Boolean}
	*/
	_silent = function(req) {
		req.method = req.method.toUpperCase();
		if ((req.method != "POST") && (req.method == "GET")) {
			_console(__name_script + " > " + $_name + "#_silent(): Неизвестный метод [" + req.method + "], операция прервана.");
			return false;
		}
		//проверка параметров uri
		var url;
		if ((typeof req.url != "string") || !req.url) url = $.d.location.href.replace("http://" + $.d.domain, "");
		else url = ((req.url.indexOf("http") != -1) ? req.url : ($_dirs.root + req.url).replace(/\/\//g, "/"));
		var query = "";
		url = url.split("?");
		if (typeof url[1] != "undefined") query = url[1];
		url = url[0];
		//полный http путь запроса
		req.url = url + (query ? ("?" + query) : "");
		//формируем GET и POST URL-параметры
		var actionPar = $_Render.elemId("Action"),
			merge = {silent: null};
		//формируем GET
		if (req.method == "GET") merge[actionPar] = req.action;
		req.dataGET = _silentDataBuild(req.dataGET, merge);
		//формируем POST
		if (req.method == "POST") {
			delete merge.silent;
			merge[actionPar] = req.action;
			req.dataPOST = _silentDataBuild(req.dataPOST, merge, true);
		}
		req.url = req.url + (req.dataGET ? ((query ? "&" : "?") + req.dataGET) : "");
		//сохраняем в стек
		$_silentReqs.push(req);
		//отправляем
		_silentSend();
		return true;
	},

	/**
	* Функция сливает два объекта в один (если есть второй объект),
	* и при заданном флаге str==true преобразовывает
	* резултатирующий объект в строку URL-параметров
	* вида par1=value1&par2=value2...
	*
	* @param {Object} d - data, первый объект
	* @param {Object|Boolean} merge - второй объект либо false
	* @param {Boolean} str - булевский флаг, указывающий на фрмирование строки URL-парамтеров
	*
	* @returns {Object|string}
	*/
	_silentDataBuild = function(d, merge, str) {
		//проверяем умолчания
		if (typeof merge != "object") merge = false;
		if (typeof str != "boolean") str = true;
		//если d был передан как строка, то преобразуем его в объект
		if (!d)	{
			if (!merge) return "";
			d = {};
		} else {
			if (typeof d == "string") {
				var pars = d.split("&");
				for (var par in pars) {
					if (!pars.hasOwnProperty(par)) continue;
					if (pars[par]) {
						var pair = pars[par].split("=");
						if (pair[0]) d[pair[0]] = ((typeof pair[1] != "undefined" && pair[1]) ? pair[1] : null);
					}
				}
			} else {
				if (typeof d != "object") d = {};
			}
		}
		//если задан второй объект, то сливаем оба объекта
		if ((typeof merge == "object") && merge) {
			for (var id in merge) {
				if (!merge.hasOwnProperty(id)) continue;
				d[id] = merge[id];
			}
		}
		//если установлен флаг преобразования в строку URL-параметров,
		//то преобразуем...
		if (str) {
			var p = [];//временный стек для результатов конкетации
			for (var id in d) {
				if (!d.hasOwnProperty(id)) continue;
				//проверяем некоторые типы значений
				if (typeof d[id] != "string") {
					if (typeof d[id] == "boolean") d[id] = (d[id] ? "1" : "0");
					else {
						if (typeof d[id] == "number") d[id] = ("").concat(d[id]);
						else d[id] = null;
					}
				}
				//пушим во временный стек конкетацию вида "var=value"
				if (d[id] === null) p.push(("").concat(id));
				else {
					if (d[id]) d[id] = encodeURIComponent(d[id]);
					p.push(("").concat(id, "=", d[id]));
				}
			}
			//возвращаем строку вида var1=value1&var2=value2,
			//образованную соединением элементов временного стека
			return (p.join("&") || "");
		} else return d;//иначе возвращаем результатирующий объект
	};

	/**
	* Проверяет состояние тихого запроса
	*
	* @param {Object} req
	*/
	_silentOnState = function(req) {
		if (req.done) return;//? что-то пошло не так :/
		if (req.r.readyState != 4) return;
		var n = $_name;
		req.done = true;
		if (req.r.status == 200) {
			if (req.action) {
				var r = _silentReqPendingFind(req.action);
				if (r) {
					r.r.open(r.method, r.url, true);
					r.r.onreadystatechange = function() {
						_silentOnState(r);
					}
					if (r.method == "POST") r.r.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
					r.r.send(r.method == "POST" ? r.dataPOST : null);
					r.sent = true;
				}
			}
			if (req.json) {
				if ((req.r.responseText == "ok") || (req.r.responseText == "true") || ((req.r.responseText == "") && !req.needResponse))
					req.response = {res: true, msg: ""};
				else {
					if ((req.r.responseText == "error") || (req.r.responseText == "false") || ((req.r.responseText == "") && (req.needResponse))) {
						req.response = {res: false, msg: ""};
						if (req.r.responseText == "") req.response.msg = "Сервер вернул пустой ответ.";
					} else {
						try {
							req.response = eval("(" + req.r.responseText + ")");
						} catch(e) {
							req.response = {res: false};
							req.response.msg = "Ошибка парсинга json-ответа [" + req.action + ": " + req.key + "]";;
							req.response.msgExt = "Сообщение интерпретатора: [" + e.name + "/" + e.message + "]";
							_console(__name_script + " > " + n + ".silentOnState(): " + req.response.msg + ". " + req.response.msgExt);
							_console("Необработанные данные: " + req.r.responseText);
						}
					}
				}
			} else req.response = {res: true, msg: "", data: req.r.responseText};
			if (req.debug) {
				if ((typeof req.response.debug == "object") && (req.response.debug)) {
					var cnt = 0;
					for (var c in req.response.debug) {
						if (!req.response.debug.hasOwnProperty(c)) continue;
						cnt++;
						if (cnt == 1) _console("Доступны отладочные данные:");
						_console("[" + c + "/" + typeof req.response.debug[c] + "]: " + req.response.debug[c]);
					}
				}
			}
		} else {
			req.response = {res: false, msg: "Ошибка выполнения XmlHttpRequest операции [status: " + req.r.status + "]", data: ""};
			_console(__name_script + " > " + n + "#_silentOnState(): " + req.response.msg + ", [" + req.action + ": " + req.key + "]");
		}
		if (typeof req.cbFunc == "function") {
			try {
				if (!req.cbBound) {
					if (req.owner)
						req.cbFunc.apply(req.owner, [req]);
					else
						_console(__name_script + " > " + n + "#_silentOnState(): Callback-функция не может быть выполнена при заданных параметрах [cbBound: false, owner: null].");
				} else req.cbFunc(req);
			} catch(e) {
				_console(__name_script + " > " + n + "#_silentOnState(): Ошибка выполнения callback-функции [" + req.action + ": " + req.key + "].");
				_console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
			}
		}
		if ((req.response.msg != "") && req.msgDisplay) {
			var shown = false;
			if (req.msgDisplayWay == "popup") {
					if ((typeof $_plugins[__name_msgr] != "undefined") && (typeof $_plugins[__name_msgr].obj == "object") && $_plugins[__name_msgr].obj) {
						var type = ((typeof req.response.res == "undefined") || req.response.res) ? "wrn" : "err";
						$_plugins[__name_msgr].obj.dlgAlert(req.response.msg, type, "300px");
						shown = true;
					} else {
						if ((typeof this._plugins[__name_popup] != "undefined") && (typeof this._plugins[__name_popup].obj == "object") && this._plugins[__name_popup].obj) {
							var pu = this._plugins[__name_popup].obj.add({
								content: req.response.msg,
								showcloser: true,
								windowed: true
							});
							$_plugins[__name_popup].obj.show(pu);
							shown = true;
						}
					}
			}
			if (!shown) alert(req.response.msg);
		}
		var l = $_silentReqs.length;
		if (!l) return;
		for (; l--;)
			if ($_silentReqs[l] === req) {
				delete $_silentReqs[l]["r"];
				$_silentReqs.splice(l, 1);
				break;
			}
	},

	/**
	* Формирование шаблона структуры тихого запроса
	*
	* @param {Object} owner - владелец запроса (плагин или расширение)
	*
	* @returns {Object}
	*/
	_silentReqBuild = function(owner) {
		return {
			action:			"",
			//если cbBound == false, ядро попытается
			//выполнить cbFunc в контексте owner
			cbBound:		false,
			cbFunc:			null,
			dataGET:		{},
			dataPOST:		{},
			debug:			false,
			done:			false,
			json:			true,
			key:			$_Lib.seed(),
			method:			"POST",
			msgDisplay:		true,
			msgDisplayWay:	"popup",//or alert
			needResponse:	true,
			owner: 			owner || null,
			owner_store:	{},
			r:				_xmlHttpGet(),
			response:		null,
			sent:			false,
			//владелец запроса для всех своих поочерёдных запросов
			//должен контроллировать индекс очереди сам,
			//т.е. после отправки первого запроса из очереди
			//индекс очереди нужно сохранить и потом использовать его
			//(назначать самостоятельно аля req.queue = ind)
			//в других запросах из этой же очереди
			queue:			-1,
			queueable:		false,
			time:			(new Date()).getTime(),
			url:			""
		};
	},

	/**
	* Находим следующий запрос(ы)
	* и отправляем
	*/
	_silentSend = function() {
		var req;
		//функция непосредственно отправки
		var send = function(req) {
			req.r.open(req.method, req.url, true);
			req.r.onreadystatechange = function(){
				_silentOnState(req);
			};
			if (req.method == "POST") req.r.setRequestHeader("Content-Type", "application/x-www-form-urlencoded");
			req.r.send(req.method == "POST" ? req.dataPOST : null);
			req.sent = true;
		};
		//сначала удаляем из стека $_silentReqs все отработанные запросы
		var l = $_silentReqs.length;
		for (; l--; ) {
			req = $_silentReqs[l];
			if (req.done) {
				if ($_debug) $_silentReqsDone.push(req);
				$_silentReqs.splice(l, 1);
			}
		}
		//отправляем запрос, который помечен
		//для отправки вне очереди (sequential == false)
		//он должен быть всего один (или должен отсутствовать вобще)
		l = $_silentReqs.length;
		for (; l--; ) {
			req = $_silentReqs[l];
			if (!req.queueable && !req.sent) {
				send(req);
				break;
			}
		}
		//формируем список очередей, состоящий из
		//простых объектов структуры
		//	{
		//		alreadySent: false,
		//		readyToSend: false
		//	}
		//где в readyToSend записывается последний найденный
		//и готовый к отправке запрос по текущей очереди
		var list = Object.create(null),
			qu;
		l = $_silentReqs.length;
		for (; l--; ) {
			req = $_silentReqs[l];
			if (!req.queueable) continue;
			//если у запроса нет индекса очереди, то генерируем его
			if (req.queue == -1) req.queue = ++$_silentQueue;
			//если очереди в списке еще нет, то регистрируем ее
			if (!list["q" + req.queue])
				qu = {
					readyToSend: req.sent ? false : req,
					sent: req.sent ? req : false
			}; else {
			//а если уже есть, то берем текущий запрос
				qu = list["q" + req.queue];
				//естественно, запрос, который будет отправлятся,
				//обновляется только в том случае, если
				//все предыдущие запросы из этой очереди уже отработали
				//и если данный запрос еще не отправлен
				if (!qu.sent) {
					if (!req.sent) qu.readyToSend = req;
					else qu.sent = req;
				}
			}
		}
		//теперь проходим по всем очередям и
		//отправляем (если доступен) следующий в текущей очереди запрос
		for (var c in list) {
			qu = list[с];
			//пропускаем очередь, если один из ее запросов еще в работе
			if (qu.sent) continue;
			//отправляем запрос
			if (qu.readyToSend) send(qu.readyToSend);
		}
	},

	/**
	* Выполнение фонового ("тихого") запроса на базе HTML-элемента FORM,
	* обычно используется для фоновой отправки файлов и в кросс-доменных запросах.
	*
	* @param {Object} req - объект типа "запрос", см. _silentXReqBuild()
	*
	* @returns {Boolean}
	*/
	_silentX = function(req) {
		req.method = req.method.toUpperCase();
		if ((req.method != "POST") && (req.method != "GET")) {
			_console(__name_script + " > " + $_name + "#_silentX(): Неизвестный метод [" + req.method + "], операция прервана.");
			return false;
		}
		//проверка параметров uri
		var url;
		if ((typeof req.url != "string") || !req.url) url = $.d.location.href.replace("http://" + $.d.domain, "");
		else url = ((req.url.indexOf("http") != -1) ? req.url : ($_dirs.root + req.url).replace(/\/\//g, "/"));
		var query = "";
		url = url.split("?");
		if (typeof url[1] != "undefined") query = url[1];
		url = url[0];
		//полный http путь запроса
		req.url = url + (query ? ("?" + query) : "");
		//GET, POST данные
		var merge = {silent: ""},
			actName = $_Render.elemId("Action");
		if (req.method == "GET") {
			merge[$_name + "-action-key"] = req.key;
			merge[actName] = req.action;
		}
		req.dataGET = _silentDataBuild(req.dataGET, merge);
		if (req.method == "POST") {
			delete merge.silent;
			merge[$_name + "-action-key"] = req.key;
			merge[actName] = req.action;
			req.dataPOST = _silentDataBuild(req.dataPOST, merge, false);
		}
		//отправка
		req.url = req.url + (req.dataGET ? ((query ? "&" : "?") + req.dataGET) : "");
		if (req.method == "GET") {
			req.domWorker = $.ce("SCRIPT", {type: "text/javascript"});
		}
		if(req.method == "POST") {
			var name = $_name + "-postform-" + $_Lib.seed();
			req.domWorker = $.ce("IFRAME", {
				id: name,
				name: name,
				src: "javascript:false;"
			});
			req.domForm = $.ce("FORM", {
				action: req.url,
				enctype: "multipart/form-data",
				method: "POST",
				target: name
			});
			if (typeof req.dataPOST == "object") {
				for (var c in req.dataPOST) {
					if (!req.dataPOST.hasOwnProperty(c)) continue;
					_silentXFormFieldAdd(req.domForm, c, req.dataPOST[c]);
				}
			}
			req.domMain = $.ce("DIV", {
				style: {
					display: "none",
					overflow: "hidden",
					height: "0"
				}
			});
			req.domMain.appendChild(req.domWorker);
			req.domMain.appendChild(req.domForm);
			if ($.b.childNodes.length)
				$.b.insertBefore(req.domMain, $.b.firstChild);
			else
				$.b.appendChild(req.domMain);
		}
		if (req.action && req.sequential) {
			var r = _silentXReqPendingFind(req.action);
			if (r) return true;
		}
		_silentXReqs.push(req);
		if (req.method == "GET") {
			req.domWorker.src = req.url;
			$.h.appendChild(req.domWorker);
		}
		if(req.method == "POST") {
			req.onready = function(){
				_silentXOnReady(req);
			};
			$_Render.evWinAdd(req.domWorker, "load", req.onready);
			req.domForm.submit();
		}
		req.sent = true;
		return true;
	},

	/**
	* Добавляет поле определенного HTML-типа
	* в указанную форму
	*
	* @param {Object} form - DOM-элемент формы
	* @param {string} name - назание поля (атрибут name)
	* @param {mixed} value - содержимое поля
	*
	* @returns {Boolean}
	*/
	_silentXFormFieldAdd = function(form, name, value) {
		if (typeof form == "undefined") return false;
		if (typeof name == "undefined") return false;
		if (typeof value == "undefined") value = "false";
		var type = "input";
		var val = "unknown value";
		if ((typeof value == "object") && (typeof value.nodeName != "undefined")) {
			if (typeof value.tagName != "undefined") {
				var tag = value.tagName.toLowerCase();
				switch (tag) {
					case "input":
						switch (value.type) {
							case "button":
								val = value.value;
								break;
							case "checkbox":
								val = value.checked ? "on" : "off";
								break;
							case "file":
								type = "file";
								break;
							case "hidden":
								val = value.value;
								break;
							case "password":
								val = value.value;
								break;
							case "submit":
								val = value.value;
								break;
							case "text":
								val = value.value;
								break;
							default:
								val = "not supported input type";
								break
						}
						break;
					case "textarea":
						type = "textarea";
						val = value.value;
						break;
					default:
						val = "not supported input";
						break;
				}
			} else {
				if (typeof value.textContent != "undefined") {
					type = "textarea";
					val = value.textContent;
				} else if (typeof value.innerText != "undefined") {
					type = "textarea";
					val = value.innerText;
				} else if (typeof value.innerHTML != "undefined") {
					type = "textarea";
					val = value.innerHTML;
				} else if (typeof value.nodeValue != "undefined") {
					type = "textarea";
					val = value.nodeValue;
				} else if (typeof value.toString != "undefined") {
					type = "textarea";
					val = value.toString();
				}
			}
		} else {
			if ((typeof value == "string") || (typeof value == "number")) val = "" + value;
			else {
				if (typeof value.toString != "undefined") val = value.toString();
				else val = "" + value;
			}
		}
		var el;
		switch (type) {
			case "input":
				el = $.ce("INPUT", {
					type: "hidden",
					name: name,
					value: val
				});
				form.appendChild(el);
				break;
			case "textarea":
				el = $.ce("TEXTAREA", {
					name: name,
					value: val
				});
				form.appendChild(el);
				break;
			case "file":
				value.name = name;
				form.appendChild(value);
				break;
			default:
				return false;
		}
		return true;
	},

	/**
	* Функция выполняется после того, как был отработан
	* silentX запрос и далее делает новый запрос на сервер
	* чтобы получить статус операции по первому запросу
	*
	* @param {Object} req
	*/
	_silentXOnReady = function(req) {
		if (req.done) return;
		if (typeof req.statusUrl == "string") {
			var rstat = _silentXReqBuild();
			rstat.method = "GET";
			var func = req.key + $_Lib.seed();
			rstat.dataGET[$_name + "-xcb"] = "window[\"" + func + "\"]";
			rstat.dataGET[$_name + "-xkey"]= req.key;
			rstat.dataGET.status = "";
			rstat.owner_store.req = req;
			rstat.owner_store.tempFunc = func;
			$.w[func] = _silentXOnStatus;
			rstat.url = req.statusUrl;
			_silentX(rstat);
		} else {
			req.done = true;
			if (typeof req.cbFunc == "function") {
				try {
					req.cbFunc(true);
				} catch(e) {
					_console(__name_script + " > " + $_name + "#_silentXOnReady(): Ошибка выполнения callback-функции.");
					_console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
				}
			}
		}
		$_Render.evWinRem(req.domWorker, "load", req.onready);
		req.domWorker.parentNode.parentNode.removeChild(req.domWorker.parentNode);
	},

	/**
	* Функция получает статус (результат) silentX запроса
	*
	* @param {string} resp - ответ сервера
	* @param {string} key - уникальный ключ операции
	*/
	_silentXOnStatus = function(resp, key) {
		var l = $_silentXReqs.length,
			sreq;
		for (; l--;) {
			sreq = $_silentXReqs[l];
			if (sreq.done) continue;
			if (typeof sreq.owner_store.req == "undefined") continue;
			if (typeof sreq.owner_store.tempFunc == "undefined") continue;
			if (sreq.owner_store.req.key !== key) continue;
			sreq.done = true;
			var req = sreq.owner_store.req;
			req.done = true;
			delete $.w[sreq.owner_store.tempFunc];
			if (req.action) {
				var r = _silentXReqPendingFind(req.action);
				if (r) {
					if (r.method == "GET") {
						r.domWorker.src = r.url;
						$.h.appendChild(r.domWorker);
					}
					if (r.method == "POST") {
						r.onready = function() {
							_silentXOnReady(r);
						};
						$_Render.evWinAdd(r.domWorker, "load", r.onready);
						r.domForm.submit();
					}
					r.sent = true;
				}
			}
			if (req.json) {
				if ((resp == "ok") || (resp == "true") || ((resp == "") && !req.needResponse))
					req.response = {res: true, msg: ""};
				else {
					if ((resp == "error") || (resp == "false") || ((resp == "") && (req.needResponse))) {
						req.response = {res: false, msg: ""};
						if (resp == "") req.response.msg = "Сервер вернул пустой ответ.";
					} else {
						try {
							req.response = eval("(" + resp + ");");
						} catch(e) {
							req.response = {res: false};
							req.response.msg = "Ошибка парсинга json-ответа.";
							req.response.data = resp;
							req.msgDisplay = true;
							req.msgDisplayWay = "popup"
							_console(__name_script + " > " + $_name + "#_silentXOnStatus(): " + req.response.msg);
							_console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
							_console("Необработанные данные: " + resp);
						}
					}
				}
			} else req.response = {res: true, msg: "", data: resp};
			if (typeof req.cbFunc == "function") {
				try {
					if (!req.cbBound) {
						if (req.owner)
							req.cbFunc.apply(req.owner, [req]);
						else {
							_console(__name_script + " > " + $_name + "#_silentXOnStatus(): Callback-функция не может быть выполнена при заданных параметрах [cbBound: false, owner: null].");
							_console(req);
						}
					} else req.cbFunc(req);
				} catch(e) {
					_console(__name_script + " > " + $_name + "#_silentXOnStatus(): Ошибка выполнения callback-функции.");
					_console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
				}
				if (req.debug) {
					if ((typeof req.response.debug == "object") && (req.response.debug)) {
						var cnt = 0;
						for (var d in req.response.debug) {
							if (!req.response.debug.hasOwnProperty(d)) continue;
							cnt++;
							if (cnt == 1) _console("Доступны отладочные данные:");
							_console("[" + d + "/" + (typeof req.response.debug[d]) + "]: " + req.response.debug[d]);
						}
					}
				}
			}
			if (req.response.msg) {
				if (req.msgDisplay) {
					var shown = false;
					if (req.msgDisplayWay == "popup") {
						if (typeof $_Msgr != "undefined") {
							var type = ((typeof req.response.res == "undefined") || req.response.res) ? "wrn" : "err";
							$_Msgr.dlgAlert(req.response.msg, type, "300px");
							shown = true;
						} else {
							if (typeof $_Lightbox != "undefined") {
								var lb = $_Lightbox.create({
									content: req.response.msg,
								});
								shown = true;
							}
						}
					}
					if (!shown) $.w.alert(req.response.msg);
				}
			}
			sreq.domWorker.parentNode.removeChild(sreq.domWorker);
			return;
		}
		_console(__name_script + " > " + $_name + "#_silentXOnStatus(): Предупреждение, получен статус незарегистрированной операции [key: " + key + "]. Содержимое ответа: ");
		_console(resp);
	},

	/**
	* Формирование структуры silentX запроса
	*
	* @param {Object} o - контекст (владелец) запроса, необязательный параметр
	*
	* @returns {Object}
	*/
	_silentXReqBuild = function(o) {
		if (typeof o == "undefined") o = null;
		var r = {
			action:			"",
			cbBound:		true,
			cbFunc:			false,
			dataGET:		{},
			dataPOST:		{},
			debug:			false,
			domForm:		null,
			domMain:		null,
			domWorker:		null,
			done:			false,
			encode:			true,
			json:			true,
			key:			"" + (Math.floor((Math.random()*1000000000) + 1)),
			method:			"POST",
			msgDisplay:		true,
			msgDisplayWay:	"popup",//or alert
			needResponse:	true,
			owner:			o,
			owner_store:	{},
			response:		null,
			sent:			false,
			sequential:		false,
			statusUrl:		false,
			time:			(new Date()).getTime(),
			url:			""
		};
		return r;
	},

	/**
	* Возвращает структуру запроса silentX
	* из стека всех запросов, используя для идентификации
	* ключ запроса req.key
	*
	* @param {string} key - ключ запроса
	*
	* @returns {Object|boolean}
	*/
	_silentXReqFetch = function(key) {
		var l = $_silentXReqs.length,
			r;
		for (; l--;) {
			r = $_silentXReqs[l];
			if (r.key == key) return r;
		}
		return false;
	},

	/**
	* Возвращает структуру еще не отправленного запроса silentX
	* из стека всех запросов, используя для идентификации
	* название операции req.action
	*
	* @param {string} action - название операции
	*
	* @returns {Object|boolean}
	*/
	_silentXReqPendingFind = function(action) {
		if (typeof action != "string") return false;
		var l = $_silentXReqs.length,
			r;
		for (; l--;) {
			r = $_silentXReqs[l];
			if ((r.action === action) && (!r.sent)) return r;
		}
		return false;
	},

	/**
	* Создает объект запроса xmlHttp
	*
	* @returns {object} r - xmlHttp request object
	*/
	_xmlHttpGet = function() {
		var r;
		try {
			r = new ActiveXObject("Msxml2.XMLHTTP");
		} catch (e) {
			try {
				r = new ActiveXObject("Microsoft.XMLHTTP");
			} catch (e) {
				r = false;
			}
		}
		if (!r && typeof XMLHttpRequest != "undefined")
			r = new XMLHttpRequest();
		else {
			_console(__name_script + " > " + $_name + ".xmlHttpGet(): Невозможно создать объект [XmlHttpRequest]");
			r = null;
		}
		return r;
	};

	/* --------------------------------- */


	/* -------- публичные методы ------- */

	/**
	* Загрузка расширений
	*/
	this.extensionsLoad = function() {
		//делаем заглушку на след. вызов
		this.extensionsLoad = function() {
			_console(__name_script + " > " + $_name + ".extensionsLoad(): All extensions already loaded!");
		};
		//загружаем расширения
		if (!_extensionsLoad()) return;
		//ставим таймер на инициализацию, которая должна
		//начаться при появлении определенного тега (например, id="core-init-ready")
		//в верстке страницы
		$_init.ivalObj = window.setInterval(function(){
			if ($.ge($_init.readyId)) {
				$.w.clearInterval($_init.ivalObj);
				_init();
			}
		}, $_init.ival);
		//таймер используем для того, чтоб
		//не создавать новый публичный метод
		return [_module];
	};

	/* --------------------------------- */
};

//показываем конструктор в глобальном пространстве
window.FlexClient = core;

})();

})();