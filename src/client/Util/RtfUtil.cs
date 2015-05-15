using System.Text;

namespace Sdm.Client.Util
{
    public static class RtfUtil
    {
        public static void EscapeString(StringBuilder dst, string src)
        {
            for (int i = 0; i < src.Length; i++)
            {
                var c = src[i];
                switch (c)
                {
                case '\\': dst.Append(@"\\"); continue;
                case '{': dst.Append(@"\{"); continue;
                case '}': dst.Append(@"\}"); continue;
                case '\n': dst.Append(@"\line "); continue;
                }
                if (c <= 0x7f)
                    dst.Append(c);
                else
                    dst.Append(@"\u").Append((uint)c).Append('?');
            }
        }
    }
}
