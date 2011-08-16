using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Web.UI;

namespace freakcode.DynamicMarkup
{
    class Program
    {
        static void Main(string[] args)
        {
            var tags = Markup.New;

            var sw = Stopwatch.StartNew();

            int count = 100000;
            var e = Markup.New.div;

            for (int i = 0; i < count; i++)
                e = Markup.New.div.appendTo(e);


            sw.Stop();

            Console.WriteLine(sw.Elapsed.TotalSeconds);
            Console.ReadLine();

            //var div = Markup.New.div;

            //for (int i = 0; i < count; i++)
            //{
            //    Markup.New.p(
            //        "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            //        Markup.New.a("Morbi congue justo et lectus vulputate et fringilla purus elementum.", href: "#"),
            //        "Sed posuere consectetur sodales."
            //    ).AppendTo(div);
            //}

            //return;

            //Console.WriteLine(div.ToString().Length.ToString("N0"));
            //Console.WriteLine(sw.Elapsed.TotalSeconds);
            //Console.ReadLine();

            //sw.Restart();
            StringBuilder sb = new StringBuilder();

            count = 1;

            using (var stw = new StringWriter(sb))
            using (var hw = new HtmlTextWriter(stw))
            {
                hw.RenderBeginTag("div");

                for (int i = 0; i < count; i++)
                {
                    hw.RenderBeginTag("div");

                    hw.WriteLine("Lorem ipsum dolor sit amet, consectetur adipiscing elit.");
                    
                    hw.AddAttribute("href", "#");
                    hw.RenderBeginTag("a");
                    hw.Write("Morbi congue justo et lectus vulputate et fringilla purus elementum.");
                    hw.RenderEndTag();

                    hw.WriteLine("Sed posuere consectetur sodales.");

                    hw.RenderEndTag();
                }

                hw.RenderEndTag();
            }

            var div = tags.div(

                tags.div(
                    tags.text("Lorem ipsum dolor sit amet, consectetur adipiscing elit."),
                    tags.a("Morbi congue justo et lectus vulputate et fringilla purus elementum.", href: "#"),
                    tags.text("Sed posuere consectetur sodales."),
                    tags.p()
                )

            );

            //tags.p.addClass("foo").appendTo(div);

            div.append(tags.p("foo!"));
            Console.WriteLine(div);

            Console.ReadLine();
            return;

            //Console.WriteLine(div.ToString().Length.ToString("N0"));
            //Console.WriteLine(sw.Elapsed.TotalSeconds);
            //Console.ReadLine();

            //string className = "box";

            //if (htmlAttributes != null)
            //{
            //    var attributeDictionary = new RouteValueDictionary(htmlAttributes);

            //    foreach (var kv in attributeDictionary)
            //    {
            //        if (kv.Key == "class")
            //            className += " " + kv.Value.ToString();
            //        else
            //            htmlWriter.AddAttribute(kv.Key, kv.Value.ToString());
            //    }
            //}

            //htmlWriter.AddAttribute("class", className);

            //htmlWriter.RenderBeginTag("div");

            //htmlWriter.AddAttribute("class", "content");
            //htmlWriter.RenderBeginTag("div");

            //if (title != null)
            //{
            //    htmlWriter.RenderBeginTag("h4");
            //    htmlWriter.Write(title);
            //    htmlWriter.RenderEndTag();
            //}

            //content(null).WriteTo(writer);

            //htmlWriter.RenderEndTag();

            //htmlWriter.AddAttribute("class", "flip");
            //htmlWriter.RenderBeginTag("div");
            //htmlWriter.RenderEndTag();

            //htmlWriter.RenderEndTag();


            var box = tags.div(new { @class = "box", foo = "bar" });
            Console.WriteLine(box);
            Console.ReadLine();
            return;


            var container = tags.div(
                tags.h1("Hey hey hey!"),

                tags.p("foobar foobar and foobar"),
                tags.p(
                    "Blaa!!",
                    tags.a("Läs mer!", href: "http://f<arlig>.com"),
                    "Foo!",
                    tags.text("foo {0} {1:C2}", "bar", 14.25M),

                    id: "willis"
                )
            );

            Console.WriteLine(container);

            var secondContainer = tags.pre(container[0]);
            Console.WriteLine(secondContainer);

            Console.WriteLine(container);
            Console.ReadLine();
        }
    }
}
