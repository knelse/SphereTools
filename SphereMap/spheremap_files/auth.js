/**
 * @module FlexClient/auth
 * @author Bogdan Nazar [me@bogdan-nazar.ru]
 * @version 3.2.0 (15.12.2016 17:30 +0400)
 * @description Расширение по управлению авторизацией Auth
 * @copyright 2003-2016 Bogdan Nazar
 * @license MIT & FlexEngine License
 * 		http://www.opensource.org/licenses/mit-license.php
 * 		http://www.flexengin.ru/docs/license
 */

/**
 * Примеры и документация: http://www.flexengin.ru/docs/client/core/auth-js
 *
 * Требования: FlexClient Core 3.2.0+
 */

(function(){

var $,
	__name_this = "auth",
	__name_script = __name_this + ".js";

//ищем FlexClient
if (typeof window.FlexClient != "function") {
	console.log(__name_script + " > Client application is not found.");
	return;
}

/**
* @class auth
*/
var auth = function() {

	/* ------- приватные свойства ------ */
	var $_deps			=	[
			"Core",
			"Lib",
			"Lightbox",
			"Render"
		],
		$_elems			=	{
			loginName:		{
				elem:		null
			},
			loginPass:		{
				elem:		null
			},
		},
		$_export		=	[
		],
		$_name			=	__name_this,

		//расширения
		$_Core			=	null,
		$_Lightbox		=	null,
		$_Lib			=	null,
		$_Render		=	null;
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

	_login = function() {
		var l = $.ge($_name + "-login-name"),
			p = $.ge($_name + "-login-pass");
		if (!l || !p) {
			alert("Действие невозможно: ошибка набора элементов страницы!");
			return false;
		}
		if (!l.value || (!p.value)) {
			alert("Все поля обязательны к заполнению!");
			return false;
		}
		$_Core.action($_name + "-login");
	},

	_logoff = function() {
		$_Core.action($_name + "-logoff");
	},

	_register = function() {
		var l = $.ge($_name + "-reg-name"),
			p = $.ge($_name + "-reg-pass"),
			p2 = $.ge($_name + "-reg-pass2"),
			e = $.ge($_name + "-reg-email"),
			d = $.ge($_name + "-reg-display");
		if (!l || !p || !p2 || !e || !d) {
			alert("Действие невозможно: ошибка набора элементов страницы!");
			return false;
		}
		if (!l.value) {
			alert("Введите логин!");
			l.focus();
			return false;
		}
		if (l.value.length < 4) {
			alert("Длина логина должна быть не менее 4-х символов!");
			l.focus();
			return false;
		}
		if (!p.value) {
			alert("Введите пароль!");
			p.focus();
			return false;
		}
		if (p.value.length < 6) {
			alert("Длина пароля должна быть не менее 6-ти символов!");
			p.focus();
			return false;
		}
		if (!p2.value || (p.value != p2.value)) {
			alert("Введенные пароли не совпадают!");
			if (!p2.value) p2.focus();
			else p.focus();
			return false;
		}
		if (!d.value) {
			alert("Укажите свое имя (как к вам обращаться)!");
			d.focus();
			return false;
		}
		if (!e.value) {
			alert("Введите корректный e-mail! В случае утери пароля, новый будет выслан вам по электронной почте.");
			e.focus();
			return false;
		}
		$_Core.action($_name + "-register");
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
window.FlexClientAuth = new auth();

})();