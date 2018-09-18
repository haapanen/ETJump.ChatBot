using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace ETJump.Communication
{
    public class Encoder
    {
        private readonly Regex _unauthorizedChars = new Regex("(;|\"|\\\\)*");

        public string Encode(string message)
        {
            message = _unauthorizedChars.Replace(message, "");

            var stringBuilder = new StringBuilder();

            foreach (var c in message)
            {
                if (c <= 127)
                {
                    stringBuilder.Append(c);
                }
                else
                {
                    stringBuilder.Append("=");
                    stringBuilder.Append(((byte) c).ToString("X"));
                }
            }

            return stringBuilder.ToString();
        }
    }
}
