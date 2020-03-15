using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;

namespace REVUnit.AutoArknights.Library.Automation_Script
{
    public abstract class Keyword : Token
    {
        public string[] Names { get; }
        public string Name => Names[0];
        private static readonly IReadOnlyList<Keyword> Keywords;

        static Keyword()
        {
            var fields = typeof(Keyword).GetFields(BindingFlags.Public|BindingFlags.DeclaredOnly|BindingFlags.Static);
            Keywords = fields.Where(field => field.FieldType == typeof(Keyword))
                .Select(field => (Keyword) field.GetValue(null)!).ToArray();
        }

        protected Keyword(params string[] names)
        {
            Names = names;
        }

        public static Keyword Parse(string str)
        {
            return Keywords.First(it => it.Names.Contains(str));
        }
        
        public static bool TryParse(string str, out Keyword keyword)
        {
            keyword = Keywords.FirstOrDefault(it => it.Names.Contains(str));
            return keyword != null;
        }
        
        public static Keyword? TryParse(string str)
        {
            return Keywords.FirstOrDefault(it => it.Names.Contains(str));
        }

        public static readonly Keyword Click = new Click("点击");
        public static readonly Keyword Wait = new Wait("等待");
        public static readonly Keyword Retry = new Retry("重试");
        public static readonly Keyword If = new If("如果");
        public static readonly Keyword Found = new CanFind("找到了", "能找到");
        public static readonly Keyword NotFound = new NotFound( "不能找到");
    }
}