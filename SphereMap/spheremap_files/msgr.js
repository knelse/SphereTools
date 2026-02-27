/**
 * @module FlexClient/msgr
 * @author Bogdan Nazar [me@bogdan-nazar.ru]
 * @version 3.2.0 (15.12.2016 18:05 +0400)
 * @description Расширение по управлению сообщениями системы Msgr
 * @copyright 2003-2016 Bogdan Nazar
 * @license MIT & FlexEngine License
 * 		http://www.opensource.org/licenses/mit-license.php
 * 		http://www.flexengin.ru/docs/license
 */

/**
 * Примеры и документация: http://www.flexengin.ru/docs/client/core/msgr-js
 *
 * Требования: FlexClient Core 3.2.0+
 */

(function(){

var $,
	__name_this = "msgr",
	__name_script = __name_this + ".js";

//ищем FlexClient
if (typeof window.FlexClient != "function") {
	console.log(__name_script + " > Client application is not found.");
	return;
}

/**
* @class msgr
*/
var msgr = function() {

	/* ------- приватные свойства ------ */
	var $_alerts		=	[],
		$_cfg			=	{},
		$_deps			=	[
			"Core",
			"Lib",
			"Lightbox",
			"Render"
		],
		$_elems			=	{
			srvMsgs:		{
				id:			"",
				elem:		null
			}
		},
		$_export		=	[
			"dlgAlert",
			"dlgConfirm"
		],
		$_name			=	__name_this,
		$_srvMsgs		=	{
			ival:			300,
			ivalObj:		null
		},

		//расширения
		$_Core			=	null,
		$_Lightbox		=	null,
		$_Lib			=	null,
		$_Render		=	null;
	/* --------------------------------- */


	/* -------- приватные методы ------- */

	var __init = function(cfg, _$, deps) {
		//конфиг
		if ((typeof cfg == "object") && cfg) $_cfg = cfg;
		if (!cfg.msgs) cfg.msgs = {};

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
				console.log(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
				return false;
			}
		}

		//ставим таймер на инициализацию, которая должна
		//начаться при появлении определенного тега (например, id="core-init-ready")
		//в верстке страницы
		$_elems.srvMsgs.id = $_name + "-items";
		$_srvMsgs.ivalObj = $.w.setInterval(function(){
			var el = $.ge($_elems.srvMsgs.id);
			if (el) {
				$_elems.srvMsgs.elem = el;
				$.w.clearInterval($_srvMsgs.ivalObj);
				_itemsShow();
			}
		}, $_srvMsgs.ival);

		return true;
	},

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
				console.log(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
			}
		}
		return obj;
	},

	_dialog = function(msg, pars) {
		var el = null;
		if ((typeof msg != "string") && (typeof msg.nodeType == "undefined")) {
			var err = false;
			if (_isDialog(msg)) {
				if ($_alerts[msg.id].lb) {
					if ($_alerts[msg.id].lb.show()) return msg;
				} else {
					$_Core.console(__name_script + " > " + $_name + "#_dialog(): Can't re-use the modal box from the stach [$_alerts]: not found.");
				}
			} else {
				$_Core.console(__name_script + " > " + $_name + "#_dialog(): Can't create the modal box: message type is unsupported [" + (typeof msg) + "].");
				$_Core.console(msg);
			}
			return false;
		} else {
			if (typeof msg == "string") {
				el = $.ce("DIV", {
					className: "mab-body",
					innerHTML: msg
				});
			} else {
				el = msg;
				el.className = (el.className ? el.className.concat(" ") : "").concat("ab-body");
			}
		}
		//параметры по-умолчанию
		var params = {
			alertType: "inf",
			buttons: {
				action: {
					caption: $_cfg.msgs.btnCapConfirm || "Подтвердить",
					cb: null
				},
				close: {
					caption: $_cfg.msgs.btnCapCancel || "Отмена",
					cb: null
				}
			},
			title: false,
			type: "alert",
			useHide: false,
			width: false//auto
		};
		//перебираем все параметры и обновлем дефолты
		for (var c in pars) {
			if (!pars.hasOwnProperty(c)) continue;
			if (typeof params[c] == "undefined") continue;
			params[c] = pars[c];
		}
		//строим DOM
		var m = $.ce("DIV", {
			className: $_name
		});
		//заголовок
		var title = $.ce("DIV", {className: $_name}),
			title_str;
		var el1 = $.ce("DIV", {
			className: "alert-box"
		});
		title.appendChild(el1);
		switch(params.type) {
			case "alert":
				if (params.title === false) {
					switch (params.alertType) {
						case "err":
							title_str = $_cfg.msgs.dlgAlertTitleErr || "Ошибка";
							break;
						case "wrn":
							title_str = $_cfg.msgs.dlgAlertTitleWrn || "Предупреждение";
							break;
						default:
							title_str = $_cfg.msgs.dlgAlertTitleInf || "Информация";
							break;
					}
				} else {
					title_str = params.title;
				}
				break;
			case "confirm":
				if (params.title === false) {
					title_str = $_cfg.msgs.dlgConfirmTitleInf || "Подтвердите действие";
				} else {
					title_str = params.title;
				}
				break;
		}
		//заголовок и CSS-класс для заголовка
		var titleClass = "";
		switch(params.type) {
			case "alert":
				titleClass = params.alertType
				break;
			case "confirm":
				titleClass = "confirm";
				break;
		}
		el1.appendChild($.ce("DIV", {
			className: "mab-title" + (titleClass ? (" mab-" + titleClass) : ""),
			innerHTML: title_str
		}));
		//основной контейнер
		el1 = $.ce("DIV", {
			className: "alert-box"
		});
		m.appendChild(el1);
		el1.appendChild(el);
		//кнопки
		var el2;
		if (params.buttons && (params.buttons.action || params.buttons.close)) {
			el2 = $.ce("DIV", {
				className: "mab-buttons"
			});
			if (params.buttons.action) {
				params.buttons.action.btn = $.ce("DIV", {
					className: "mab-btn mab-act",
					innerHTML: params.buttons.action.caption
				});
				el2.appendChild(params.buttons.action.btn);
			}
			if (params.buttons.close) {
				params.buttons.close.btn = $.ce("DIV", {
					className: "mab-btn mab-cl",
					innerHTML: params.buttons.close.caption
				});
				el2.appendChild(params.buttons.close.btn);
			}
			el1.appendChild(el2);
		}
		//создаем объект диалога
		var obj = {
			id: $_alerts.length,
			params: params,
			lb: null,
			lbparams: {
				btns: null,
				content: m,
				title: title,
				width: params.width
			}
		};
		//создаем лайтбокс
		obj.lb = $_Lightbox.create(obj.lbparams, false);
		//обработчики кликов
		if (el2) {
			if (params.buttons.action) {
				$_Render.evWinAdd(params.buttons.action.btn, "click", function(){
					if (typeof params.buttons.action.cb == "function") {
						try {
							params.buttons.action.cb();
						} catch(e) {
							$_Core.console(__name_script + " > " + $_name + "#_dialog(): Callback execution error while processing the lightbox action.");
							$_Core.console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
						}
					}
					if (params.useHide) obj.lb.hide();
					else obj.lb.close();
				});
			}
			if (params.buttons.close) {
				$_Render.evWinAdd(params.buttons.close.btn, "click", function(){
					if (typeof params.buttons.close.cb == "function") {
						try {
							params.buttons.close.cb();
						} catch(e) {
							$_Core.console(__name_script + " > " + $_name + "#_dialog(): Callback execution error while closing the lightbox.");
							$_Core.console(__name_script + " > " + $_name + "#JS_Error: [" + e.message + "].");
						}
					}
					if (params.useHide) obj.lb.hide();
					else obj.lb.close();
				});
			}
		}
		$_alerts.push(obj);
		return obj;
	},

	_dlgAlert = function(msg, type, pars) {
		if (_isDialog(msg)) {
			return _dialog(msg);
		}
		//проверяем тип алерта
		if (typeof type != "string") type = "inf";
		else {
			if ((type != "inf") && (type != "wrn") && (type != "err")) type = "inf";
		}
		//проверяем pars, если pars не является
		//объектом, то трактуем его как width
		if (typeof pars != "object") {
			var wd = parseInt(pars, 10);
			if (isNaN(wd)) {
				if (typeof pars != "undefined") wd = 300;
				else wd = false;//(автоопределение)
			} else {
				if (par.indexOf("%") != -1) wd = wd + "%";
			}
			pars = {
				width: wd
			};
		}
		//параметры по-умолчанию для алерта
		var params = {
			buttons: {
				action: false,
				close: {
					caption: $_cfg.msgs.btnCapClose || "Закрыть",
					cb: null
				}
			},
			title: false,
			useHide: false,
			width: false
		};
		//проверяем все входящие параметры
		for (var c in pars) {
			if (!pars.hasOwnProperty(c)) continue;
			switch(c) {
				case "btnCaption":
					if (typeof pars.btnCaption == "string")
						params.buttons.close.caption = pars.btnCaption;
					break;
				case "title":
					if (typeof pars.title == "string")
						params.title = pars.title;
					break;
				case "useHide":
					if (typeof pars.useHide == "boolean")
						params.useHide = pars.useHide;
					break;
				case "width":
					params.width = pars.width;
					break;
			}
		}
		params.alertType = type;
		params.type = "alert";
		//создаем
		return _dialog(msg, params);
	},

	_dlgConfirm = function(msg, pars, cb) {
		//проверяем pars, если pars не является
		//объектом, то трактуем его как width
		if (typeof pars != "object") {
			if (typeof pars == "function") {
				cb = pars;
				pars = {};
			} else {
				var wd = parseInt(pars, 10);
				if (isNaN(wd)) {
					if (typeof pars != "undefined") wd = 300;
					else wd = false;//(автоопределение)
				} else {
					if (par.indexOf("%") != -1) wd = wd + "%";
				}
				pars = {
					width: wd
				};
			}
		}
		if (typeof cb != "function") cb = null;
		if (_isDialog(msg)) {
			//обновляем только callback-функцию (если была передана еще раз)
			if (cb) msg.params.buttons.action.cb = cb;
			return _dialog(msg);
		}
		//параметры по-умолчанию для алерта
		var params = {
			buttons: {
				action: {
					caption: $_cfg.msgs.btnCapConfirm || "Подтвердить",
					cb: cb
				},
				close: {
					caption: $_cfg.msgs.btnCapCancel || "Отмена",
					cb: null
				}
			},
			title: false,
			useHide: false,
			width: false
		};
		//проверяем все входящие параметры
		for (var c in pars) {
			if (!pars.hasOwnProperty(c)) continue;
			switch(c) {
				case "btnAction":
					if (typeof pars.btnAction == "string")
						params.buttons.action.caption = pars.btnAction;
					break;
				case "btnClose":
					if (typeof pars.btnClose == "string")
						params.buttons.close.caption = pars.btnClose;
					break;
				case "title":
					if (typeof pars.title == "string")
						params.title = pars.title;
					break;
				case "useHide":
					if (typeof pars.useHide == "boolean")
						params.useHide = pars.useHide;
					break;
				case "width":
					params.width = pars.width;
					break;
			}
		}
		params.type = "confirm";
		//создаем
		return _dialog(msg, params);
	},

	_isDialog = function(obj) {
		return (typeof obj == "object") &&
			(typeof obj.id == "number") &&
			(typeof $_alerts[obj.id] != "undefined");
	},

	_itemsShow = function() {
		var c = 0,
			cnt = 0,
			el = $_elems.srvMsgs.elem,
			items = [],
			l = el.childNodes.length,
			node;
		for (; c < l; c++) {
			node = el.childNodes[c];
			if (node.nodeName && (node.nodeName.toLowerCase() == "div")) {
				if (node.id && node.id.indexOf($_name + "-item-") != -1) {
					items.push(node);
					cnt++;
				}
			}
		}
		if (!cnt) return;
		var msg,
			type;
		c = 0;
		for (; c < cnt; c++) {
			msg = items[c];
			type = msg.getAttribute("data-msgtype");
			if (msg.getAttribute("data-showtype") == "console") {
				switch(type) {
					case "wrn":
						type = "Warning";
						break;
					case "err":
						type = "Error";
						break;
					default:
						type = "Information";
						break;
				}
				$_Core.console(__name_script + " > " + $_name + "#_itemsShow(): " + type + " > " + msg.innerHTML);
			} else
			_dlgAlert(msg.innerHTML, type, {
				title: msg.getAttribute("data-title")
			});
		}
	};
	/* --------------------------------- */

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
window.FlexClientMsgr = new msgr();

})();