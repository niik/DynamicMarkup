DynamicMarkup
=============
An easy way of generating markup in C# without having to resort to the dreaded HtmlTextWriter. 
Uses C# 4 dynamic bindings to provide a nice interface modelled after the jQuery API (which we all
know rocks at manipulating markup)

Documentation is sparse at the moment, check out the [unit tests](https://github.com/markus-olsson/DynamicMarkup/tree/master/freakcode.DynamicMarkup.Tests) 
for more details.

Examples
----------------

### Html5 article

```c#
var tags = Markup.New;
var article = tags.article(
	tags.h1("DynamicMarkup - html is easy"),
	tags.p(
		"Finally there's a new way of dealing with server-side markup creation! ",
		"Read more at ", tags.a("github", href: "http://github.com/markus-olsson/DynamicMarkup")
	),
	tags.p(
		"Hope to see you soon!"
	)
);

Console.WriteLine(article);
```
Result

	<article>
        <h1>DynamicMarkup - html is easy</h1>
        <p>
			Finally there&#39;s a new way of dealing with server-side markup creation!
			Read more at <a href="http://github.com/markus-olsson/DynamicMarkup">github</a>
        </p>
        <p>Hope to see you soon!</p>
	</article>
	
#### The same result with HtmlTextWriter

```c#
var sb = new StringBuilder();

using(var sw = new StringWriter(sb))
using(var hw = new HtmlTextWriter(hw))
{
    hw.RenderBeginTag("article");
    
    hw.RenderBeginTag("h1");
    hw.WriteEncodedText("DynamicMarkup - html is easy");
    hw.RenderEndTag();
    
    hw.RenderBeginTag("p");
    hw.WriteEncodedText("Finally there's a new way of dealing with server-side markup creation! Read more at ");
    
    hw.AddAttribute("href", "http://github.com/markus-olsson/DynamicMarkup");
    hw.RenderBeginTag("a");
    hw.WriteEncodedText("github");
    hw.RenderEndTag();
    
    hw.RenderBeginTag("p")
    hw.WriteEncodedText("Hope to see you soon!");
    hw.RenderEndTag();
    
    hw.RenderEndTag();
}
```

### jQuery-like syntax
This example is intentionally messy and uses several different ways of achieving the same thing.
Ff you dislike the jQuery-style use of lower case methods and properties; don't worry, you can
use whichever style you want.

```c#
var tags = Markup.New;
var entry = tags.div(@class: "blog-post");

var header = tags.h2("Foo", id: "post-header-1");
header.addClass("entry-header");

header.appendTo(entry); // or entry.append(header);

foreach(var line in myTextFile.SplitLines())
	entry.append(tags.p(line));
	
var footer = tags.div().addClass("footer").appendTo(header);

footer.add(
	tags.div(
		"Written by ", tags.span("Foo Bar", @class: "author")
	)
);

// Format strings
footer.add(
	tags.div(
		tags.text("Published at {0:yyyy-MM-dd}", DateTime.Now)
	)
)

```

Planned features
----------------

### CSS property for easy style-attribute modification

Hopefully implemented as close to jQuery.css as possible, initially only with the dual key/value overload
but eventually with dictionary-like anonymous types as well.

    var p = Markup.New.p;
        .css("background", "white")
        .css("color", "black")
	
    Assert.AreEqual("background: white; color: black", p.style);
    
License
-------

Licensed under the MIT License

Permission is hereby granted, free of charge, to any person obtaining a copy of this 
software and associated documentation files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use, copy, modify, merge, publish, 
distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following conditions:
 
The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.