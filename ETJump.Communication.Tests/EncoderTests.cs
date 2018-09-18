using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ETJump.Communication.Tests
{
    public class EncoderTests
    {
        [Fact]
        public void Encode_RemovesDisallowedCharacters()
        {
            Assert.Equal("abc", new Encoder().Encode("a;b\"\\c"));
        }

        [Fact]
        public void Encode_EncodesExtendedAscii()
        {
            Assert.Equal("a=E4b=E4c=F6d=F6", new Encoder().Encode("aäbäcödö"));
        }
    }
}
