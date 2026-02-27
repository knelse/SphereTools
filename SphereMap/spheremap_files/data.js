var _map_mmoclub = function() {
	this._dirBase		=	"/data/_content/" + pageId + "/";
	this._dirImages		=	"images";
	this._dirScripts	=	"scripts";
	this._pus			=	[];
	this._pusAct		=	[];
	this.plPu			=	null;
};
_map_mmoclub.prototype.close = function() {
	if (!this._pusAct.length) return;
	var pu = this._pusAct[this._pusAct.length - 1];
	this.plPu.hide(pu);
	return true;
};
_map_mmoclub.prototype.onClose = function() {
	this._pusAct.splice(this._pusAct.length - 1, 1);
};
_map_mmoclub.prototype.open = function(el) {
	var city = el.id.replace("btn-map-opener-", "");
	if ((typeof this._pus[city] == "undefined") || (this._pus[city] == -1)) {
		if (typeof core == "undefined") return;
		if (!this.plPu) {
			this.plPu = core.pluginGet("popup");
			if (!this.plPu) return;
		}
		var d = document.createElement("DIV");
		this._pus[city] = this.plPu.add({content: d, windowed: true, showcloser: true, onclose: this.onClose.bind(this)});
		if (this._pus[city] == -1) return;
		var f = document.createElement("IFRAME");
		f.scrolling = "no";
		f.style.border = "0";
		f.style.width = "576px";
		f.style.height = "576px";
		d.appendChild(f);
		f.src = this._dirBase + city + ".htm";
	}
	this.plPu.show(this._pus[city]);
	this._pusAct.push(this._pus[city]);
};
var map_mmoclub = new _map_mmoclub();
/* ---------------------------------------------------------------------*/
function lib_bwcheck() { //Browsercheck (needed)
	this.ver = navigator.appVersion;
	this.agent = navigator.userAgent;
	this.dom = document.getElementById ? 1 : 0;
	this.opera5 = this.agent.indexOf("Opera 5") > -1;
	this.ie5 = (this.ver.indexOf("MSIE 5")>-1 && this.dom && !this.opera5) ? 1 : 0;
	this.ie6 = (this.ver.indexOf("MSIE 6")>-1 && this.dom && !this.opera5) ? 1 : 0;
	this.ie4 = (document.all && !this.dom && !this.opera5) ? 1 : 0;
	this.ie = this.ie4 || this.ie5 || this.ie6;
	this.mac = this.agent.indexOf("Mac")>-1;
	this.ns6 = (this.dom && parseInt(this.ver) >= 5) ? 1 : 0;
	this.ns4 = (document.layers && !this.dom) ? 1 : 0;
	this.bw = (this.ie6 || this.ie5 || this.ie4 || this.ns4 || this.ns6 || this.opera5);
	return this;
}
var bw = new lib_bwcheck();
fromX = 2;
fromY = 2;
function makeObj(obj) {
   	this.evnt = bw.dom ? document.getElementById(obj) : bw.ie4?document.all[obj] : bw.ns4?document.layers[obj] : 0;
	if (!this.evnt) return false;
	this.css = bw.dom || bw.ie4 ? this.evnt.style : bw.ns4 ? this.evnt : 0;
   	this.wref = bw.dom || bw.ie4 ? this.evnt : bw.ns4 ? this.css.document : 0;
	this.writeIt = b_writeIt;
	return this;
}
var px = bw.ns4 || (window.opera ? "" : "px");
function b_writeIt(text) {
	if (bw.ns4) {
		this.wref.write(text);
		this.wref.close();
	} else this.wref.innerHTML = text;
}
//Capturing mousemove
var descx = 0;
var descy = 0;
function popmousemove(e){descx=bw.ns4||bw.ns6?e.pageX:event.x; descy=bw.ns4||bw.ns6?e.pageY:event.y};
var oDesc;
function popup(num){
	console.log(num);
	console.log(oDesc);
    if (oDesc) {
		oDesc.writeIt('<div class="clDescription">' + messages[num] + '</div>');
		if (bw.ie5 || bw.ie6) descy = descy + document.body.scrollTop;
		oDesc.css.left = (descx + fromX - 440) + "px";
		oDesc.css.top = (descy + fromY - 275) + "px";
		oDesc.css.visibility = "visible";
    }
}
function popout(){
	if (oDesc) oDesc.css.visibility = "hidden";
}
function setPopup() {
   	if (bw.ns4) document.captureEvents(Event.MOUSEMOVE);
    document.onmousemove = popmousemove;
	oDesc = new makeObj('divDescription');
}


