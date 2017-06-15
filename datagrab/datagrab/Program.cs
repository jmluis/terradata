using Microsoft.Xna.Framework;
using System;
using System.IO;
using System.Reflection;

namespace datagrab
{
#if WINDOWS || XBOX
    static class Program
    {
        public static string WORKING_FOLDER;
        public static string CONTENT_PATH;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            WORKING_FOLDER = args[0];
            CONTENT_PATH = WORKING_FOLDER + @"\Content";
            Directory.SetCurrentDirectory(WORKING_FOLDER);
            var terraGrab = Assembly.GetExecutingAssembly().GetType("datagrab.Hook", true, true);
            ((Game)Activator.CreateInstance(terraGrab)).Run();
        }
    }
#endif
}

