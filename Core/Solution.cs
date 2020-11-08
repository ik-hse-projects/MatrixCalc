using System.Collections.Generic;
using System.Text;

namespace MatrixCalc.Core
{
    /// <inheritdoc />
    /// <summary>
    /// Решение СЛАУ. Представляет из себя матрицу [в каноническом виде], но с другим форматом вывода пользователю.
    /// </summary>
    public class Solution : Matrix
    {
        /// <inheritdoc />
        public Solution(Matrix matrix) : base(matrix.Size, matrix.Data)
        {
        }

        /// <inheritdoc />
        public override string[,] Display()
        {
            var row = 1;
            var column = 1;
            var lines = new List<string>();
            while (row <= Size.rows)
            {
                // Двигаемся вправо.
                while (column < Size.columns && this[row, column].IsZero())
                {
                    column++;
                }

                if (column >= Size.columns)
                {
                    // У нас есть свободные переменные. Надо убедиться, что правый столбец состоит из нулей.
                    for (var i = row; i <= Size.rows; i++)
                    {
                        if (!this[i, Size.columns].IsZero())
                        {
                            return new[,] {{"Нет решений."}};
                        }
                    }

                    break;
                }

                // Затем "вычисляем" значение переменной.
                var line = new StringBuilder();

                var free = this[row, Size.columns];
                line.Append(' ').Append(free.ToString("0.#######"));

                for (var i = column + 1; i < Size.columns; i++)
                {
                    var number = -this[row, i];
                    if (!number.IsZero())
                    {
                        var sign = "+";
                        if (number < 0)
                        {
                            number = -number;
                            sign = "-";
                        }

                        line.Append(' ').Append($"{sign} {number:0.#######}⋅x{i}");
                    }
                }

                if (line.Length == 0)
                {
                    line.Append(" 0");
                }

                lines.Add($"x{column} ={line}");
                row++;
            }

            var result = new string[lines.Count, 1];
            for (var i = 0; i < lines.Count; i++)
            {
                result[i, 0] = lines[i];
            }

            return result;
        }

        public new Solution FillFrom(IEnumerator<string> lines)
        {
            Data = new Gauss(base.FillFrom(lines), false).Calculate().Result.Data;
            return this;
        }
    }
}