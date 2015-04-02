using System;
using System.Reflection;


namespace mk.LibLangExt
{
    public static class EnumExtentions
    {
        public static string GetCaption(Enum e)
        {
            if (null == e)
            {
                return null;
            }

            string s = e.ToString();
            Type t = e.GetType();
            FieldInfo f = t.GetField(s);
            CaptionAttribute[] c = f.GetCustomAttributes(typeof(CaptionAttribute), false) as CaptionAttribute[];
            return (null == c || 0 == c.Length) ? s : c[0].Value;
        }
    }

    public class CaptionAttribute : Attribute
    {
        public CaptionAttribute(string caption)
        {
            Value = caption;
        }

        public string Value { get; private set; }
        public override string ToString() { return Value; }
    }
}
