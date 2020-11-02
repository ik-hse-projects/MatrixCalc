using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MatrixCalc.Core;

namespace MatrixCalc
{
    /// <summary>
    /// Хранилище переменных, которые загружаются в файл.
    /// </summary>
    public class Storage
    {
        public readonly Dictionary<string, IBasic> Values = new Dictionary<string, IBasic>();

        private (string name, IBasic value)? ParseValue(IEnumerator<string> enumerator, string[] splitted,
            HashSet<string>? varNames)
        {
            var type = splitted[0];
            var name = splitted[1];

            if (varNames != null && !varNames.Contains(name))
            {
                if (type == "Matrix" || type == "Solution")
                {
                    while (enumerator.MoveNext())
                    {
                        if (enumerator.Current == "")
                        {
                            return null;
                        }
                    }
                }

                return null;
            }

            var x = 0;
            var y = 0;
            if (splitted.Length >= 3)
            {
                if (!int.TryParse(splitted[2], out x) || x <= 0)
                {
                    throw new ComputationError($"Некорректное значение ({splitted[3]})");
                }
            }

            if (splitted.Length >= 4)
            {
                if (!int.TryParse(splitted[3], out y) || y <= 0)
                {
                    throw new ComputationError($"Некорректное значение ({splitted[4]})");
                }
            }

            IBasic value = type switch
            {
                "Number" => new Number(x),
                "IdentityMatrix" => new IdentityMatrix(x),
                "Matrix" when splitted.Length == 4 => new Matrix(x, y).FillFrom(enumerator),
                "Solution" when splitted.Length == 4 => new Solution(new Matrix(x, y)).FillFrom(enumerator),
                _ => throw new ComputationError($"Некорректное объявление переменной с типом {type}")
            };

            return (name, value);
        }

        /// <summary>
        /// Загружает перечисленные переменные из файла в память.
        /// </summary>
        /// <param name="filename">Файл, который содержит переменные</param>
        /// <param name="varNames">Имена переменных, которые надо загрузить. null, если загружать все.</param>
        public void Load(string filename, HashSet<string>? varNames = null)
        {
            string[] lines;
            try
            {
                lines = File.ReadAllLines(filename);
            }
            catch (Exception)
            {
                lines = null;
            }

            if (lines == null)
            {
                throw new ComputationError("Невозможно прочесть файл.");
            }

            var enumerator = lines
                .Where(ln => !ln.StartsWith("#") && !ln.StartsWith("//"))
                .GetEnumerator();
            while (enumerator.MoveNext())
            {
                var line = enumerator.Current;
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                var splitted = line.Split();
                if (splitted.Length < 2)
                {
                    throw new ComputationError($"Некорректное объявление переменной: {line}");
                }

                var parsed = ParseValue(enumerator, splitted, varNames);
                if (parsed != null)
                {
                    var (name, value) = parsed.Value;
                    Values[name] = value;
                }
            }
        }

        private static void SaveMatrix(List<string> output, Matrix matrix)
        {
            for (var row = 1; row <= matrix.Size.rows; row++)
            {
                var line = string.Join('\t', Enumerable
                    .Range(1, matrix.Size.columns)
                    .Select(column => matrix[row, column]));
                output.Add(line);
            }

            output.Add("");
        }

        public void Save(string filename)
        {
            var lines = new List<string>();
            foreach (var (name, value) in Values)
            {
                switch (value)
                {
                    case Solution solution:
                        lines.Add($"Solution {name} {solution.Size.rows} {solution.Size.columns}");
                        SaveMatrix(lines, solution);
                        break;
                    case Matrix matrix:
                        lines.Add($"Matrix {name} {matrix.Size.rows} {matrix.Size.columns}");
                        SaveMatrix(lines, matrix);
                        break;
                    case Number number:
                        lines.Add($"Number {name} {number.Value}");
                        break;
                    case IdentityMatrix identity:
                        lines.Add($"IdentityMatrix {name} {identity.Value}");
                        break;
                    default:
                        throw new InternalError($"Невозможно сохранить {value}.");
                }
            }

            try
            {
                File.WriteAllLines(filename, lines);
            }
            catch (Exception e)
            {
                throw new ComputationError($"Не удалось записать файл: {e}");
            }
        }
    }
}