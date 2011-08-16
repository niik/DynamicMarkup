DynamicMarkup
=============


Examples
----------------


Planned features
----------------

### CSS property for easy style-attribute modification

Hopefully implemented as close to jQuery.css as possible, initially only with the dual key/value overload
but eventually with dictionary-like anonymous types as well.

    var p = Markup.New.p;
		.css("background", "white")
		.css("color", "black")
	
	Assert.AreEqual("background: white; color: black", p.style);