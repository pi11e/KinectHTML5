


/**
 * @constructor
 */
avgl.Graph = function(data) {
  var self = this,
      levelDistance = 120;
  this.$overlay = jQuery('#ns-avgl-facetgraph-overlay');
  self.rgraph = new $jit.RGraph({
    //id of the visualization container
    injectInto: avgl._graph,
    //change distance between nodes (default is 100)
    levelDistance: levelDistance,
    //SLUB limit sidebar to 550
    width: 1280,
    height: 1024,
    // no fancy movements, other value would be 'polar'
    interpolation: 'linear',
    //values are 'full' -> all is red, 'breadcrumb', -> like BC,
    //'hybrid' -> half/half, red like breadcrumb, full in thicker grey
    historyStyle: 'hybrid',
    // enable mouse zoom interaction
    Navigation: {
      enable: true,
      panning: true,
      zooming: 75,
      zoomingBorder: 20
    },
    //Change node and edge styles such as color, width and dimensions.
    Node: {
      dim: 9,
      color: avgl.global['SLUB_COLORS'].def,
      selected: true
    },
    Edge: {
      lineWidth: 1,
      type: 'line',
      color: avgl.global['SLUB_COLORS'].fnt,
      overridable: true
    },
    Events: {
      enable: true,
      type: 'Native',
      onClick: function(node) {
        node && self._onClick(node);
      },
      onMouseEnter: function(node) {
        node && (function() {
          self.rgraph.canvas.getElement().style.cursor = 'pointer';
        })();
      },
      onMouseLeave: function(node) {
        node && (function() {
          self.rgraph.canvas.getElement().style.cursor = '';
        })();
      },
      onMouseWheel: function(node) {
        self.buildSlider();
      }
    },

    onBeforeCompute: function(node) {
      if (!node)
        return;
      self.rgraph.addNodeInPath(node.id);
      
    },
    
    onCreateLabel: function(domElement, node) 
    {
      node.pos.x = node.pos.x + 100;
      
      domElement.innerHTML =
    	  // add a <span class="node-text>(node name here)</span>
    	  '' +
          '<span class="interactive">' + 
    	  	node.name + 
    	  '</span>' +
    	  // then see if the node's data contains a 'cnt' number (count)
    	  // - if so, add a <span class="node-howmany">(node count here)</span>
    	  // - otherwise, add nothing ('')
          (
        		  node.data.cnt && typeof +node.data.cnt === "number" &&
		              +node.data.cnt == +node.data.cnt ? (
		          '<span class="node-howmany">(' + node.data.cnt + ')</span>'
		          ) : ''
		  );
		  
      console.log(domElement.innerHTML);
      
      // add click event handler
      $jit.util.addEvent(domElement, 'click', function() {
        self.rgraph.addNodeInPath(node.id);
        self._onClick(node);
      });
    },
    //Change node styles when labels are placed or moved.
    onPlaceLabel: function(domElement, node) {
      var style = domElement.style,
          left = parseInt(style.left, 10),
          top = parseInt(style.top, 10);
      if (node._depth == 0) {
        style.zIndex = '10';
        style.fontSize = '22px';
        style.color = avgl.global['SLUB_COLORS'].act;
        style.top = (top - 15) + 'px';
      } else if (node._depth < 2) {
        style.zIndex = '1';
        style.fontSize = '14px';
        style.color = avgl.global['SLUB_COLORS'].def;
        style.top = (top - 8) + 'px';
      } else {
        style.display = 'none';
      }
      style.left = (left + 15) + 'px';
    },

    onComplete: function() {
      self.$overlay.hide(300);
      self.buildBreadcrumb.apply(self, []);
    }
  });
  self.init(data);
};

