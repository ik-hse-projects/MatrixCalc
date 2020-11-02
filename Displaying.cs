using System.Linq;
using System.Text;

namespace MatrixCalc
{
    /// <summary>
    /// Вспомогательные функции для красивого вывода значений.
    /// </summary>
    public static class Displaying
    {
        public static GridFormat BoxedGrid = new GridFormat
        {
            Vertical = " │ ",
            Horizontal = '─',
            Corner = "─┼─"
        };

        public static GridFormat SpacedGrid = new GridFormat
        {
            Vertical = " ",
            Horizontal = '\0',
            Corner = " "
        };

        /// <summary>
        /// Добавляет матричные круглые скобки.
        /// </summary>
        public static string[,] Parentheses(this string[,] data)
        {
            var height = data.GetLength(0);
            var width = data.GetLength(1);
            if (height == 0)
            {
                return data;
            }

            var result = new string[height, width + 2];

            for (var row = 0; row < height; row++)
            for (var col = 0; col < width; col++)
            {
                result[row, col + 1] = data[row, col];
            }

            if (height == 1)
            {
                result[0, 0] = "(";
                result[0, width + 1] = ")";
            }
            else
            {
                // В юникоде найдётся символ для чего угодно.
                result[0, 0] = "⎛";
                result[0, width + 1] = "⎞";
                result[height - 1, 0] = "⎝";
                result[height - 1, width + 1] = "⎠";
                for (var row = 1; row < height - 1; row++)
                {
                    result[row, 0] = "⎜";
                    result[row, width + 1] = "⎜";
                }
            }

            return result;
        }

        /// <summary>
        /// Добавляет разделители указанного формата между всеми строками и столбцами.
        /// </summary>
        public static string[,] Grid(this string[,] data, GridFormat format)
        {
            var height = data.GetLength(0);
            var width = data.GetLength(1);
            if (height == 0 || width == 0)
            {
                return new[,] {{""}};
            }

            var result = new string[format.Slim ? height : height * 2 - 1, width * 2 - 1];
            var columnWidths = FindColumnWidths(data);

            for (var row = 0; row < height; row++)
            {
                for (var column = 0; column < width; column++)
                {
                    var y = format.Slim ? row : 2 * row;
                    var x = 2 * column;

                    // Center.
                    result[y, x] = data[row, column];

                    var drawRight = column != width - 1;
                    var drawBottom = !format.Slim && row != height - 1;
                    var drawCorner = drawRight && drawBottom;

                    if (drawRight)
                    {
                        result[y, x + 1] = format.Vertical;
                    }

                    if (drawBottom)
                    {
                        result[y + 1, x] = new string(format.Horizontal, columnWidths[column]);
                    }

                    if (drawCorner)
                    {
                        result[y + 1, x + 1] = format.Corner;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Вычисляет максимальные ширины всех столбцов.
        /// </summary>
        private static int[] FindColumnWidths(string[,] data)
        {
            var height = data.GetLength(0);
            var width = data.GetLength(1);

            var columnWidths = new int[width];
            for (var column = 0; column < width; column++)
            {
                var columnWidth = Enumerable.Range(0, height)
                    .Max(row => data[row, column].Length);
                columnWidths[column] = columnWidth;
            }

            return columnWidths;
        }

        /// <summary>
        /// Выравнивает содержимое всех столбцов по центру.
        /// </summary>
        public static string[,] AlignCenter(this string[,] data)
        {
            var height = data.GetLength(0);
            var width = data.GetLength(1);

            var columnWidths = FindColumnWidths(data);

            var result = new string[height, width];
            for (var row = 0; row < height; row++)
            {
                for (var column = 0; column < width; column++)
                {
                    var cell = data[row, column];
                    var targetWidth = columnWidths[column];
                    var totalPad = targetWidth - cell.Length;
                    var leftPad = totalPad / 2;
                    var rightPad = totalPad - leftPad;
                    result[row, column] = string.Format("{0}{1}{2}",
                        new string(' ', leftPad),
                        cell,
                        new string(' ', rightPad));
                }
            }

            return result;
        }

        /// <summary>
        /// Превращает таблицу строк в одну большую строку с переносами.
        /// </summary>
        public static string Render(this string[,] data)
        {
            var height = data.GetLength(0);
            var width = data.GetLength(1);

            var result = new StringBuilder();

            for (var row = 0; row < height; row++)
            {
                for (var column = 0; column < width; column++)
                {
                    result.Append(data[row, column]);
                }

                result.AppendLine();
            }

            return result.ToString();
        }

        /// <summary>
        /// Добавляет слева отступы к строке.
        /// </summary>
        public static string IndentLines(this string str)
        {
            if (str == null)
            {
                return null;
            }

            var lines = str.Split("\n");
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (i == lines.Length - 1)
                {
                    if (line != "")
                    {
                        lines[i] = $"   {line}";
                    }
                }
                else
                {
                    lines[i] = $"   {line}\n";
                }
            }

            return string.Concat(lines);
        }

        /// <summary>
        /// Описание того, как следует делать сетку.
        /// </summary>
        public struct GridFormat
        {
            /// <summary>
            /// Вертикальный разделитель. Находися между столбцов.
            /// </summary>
            public string Vertical;

            /// <summary>
            /// Горизонтальный разделитель. Находися между строк.
            /// </summary>
            public char Horizontal;

            /// <summary>
            /// То, что находится на пересечении разделителей строк и разделителей столбцов.
            /// </summary>
            public string Corner;

            /// <summary>
            /// Используется ли разделитель между строками.
            /// </summary>
            public bool Slim => Horizontal == '\0';
        }
    }
}