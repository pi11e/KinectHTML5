

/**
 * @define {boolean} Overridden to true by the compiler when --closure_pass
 *     or --mark_as_compiled is specified.
 */
var COMPILED = false;


/**
 * Base namespace for the library.  Checks to see avgl is
 * already defined in the current scope before assigning to prevent
 * clobbering.
 *
 * @const
 */
var avgl = avgl || {}; // Identifies this file as the library base.


/**
 * Reference to the global context.  In most cases this will be 'window'.
 */
//noinspection ThisExpressionReferencesGlobalObjectJS
avgl.global = this;


/**
 * @define {boolean} DEBUG is provided as a convenience so that debugging code
 * that should not be included in a production js_binary can be easily stripped
 * by specifying --define avgl.DEBUG=false to the JSCompiler. For example, most
 * toString() methods should be declared inside an "if (avgl.DEBUG)" conditional
 * because they are generally used for debugging purposes and it is difficult
 * for the JSCompiler to statically determine whether they are used.
 */
avgl.DEBUG = true;


/**
 * Reference to the current jQueryXHR Object of false if no AJAX is ongoing
 * @type {jQuery.jqXHR|boolean}
 */
avgl._jqxhr = false;


/**
 * Selector for the resultContainer
 * @type {?String}
 */
avgl._resultContainer = '#sideResultContainer';


/**
 * Selector to the title field that is used for displaying the current concept
 * @type {string}
 */
avgl._conceptSearchedFor = '#ns-avgl-show-results-for';


/**
 * Selector for the container that will hold the tree
 * @type {string}
 */
avgl._tree = '#ns-avgl-facettree';


/**
 * ID from the element that will hold the Graph
 * @type {string}
 */
avgl._graph = 'ns-avgl-facetgraph-infovis';


avgl.unicode = {
  chr: function(code) {
    return String.fromCharCode(code);
  },
  code2utf: function(code) {
    if (code < 128) return avgl.unicode.chr(code);
    if (code < 2048) return avgl.unicode.chr(192 + (code >> 6)) + avgl.unicode.chr(128 + (code & 63));
    if (code < 65536) return avgl.unicode.chr(224 + (code >> 12)) + avgl.unicode.chr(128 + ((code >> 6) & 63)) + avgl.unicode.chr(128 + (code & 63));
    if (code < 2097152) return avgl.unicode.chr(240 + (code >> 18)) + avgl.unicode.chr(128 + ((code >> 12) & 63)) + avgl.unicode.chr(128 + ((code >> 6) & 63)) + avgl.unicode.chr(128 + (code & 63));
  },
  _utf8Encode: function(str) {
    var utf8str = new Array();
    for (var i = 0; i < str.length; i++) {
      utf8str[i] = avgl.unicode.code2utf(str.charCodeAt(i));
    }
    return utf8str.join('');
  },

  encode: function(str) {
    var utf8str = new Array();
    var pos, j = 0;
    var tmpStr = '';

    while ((pos = str.search(/[^\x00-\x7F]/)) != -1) {
      tmpStr = str.match(/([^\x00-\x7F]+[\x00-\x7F]{0,10})+/)[0];
      utf8str[j++] = str.substr(0, pos);
      utf8str[j++] = avgl.unicode._utf8Encode(tmpStr);
      str = str.substr(pos + tmpStr.length);
    }

    utf8str[j++] = str;
    return utf8str.join('');
  },


  _utf8Decode: function(utf8str) {
    var str = new Array();
    var code, code2, code3, code4, j = 0;
    for (var i = 0; i < utf8str.length;) {
      code = utf8str.charCodeAt(i++);
      if (code > 127) code2 = utf8str.charCodeAt(i++);
      if (code > 223) code3 = utf8str.charCodeAt(i++);
      if (code > 239) code4 = utf8str.charCodeAt(i++);

      if (code < 128) str[j++] = avgl.unicode.chr(code);
      else if (code < 224) str[j++] = avgl.unicode.chr(((code - 192) << 6) + (code2 - 128));
      else if (code < 240) str[j++] = avgl.unicode.chr(((code - 224) << 12) + ((code2 - 128) << 6) + (code3 - 128));
      else str[j++] = avgl.unicode.chr(((code - 240) << 18) + ((code2 - 128) << 12) + ((code3 - 128) << 6) + (code4 - 128));
    }
    return str.join('');
  },


  decode: function(utf8str) {
    var str = new Array();
    var pos = 0;
    var tmpStr = '';
    var j = 0;
    while ((pos = utf8str.search(/[^\x00-\x7F]/)) != -1) {
      tmpStr = utf8str.match(/([^\x00-\x7F]+[\x00-\x7F]{0,10})+/)[0];
      str[j++] = utf8str.substr(0, pos) + avgl.unicode._utf8Decode(tmpStr);
      utf8str = utf8str.substr(pos + tmpStr.length);
    }

    str[j++] = utf8str;
    return str.join('');
  }
};

avgl.load = {
  graph: function() {
    typeof ns_avgl_jsondata_graph == 'undefined' && jQuery.getScript(
        'graph_data.js', function() {
          new avgl.Graph(ns_avgl_jsondata_graph);
        }
    );
  }
};

/**
 * Determines whether a variable is defined or not
 * @param {*} val Variable to test.
 * @return {boolean} Whether the variable is defined.
 */
avgl.isDef = function(val) {
  var undefined;
  return val !== undefined;
};


avgl._hasLS = (function(){
  try {
    localStorage.setItem('avgl', 'avgl');
    localStorage.removeItem('avgl');
    return true;
  } catch(e) {
    return false;
  }
})();

avgl._hasSS = (function(){
  try {
    sessionStorage.setItem('avgl', 'avgl');
    sessionStorage.removeItem('avgl');
    return true;
  } catch(e) {
    return false;
  }
})();

avgl.setPersistentItem = avgl._hasLS ?
    function(key, val) {
      return localStorage.setItem(key, val);
    } :
    function(key, val) {
      return jQuery.cookie(key, val, {expires: 7});
    };

avgl.getPersistentItem = avgl._hasLS ?
    function(key) {
      return localStorage.getItem(key);
    } :
    function(key) {
      return jQuery.cookie(key);
    };

avgl.removePersistentItem = avgl._hasLS ?
    function(key) {
      return localStorage.removeItem(key);
    } :
    function(key) {
      return jQuery.cookie(key, null);
    };

avgl.setTemporaryItem = avgl._hasSS ?
    function(key, val) {
      return sessionStorage.setItem(key, val);
    } :
    function(key, val) {
      return jQuery.cookie(key, val, {expires: null});
    };

avgl.getTemporaryItem = avgl._hasSS ?
    function(key) {
      return sessionStorage.getItem(key);
    } :
    function(key) {
      return jQuery.cookie(key);
    };

avgl.removeTemporaryItem = avgl._hasSS ?
    function(key) {
      return sessionStorage.removeItem(key);
    } :
    function(key) {
      return jQuery.cookie(key, null);
    };
