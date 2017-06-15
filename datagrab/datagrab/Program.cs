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
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            WORKING_FOLDER = args[0];
            Directory.SetCurrentDirectory(WORKING_FOLDER);
            var terraGrab = Assembly.GetExecutingAssembly().GetType("datagrab.Hook", true, true);
            ((Game) Activator.CreateInstance(terraGrab)).Run();
        }
    }
#endif
}

