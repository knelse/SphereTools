/**
 * @module FlexClient/render
 * @author Bogdan Nazar [me@bogdan-nazar.ru]
 * @version 3.2.0 (15.12.2016 16:53 +0400)
 * @description Расширение по управлению рендерингом страницы Render
 * @copyright 2003-2016 Bogdan Nazar
 * @license MIT & FlexEngine License
 * 		http://www.opensource.org/licenses/mit-license.php
 * 		http://www.flexengin.ru/docs/license
 */

/**
 * Примеры и документация: http://www.flexengin.ru/docs/client/core/render-js
 *
 * Требования: FlexClient Core 3.2.0+
 */

(function(){

var $,
	__name_this = "render",
	__name_script = __name_this + ".js";

//ищем FlexClient
if (typeof window.FlexClient != "function") {
	console.log(__name_script + " > Client application is not found.");
	return;
}

/**
* @class render
*/
var render = function() {

	/* ------- приватные свойства ------ */
	var $_deps			=	[
			"Core",
			"Lib",
			"Lightbox"
		],
		$_elems			=	{
			Action:			{
				el:			null,
				name:		""
			},
			Form:			{
				action:		"",
				el:			null,
				name:		"",
				target:		""
			}
		},
		$_export		=	[
			"cssInsert",
			"elemId",
			"evWinAdd",
			"evWinFix",
			"evWinRem",
			"evWinStop",
			"ltDocClientHeight",
			"ltDocClientWidth",
			"ltDocScrolledBy",
			"ltClassAdd",
			"ltClassHas",
			"ltClassRem",
			"ltElemInViewport",
			"ltPos",
			"ltScrollTo",
			"ltStyle",
			"ltToggle",
			"template",
			"waiterHide",
			"waiterShow"
		],
		$_name			=	__name_this,
		$_page			=	{
			alias:			"",
			loaded:			false,
			template:		"",
			title:			"",
			url:			{},
			urlMode:		"first"//or "last"
		},
		$_waiter		=	{
			inited:			false,
			inst:			null,
			visible:		false
		},

		//расширения
		$_Core			=	null,
		$_Lightbox		=	null,
		$_Lib			=	null;
	/* --------------------------------- */


	/* -------- приватные методы ------- */

	/**
	* Инициализация
	*
	* @returns {Boolean}
	*/
	var __init = function(cfg, _$, deps) {
		//конфиг
		//-- elems
		if (cfg && (typeof cfg.elems != "object")) {
			console.log(__name_script + " > " + $_name + "#_init(): Конфигурационные данные отсутствуют или имеют неверный формат [cfg.elems]!");
			return false;
		}
		for (var c in cfg.elems) $_elems[c].name = cfg.elems[c];
		//-- page
		if (typeof cfg.page != "object") {
			console.log(__name_script + " > " + $_name + "#_init(): Конфигурационные данные отсутствуют или имеют неверный формат [cfg.page]!");
			return false;
		}
		for (var c in cfg.page) $_page[c] = cfg.page[c];

		//ссылки
		$ = _$;

		//зависимости
		var l = $_deps.length,
			name;
		for (; l--;) {
			name = $_deps[l];
			if (!deps[name]) {
				console.log(__name_script + " > " + $_name + "#_init(): Dependency not found [" + name + "]!");
				return false;
			}
			try {
				eval("$_" + name + " = deps." + name + ";");
			} catch(e){
				console.log(__name_script + " > " + $_name + "#_init(): Error while creating the local link to dependency [$_" + name + "]!");
				return false;
			}
		}

		//ищем DOM элементы приложения

		//элемент Форма
		var form = $_elems.Form;
		form.el = $.ge($_name + "-" + form.name) || false;
		if(!form.el) {
			$_Core.console(__name_script + " > " + $_name + "#_init(): Форма отправки данных приложения не найдена [" + $_name + "-" + form-name + "]!");
			return false;
		} else {
			//сохраняем оригинальные значения
			form.target = form.el.target;
			form.action = form.el.action;
		}

		//элемент Действия
		var action = $_elems.Action;
		action.el = $.ge($_name + "-" + action.name) || false;
		if(!action.el) {
			$_Core.console(__name_script + " > " + $_name + "#_init(): Поле операций ядра не найдено [" + $_name + "-" + action.name + "]!");
			return false;
		}

		return true;
	},

	/**
	* Sending the main form data to the server
	*
	* @param {string} action - action name
	* @param {string} path - URL path
	* @param {string} query - query string
	* @param {string} target - form target
	* @param {boolena} seed - whether to seed the final URL
	*/
	_action = function(action, path, query, target, seed) {
		var e = $_elems;
		e.Action.el.value = "" + action;
		e.Form.el.action = $_Lib.urlBuild(path, query, seed);
		e.Form.el.target = ((typeof target == "string") && target) ? target : e.Form.el.target;
		e.Form.el.submit();
		return true;
	},

	/**
	* Вставка набора стилей в страницу
	*
	* @param {Array} st
	*
	* @returns {Boolean}
	*/
	_cssInsert = function(st) {
		if (typeof st == "string") st = [st];
		var l = st.length;
		if (!l) return true;
		var ss = $.ce("STYLE");
		ss.setAttribute("type", "text/css");
		$.h.appendChild(ss);
		if (ss.sheet && (typeof ss.sheet.insertRule == "function")) {
			ss.appendChild($.d.createTextNode(""));//для safari, хз зачем
			var c = 0,
				s = ss.sheet;
			for (; c < l; c++) {
				try {
					s.insertRule(st[c], s.cssRules.length);
				} catch(e) {
					console.log("#cssInsert(): CSS-rule insertion failed.");
					console.log(st[c]);
				}
			}
			return true;//webkit or other html5
		} else {
			if (ss.styleSheet && (typeof ss.styleSheet.cssText != "undefined")) ss.styleSheet.cssText = st.join("");
			return false;//ie8 or other non-html5
		}
	},

	/**
	* Возвращает id/name элемента по его системному алиасу
	*
	* @param name
	*
	* @returns {Boolean}
	*/
	_elemId = function(name) {
		if ($_elems[name]) return $_elems[name].name;
		else return false;
	},

	/**
	* Функция добавляет обработчик
	* указанного DOM-события для указанного DOM-элемента
	*
	* @param {DOM} el - элемент
	* @param {string} evnt - событие
	* @param {Function} func - обработчик
	*/
	_evWinAdd = function(el, evnt, func) {
		if (el.addEventListener) {
			el.addEventListener(evnt, func, false);
		} else if (el.attachEvent) {
			el.attachEvent("on" + evnt, func);
		} else {
			el[evnt] = func;
		}
	},

	/**
	* Кроссбраузерная нормализация объекта события
	*
	* @param {Event} e
	*
	* @returns {Event}
	*/
	_evWinFix = function(e) {
		// получить объект событие для IE
		e = e || window.event;
		// добавить pageX/pageY для IE
		if (e.pageX == null && e.clientX != null) {
			var html = document.documentElement;
			var body = document.body;
			e.pageX = e.clientX + (html && html.scrollLeft || body && body.scrollLeft || 0) - (html.clientLeft || 0);
			e.pageY = e.clientY + (html && html.scrollTop || body && body.scrollTop || 0) - (html.clientTop || 0);
		}
		// добавить which для IE
		if (!e.which && e.button) {
			e.which = (e.button & 1) ? 1 : ((e.button & 2) ? 3 : ((e.button & 4) ? 2 : 0));
		}
		if (!e.target && e.srcElement) {
			e.target = e.srcElement;
		}
		return e;
	},

	/**
	* Функция удаляет обработчик
	* указанного DOM-события для указанного DOM-элемента
	*
	* @param {DOM} el - элемент
	* @param {string} evnt - событие
	* @param {Function} func - обработчик
	*/
	_evWinRem = function(el, evnt, func) {
		if (el.removeEventListener) {
			el.removeEventListener(evnt, func, false);
		} else if (el.attachEvent) {
			el.detachEvent("on" + evnt, func);
		} else {
			el[evnt] = null;
		}
	},

	/**
	* Функция блокирует дальнейшую иерархическую
	* обработку указанного DOM-события (всплывание)
	*
	* @param {Event} e - событие
	*/
	_evWinStop = function(e) {
		if (typeof e != "object" || !e) return;
		if (e.preventDefault) {
			e.preventDefault();
			e.stopPropagation();
		} else {
			e.returnValue = false;
			e.cancelBubble = true;
		}
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
		}
		return obj;
	},

	/**
	* Обновляет параметры футера (margin & height)
	* после его рендеринга
	*/
	_footerUpdate = function() {
		var fm = document.getElementById($_name + "-footer-margin");
		var f = document.getElementById($_name + "-footer");
		if(!fm || !f) return;
		var fh = $(f).outerHeight(true);
		f.style.marginTop = "-" + fh + "px";
		fm.style.height = "" + fh + "px";
	},

	/**
	* Высота документа (body)
	*
	* @param el
	*/
	_ltDocClientHeight = function(el) {
		if (!el) el = $.d;
		return ($.d.compatMode == "CSS1Compat" && !window.opera) ? $.d.documentElement.clientHeight : $.d.body.clientHeight;
	},

	/**
	* Ширина документа (body)
	*/
	_ltDocClientWidth = function () {
		return ($.d.compatMode == "CSS1Compat" && !window.opera) ? $.d.documentElement.clientWidth : $.d.body.clientWidth;
	},

	/**
	* Возвращает значение прокрутки страницы
	* по горозионтали и вертикали
	*
	* @returns {Object}
	*/
	_ltDocScrolledBy = function () {
		//snippet source: http://learn.javascript.ru/metrics-window
		if (typeof $.w.pageXOffset != "undefined") {
			return {
				left: $.w.pageXOffset,
				top: $.w.pageYOffset
			};
		}

		var html = $.d.documentElement,
			body = $.d.body;
		var top = html.scrollTop || body && body.scrollTop || 0;
		top -= html.clientTop;
		var left = html.scrollLeft || body && body.scrollLeft || 0;
		left -= html.clientLeft;

		return {top: top, left: left };
	},

	/**
	* Возвращает размеры документа (окна)
	* а также размер сраницы (страница может быть меньше чем документ)
	*
	* @returns {Object}
	*/
	_ltDocSize = function() {
		var xScroll, yScroll;
		if ($.w.innerHeight && $.w.scrollMaxY) {
			xScroll = $.d.body.scrollWidth;
			yScroll = $.w.innerHeight + $.w.scrollMaxY;
		} else if ($.d.body.scrollHeight > $.d.body.offsetHeight){ // all but Explorer Mac
			xScroll = $.d.body.scrollWidth;
			yScroll = $.d.body.scrollHeight;
		} else if ($.d.documentElement && $.d.documentElement.scrollHeight > $.d.documentElement.offsetHeight){ // Explorer 6 strict mode
			xScroll = $.d.documentElement.scrollWidth;
			yScroll = $.d.documentElement.scrollHeight;
		} else { // Explorer Mac...would also work in Mozilla and Safari
			xScroll = $.d.body.offsetWidth;
			yScroll = $.d.body.offsetHeight;
		}
		var wWidth, wHeight;
		if ($.w.innerHeight) { // all except Explorer
			wWidth = $.w.innerWidth;
			wHeight = $.w.innerHeight;
		} else if ($.d.documentElement && $.d.documentElement.clientHeight) { // Explorer 6 Strict Mode
			wWidth = $.d.documentElement.clientWidth;
			wHeight = $.d.documentElement.clientHeight;
		} else if ($.d.body) { // other Explorers
			wWidth = $.d.body.clientWidth;
			wHeight = $.d.body.clientHeight;
		}
		var pWidth, pHeight;
		// for small pages with total height less then height of the viewport
		if(yScroll < wHeight) pHeight = wHeight;
		else pHeight = yScroll;
		// for small pages with total width less then width of the viewport
		if(xScroll < wWidth) pWidth = wWidth;
		else pWidth = xScroll;
		return {w: wWidth, h: wHeight, pw: pWidth, ph: pHeight};
	},

	/**
	* Добавляет CSS-класс в элемент
	*
	* @param {DOM} el - DOM-элемент
	* @param {string} name - имя класса
	*/
	_ltClassAdd = function(el, name) {
		var re = new RegExp("(?:^|\\\s)" + name + "(?!\\\S)", "g");
		if (!re.test(el.className))	el.className += (" " + name);
	},

	/**
	* Проверяет наличие CSS-класса у элемента
	*
	* @param {DOM} el - DOM-элемент
	* @param {string} name - имя класса
	*/
	_ltClassHas = function(el, name) {
		var re = new RegExp("(?:^|\\\s)" + name + "(?!\\\S)", "g");
		return re.test(el.className);
	},

	/**
	* Удаляет указанный CSS-класс у элемента
	*
	* @param {DOM} el - DOM-элемент
	* @param {string} name - имя класса
	*/
	_ltClassRem = function(el, name) {
		var re = new RegExp("(?:^|\\\s)" + name + "(?!\\\S)", "g");
		el.className = el.className.replace(re, "");
	},

	/**
	* Проверяет, находится ли элемент в пределах
	* вьюпорта браузера
	*
	* @param {DOM} el - DOM-элемент или его id
	*
	* @returns {boolean}
	*/
	_ltElemInViewport = function(el) {
		var elem;
		if (typeof el == "string") elem = $.ge(el);
		else elem = el;

		var ps = _ltScrolledBy(),
			ep = _ltPos(el);
		if (($.d.documentElement.clientHeight + ps.top) >= ep.top) return true;
		else return false;
	},

	/**
	* Определяет позицию элемента в пикселях
	* относительно верхнего левого угла страницы
	*
	* @param {DOM|string} el - DOM-элемент или его id
	*
	* @returns {Object}
	*/
	_ltPos = function(el) {
		var elem;
		if (typeof el == "string") elem = $.ge(el);
		else elem = el;

		var offsetSum = function(elem) {
			var top = 0,
				left = 0;
			while (elem) {
				top = top + parseInt(elem.offsetTop, 10);
				left = left + parseInt(elem.offsetLeft, 10);
				elem = elem.offsetParent;
			}
			return {top: top, left: left};
		},

		offsetRect = function(elem) {
			// (1)
			var box = elem.getBoundingClientRect();
			// (2)
			var body = $.d.body;
			var docElem = $.d.documentElement;
			// (3)
			var scrollTop = $.w.pageYOffset || docElem.scrollTop || body.scrollTop;
			var scrollLeft = $.w.pageXOffset || docElem.scrollLeft || body.scrollLeft;
			// (4)
			var clientTop = docElem.clientTop || body.clientTop || 0;
			var clientLeft = docElem.clientLeft || body.clientLeft || 0;
			// (5)
			var top = box.top + scrollTop - clientTop;
			var left = box.left + scrollLeft - clientLeft;
			return {top: top, left: left};
		};


		if (elem.getBoundingClientRect) {
			// "правильный" вариант
			return offsetRect(elem);
		} else {
			// пусть работает хоть как-то
			return offsetSum(elem);
		}
	},

	/**
	* Прокручивает окно браузера до элемента
	*
	* @param {DOM} el - DOM-элемент или его id
	* @param {Boolean} offset - дополнительное смещение
	*/
	_ltScrollTo = function(el, offset) {
		var elem;
		if (typeof el == "string") elem = $.ge(el);
		else elem = el;

		if (typeof offset != "number") offset = false;

		var coords = _ltPos(elem);
		$.w.scrollTo(0, coords.top + (offset ? offset : 0));
	},

	/**
	* Возвращает значение указанного inline-стиля
	*
	* @param {DOM|string} el - DOM-элемент или его id
	* @param {string} styleProp - искомый стиль в формате "margin-top"
	*
	* @returns {string}
	*/
	_ltStyle = function(el, styleProp) {
		var elem,
			val = "";
		if (typeof el == "string") elem = $.ge(el);
		else elem = el;

		if (elem.currentStyle) val = elem.currentStyle[styleProp];
		else if (window.getComputedStyle)
			val = $.d.defaultView.getComputedStyle(elem, null).getPropertyValue(styleProp);
		return val;
	},

	/**
	* Переключает видимость элемента на странице
	*
	* @param {Object} el - DOM-элемент или его id
	* @param {String} display - установить видимость, отличную от "block" (напр. inline-block)
	*/
	_ltToggle = function(el, display) {
		var elem,
			val = "";
		if (typeof el == "string") elem = $.ge(el);
		else elem = el;

		if (typeof display != "string" || (!display)) display = "block";
		if (el.style.display == "none") el.style.display = display;
		else el.style.display = "none";
	},

	/**
	* Возвращает название текущего шаблона страницы
	*
	* @returns {string}
	*/
	_template = function() {
		return $cfg.template;
	},

	/**
	* Прячет индикатор загрузки
	*/
	_waiterHide = function() {
		if (!$_waiter.inited || !$_waiter.inst) {
			$_Core.console(__name_script + " > " + $_name + "#_waiterHide(): Модуль всплывающего (модального) окна [Lightbox] отсутствует или не инициализирован.");
			return;
		}
		if (!$_waiter.visible) return;
		$_waiter.inst.hide();
		$_waiter.visible = false;
	},

	/**
	* Инициализаци DOM индикатора загрузки
	*/
	_waiterInit = function() {
		//примитивная локализация
		var text;
		if ($_Core.lang() == "ru-Ru") text = "Загрузка...";
		else text = "Loading...";
		//создаем блок с контентом индикатора
		var d = $.ce("DIV");
		d.className = $_name + "-loader " + $_name + "-loading";
		d.innerHTML = text;
		//создаем попап
		$_waiter.inst = $_Lightbox.create({
			btns: false,
			head: false,
			height: 0,
			width: 0,
			content: d,
			onOk: function() {
				alert("Soon.");
			}
		});
	},

	/**
	* Показывает индикатор загрузки
	*/
	_waiterShow = function() {
		if (!$_waiter.inited) {
			//инициализируем индикатор загрузки/ожидания
			_waiterInit();
		}
		if (!$_waiter.inst) {
			$_Core.console(__name_script + " > " + $_name + "#_waiterShow(): Модуль всплывающего (модального) окна [Lightbox] отсутствует или не инициализирован.");
			return;
		}
		$_waiter.inst.show();
		$_waiter.visible = true;
	};
	/* --------------------------------- */

	/* -------- публичные методы ------- */

	this.actionSend = _action;

	/**
	* Возвращает в ядро список зависимостей
	*/
	this.deps = function() {
		return $_deps.slice();
	};

	/**
	* Инициализация
	*/
	this.init = function() {
		return __init.apply(null, arguments);
	};

	/**
	* Функция расширяет указанный объект
	* на определенные реестром набор своих методов
	*/
	this.export_ = function(obj) {
		return _export.apply(null, [obj]);
	};
	/* --------------------------------- */
};

//регистрируем глобально
window.FlexClientRender = new render();

})();

