using System;
using System.Collections.Generic;
using System.Linq;
using MatrixCalc.Core;

namespace MatrixCalc
{
    /// <summary>
    /// Точка входа.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// Запрашивает у пользователя матрицу.
        /// </summary>
        /// <param name="context">Контекст, в котором вычисляются знаяения матрицы.</param>
        /// <returns>Заполненную матрицу.</returns>
        public static Matrix ReadMatrix(Context context)
        {
            Console.WriteLine("Вводите матрицу.\n" +
                              "Разделяйте значения точкой с запятой (;), а строки нажатием Enter.\n" +
                              "Чтобы завершить ввод, нажмите Enter на пустой строке.");
            var lines = new List<string>();
            while (true)
            {
                var ln = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(ln))
                {
                    break;
                }

                lines.Add(ln);
            }

            if (lines.Count == 0)
            {
                return new Matrix(0, 0);
            }

            var splitted = lines.Select(s => s.Split(";")).ToArray();
            var columns = splitted.Max(values => values.Length);
            var rows = lines.Count;

            var matrix = new Matrix(rows, columns);
            for (var row = 0; row < rows; row++)
            {
                var rowValues = splitted[row];
                for (var column = 0; column < rowValues.Length; column++)
                {
                    var value = HighParser.Parse(context, rowValues[column]);
                    var number = value.AsNumber(true)?.Value;
                    matrix[row + 1, column + 1] = number ?? 0;
                }
            }

            return matrix;
        }

        /// <summary>
        /// Запрашивает у пользователя значение и вычисляет его.
        /// </summary>
        /// <param name="context">Контекст, в котором порисходят вычисления.</param>
        /// <param name="question">Вопрос к аользователю.</param>
        /// <returns>Вычисленное значение.</returns>
        private static IBasic? AskUser(this Context context, string question)
        {
            Console.WriteLine(question);
            Console.Write("> ");
            var ln = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(ln))
            {
                return null;
            }

            return HighParser.Parse(context, ln);
        }

        /// <summary>
        /// Задаёт пользователю вопрос, на который предполагается ответ да/нет.
        /// </summary>
        /// <param name="question">Вопрос к пользователю.</param>
        /// <returns>true, если пользователь ответил да.</returns>
        private static bool AskBool(string question)
        {
            Console.Write($"{question} (y/n) ");
            while (true)
            {
                var key = Console.ReadKey(true);
                switch (key.KeyChar)
                {
                    case 'y':
                        Console.WriteLine("y");
                        return true;
                    case 'n':
                        Console.WriteLine("n");
                        return false;
                }
            }
        }

        /// <summary>
        /// Заполняет переданную матрицу случайными целыми числами в указанном диапазоне
        /// </summary>
        private static void FillMatrixWithRandom(this Matrix matrix, int minValue, int maxValue)
        {
            var rng = new Random();
            matrix.MapInplace(_ => rng.Next(minValue, maxValue));
        }

        /// <summary>
        /// Заполняет переданную матрицу случайными числами с плавающей запятой в указанном диапазоне
        /// </summary>
        private static void FillMatrixWithRandom(this Matrix matrix, double minValue, double maxValue)
        {
            var rng = new Random();
            var delta = maxValue - minValue;
            matrix.MapInplace(_ => (decimal)(rng.NextDouble() * delta + minValue));
        }

        /// <summary>
        /// Генерирует случайную матрицу, запрашивая от пользователя все необходимые параметры.
        /// </summary>
        /// <param name="context">Контекст, в котором происходят вычисления.</param>
        /// <returns>Сгенерированная матрицы.</returns>
        public static Matrix GenerateMatrix(Context context)
        {
            Console.WriteLine("Генерация случайной матрицы.");
            Console.WriteLine();

            var rows = context.AskUser("Количество строк:").AsNumber()!.Int();
            var columns = context.AskUser("Количество столбцов:").AsNumber()!.Int();
            var result = new Matrix(rows, columns);
            var isInteger = AskBool("Генерировать только целые числа?");
            if (isInteger)
            {
                var minValue = context.AskUser("Минимальное значение:").AsNumber()!.Int();
                var maxValue = context.AskUser("Максимальное значение:").AsNumber()!.Int();
                result.FillMatrixWithRandom(minValue, maxValue);
            }
            else
            {
                var minValue = context.AskUser("Минимальное значение:").AsNumber()!.Value;
                var maxValue = context.AskUser("Максимальное значение:").AsNumber()!.Value;
                result.FillMatrixWithRandom((double) minValue, (double) maxValue);
            }

            return result;
        }

        /// <summary>
        /// Считывает от пользователя одну строку, вычисляет всё необходимое и выводит результат.
        /// </summary>
        /// <param name="context">Конекст, в котором происходят вычисления.</param>
        private static void SingleStep(Context context)
        {
            Console.Write("> ");
            var input = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(input) || input.StartsWith("#") || input.StartsWith("//"))
            {
                Console.WriteLine();
                return;
            }

            if (input.StartsWith(":"))
            {
                Prompt.HandleSpecialCommand(context, input);
                return;
            }

            var output = HighParser.Parse(context, input);
            Console.WriteLine(output?.Display().Render());
            context.SetVariable("_", output);
        }

        /// <summary>
        /// Точка входа.
        /// </summary>
        private static void Main()
        {
            Console.WriteLine("Настоятельно рекомендуется прочитать справку. Для этого надо ввести «:help»");
            var context = new Context();
            while (true)
            {
                var errorOccurred = false;
                context = context.Snapshot();

                try
                {
                    SingleStep(context);
                }
                catch (OutOfMemoryException)
                {
                    Console.WriteLine("О нет! Не хватило памяти!");
                    Console.WriteLine("Эта программа не способна работать с большими объёмами данных. " +
                                      "Рекомендую использовать MatLab или что-то ещё.");
                    errorOccurred = true;
                }
                catch (Error e)
                {
                    errorOccurred = true;
                    Console.WriteLine($"Произошла ошибка {e.Kind}, но вы всегда можете попробовать ещё раз.");
                    Console.WriteLine(e.Display().IndentLines());
                }

                if (errorOccurred)
                {
                    Console.WriteLine("Любые изменения переменных не были сохранены.");
                    context = context.Rollback();
                }
                else
                {
                    context = context.Save();
                }
            }
        }
    }
}