avgl.Graph.prototype.init = function(data) {
  var self = this;
  //load JSON data.
  self.rgraph.loadJSON(data);
  //remove book count from root node
  var tmp = self.rgraph.root && self._getNode(self.rgraph.root);
  tmp && tmp.data && tmp.data.cnt && (delete tmp.data.cnt);
  //compute positions and plot.
  self.rgraph.refresh();

  //cache breadcrumbers
  self.$bcContainer = jQuery('#ns-avgl-facetgraph-breadcrumb');
  self.sliderVal = 0;
  self.$zDiv = jQuery('#ns-avgl-graph-zoom').bind({
    slidestart  : function(event, ui) {
      avgl.DEBUG && console.log('start', ui.value);
      self.sliderVal = ui.value;
    },
    slide : function(event, ui) {
      var x = y = (Math.pow(Math.E, (ui.value - self.sliderVal) / 5));
      avgl.DEBUG && console.log(ui.value, x);
      ui.doNotSave || avgl.setTemporaryItem('avgl.gzl', ui.value);
      self.rgraph.canvas.scale(x, y);
      self.sliderVal = ui.value;
    }
  });

  //enable breadcrumb anvigation
  self.$bcContainer.delegate('a', 'click', function(event) {
    return self._onClick(jQuery(this).attr('rel'), event);
  });

  var prevRoot = avgl.getTemporaryItem('avgl.grt');
  if (prevRoot) {
    this._onClick(prevRoot);
  }
  var prevBC = avgl.getTemporaryItem('avgl.gbc');
  if (prevBC) {
    prevBC = jQuery.parseJSON(prevBC);
    self.rgraph.nodesInPath = prevBC.path;
    self.rgraph.overallNodeHistory = prevBC.hist;
    self.rgraph.nodeIdTitleMap = prevBC.hmap;
    prevBC = true;
  } else {
    prevBC = false;
  }

  //and initialize breadcrumbs
  self.buildBreadcrumb(prevBC);
  self.buildSlider(avgl.getTemporaryItem('avgl.gzl'));
};

avgl.Graph.prototype._onClick = function(node, event) {
  if (typeof node != 'object') {
    node = this._getNode(node);
  }
  if (node.id == this.rgraph.root) {
    return;
  }
  this.$overlay.show(300);
  var self = this,
      target = node.id,
      href = node.data.href,
      title = node.name;
  avgl.setTemporaryItem('avgl.grt', target);

  return self.rgraph.onClick(target, {
    hideLabels: false,
    onComplete: function() {
      self.rgraph.controller.onComplete();
    }
  });
};

avgl.Graph.prototype._getNode = function(id) {
  return this.rgraph.graph.getNode(id);
};

avgl.Graph.prototype.buildBreadcrumb = function(doNotSave) {
  var self = this,
      path = this.rgraph.nodesInPath,
      root = this.rgraph.root,
      map = this.rgraph.nodeIdTitleMap,
      cfg = avgl.global['breadcrumbCollapse'] || [2, 2],
      html = '<ul class="breadcrumb">',
      hasEllipsis = false,
      titleattrib = '',
      nametag,
      name,
      hellip;

  doNotSave || avgl.setTemporaryItem('avgl.gbc', jQuery.toJSON({
    path: self.rgraph.nodesInPath,
    hist: self.rgraph.overallNodeHistory,
    hmap: self.rgraph.nodeIdTitleMap
  }));

  for (var i = 0, l = path.length - 1, n; n = path[i]; i++) {
    nametag = map[n];
    if (!avgl.isDef(nametag)) {
      var tmpnode = this._getNode(n);
      map[n] = [tmpnode.name.truncate(avgl.global.titleCollapse || [10,10]), tmpnode.name];
      nametag = map[n];
    }
    if (typeof nametag == 'object') {
      name = nametag[1];
      nametag = ('<span title="' + nametag[1] + '">' + nametag[0] + '</span>');
    } else if (nametag != null){
      name = nametag;
      nametag = ('<span title="' + nametag + '">' + nametag + '</span>');
    } else {
      continue;
    }
    if (i > cfg[0] - 1 && i <= (l - cfg[1])) {
      if (!hasEllipsis) {
        html += ('<li><span id=breadcrumbhellip>&hellip;</span> ' +
            '<span class="divider">&nbsp;</span> ');
        hasEllipsis = true;
      }
      if (titleattrib)
        titleattrib += ' \u232a ';
      titleattrib += name;
      continue;
    }
    html += '<li';
    if (root == n) {
      html += ' class="active">';
    } else {
      html += '><a href="#" rel="' + n + '">';
    }
    html += nametag;
    if (root != n) {
      html += '</a>';
    }
    if (i < l) {
      html += ' <span class="divider">&nbsp;</span> ';
    }
  }
  html += '</ul>';

  this.$bcContainer.html(html);
  if (hellip || (hellip = document.getElementById('breadcrumbhellip'))) {
    hellip.title = titleattrib;
  }
};

avgl.Graph.prototype.buildSlider = function(prevZL) {
  if (prevZL != null) {
    prevZL = +prevZL;
    if (prevZL == prevZL) {
      this.$zDiv.slider('value', prevZL).trigger("slide", {
        value:prevZL,
        doNotSave : true
      });
    }
  } else {
    prevZL = this.rgraph.canvas.zoomLevel;
    avgl.setTemporaryItem('avgl.gzl', prevZL);
    this.$zDiv.slider('value', prevZL);
  }
};
