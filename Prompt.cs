using System;
using System.Collections.Generic;
using System.Linq;
using MatrixCalc.Core;

namespace MatrixCalc
{
    /// <summary>
    /// Здесь находится всё, связанное с обработкой спец. команд.
    /// </summary>
    public static class Prompt
    {
        /// <summary>
        /// Обработчик команды `:help`.
        /// </summary>
        private static void DoHelp(string[] args)
        {
            if (args.Length > 1)
            {
                Help.ShowHelp(string.Join(' ', args.Skip(1)));
            }
            else
            {
                Help.ShowHelp();
            }
        }

        /// <summary>
        /// Извлекает из аргументов команды имя файла и список переменных, если они указаны.
        /// </summary>
        /// <param name="args">Список аргументов</param>
        private static (string filename, IEnumerable<string>? variables) ExtractFilenameAndVariables(string[] args)
        {
            if (args.Length < 2)
            {
                throw new ParsingError("Нужно обязательно ввести имя файла.");
            }

            var filename = args[1];
            var vars = args.Skip(2).ToArray();
            if (vars.Length == 0)
            {
                vars = null;
            }

            return (filename, vars);
        }

        /// <summary>
        /// Обработчик команды :load.
        /// </summary>
        private static void DoLoad(Context context, string[] args)
        {
            var (filename, vars) = ExtractFilenameAndVariables(args);
            var variables = vars?.ToHashSet();
            var storage = new Storage();
            storage.Load(filename, variables);
            if (variables != null && storage.Values.Count != variables.Count)
            {
                variables.ExceptWith(storage.Values.Keys.ToHashSet());
                Console.WriteLine("Не удалось загрузить некоторые переменные:");
                foreach (var variable in variables)
                {
                    Console.WriteLine($"   {variable}");
                }
            }

            foreach ((string name, IBasic value) in storage.Values)
            {
                context.SetVariable(name, value);
            }
        }

        /// <summary>
        /// Обработчик команды :save.
        /// </summary>
        private static void DoSave(Context context, string[] args)
        {
            var (filename, vars) = ExtractFilenameAndVariables(args);
            var storage = new Storage();
            vars ??= context.ListVariables();
            foreach (var name in vars)
            {
                storage.Values[name] = context.GetVariable(name);
            }

            storage.Save(filename);
        }

        /// <summary>
        /// Обрабатывает специальные команды (начинаются с :)
        /// </summary>
        /// <param name="context">Контекст, в котором происходят вычисления.</param>
        /// <param name="line">Строка с командой.</param>
        public static void HandleSpecialCommand(Context context, string line)
        {
            var splitted = line.Trim().Split();
            var command = splitted[0];
            switch (command)
            {
                case ":help":
                    DoHelp(splitted);
                    break;
                case ":load":
                    DoLoad(context, splitted);
                    break;
                case ":save":
                    DoSave(context, splitted);
                    break;
                case ":exit":
                {
                    Console.WriteLine("До новых встреч!");
                    Environment.Exit(0);
                    break;
                }
            }
        }
    }
}