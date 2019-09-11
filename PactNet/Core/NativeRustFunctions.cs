namespace PactNet.Core
{
    using System;
    using System.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Text;
    public static unsafe class NativeRustFunctions
    {
        public static string SystemStringFromCStyleString(byte* input)
        {
            int length = 0;
            while (input[length] != 0)
            {
                length++;
            }

            var encoding = Encoding.UTF8;
            var charCount = encoding.GetCharCount(input, length);
            var outputString = new string('\0', charCount);
            fixed (char* output = outputString)
            {
                encoding.GetChars(input, length, output, charCount);
            }

            return outputString;
        }

        public static byte[] CStyleStringFromSystemString(string input)
        {
            var encoding = Encoding.UTF8;

            fixed (char* src = input)
            {
                var output = new byte[encoding.GetByteCount(src, input.Length) + 1];
                fixed (byte* dst = output)
                {
                    var r = encoding.GetBytes(src, input.Length, dst, output.Length);
                    Debug.Assert(r + 1 == output.Length);
                    return output;
                }
            }
        }
    }
}
