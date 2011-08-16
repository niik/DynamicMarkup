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
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using freakcode.DynamicMarkup.Extensions;
using System.Text;
using System.IO;

namespace freakcode.DynamicMarkup
{
    public sealed class MarkupElement : MarkupNode, IEnumerable<MarkupNode>
    {
        public IEnumerable<MarkupNode> Children { get { return this._children ?? Enumerable.Empty<MarkupNode>(); } }

        public IEnumerable<MarkupNode> Descendants
        {
            get
            {
                foreach (var child in this.Children)
                {
                    yield return child;

                    if (child.IsHtmlNode)
                    {
                        foreach (var descendant in ((MarkupElement)child).Descendants)
                            yield return descendant;
                    }
                }
            }
        }

        public MarkupNode this[int index]
        {
            get { return this.Children.ElementAt(index); }
        }

        public string this[string attribute]
        {
            get { return this.tb.Attributes.GetValueOrDefault(attribute); }
            set { this.tb.Attributes[attribute] = value; }
        }

        public override bool IsHtmlNode { get { return true; } }
        public override bool IsTextNode { get { return false; } }
        public override bool IsEmpty { get { return !this.Children.Any(); } }

        private TagBuilder tb;
        private List<MarkupNode> _children;
        public string TagName { get { return this.tb.TagName; } }

        public override string InnerText
        {
            get { return string.Concat(this.Descendants.OfType<MarkupText>().Select(t => t.Text)); }
        }

        public override string InnerHtml
        {
            get
            {
                var sb = new StringBuilder();

                using (var sw = new StringWriter(sb))
                using (var hw = new HtmlTextWriter(sw))
                    this.WriteTo(hw, childrenOnly: true);

                return sb.ToString();
            }
        }

        public MarkupElement(string tagName)
        {
            if (tagName == null)
                throw new ArgumentNullException("tagName");

            this.tb = new TagBuilder(tagName.ToLower());
            this._children = new List<MarkupNode>();
        }

        public MarkupElement Append(MarkupNode child)
        {
            return InternalInsertChild(-1, child);
        }

        public MarkupElement AppendTo(MarkupElement parent)
        {
            parent.Append(this);
            return this;
        }

        public MarkupElement PrependTo(MarkupElement parent)
        {
            parent.Prepend(this);
            return this;
        }

        public MarkupElement Prepend(MarkupNode child)
        {
            return InternalInsertChild(0, child);
        }

        private MarkupElement InternalInsertChild(int index, MarkupNode child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            if (this._children == null)
                this._children = new List<MarkupNode>();

            this._children.Insert(index == -1 ? this._children.Count : index, child);

            child.Parent = this;

            return this;
        }

        public int IndexOf(MarkupNode child)
        {
            if (this._children == null)
                return -1;

            return this._children.IndexOf(child);
        }

        public MarkupElement Remove(MarkupNode child)
        {
            if (child == null)
                throw new ArgumentNullException("child");

            if (this._children != null && this._children.Remove(child))
                child.Parent = null;

            return this;
        }

        public MarkupElement Clear()
        {
            if (this._children != null)
            {
                foreach (var child in this._children.ToArray())
                    child.Parent = null;

                this._children = null;
            }

            return this;
        }

        public MarkupElement AddClass(string className)
        {
            if (IsTextNode)
                return this;

            if (className == null)
                throw new ArgumentNullException("className");

            this["class"] = string.Join(" ", GetClassNames().Union(new[] { className }));
            return this;
        }

        public bool HasClass(string className)
        {
            if (IsTextNode)
                return false;

            if (className == null)
                throw new ArgumentNullException("className");

            return GetClassNames().Any(c => c == className);
        }

        public MarkupElement RemoveClass(string className)
        {
            if (IsTextNode)
                return this;

            if (className == null)
                throw new ArgumentNullException("className");

            var classNames = GetClassNames();

            if (className.Any())
            {
                tb.Attributes["class"] = string.Join(" ", GetClassNames().Where(c => c != className));

                if (string.IsNullOrWhiteSpace(tb.Attributes["class"]))
                    tb.Attributes.Remove("class");
            }

            return this;
        }

