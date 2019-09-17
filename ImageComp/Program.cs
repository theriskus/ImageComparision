using System;
using System.Collections.Generic;
using System.Linq;
using XnaFan.ImageComparison;

// Created in 2012 by Jakob Krarup (www.xnafan.net).
// Use, alter and redistribute this code freely,
// but please leave this comment :)

namespace ConsoleComparison
{

    /// <summary>
    /// Console program which compares two iages and returns the difference in percentage as an errorlevel between zero and a hundred (both included).
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("IMAGE COMPARISON CONSOLE APPLICATION");
                Console.WriteLine("  by Jakob 'xnafan' Krarup, January 2013");
                Console.WriteLine("  Licensed under The Code Project Open License (CPOL)");
                Console.WriteLine("  Compares two images and returns the difference in percent");
                Console.WriteLine("    as an errorlevel (0 to 100)");
                Console.WriteLine();
                Console.WriteLine(@"  Usage: 'ImageComparisonConsole.exe [image1 path] [image2 path]");
                Console.WriteLine(@"  Sample usage: 'ImageComparisonConsole.exe ""c:\image1.jpg"" ""c:\image2.bmp""");
                return -1;
            }
            else
            {
                Console.WriteLine("HASH: " + ImageTool.HashGenerator(args[0]));
                return 0;
            }
        }

        
    }
}
