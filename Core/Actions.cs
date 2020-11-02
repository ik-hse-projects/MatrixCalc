using System;

namespace MatrixCalc.Core
{
    /// <summary>
    /// Содержит все реализованные действия над объектами.
    /// </summary>
    public static class Actions
    {
        // Здесь 37 методов, из которых большинство тривиальны. Если я не написал к ним всем документацию, то прошу меня
        // извинить. В конце-концов в этом файле имя метода и типы должны прерасно описывать что именно он делает.
        // Если действительно что-то непонятно, то команды в Commands.cs все имеют хорошее описание.

        public static Number Add(this Number a, Number b)
        {
            return new Number(a.Value + b.Value);
        }

        public static Matrix Add(this Matrix a, IdentityMatrix b)
        {
            var result = a.Clone();
            for (var i = 1; i <= a.MinDimension; i++)
            {
                result[i, i] = a[i, i] + b.Value;
            }

            return result;
        }

        public static Matrix Add(this IdentityMatrix a, Matrix b)
        {
            return b.Add(a);
        }

        public static IdentityMatrix Add(this IdentityMatrix a, IdentityMatrix b)
        {
            return new IdentityMatrix(a.Value + b.Value);
        }

        public static Matrix Add(this Matrix a, Matrix b)
        {
            return a.Zip(b, (x, y) => x + y);
        }

        public static Number Negate(this Number number)
        {
            return new Number(-number.Value);
        }

        public static Matrix Negate(this Matrix matrix)
        {
            return matrix.Multiply(new Number(-1));
        }

        public static IdentityMatrix Negate(this IdentityMatrix matrix)
        {
            return new IdentityMatrix(-matrix.Value);
        }

        public static Number Subtract(this Number a, Number b)
        {
            return new Number(a.Value - b.Value);
        }

        public static Matrix Subtract(this Matrix a, IdentityMatrix b)
        {
            return a.Add(b.Negate());
        }

        public static Matrix Subtract(this IdentityMatrix a, Matrix b)
        {
            return b.Add(b.Negate());
        }

        public static Matrix Subtract(this Matrix a, Matrix b)
        {
            return a.Zip(b, (x, y) => x - y);
        }

        public static IdentityMatrix Subtract(this IdentityMatrix a, IdentityMatrix b)
        {
            return new IdentityMatrix(a.Value - b.Value);
        }

        public static Matrix Multiply(this IdentityMatrix identity, Matrix matrix)
        {
            return matrix.Multiply(identity);
        }

        public static Matrix Multiply(this Matrix matrix, IdentityMatrix identity)
        {
            return matrix.Map(x => x * identity.Value);
        }

        public static IdentityMatrix Multiply(this IdentityMatrix a, IdentityMatrix b)
        {
            return new IdentityMatrix(a.Value * b.Value);
        }

        public static Number Multiply(this Number a, Number b)
        {
            return new Number(a.Value * b.Value);
        }

        /// <summary>
        /// Матричное умножение матриц.
        /// </summary>
        public static Matrix Multiply(this Matrix left, Matrix right)
        {
            var (n, m1) = left.Size;
            var (m2, k) = right.Size;
            if (m1 != m2)
            {
                throw new ComputationError("Невозможно умножить: размеры матриц не сходятся.")
                {
                    Left = left,
                    Right = right
                };
            }

            var result = new Matrix(n, k);
            for (var row = 1; row <= n; row++)
            {
                for (var column = 1; column <= k; column++)
                {
                    var sum = 0d;
                    for (var i = 1; i <= m1; i++)
                    {
                        sum += left[row, i] * right[i, column];
                    }

                    result[row, column] = sum;
                }
            }

            return result;
        }

        /// <summary>
        /// Поэлементное умножение матриц.
        /// </summary>
        public static Matrix MultiplyElements(this Matrix a, Matrix b)
        {
            return a.Zip(b, (x, y) => x * y);
        }

        public static Number Divide(this Number left, Number right)
        {
            return new Number(left.Value.SafeDiv(right.Value));
        }

        public static IdentityMatrix Divide(this IdentityMatrix left, IdentityMatrix right)
        {
            return new IdentityMatrix(left.Value.SafeDiv(right.Value));
        }

        public static Matrix Divide(this Matrix left, IdentityMatrix right)
        {
            return left.Map(x => x.SafeDiv(right.Value));
        }

        public static Number Power(this Number number, double power)
        {
            return new Number(Math.Pow(number.Value, power));
        }

        public static IdentityMatrix Power(this IdentityMatrix matrix, double power)
        {
            return new IdentityMatrix(Math.Pow(matrix.Value, power));
        }

