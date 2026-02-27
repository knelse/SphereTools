/**
 * @module FlexClient/lib
 * @author Bogdan Nazar [me@bogdan-nazar.ru]
 * @version 3.2.0 (15.12.2016 14:46 +0400)
 * @description Расширение Runtime Code Library
 * @copyright 2003-2016 Bogdan Nazar
 * @license MIT & FlexEngine License
 * 		http://www.opensource.org/licenses/mit-license.php
 * 		http://www.flexengin.ru/docs/license
 */

/**
 * Примеры и документация: http://www.flexengin.ru/docs/client/core/lib-js
 *
 * Требования: FlexClient Core 3.2.0+
 */

(function(){

var $,
	__name_this = "lib",
	__name_script = __name_this + ".js";

//ищем FlexClient
if (typeof window.FlexClient != "function") {
	console.log(__name_script + " > Client application is not found.");
	return;
}

/**
* @class lib
*/
var lib = function() {

	/* ------- приватные свойства ------ */
	var $_bind				=	(typeof Function.prototype.bind != "function"),
		$_export			=	[
			"bind",
			"cyr2lat",
			"clone",
			"fileExt",
			"imageData",
			"lastMsg",
			"numberFormat",
			"seed",
			"urlBuild",
			"urlParse",
			"validDateRu",
			"validEmail",
			"validString"
		],
		$_imgs				=	{
			empty: 				"data:image/gif;base64,R0lGODlhAQABAIAAAP///wAAACH5BAEAAAAALAAAAAABAAEAAAICRAEAOw==",
			folder: 			"data:image/gif;base64,R0lGODlhEAAOALMAAOazToeHh0tLS/7LZv/0jvb29t/f3//Ub//ge8WSLf/rhf/3kdbW1mxsbP//mf///yH5BAAAAAAALAAAAAAQAA4AAARe8L1Ekyky67QZ1hLnjM5UUde0ECwLJoExKcppV0aCcGCmTIHEIUEqjgaORCMxIC6e0CcguWw6aFjsVMkkIr7g77ZKPJjPZqIyd7sJAgVGoEGv2xsBxqNgYPj/gAwXEQA7",
			loading: 			"data:image/gif;base64,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"
		},
		$_msg				=	"",					//последнее сообщение от функций процедурного типа
		$_name				=	__name_this;
	/* --------------------------------- */


	/* -------- приватные методы ------- */

	/**
	* Инициализация
	*
	* @returns {Boolean}
	*/
	var __init = function(cfg, _$, deps) {
		$ = _$;
		return true;
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
				console.log(__name_script + " > " + $_name + "#_export(): > Export failed, function: [" + func + "]!");
			}
			if ($_bind && (func != "bind")) obj[func].bind = _bind;
		}
		return obj;
	},

	/**
	* Прикрепление выполнения функции
	* к указанному контексту
	*
	* @param slice
	*/
	_bind = (function (slice){
		// based on [(C) WebReflection - Mit Style License]
		function bind(context) {
			var self = this;
			if (1 < arguments.length) {
				var $arguments = slice.call(arguments, 1);
				return function() {
					return self.apply(context, arguments.length ? $arguments.concat(slice.call(arguments)) : $arguments);
				};
			}
			return function () {
				return arguments.length ? self.apply(context, arguments) : self.call(context);
			};
		}
		return bind;
	}(Array.prototype.slice)),

	/**
	* Функция клонирования объекта
	*
	* @param {Object} o - клонируемый объект
	*
	* @returns {Object}
	*/
	_clone = function(o, skip) {
		if ((typeof o != "object") || !o) return o;
		var maxRec = 100;//safe exit for deep mode
		var curRec = -1;
		if (typeof deep != "boolean") {
			if (typeof deep != "number") deep = maxRec;
			else {
				if (deep < 0) deep = 0;
			}
		} else {
			if (deep) deep = maxRec;
			else deep = 0;
		}
		if ((typeof skip != "object") || !(skip instanceof Array)) skip = false;

		//список указателей
		var pts = [];

		//функция поиска указателя
		//на уже добавленный объект
		var findPt = function(o) {
			var l = pts.length;
			for (; l--;) if (pts[l][0] === o) return pts[l][1];
			if (skip) {
				l = skip.length;
				for (; l--;) if (skip[l] === o) return o;
			}
			return false;
		};
		var worker, no = {};
		worker = function(o, no) {
			curRec++;
			pts.push([o, no]);
			var rec = true;
			if ((curRec >= deep) || (curRec >= maxRec)) rec = false;
			for (var c in o) {
				if (typeof o.hasOwnProperty == "function") {
					if (!o.hasOwnProperty(c)) continue;
				}
				if (rec && (typeof o[c] == "object") && o[c]) {
					no[c] = findPt(o[c]);
					if (no[c] === false) {
						no[c] = (o[c] instanceof Array ? [] : {});
						worker(o[c], no[c]);
					}
				} else no[c] = o[c];
			}
		};
		worker(o, no);
		return no;
	};

	/**
	* Преобразует строку из кириллицы в латиницу
	*
	* @param {string} str - исходная строка
	*
	* @returns {string}
	*/
	_cyr2lat = function(str) {
	    var cyr2latChars = new Array(
			['а', 'a'], ['б', 'b'], ['в', 'v'], ['г', 'g'],
			['д', 'd'],  ['е', 'e'], ['ё', 'yo'], ['ж', 'zh'], ['з', 'z'],
			['и', 'i'], ['й', 'y'], ['к', 'k'], ['л', 'l'],
			['м', 'm'],  ['н', 'n'], ['о', 'o'], ['п', 'p'],  ['р', 'r'],
			['с', 's'], ['т', 't'], ['у', 'u'], ['ф', 'f'],
			['х', 'h'],  ['ц', 'c'], ['ч', 'ch'],['ш', 'sh'], ['щ', 'shch'],
			['ъ', ''],  ['ы', 'y'], ['ь', ''],  ['э', 'e'], ['ю', 'yu'], ['я', 'ya'],

			['А', 'A'], ['Б', 'B'],  ['В', 'V'], ['Г', 'G'],
			['Д', 'D'], ['Е', 'E'], ['Ё', 'YO'],  ['Ж', 'ZH'], ['З', 'Z'],
			['И', 'I'], ['Й', 'Y'],  ['К', 'K'], ['Л', 'L'],
			['М', 'M'], ['Н', 'N'], ['О', 'O'],  ['П', 'P'],  ['Р', 'R'],
			['С', 'S'], ['Т', 'T'],  ['У', 'U'], ['Ф', 'F'],
			['Х', 'H'], ['Ц', 'C'], ['Ч', 'CH'], ['Ш', 'SH'], ['Щ', 'SHCH'],
			['Ъ', ''],  ['Ы', 'Y'],
			['Ь', ''],
			['Э', 'E'],
			['Ю', 'YU'],
			['Я', 'YA'],

			['a', 'a'], ['b', 'b'], ['c', 'c'], ['d', 'd'], ['e', 'e'],
			['f', 'f'], ['g', 'g'], ['h', 'h'], ['i', 'i'], ['j', 'j'],
			['k', 'k'], ['l', 'l'], ['m', 'm'], ['n', 'n'], ['o', 'o'],
			['p', 'p'], ['q', 'q'], ['r', 'r'], ['s', 's'], ['t', 't'],
			['u', 'u'], ['v', 'v'], ['w', 'w'], ['x', 'x'], ['y', 'y'],
			['z', 'z'],

			['A', 'A'], ['B', 'B'], ['C', 'C'], ['D', 'D'],['E', 'E'],
			['F', 'F'],['G', 'G'],['H', 'H'],['I', 'I'],['J', 'J'],['K', 'K'],
			['L', 'L'], ['M', 'M'], ['N', 'N'], ['O', 'O'],['P', 'P'],
			['Q', 'Q'],['R', 'R'],['S', 'S'],['T', 'T'],['U', 'U'],['V', 'V'],
			['W', 'W'], ['X', 'X'], ['Y', 'Y'], ['Z', 'Z'],

			[' ', '-'],['0', '0'],['1', '1'],['2', '2'],['3', '3'],
			['4', '4'],['5', '5'],['6', '6'],['7', '7'],['8', '8'],['9', '9'],
			['-', '-']
	    );
		var newStr = new String(),
			ch;
		for (var i = 0; i < str.length; i++) {
			ch = str.charAt(i);
			var newCh = '';
			for (var j = 0; j < cyr2latChars.length; j++) {
				if (ch == cyr2latChars[j][0]) {
					newCh = cyr2latChars[j][1];
				}
			}
			// Если найдено совпадение, то добавляется соответствие, если нет - пустая строка
			newStr += newCh;
		}
		// Удаляем повторяющие знаки - Именно на них заменяются пробелы.
		// Так же удаляем символы перевода строки, но это наверное уже лишнее
		return newStr.replace(/[-]{2,}/gim, '-').replace(/\n/gim, '');
	},

	/**
	* Функция получает подстроку расширения файла
	* из строки полного имени файла
	*
	* @param {string} fileName - полное имя файла
	*
	* @returns {string}
	*/
	_fileExt = function(fileName) {
		return (-1 !== fileName.indexOf(".")) ? fileName.replace(/.*[.]/, "") : "";
	},

	/**
	* Возвращает байтовый массив битмапа
	* (для использования в URL ресурсов)
	*
	* @param {string} name - сроковой идентификатор изображения
	*
	* @returns {string}
	*/
	_imageData = function(name) {
		if (typeof $_imgsData[name] != "undefined") return $_imgsData[name];
		else return $_imgsData["empty"];
	},

	/**
	* Возвращает последнее сообщение, которое
	* было сформировано функцией процедурного типа
	*
	* @return {string}
	*/
	_lastMsg = function() {
		var msg = $_msg;
		$_msg = "";
		return msg;
	},

	/**
	* Форматирует строковое представление чисел
	* с плавающей точкой
	*
	* @param {number} number
	* @param {number} decimals
	* @param {string} dec_point
	* @param {string} thousands_sep
	*
	* @returns {string}
	*/
	_numberFormat = function(number, decimals, dec_point, thousands_sep) {
		// Format a number with grouped thousands
	    //
	    // +   original by: Jonas Raoni Soares Silva (http://www.jsfromhell.com)
	    // +   improved by: Kevin van Zonneveld (http://kevin.vanzonneveld.net)
	    // +     bugfix by: Michael White (http://crestidg.com)
	    // + refactored by: Bogdan Nazar (http://flexengine.ru)
	    //
	    var i, j, kw, kd, km;
	    // input sanitation & defaults
	    if (isNaN(decimals = Math.abs(decimals))) decimals = 2;
	    if (typeof dec_point == "undefined") dec_point = ",";
	    if (typeof thousands_sep == "undefined") thousands_sep = "";
	    i = parseInt(number = (+number || 0).toFixed(decimals), 10) + "";
	    if ((j = i.length) > 3) j = j % 3; else j = 0;
	    km = (j ? i.substr(0, j) + thousands_sep : "");
	    kw = i.substr(j).replace(/(\d{3})(?=\d)/g, "$1" + thousands_sep);
	    kd = (decimals ? dec_point + Math.abs(number - i).toFixed(decimals).replace(/-/, 0).slice(2) : "");
	    return km + kw + kd;
	},

	/**
	* Функция возвращает рандомную строку вида 6435634534
	*/
	_seed = function() {
		var m = window.Math;
		return "" + (m ? (m.floor((m.random() * 1000000000) + 1)) : (new Date()).getTime());
	},

	/**
	* Формирует новый полный URL2, на основе:
	* 	полного URL1 с параметрами
	* 	и дополнительных query-параметров
	* с замещением старых параметров новыми, если такие имеются
	*
	* @param {string|boolean} url - входящий строка URL1 или false для использования location.href
	* @param {Object|string} query - новые параметры
	* @param {boolean} seed - флаг сидирования URL2
	*
	* @returns {string}
	*/
	_urlBuild = function(url, query, seed) {
		//проверяем аргументы
		if (typeof url != "string") url = window.location.href;
		if (typeof query == "string") {
			if (query) {
				query = _urlParse(query);
				query = query.params;
			} else query = {};
		} else {
			if ((typeof query != "object") || !query) query = {};
		}
		if (typeof seed != "boolean") seed = false;
		//парсим URL
		url = _urlParse(url);
		//сливаем параметры
		for (var c in query) {
			if (!query.hasOwnProperty(c)) continue;
			url.params[c] = query[c];
		}
		//добавляем сидирование
		if (seed) url.params["seed"] = _seed();
		//формируем предварительный массив с закодированными парами имя=значение
		var p = [];
		for (var id in url.params) {
			if (!url.params.hasOwnProperty(id)) continue;
			if (typeof url.params[id] == "object") continue;//пропускаем объекты
			p.push(id + "=" + (typeof url.params[id] == "booelan" ? (url.params[id]).toString : encodeURIComponent(url.params[id])));
		}
		//формируем результатирующую строку URL
		var q = p.join("&");
		return url.path + (q ? ("?" + q) : "") + (url.hash ? ("#" + url.hash) : "");
	},

	/**
	* Функция парсинга URL строки
	*
	* @param {string} url - строка URL
	*
	* @returns {Object}
	*/
	_urlParse = function(url) {
		if (typeof url != "string") url = window.location.href;
		var a = document.createElement("A");
		a.href = url;
		return {
			file: (a.pathname.match(/\/([^\/?#]+)$/i) || [, ""])[1],
			hash: a.hash.replace("#", ""),
			host: a.hostname,
			params: (function(){
				var ret = {},
					seg = a.search.replace(/^\?/, "").split("&"),
					len = seg.length, i = 0, s;
				for (; i < len; i++) {
					if (!seg[i]) continue;
					s = seg[i].split("=");
					ret[s[0]] = s[1] ? s[1] : "";
				}
				return ret;
			})(),
			path: a.pathname.replace(/^([^\/])/, "/$1"),
			port: a.port,
			protocol: a.protocol.replace(":", ""),
			query: a.search,
			relative: (a.href.match(/tps?:\/\/[^\/]+(.+)/) || [, ""])[1],
			segments: a.pathname.replace(/^\//, "").split("/"),
			source: a.href
		};
	},

	/**
	* Функция проверки строки даты в российском формате
	*
	* @param {string} dt - дата
	*
	* @returns {boolean}
	*/
	_validDateRu = function(dt, time) {
		if (typeof time != "boolean") time = true;
		//tnx to halcyon: http://stackoverflow.com/questions/20972728/validate-datetime-with-javascript-and-regex
		var matches;
		if (time)
			matches = dt.match(/^(\d{2})\.(\d{2})\.(\d{4}) (\d{2}):(\d{2}):(\d{2})$/);
		else
			matches = dt.match(/^(\d{2})\.(\d{2})\.(\d{4})$/);
		//alt:
		// value.match(/^(\d{2}).(\d{2}).(\d{4}).(\d{2}).(\d{2}).(\d{2})$/);
		// also matches 22/05/2013 11:23:22 and 22a0592013,11@23a22
		if (matches === null) return false;
	    // now lets check the date sanity
	    var year = parseInt(matches[3], 10);
	    var month = parseInt(matches[2], 10) - 1; // months are 0-11
	    var day = parseInt(matches[1], 10);
	    var hour = parseInt(matches[4], 10);
	    var minute = parseInt(matches[5], 10);
	    var second = parseInt(matches[6], 10);
	    var date = new Date(year, month, day, hour, minute, second);
	    if (date.getFullYear() !== year
	      || date.getMonth() != month
	      || date.getDate() !== day
	      || date.getHours() !== hour
	      || date.getMinutes() !== minute
	      || date.getSeconds() !== second
	    ) return false;
	    return true;
		//other way from vemax
		//http://www.sql.ru/forum/actualthread.aspx?tid=637923
		/*
		var r = /^(\d{2})\.(\d{2})\.(\d{4})$/;
		if (r.test(dt)) {
			var d = RegExp.$1 * 1;
			var m = RegExp.$2 * 1;
			var y = RegExp.$3 * 1;
			var test = new Date(y, m - 1, d);
			return ((test.getFullYear() == y) && (test.getMonth() == (m - 1)) && (test.getDate() == d));
		} else return false;
		*/
	},

	/**
	* Функция проверки строки e-mail
	*
	* @param {string} e - email
	*
	* @returns {boolean}
	*/
	_validEmail = function(e) {
	    var re = /^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
	    return re.test(e);
	},

	/**
	* Функция проверки строки по заданным правилам
	*
	* @param {string} st - проверяемая строка
	* @param {string} tp - тип строки ("pass","sent","user","name","spon","addr","int","dec","phon","rusname")
	* @param {number} minlen - минимальная длина строки
	* @param {number} maxlen - максимальная длина строки
	* @param {boolean} ignore_empty - флаг игнорирования пустой строки
	* @param {string} fldname - название проверяемого поля (для текста ошибки)
	*
	* @returns {Boolean}
	*/
	_validString = function(st, tp, minlen, maxlen, ignore_empty, fldname) {
		$_msg = "";
		if (!st) {
			if (ignore_empty) return true;
			else {
				$_msg = "Поле \"" + fldname + "\" не может быть пустым!";
				return false;
			}
		}
		if (!tp) {
			$_msg = "Ошибка клиентского скрипта: неверные параметры функции!";
			return false;
		}
		if ((typeof(minlen) != "number") || (typeof(maxlen) != "number")) {
			$_msg="Ошибка клиентского скрипта: неверные параметры функции!";
			return false;
		}
		var len = st.length;
		var chars = "";
		if (len < minlen) {
			$_msg = "Поле \"" + fldname + "\" слишком короткое (мин. длина - " + minlen + ")!";
			return false;
		}
		if (len > maxlen) {
			$_msg = "Поле \"" + fldname + "\" слишком длинное (макс. длина - " + maxlen + ")!";
			return false;
		}
		var lat = "QWERTYUIOPASDFGHJKLZXCVBNMqwertyuiopasdfghjklzxcvbnm",
			digs = "0123456789";
		if (tp == "pass") chars = lat + digs + "%@$*_";
		if (tp == "sent") chars = lat + digs + " .,-?";
		if (tp == "user") chars = lat + digs + "_-";
		if (tp == "name") chars = lat + " .-";
		if (tp == "spon") chars = lat + digs + "_-$.";
		if (tp == "addr") chars = lat + digs + ".- ";
		if (tp == "rusname") chars = "ЁЙЦУКЕНГШЩЗХЪФЫВАПРОЛДЖЭЯЧСМИТЬБЮёйцукенгшщзхъфывапролджэячсмитьбю";
		if (tp == "int") chars = digs;
		if (tp == "dec") chars = digs + ".";
		if (tp == "phon") chars = digs + "+- ()";
		var ch = "";
		for (var cnt = 0; cnt < len; cnt++) {
			ch = st.substr(cnt, 1);
			if (chars.indexOf(ch) == -1) {
				$_msg = "Поле \"" + fldname + "\" содержит некорректные символы! Ожидаются ";
				if (tp == "pass") $_msg = $_msg + "[A-Z],[a-z],[0-9],[%@$*_]!";
				if (tp == "sent") $_msg = $_msg + "[A-Z],[a-z],[0-9],[ .,-?]!";
				if (tp == "user") $_msg = $_msg + "A-Z],[a-z],[0-9],[_-]!";
				if (tp == "name") $_msg = $_msg + "[A-Z],[a-z],[ .-]!";
				if (tp == "spon") $_msg = $_msg + "[A-Z],[a-z],[_-$.]!";
				if (tp == "addr") $_msg = $_msg + "[A-Z],[a-z],[0-9],[ .-]!";
				if (tp == "int") $_msg = $_msg + "[0-9]!";
				if (tp == "dec") $_msg = $_msg + "[0-9],[.]!";
				if (tp == "phon") $_msg = $_msg + "[0-9],[ +-()]";
				if (tp == "rusname") $_msg = $_msg + "[А-Я],[а-я],[ -.]";
				return false;
			}
		}
		return true;
	};
	/* --------------------------------- */

	/* -------- публичные методы ------- */

	/**
	* Инициализация
	*/
	this.init = function() {
		return __init.apply(null, arguments);
	};

	/**
	* Функция расширяет указанный в аргументах объект
	* на определенные реестром набор своих методов
	*/
	this.export_ = function(obj) {
		return _export.apply(null, [obj]);
	};

	/* --------------------------------- */
};

//регистрируем глобально
window.FlexClientLib = new lib();
})();