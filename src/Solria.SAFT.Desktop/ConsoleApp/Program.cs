﻿using Spectre.Console;
using Spectre.Console.Cli;

namespace ConsoleApp
{
    class Program
    {
        static int Main(string[] args)
        {
            AnsiConsole.Write(new FigletText("SolRIA").Centered().Color(Color.Red));
            var rule = new Rule("[bold yellow on blue]SAFT-PT/STOCKS Parser![/]");
            AnsiConsole.Write(rule);

            var app = new CommandApp<OpenFileCommand>();
            
            return app.Run(args);
        }
    }
}