//==========================================
// Set up
//==========================================
var uagent    = navigator.userAgent.toLowerCase();
var is_safari = ((uagent.indexOf('safari') != -1) || (navigator.vendor == "Apple Computer, Inc."));
var is_ie     = ((uagent.indexOf('msie') != -1) && (!is_opera) && (!is_safari) && (!is_webtv));
var is_ie4    = ((is_ie) && (uagent.indexOf("msie 4.") != -1));
var is_moz    = (navigator.product == 'Gecko');
var is_ns     = ((uagent.indexOf('compatible') == -1) && (uagent.indexOf('mozilla') != -1) && (!is_opera) && (!is_webtv) && (!is_safari));
var is_ns4    = ((is_ns) && (parseInt(navigator.appVersion) == 4));
var is_opera  = (uagent.indexOf('opera') != -1);
var is_kon    = (uagent.indexOf('konqueror') != -1);
var is_webtv  = (uagent.indexOf('webtv') != -1);
var is_win    = ((uagent.indexOf("win") != -1) || (uagent.indexOf("16bit") !=- 1));
var is_mac    = ((uagent.indexOf("mac") != -1) || (navigator.vendor == "Apple Computer, Inc."));
var ua_vers   = parseInt(navigator.appVersion);
//==========================================
// Get element by id
//==========================================
function my_getbyid(id) {
	var itm = null;
	if (document.getElementById) {
		itm = document.getElementById(id);
	} else if (document.all) {
		itm = document.all[id];
	} else if (document.layers) {
		itm = document.layers[id];
	}
	return itm;
};
//==========================================
// Show/hide toggle
//==========================================
function toggleview(id) {
	if (!id) return;
	var itm;
	if (itm = my_getbyid(id)) {
		if (itm.style.display == "none") {
			my_show_div(itm);
		} else {
			my_hide_div(itm);
		}
	}
};
//==========================================
// Set DIV ID to hide
//==========================================
function my_hide_div(itm) {
	if (!itm) return;
	itm.style.display = "none";
};
//==========================================
// Set DIV ID to show
//==========================================
function my_show_div(itm) {
	if (!itm) return;
	itm.style.display = "";
};
function my_show_grnz(itm, sour) {
	if (!itm) return;
	itm.src = "/data/_content/" + pageId + "/images/" + sour;
};
//==========================================
// Toggle category
//==========================================
function togglecategory(fid, add) {
	saved = new Array();
	clean = new Array();
	//-----------------------------------
	// Get any saved info
	//-----------------------------------
	//-----------------------------------
	// Remove bit if exists
	//-----------------------------------
	for (i = 0; i < saved.length; i++) {
		if ( saved[i] != fid && saved[i] != "" ) {
			clean[clean.length] = saved[i];
		}
	}
	//-----------------------------------
	// Add?
	//-----------------------------------
	if (add) {
		clean[ clean.length ] = fid;
		my_show_div(my_getbyid('fc_' + fid));
		my_hide_div(my_getbyid('fo_' + fid));
	} else {
		my_show_div( my_getbyid('fo_'+fid));
		my_hide_div( my_getbyid('fc_' + fid));
	}
};
function opengrnz(fid, typ) {
	saved = new Array();
	clean = new Array();
	//-----------------------------------
	// Get any saved info
	//-----------------------------------
	//-----------------------------------
	// Remove bit if exists
	//-----------------------------------
	for (i = 0; i < saved.length; i++) {
		if ( saved[i] != fid && saved[i] != "" ) {
			clean[clean.length] = saved[i];
		}
	}
	//-----------------------------------
	// Add?
	//-----------------------------------
	clean[ clean.length ] = fid;
	my_show_div( my_getbyid('fc_'+fid));
	my_hide_div( my_getbyid('fo_'+fid));
	if (typ) {
		my_show_grnz(my_getbyid('grn_'+fid), "grnz.gif");
	} else {
		var browserName = navigator.appName;
		if (browserName == "Microsoft Internet Explorer") {
			my_getbyid( 'grn_'+fid  ).style.filter = "Alpha(Opacity=52)";
			my_show_grnz(my_getbyid( 'grn_'+fid  ) , "castle.gif");
		} else {
			my_show_grnz(my_getbyid( 'grn_'+fid  ) , "castle.png");
		}
	}
};
function PopUp(url, name, width, height, center, resize, scroll, posleft, postop) {
	var showx = "";
	var showy = "";
	var X, Y;
	if (posleft != 0) { X = posleft }
	if (postop  != 0) { Y = postop  }
	if (!scroll) { scroll = 1 }
	if (!resize) { resize = 1 }
	if ((parseInt (navigator.appVersion) >= 4 ) && (center)) {
		X = (screen.width  - width ) / 2;
		Y = (screen.height - height) / 2;
	}
	if ( X > 0 ) {
		showx = ',left=' + X;
	}
	if ( Y > 0 ) {
		showy = ',top=' + Y;
	}
	if (scroll != 0) { scroll = 1 }
	var Win = window.open(url, name, 'width=' + width + ',height=' + height + showx + showy + ',resizable=' + resize + ',scrollbars=' + scroll + ',location=no,directories=no,status=no,menubar=no,toolbar=no');
};

