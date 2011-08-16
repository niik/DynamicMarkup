using freakcode.DynamicMarkup;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Text.RegularExpressions;

namespace freakcode.DynamicMarkup.Tests
{
    [TestClass]
    public class MarkupTest
    {
        private static Regex stripRe = new Regex("[\r\n\t]");
        private static dynamic tags = Markup.New;

        private static string strip(MarkupNode node)
        {
            if (node.IsTextNode)
                return node.InnerText;

            return stripRe.Replace(node.ToHtmlString(), string.Empty);
        }

        private static void AssertMarkup(string expected, MarkupNode node)
        {
            Assert.AreEqual(expected, strip(node));
        }

        [TestMethod]
        public void EmptyTagsTest()
        {
            Assert.AreEqual("<p></p>", strip(tags.p));
        }

        [TestMethod]
        public void TagTextTest()
        {
            Assert.AreEqual("<p>foo</p>", strip(tags.p("foo")));
            Assert.AreEqual("<p>foo</p>", strip(tags.p.text("foo")));
        }

        [TestMethod]
        public void AttributesTest()
        {
            Assert.AreEqual("<p class=\"foo\"></p>", strip(tags.p(@class: "foo")));
        }

        [TestMethod]
        public void ChildElementsTest()
        {
            Assert.AreEqual("<p><span>foo</span></p>", strip(tags.p(tags.span("foo"))));
        }

        [TestMethod]
        public void TextMethodTest()
        {
            var p = tags.p;
            AssertMarkup("<p></p>", p);

            p.text("foo");
            AssertMarkup("<p>foo</p>", p);
            Assert.AreEqual("foo", p.text());
            Assert.AreEqual("foo", p.text);
        }

        [TestMethod]
        public void TextNodeTextMethodTest()
        {
            var t = tags.text("foo");

            Assert.AreEqual("foo", t.text());
            Assert.AreEqual("foo", t.text);

            t.text("bar");

            Assert.AreEqual("bar", t.text());

            t.text("foo {0}", "bar");

            Assert.AreEqual("foo bar", t.text());

            t.text = "bla";

            Assert.AreEqual("bla", t.text());

        }

        [TestMethod]
        public void FormatStringTests()
        {
            Assert.AreEqual("foo bar", tags.text("foo {0}", "bar").text());
        }

        [TestMethod]
        public void AddClassTest()
        {
            var p = tags.p;
            AssertMarkup("<p></p>", p);

            p.addClass("foo");
            AssertMarkup("<p class=\"foo\"></p>", p);

            Assert.AreEqual("foo", p.className);
            Assert.AreEqual("foo", p["class"]);

            p.addClass("bar");
            AssertMarkup("<p class=\"foo bar\"></p>", p);
        }

        [TestMethod]
        public void DuplicateAddClassTest()
        {
            var p = tags.p;

            p.addClass("foo");
            p.addClass("foo");

            AssertMarkup("<p class=\"foo\"></p>", p);
        }

        [TestMethod]
        [Ignore] // not implemented yet
        public void MultiAddOverloadTest()
        {
            var p = tags.p;

            p.addClass("foo", "bar");
            AssertMarkup("<p class=\"foo bar\"></p>", p);

            p = tags.p;

            p.addClass(new[] { "foo", "bar" });
            AssertMarkup("<p class=\"foo bar\"></p>", p);
        }

        [TestMethod]
        public void RemoveClassTest()
        {
            var p = tags.p;

            p.addClass("foo");
            p.addClass("bar");

            AssertMarkup("<p class=\"foo bar\"></p>", p);

            p.removeClass("foo");

            AssertMarkup("<p class=\"bar\"></p>", p);
        }

        [TestMethod]
        public void HasClassTest()
        {
            var p = tags.p;

            Assert.IsFalse(p.hasClass("foo"));
            p.addClass("foo");
            Assert.IsTrue(p.hasClass("foo"));
            p.removeClass("foo");
            Assert.IsFalse(p.hasClass("foo"));
        }

        [TestMethod]
        public void RemoveChildTest()
        {
            var p = tags.p(tags.a);

            AssertMarkup("<p><a></a></p>", p);
            p.removeChild(p[0]);
            AssertMarkup("<p></p>", p);
        }

        [TestMethod]
        public void ClearTest()
        {
            var p = tags.p(tags.a, tags.div);

            AssertMarkup("<p><a></a><div></div></p>", p);
            p.clear();
            AssertMarkup("<p></p>", p);
        }

        [TestMethod]
        public void InnerHtmlTest()
        {
            var p = tags.p;

            AssertMarkup("<p></p>", p);
            Assert.AreEqual(string.Empty, p.innerHtml);

            p.add(tags.text("<foo>"));
            Assert.AreEqual("&lt;foo&gt;", p.innerHtml);
        }

        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void InnerHtmlParsingNotSupportedTest()
        {
            var div = tags.div;

            div.innerHtml = "<p>foo</p>";

            // We might support this one day but we'd have to get hold of a fast
            // html parser. It shouldn't have to be especially tolerant of bad html though
            Assert.AreEqual("p", div[0].TagName);
        }

        [TestMethod]
        public void UnsetAttributesTest()
        {
            var p = tags.p;

            Assert.AreEqual(null, p.id);
            p.id = "foo";

            AssertMarkup("<p id=\"foo\"></p>", p);
            p.id = null;
            AssertMarkup("<p></p>", p);
        }
    }
}