        public static Matrix Power(this Matrix matrix, int power)
        {
            if (power == 0)
            {
                return new IdentityMatrix(1).ToMatrix(matrix.Size.rows, matrix.Size.columns);
            }

            if (power < 0)
            {
                matrix = matrix.Invert();
                power = -power;
            }

            var result = matrix.Clone();
            for (var i = 0; i < power; i++)
            {
                result = result.Multiply(matrix);
            }

            return result;
        }

        public static Number Invert(this Number number)
        {
            return new Number(1d.SafeDiv(number.Value));
        }

        public static IdentityMatrix Invert(this IdentityMatrix identity)
        {
            return new IdentityMatrix(1d.SafeDiv(identity.Value));
        }

        /// <summary>
        /// Нахождение обратной матрицы.
        /// </summary>
        public static Matrix Invert(this Matrix matrix)
        {
            if (!matrix.IsSquare)
            {
                throw new ComputationError("Обратная матрица существует только для квадратных матриц.")
                {
                    Left = matrix
                };
            }

            var gauss = new Gauss(matrix, true).Calculate();
            for (var i = 1; i <= gauss.Result.MinDimension; i++)
            {
                if (gauss.Result[i, i].IsZero())
                {
                    throw new ComputationError("У матрицы не существует обратной.")
                    {
                        Left = gauss.Result
                    };
                }
            }

            return gauss.Trace!;
        }

        public static Solution Solve(this Matrix matrix)
        {
            var gauss = new Gauss(matrix, false).Calculate().Result;
            return new Solution(gauss);
        }

        public static Matrix Canonical(this Matrix matrix)
        {
            return new Gauss(matrix, false).Calculate().Result;
        }

        public static Number Determinant(Matrix matrix)
        {
            if (!matrix.IsSquare)
            {
                throw new ComputationError("Определитель определён только для квадратных матриц.")
                {
                    Left = matrix
                };
            }

            var gauss = new Gauss(matrix, false).Calculate().Result;
            var result = 1d;
            for (var i = 1; i < matrix.MinDimension; i++)
            {
                result *= gauss[i, i];
            }

            return new Number(result);
        }

        /// <summary>
        /// След матрицы: https://ru.wikipedia.org/wiki/След_матрицы
        /// </summary>
        public static Number Trace(Matrix matrix)
        {
            if (!matrix.IsSquare)
            {
                throw new ComputationError("След матрицы определён только для квадратных матриц.")
                {
                    Left = matrix
                };
            }

            var result = 0d;
            for (var i = 1; i < matrix.MinDimension; i++)
            {
                result += matrix[i, i];
            }

            return new Number(result);
        }

        public static Matrix Transpose(Matrix matrix)
        {
            var result = new Matrix(matrix.Size.columns, matrix.Size.rows);
            for (var row = 1; row <= matrix.Size.rows; row++)
            {
                for (var column = 1; column <= matrix.Size.columns; column++)
                {
                    result[column, row] = matrix[row, column];
                }
            }

            return result;
        }

        public static Matrix JoinHorizontal(this Matrix left, Matrix right)
        {
            if (left.Size.rows != right.Size.rows)
            {
                throw new ComputationError(
                    "У матриц различное количество строк. Не совсем понятно как их требуется расположить.")
                {
                    Left = left,
                    Right = right
                };
            }

            var rows = left.Size.rows;
            var result = new Matrix(left.Size.rows, left.Size.columns + right.Size.columns);
            for (var row = 1; row <= rows; row++)
            {
                var position = 1;
                for (var col = 1; col <= left.Size.columns; col++)
                {
                    result[row, position] = left[row, col];
                    position++;
                }

                for (var col = 1; col <= right.Size.columns; col++)
                {
                    result[row, position] = right[row, col];
                    position++;
                }
            }

            return result;
        }

        public static Matrix JoinVertical(this Matrix top, Matrix bottom)
        {
            if (top.Size.columns != bottom.Size.columns)
            {
                throw new ComputationError(
                    "У матриц различное количество столбцов. Не совсем понятно как их требуется расположить.")
                {
                    Left = top,
                    Right = bottom
                };
            }

            var columns = top.Size.columns;
            var result = new Matrix(top.Size.rows + bottom.Size.rows, columns);
            for (var column = 1; column <= columns; column++)
            {
                var position = 1;
                for (var row = 1; row <= top.Size.rows; row++)
                {
                    result[position, column] = top[row, column];
                    position++;
                }

                for (var row = 1; row <= bottom.Size.rows; row++)
                {
                    result[position, column] = bottom[row, column];
                    position++;
                }
            }

            return result;
        }
    }
}