        private string[] GetClassNames()
        {
            if (this["class"] == null)
                return new string[] { };

            return this["class"].Split(' ');
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            if (args.Length == 0)
            {
                if (binder.Name == "clear")
                {
                    result = this.Clear();
                    return true;
                }
                else if (binder.Name == "text")
                {
                    result = this.InnerText;
                    return true;
                }
            }
            else if (args.Length == 1)
            {
                // mostly jQuery-like shortcuts/aliases.
                switch (binder.Name)
                {
                    case "Add":
                    case "add":
                    case "append":
                    case "appendChild":
                        result = this.Append((MarkupNode)args[0]);
                        break;
                    case "prepend":
                    case "prependChild":
                        result = this.Prepend((MarkupNode)args[0]);
                        break;
                    case "appendTo":
                        result = this.AppendTo((MarkupElement)args[0]);
                        break;
                    case "prependTo":
                        result = this.PrependTo((MarkupElement)args[0]);
                        break;
                    case "remove":
                    case "removeChild":
                        result = this.Remove((MarkupNode)args[0]);
                        break;
                    case "addClass":
                        result = this.AddClass((string)args[0]);
                        break;
                    case "removeClass":
                        result = this.RemoveClass((string)args[0]);
                        break;
                    case "hasClass":
                        result = this.HasClass((string)args[0]);
                        break;
                    case "text":
                        this.Clear();
                        this.Append(new MarkupText(Convert.ToString(args[0])));
                        result = this;
                        break;
                    default:
                        result = null;
                        return false;
                }

                return true;
            }
            else
            {
                if (binder.Name == "text")
                {
                    this.Clear();
                    this.Append(new MarkupText(Convert.ToString(args[0]), args.Skip(1).ToArray()));
                    result = this;
                    return true;
                }
            }

            result = null;
            return false;
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            switch (binder.Name)
            {
                case "className":
                    result = this["class"];
                    return true;
                case "text":
                case "Text":
                    result = this.InnerText;
                    return true;
                case "html":
                case "Html":
                case "innerHtml":
                case "innerHTML":
                    result = this.InnerHtml;
                    return true;
                default:
                    result = tb.Attributes.GetValueOrDefault(binder.Name);
                    return true;
            }
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            switch (binder.Name)
            {
                case "className":
                    this["class"] = Convert.ToString(value);
                    return true;
                case "text":
                case "Text":
                    this.Clear();
                    this.Append(new MarkupText(HttpUtility.HtmlEncode(Convert.ToString(value))));
                    return true;
                case "html":
                case "Html":
                case "innerHtml":
                case "innerHTML":
                    throw new NotSupportedException();
                //this.Clear();
                //this.Append(new MarkupText(Convert.ToString(value)));
                //return true;
                default:
                    if (value == null)
                        tb.Attributes.Remove(binder.Name);
                    else
                        tb.Attributes[binder.Name] = Convert.ToString(value);

                    return true;
            }
        }

        public override bool TryInvoke(InvokeBinder binder, object[] args, out object result)
        {
            var call = binder.CallInfo;
            result = this;

            if (call.ArgumentCount == 0)
                return true;

            //if (call.ArgumentCount == 1 && args[0] != null)
            //{
            //    var dict = args[0] as System.Collections.IDictionary;

            //    if (dict == null && args[0].GetType(). == typeof(object))
            //        dict = (System.Collections.IDictionary)new RouteValueDictionary(args[0]);

            //    if (dict != null)
            //    {
            //        foreach (System.Collections.DictionaryEntry kv in dict)
            //            this.tb.MergeAttribute(Convert.ToString(kv.Key), Convert.ToString(kv.Value), replaceExisting: true);

            //        return true;
            //    }
            //}

            int unnamedArgumentCount = call.ArgumentCount - call.ArgumentNames.Count;

            // Process unnamed arguments (if any)
            for (int i = 0; i < unnamedArgumentCount; i++)
            {
                object arg = args[i];

                if (arg == null)
                    throw new ArgumentException("Null was passed as value for argument index " + i);

                var child = arg as MarkupNode;

                if (child != null)
                    this.Append(child);
                else
                    this.Append(new MarkupText(Convert.ToString(args[i])));
            }

            for (int i = unnamedArgumentCount; i < call.ArgumentCount; i++)
            {
                string argName = call.ArgumentNames[unnamedArgumentCount - i];
                tb.Attributes[argName] = Convert.ToString(args[i]);
            }

            return true;
        }

        public override void WriteTo(HtmlTextWriter writer)
        {
            this.WriteTo(writer, childrenOnly: false);
        }

        private void WriteTo(HtmlTextWriter writer, bool childrenOnly)
        {
            if (!this.Children.Any())
            {
                if (!childrenOnly)
                    writer.WriteLine(tb.ToString());

                return;
            }

            // Turns <a ..>\nfoo\n</a> into <a..>foo</a>
            if (this.Children.All(m => m.IsTextNode))
            {
                if (!childrenOnly)
                {
                    writer.Write(tb.ToString(TagRenderMode.StartTag));
                    foreach (var child in this.Children)
                        child.WriteTo(writer);
                    writer.Write(tb.ToString(TagRenderMode.EndTag));
                    writer.WriteLine();
                }
                else
                {
                    foreach (var child in this.Children)
                        child.WriteTo(writer);
                }
                return;
            }

            if (!childrenOnly)
            {
                writer.WriteLine(tb.ToString(TagRenderMode.StartTag));
                writer.Indent++;
            }

            foreach (var child in this.Children)
                child.WriteTo(writer);

            if (!childrenOnly)
            {
                writer.Indent--;
                writer.WriteLine(tb.ToString(TagRenderMode.EndTag));
            }
        }

        public IEnumerator<MarkupNode> GetEnumerator()
        {
            return this.Children.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)this.Children).GetEnumerator();
        }
    }
}
