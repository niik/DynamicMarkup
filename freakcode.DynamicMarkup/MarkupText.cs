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
using System.Linq;
using System.Web;

namespace freakcode.DynamicMarkup
{
    public sealed class MarkupText : MarkupNode
    {
        public override bool IsHtmlNode { get { return false; } }
        public override bool IsTextNode { get { return true; } }
        public override bool IsEmpty { get { return string.IsNullOrEmpty(this.Text); } }

        public string Text { get; set; }

        public override string InnerText { get { return this.Text; } }
        public override string InnerHtml { get { return HttpUtility.HtmlEncode(this.Text); } }

        public MarkupText()
        {
        }

        public MarkupText(string text)
        {
            this.Text = text;
        }

        public MarkupText(string format, params object[] arguments)
        {
            this.Text = string.Format(format, arguments);
        }

        public override bool TryInvokeMember(System.Dynamic.InvokeMemberBinder binder, object[] args, out object result)
        {
            if (binder.Name != "text")
            {
                result = null;
                return false;
            }

            result = this;

            if (args.Length == 0)
                result = this.InnerText;
            else if (args.Length == 1)
                this.Text = Convert.ToString(args[0]);
            else
                this.Text = string.Format(Convert.ToString(args[0]), args.Skip(1).ToArray());

            return true;
        }

        public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
        {
            if (binder.Name == "text")
            {
                result = this.Text;
                return true;
            }

            return base.TryGetMember(binder, out result);
        }

        public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
        {
            if (binder.Name == "text")
            {
                this.Text = Convert.ToString(value);
                return true;
            }

            return false;
        }

        public override void WriteTo(System.Web.UI.HtmlTextWriter writer)
        {
            if (!IsEmpty)
                writer.WriteEncodedText(this.Text);
        }

        public override bool TryInvoke(System.Dynamic.InvokeBinder binder, object[] args, out object result)
        {
            if (args.Length == 1)
            {
                this.Text = args[0] == null ? null : Convert.ToString(args[0]);
            }
            else
            {
                // Markup.New.text("foo {0}", "bar") => text: foo bar
                this.Text = string.Format(Convert.ToString(args[0]), args.Skip(1).ToArray());
            }

            result = this;
            return true;
        }
    }
}