(function(){

var $,
	$_Core = null,
	$_Lib = null,
	$_Render = null,
	__name_this = "lightbox",
	__name_script = "render.js";

var lightbox = function() {

	/* ------- приватные свойства ------ */
	var $_deps			=	[
			"Core",
			"Lib",
			"Render"
		],
		$_export		=	[
			"create"
		],
		$_name			=	__name_this;
	/* --------------------------------- */


	/* -------- приватные методы ------- */

	var __init = function(cfg, _$, deps) {

		//ссылки
		$ = _$;

		//зависимости
		var l = $_deps.length,
			name;
		for (; l--;) {
			name = $_deps[l];
			if (!deps[name]) {
				console.log(__name_script + " > " + $_name + "#_init(): Dependency not found [" + name + "]!");
				return false;
			}
			try {
				eval("$_" + name + " = deps." + name + ";");
			} catch(e){
				console.log(__name_script + " > " + $_name + "#_init(): Error while creating the local link to dependency [$_" + name + "]!");
				return false;
			}
		}

		return true;
	},

	/**
	* Создание нового лайтбокса
	*
	* @param props
	*/
	_create = function(props, show) {
		return new $_Lightbox(props, show);
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
		}
		return obj;
	};

	/* -------- публичные методы ------- */

	/**
	* Возвращает в ядро список зависимостей
	*/
	this.deps = function() {
		return $_deps.slice();
	};

	/**
	* Инициализация
	*/
	this.init = function() {
		return __init.apply(null, arguments);
	};

	/**
	* Функция расширяет указанный объект
	* на определенные реестром набор своих методов
	*/
	this.export_ = function(obj) {
		return _export.apply(null, [obj]);
	};

	/* --------------------------------- */
};

//регистрируем глобально
window.FlexClientLightbox = new lightbox();

var $_Lightbox = function(props){

	$_Lightbox.instNum++;

	/* ------- приватные свойства ------ */
	var $_DOM = {
			body: null,
			item: null,
			main: null
		},
		$_inst = {
			_num: 0,
			_zi: 0,
			bodyPad: 10,
			btns: {
				close: {
					caption: "Закрыть",
				}
			},
			btnHt: 30,
			btnPad: 5,
			content: "",
			head: true,
			headHt: 40,
			height: 0,//auto
			onOk: null,//OK click callback
			shade: true,
			title: "Lightbox",
			width: 0,//auto
			visible: false
		},
		$_name = "",
		$_ziStart = 2147473647;
	/* --------------------------------- */


	/* -------- приватные методы ------- */

	/**
	* Создает новый лайтбокс
	*
	* @param {Object} props
	*
	* @returns {Object}
	*/
	var __init = function(props, show) {
		if (props && typeof props == "object")
		for (var c in $_inst) {
			if (!$_inst.hasOwnProperty(c)) continue;
		 	if (typeof props[c] != "undefined") $_inst[c] = props[c];
		}
		if (typeof show != "boolean") show = true;
		$_inst._num = $_Lightbox.instNum;
		$_inst._zi = $_ziStart + $_inst._num;
		$_name = __name_this + $_inst._num;
		if (show) _show();
		return {
			close: function() {
				_onClickClose();
			},
			hide: function() {
				_hide();
			},
			show: function() {
				_show();
			},
			update: function(content) {
				$_DOM.body.innerHTML = "";
				if ((typeof content == "object") && (typeof content.nodeType != "undefined")) $_DOM.body.appendChild(content);
				else $_DOM.body.innerHTML = content;
			}
		};
	},

	_hide = function(inst) {
		$_inst.visible = false;
		$_DOM.item.style.display = "none";
	},

	_onClickClose = function() {
		_hide();
		$_Lightbox.main.removeChild($_DOM.main);
	},

	_onClickOk = function() {
		var res = true;
		if (typeof $_inst.onOk == "function") {
			try {
				res = $_inst.onOk();
				if (typeof res != "boolean") res = true;
			} catch(e) {
				$_Render.console(__name_script + " > " + $_name + "#_onClickOk() > Callback execution error [" + e.message + "]!");
				alert(e.message);
			}
		}
		if (!res) return;
		_hide();
		$_Lightbox.main.removeChild($_DOM.main);
	},

	_render = function() {
		var i = $_inst;
		//main wrapper
		$_DOM.main = $.ce("DIV", {className: "lightbox"});
		//other HTML
		var item = $.ce("DIV", {className: "lb-item"});
		item.style.display = (i.visible ? "block" : "none");
		item.style.zIndex = i._zi;
		$_DOM.main.appendChild(item);
		$_DOM.item = item;
		//
		var shad = $.ce("DIV", {className: "lb-shad"});
		shad.style.display = (i.shade ? "block" : "none");
		item.appendChild(shad);
		//
		var msg = $.ce("DIV", {className: "lb-msg"});
		item.appendChild(msg);
		//
		var inner = $.ce("DIV", {className: "lb-inner"});
		msg.appendChild(inner);
		//
		var el = $.ce("DIV", {className: "lb-valign"});
		inner.appendChild(el);

		//размеры
		var hh = i.head ? (i.headHt + 1) : 0,
			bsh = i.btns ? (i.btnHt + i.btnPad * 2) : 0,
			ch = (i.height && (hh || bsh)) ? (i.height + hh + bsh + (i.bodyPad * 2)) : 0;

		//
		var cont = $.ce("DIV", {className: "lb-cont"});
		if (ch) cont.style.height = ch + "px";
		//в ширине поддерживаются %
		if (typeof i.width != "boolean") {
			cont.style.width = i.width + (typeof i.width == "string" ? "" : "px");
		} else {
			if (i.width) cont.style.width = "100%";
		}
		inner.appendChild(cont);
		//
		if (i.head) {
			var head = $.ce("DIV", {className: "lb-head"});
			head.style.height = i.headHt + "px";
			head.style.lineHeight = i.headHt + "px";
			if (i.title && (typeof i.title.nodeType != "undefined")) {
				head.appendChild(i.title);
			} else {
				head.innerHTML = i.title;
			}
			cont.appendChild(head);
		}
		//
		var body = $.ce("DIV", {className: "lb-body"});
		if (i.height) body.style.height = i.height + "px";
		body.style.paddingTop = i.bodyPad + "px";
		body.style.paddingBottom = i.bodyPad + "px";
		if ((typeof i.content == "object") && (typeof i.content.nodeType != "undefined")) body.appendChild(i.content);
		else body.innerHTML = i.content;
		cont.appendChild(body);
		$_DOM.body = body;
		//
		if (i.btns) {
			var btns = $.ce("DIV", {className: "lb-btns"});
			btns.style.display = (i.btns.close || i.btns.ok) ? "block" : "none";
			btns.style.bottom = i.btnPad + "px";
			btns.style.height = i.btnHt + "px";
			btns.style.paddingTop = i.btnPad + "px";
			cont.appendChild(btns);
			//
			var btn;
			if (i.btns.ok) {
				btn = $.ce("DIV", {className: "lb-btn"});
				//2 * 1 пикс - границы, 2 * 3 пикс - верт. паддинг внутри кнопки
				btn.style.height = (i.btnHt - 2 - 2 * 3) + "px",
				btn.style.lineHeight = (i.btnHt - 2 - 2 * 3) + "px";
				btn.innerHTML = i.btns.ok.title ? i.btns.ok.title : "OK";
				btns.appendChild(btn);
				$_DOM.btnOk = btn;
				$_Render.evWinAdd(btn, "click",_onClickOk);
			}
			//
			if (i.btns.close) {
				btn = $.ce("DIV", {className: "lb-btn"});
				//2 * 1 пикс - границы, 2 * 3 пикс - верт. паддинг внутри кнопки
				btn.style.height = (i.btnHt - 2 - 2 * 3) + "px",
				btn.style.lineHeight = (i.btnHt - 2 - 2 * 3) + "px";
				btn.innerHTML = i.btns.close.title ? i.btns.close.title : "Cancel";
				btns.appendChild(btn);
				$_DOM.btnCancel = btn;
				$_Render.evWinAdd(btn, "click", _onClickClose);
			}
		}
		//
		$_Lightbox.main.appendChild($_DOM.main);
	},

	_show = function(inst) {
		if (!$_DOM.main) _render();
		$_inst.visible = true;
		$_DOM.item.style.display = "block";
	};

	if ($_Lightbox.instNum == 1) {
		$_Render.cssInsert($_Lightbox.styles);
		$_Lightbox.main = $.ce("DIV", {className: "lightbox-wrap"});
		$.b.appendChild($_Lightbox.main);
	}
	//
	return __init(props);
};
$_Lightbox.insts = [];
$_Lightbox.instNum = 0;
$_Lightbox.styles = [".lightbox {\
	position:absolute;\
	z-index:2147473647;/*max - 2147483647*/\
}",
".lightbox .lb-item {\
	position:absolute;\
	left:0;\
	top:0;\
	width:0;\
	height:0;\
}",
".lightbox .lb-item .lb-shad {\
	position:fixed;\
	left:0;\
	top:0;\
	right:0;\
	bottom:0;\
	background-color:#bcbcbc;\
	opacity:0.3;\
	filter: progid:DXImageTransform.Microsoft.gradient(startColorstr=#4cbcbcbc,endColorstr=#4cbcbcbc);\
	zoom:1;\
	z-index:1;\
}",
".lightbox .lb-item .lb-msg {\
	position:fixed;\
	left:0;\
	top:0;\
	right:0;\
	bottom:0;\
	z-index:2;\
}",
".lightbox .lb-item .lb-msg .lb-inner {\
	height:100%;\
	text-align:center;\
}",
".lightbox .lb-item .lb-msg .lb-inner .lb-valign {\
	display:inline-block;\
	width:0;\
	height:100%;\
	vertical-align:middle;\
	oveflow:hidden;\
}",
".lightbox .lb-item .lb-msg .lb-inner .lb-cont {\
	display:inline-block;\
	background-color:#fff;\
	border-radius:5px;\
	vertical-align:middle;\
	overflow:hidden;\
	box-shadow: 0 0 10px rgba(0,0,0,0.6);\
}",
".lightbox .lb-inner .lb-cont .lb-head {\
	padding:0 5px;\
	background-color:#efeff2;\
	border-bottom:1px solid #c8d2e0;\
	font-size:14px;\
	font-weight: bold;\
	text-align:left;\
	oveflow:hidden;\
}",
".lightbox .lb-inner .lb-cont .lb-body {\
	padding:0 5px;\
	font-size:14px;\
	text-align:left;\
}",
".lightbox .lb-inner .lb-cont .lb-btns {\
	position:relative;\
	padding:0 5px;\
	font-size:14px;\
	text-align:center;\
	oveflow:hidden;\
	z-index:100;\
}",
".lightbox .lb-inner .lb-cont .lb-btns .lb-btn {\
	display:inline-block;\
	padding:3px 8px;\
	font-size:12px;\
	background-color:#fff;\
	border:1px solid #bcbcbc;\
	border-radius:5px;\
	vertial-align:middle;\
	cursor:pointer;\
}",
".lightbox .lb-inner .lb-cont .lb-btns .lb-btn:hover {\
	background-color:#efeff2;\
}",
".lightbox .lb-inner .lb-cont .lb-btns.more .lb-btn:first-child {\
	margin-right:10px;\
}"];

})();