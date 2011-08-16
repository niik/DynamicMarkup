/*
 * Copyright (c) 2010-2011 Markus Olsson
 * var mail = string.Join(".", new string[] {"j", "markus", "olsson"}) + string.Concat('@', "gmail.com");
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy of this 
 * software and associated documentation files (the "Software"), to deal in the Software without
 * restriction, including without limitation the rights to use, copy, modify, merge, publish, 
 * distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the
 * Software is furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be
 * included in all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING 
 * BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
 * NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, 
 * DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, 
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 */

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;

namespace freakcode.DynamicMarkup
{
    public abstract class MarkupNode : DynamicObject, IHtmlString
    {
        public static dynamic New { get { return Markup.New; } }

        public abstract bool IsHtmlNode { get; }
        public abstract bool IsTextNode { get; }

        public abstract bool IsEmpty { get; }
        public abstract string InnerText { get; }
        public abstract string InnerHtml { get; }

        private MarkupElement _parent;

        public MarkupElement Parent
        {
            get { return _parent; }
            set
            {
                if (_parent != null && value != _parent)
                    _parent.Remove(this);

                _parent = value;
            }
        }

        public IEnumerable<MarkupNode> Siblings
        {
            get
            {
                if (this.Parent == null)
                    return Enumerable.Empty<MarkupNode>();

                return this.Parent.Children.Where(c => c != this);
            }
        }

        public override string ToString()
        {
            return ToHtmlString();
        }

        public string ToHtmlString()
        {
            if (IsTextNode && IsEmpty)
                return string.Empty;

            var sb = new StringBuilder();

            using (var sw = new StringWriter(sb))
                WriteTo(sw);

            return sb.ToString();
        }

        public void WriteTo(TextWriter writer)
        {
            using (var htw = new HtmlTextWriter(writer))
                WriteTo(htw);
        }

        public abstract void WriteTo(HtmlTextWriter writer);
    }
}