var messages = [];
messages[100] = "Торговая площадь";
messages[944] = "Общий данж";
messages[945] = "Квестовый данж";
messages[946] = "Заброшенные дома";
messages[947] = "Мельница";
messages[948] = "Кладбище";
messages[949] = "Пирамиды";
messages[950] = "Кратер";
// Таверны
messages[001] = "Таверна Джелай Бабуно (номера, еда) Ромул Канибус (квесты на титул)";
messages[002] = "Таверна Джон Рамино (номера, еда) Аллан Мамнок (квесты на степень)";
messages[003] = "Таверна Роджер Хаггар (номера, еда, снаряжение: 7-9)";
messages[004] = "Таверна Лайон Пордел (номера, еда) Рамон Ганульо (оружие: 5-6) Донат Хьюберт (броня: 5-6) Джером Лежар (квесты на титул)";
messages[005] = "Таверна Майкл Бинер (номера, еда) Тимоти Фолаун (оружие: 5-6) Элгар Паппер (броня: 5-6) Рейнольд Рейган (квесты на степень)";
messages[006] = "Таверна Бенджамин Симпсон (номера, еда) Сеймур Хеффер (оружие: 5-6) Грифин Той (броня: 5-6) Годфрид Сеймак (квесты на степень)";
messages[007] = "Таверна Скотт Фугас (номера, еда) Рональд Буш (оружие: 5-6) Эмиль Фьючер (броня: 5-6) Джабраил Даматакар (квесты на степень)";
messages[008] = "Таверна Джеки Файн (номера, еда) Кельвин Биг (квесты на титул)";
messages[009] = "Таверна Боб Бонбораус (номера, еда) Рекс Скаут (квесты на степень)";
// Торговцы оружием, броней
messages[101] = "Торговцы Марсель Дарсо (оружие: 7-8) Шон Пен (броня: 7-8)";
messages[102] = "Торговцы Бенедикт Кельвин (оружие: 9-10) Лукас Арчер (броня: 7-8)";
messages[103] = "Торговцы Боб Дугинос (оружие: 7-8) Нед Санд (броня: 7-8)";
messages[104] = "Торговцы Дидье Дьюсак (оружие: 3-4) Гийом Агнуш (броня: 3-4)";
messages[105] = "Торговцы Жан Фале (оружие: 7-8) Зак Папперс (броня: 7-8)";
messages[106] = "Торговцы Парис Лемон (оружие: 9-10) Майкл Бор (броня: 7-8)";
messages[107] = "Торговцы Кевин Дольтер (оружие: 3-4) Джеймс Кадд (броня: 3-4)";
messages[108] = "Торговцы Жак Лье (оружие: 7-8) Филип Октавел (броня: 7-8)";
messages[109] = "Торговцы Джордан Базиро (оружие: 3-4) Лайон Поддел (броня: 3-4)";
messages[110] = "Торговцы Камилус Торогус (оружие: 3-4) Эдвин Тирс (броня: 3-4)";
// Торговки магией
messages[201] = "Торговки Синтия Самонс (магия: 10-12) Вирджиния Йорк (алхимия: 11-12)";
messages[202] = "Торговки Фабия Нандор (магия: 9-11) Лина Паркер (алхимия: 7-11)";
messages[203] = "Торговки Марта Маллей (магия: 4-6) Женева Маллет (алхимия: 4-8)";
messages[204] = "Торговки Лора Лайонс (магия: 7-9) Дженни Барок (алхимия: 6-10)";
messages[205] = "Торговки Изольда Рубальски (магия: 9-11) Креола Аурунус (алхимия: 7-11)";
messages[206] = "Торговки Дебора Ономанс (магия: 4-6) Сара Иден (алхимия: 4-8)";
messages[207] = "Торговки Кара Мардок (магия: 7-9) Ирма Талес (алхимия: 6-10)";
messages[208] = "Торговки Алиса Дюшер (магия: 9-11) Сатина Магалли (алхимия: 7-11)";
messages[209] = "Торговки Полина Фергюссон (магия: 4-6) Сельма Рагель (алхимия: 4-8)";
messages[210] = "Торговки Эклера Тарантон (магия: 7-9) Роза Кларенс (алхимия: 6-10)";
messages[211] = "Торговки Биба Далтон (магия: 4-6) Дейзи МакТолен (алхимия: 4-8)";
messages[212] = "Торговки Ванда Рамонс (магия: 7-9) Клара Розенс (алхимия: 6-10)";
messages[213] = "Торговки Альбина Аннер (магия: 9-11) Катрин Рочестер (алхимия: 7-11)";
messages[214] = "Торговки Терра Лайн (магия: 10-12) Гертруда Шлоссен (алхимия: 11-12)";
// Металлы
messages[301] = "Металлы Золото Серебро Железо Свинец Медь";
messages[302] = "Металлы Медь";
messages[303] = "Металлы Медь Серебро Железо";
messages[304] = "Металлы Медь Железо Серебро Платина";
messages[305] = "Металлы Медь Железо Свинец Серебро Золото Мифрил";
//Квестовики
messages[421] = "Квесты Тенус Харбаланзо (квесты на карму)";
messages[422] = "Квесты Карамил Белендо (квесты на карму)";
messages[423] = "Квесты Манокар Арабрахнар (квесты на карму)";
messages[424] = "Квесты Анагорад Каратагор (квесты на карму)";
messages[425] = "Квесты Ивален Ханаред (квесты на карму)";
messages[426] = "Квесты Лабанар Саранаман (квесты на карму)";
messages[427] = "Квесты Аминик Айон (квесты на степень)";
//Интересное
messages[501] = "Круг Силы";
messages[502] = "Единороги";
messages[503] = "Мост Нифонов";
messages[504] = "Портал на Остров Выбора";
messages[505] = "Летающая башня";
// Замки, города
messages[801] = "Арис [15]";
messages[802] = "Льеж [15]";
messages[803] = "Пельтье [15]";
messages[804] = "Шателье [15]";
messages[805] = "Латор [30]";
messages[806] = "Каблак [30]";
messages[807] = "Эйкум-кас [30]";
messages[808] = "Фьеф [30]";
messages[809] = "Сабулат [30]";
messages[810] = "Гедеон [30]";
messages[811] = "Триумфалер [30]";
messages[812] = "Дэванагари [30]";
messages[813] = "Блессендор [30]";
messages[814] = "Аммалаэль [45]";
messages[815] = "Деффенсат [45]";
messages[816] = "Каре-Рояль [45]";
messages[817] = "Терноваль [45]";
messages[818] = "Туанод [45]";
messages[819] = "Айонат [30]";
messages[821] = "Город Санпул (клик - переход к карте города)";
messages[822] = "Город Шипстоун (клик - переход к карте города)";
messages[823] = "Город Бангвиль (клик - переход к карте города)";
messages[824] = "Город Торвиль (клик - переход к карте города)";
// Гильдии
messages[901] = "Гильдия Мастеров Стали Генри Стронг";
messages[902] = "Гильдия Архимагов Фил Оакенфилд";
messages[903] = "Гильдия Друидов Ирвин Нетралс";
messages[904] = "Гильдия Охотников Шарп Хантер";
messages[905] = "Гильдия Инквизиторов Брэд Баттер";
messages[906] = "Гильдия Чародеев Арт Кендальф";
messages[907] = "Гильдия Крестоносцев Крис Кросс";
messages[908] = "Гильдия Оружейников Абрахам Стейр";
messages[909] = "Гильдия Варваров Рейв Слоттер";
messages[910] = "Гильдия Кузнецов Роджер Смит";
messages[911] = "Гильдия Ассасинов Барт Миллер";
messages[912] = "Гильдия Воров Брэм Стиллер";
messages[913] = "Гильдия Некромантов Райс Корпс";
messages[914] = "Гильдия Бандиеров Коннор Гард";
// Точки телепортов, возрождения
messages[921] = "Телепорт На материк Харон";
messages[922] = "Телепорт На материк Феб";
messages[923] = "Сломанный Телепорт";
messages[924] = "Точка возрождения плохих";
messages[925] = "Точка возрождения помидоров";
messages[980] = "Точка телепорта С материка Феб";
//Точки телепортов
messages[1850] = "Точка телепорта К северной дороге Гипериона";
messages[1851] = "Точка телепорта На северо-западный край Гипериона";
messages[1852] = "Точка телепорта К дороге Шипстоун-Санпул";
messages[1853] = "Точка телепорта В поле близ дороги Шипстоун-Бангвиль";
messages[1854] = "Точка телепорта На северо-восток Гипериона";
messages[1855] = "Точка телепорта К Умрадскому лесу (Гиперион)";
messages[1856] = "Точка телепорта К Серебряному лесу";
messages[1857] = "Точка телепорта К дороге Шипстоун-Торвил";
messages[1858] = "Точка телепорта К горам между Шипстоуном и Бангвилем";
messages[1859] = "Точка телепорта К озеру Темер";
messages[1860] = "Точка телепорта К дороге Санпул-Шипстоун, возле озера Вортекс";
messages[1861] = "Точка телепорта К дороге Санпул-Бангвиль, возле озера Вортекс";
messages[1862] = "Точка телепорта На остров Гебер (озеро Темер)";
messages[1863] = "Точка телепорта На остров Форос (озеро Вортекс)";
messages[1864] = "Точка телепорта К устью реки Нерей";
messages[1865] = "Точка телепорта На остров Дейрос (озеро Вортекс)";
messages[1866] = "Точка телепорта К Восточному лесу";
messages[1877] = "Точка телепорта К устью реки Диомы";
messages[1878] = "Точка телепорта К Хортонскому лесу";
messages[1879] = "Точка телепорта В горы близ дороги Санпул-Торвил";
messages[1880] = "Точка телепорта На остров Патрос (озеро Атласное)";
messages[1881] = "Точка телепорта В юго-восточный горный карман";
messages[1882] = "Точка телепорта На север от Койтонского леса";
messages[1883] = "Точка телепорта На юго-западный край Гипериона";
messages[1884] = "Точка телепорта К мосту на Тантал";
messages[1885] = "Точка телепорта В Обол";
//Леса
messages[2001] = "Хортонский лес";
messages[2002] = "Койтонский лес";
messages[2003] = "Меранский лес";
messages[2004] = "Умрадский лес";
messages[2005] = "Санпульский лес";
messages[2006] = "Наттонский лес";
messages[2007] = "Торвильский лес";
messages[2008] = "Серебряный лес";
messages[2010] = "Восточный лес";
messages[2009] = "Сумеречный лес";