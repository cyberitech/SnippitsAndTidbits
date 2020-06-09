using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LibExfil.Toolbox.Base64
{
    public class Base64Utils
    {
        /// <summary>
        /// turn a single string to its b64 representation
        /// </summary>
        /// <param name="input"></param>
        /// <returns>turn a single string to its b64 representation</returns>
        public static String MakeB64(String input)     //turn a single string to its b64 representation
        { return Convert.ToBase64String(Encoding.UTF8.GetBytes(input)); }

        /// <summary>
        /// turn a string array to its b64 representation
        /// </summary>
        /// <param name="input"></param>
        /// <returns>turn a string array  to its b64 representation</returns>
        public static String[] MakeB64(String[] input)         //use functional paradigm to map a string array alements to the b64 encoded veersion
        { return input.Select(str => Convert.ToBase64String(Encoding.UTF8.GetBytes(str))).ToArray(); }


        /// <summary>
        /// turn a string list to its b64 representation
        /// </summary>
        /// <param name="input"></param>
        /// <returns>turn a string list  to its b64 representation</returns>
        public static List<String> MakeB64(List<String> input)         //use functional paradigm to map a string array alements to the b64 encoded veersion
        { return new List<String>(input.Select(str => Convert.ToBase64String(Encoding.UTF8.GetBytes(str))).ToArray()); }


        /// <summary>
        /// unencode a single b64string to its.. unencoded string
        /// </summary>
        /// <param name="input"></param>
        /// <returns>unencode a single b64string to its.. unencoded string</returns>
        public static String FromB64(String input)  //reverse, just in case its needed
        { return Encoding.UTF8.GetString(Convert.FromBase64String(input)); }


        /// <summary>
        /// unencode a  b64string array to its.. unencoded string array
        /// </summary>
        /// <param name="input"></param>
        /// <returns>unencode a  b64string array to its.. unencoded string array</returns>
        public static String[] FromB64(String[] input)         //use functional paradigm to map a string array alements  from the b64 encoded veersion to original string
        { return input.Select(str => Encoding.UTF8.GetString(Convert.FromBase64String(str))).ToArray(); }


        /// <summary>
        /// unencode a  b64string array to its.. unencoded string array
        /// </summary>
        /// <param name="input"></param>
        /// <returns>unencode a  b64string lists to its.. unencoded string array</returns>
        public static List<String> FromB64(List<String> input)         //use functional paradigm to map a string array alements from the b64 encoded veersion to original string
        { return new List<String>(input.Select(str => Encoding.UTF8.GetString(Convert.FromBase64String(str))).ToArray()); }
    }
}
