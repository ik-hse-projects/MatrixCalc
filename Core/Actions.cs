using System;

namespace MatrixCalc.Core
{
    /// <summary>
    /// Содержит все реализованные действия над объектами.
    /// </summary>
    public static class Actions
    {
        // Здесь 37 методов, из которых большинство тривиальны. К каждому написана подробная документация.
        // Получилось 224 строки кода и 207 строк комментариев (и 56 пустых).

        /// <summary>
        /// Сложение.
        /// </summary>
        /// <param name="a">Одно слагаемое.</param>
        /// <param name="b">Другое слагаемое.</param>
        /// <returns>Результат сложения.</returns>
        public static Number Add(this Number a, Number b) => new Number(a.Value + b.Value);

        /// <summary>
        /// Сложение.
        /// </summary>
        /// <param name="a">Одно слагаемое.</param>
        /// <param name="b">Другое слагаемое.</param>
        /// <returns>Результат сложения.</returns>
        public static Matrix Add(this Matrix a, IdentityMatrix b)
        {
            var result = a.Clone();
            for (var i = 1; i <= a.MinDimension; i++)
            {
                result[i, i] = a[i, i] + b.Value;
            }

            return result;
        }

        /// <summary>
        /// Сложение.
        /// </summary>
        /// <param name="a">Одно слагаемое.</param>
        /// <param name="b">Другое слагаемое.</param>
        /// <returns>Результат сложения.</returns>
        public static Matrix Add(this IdentityMatrix a, Matrix b) => b.Add(a);

        /// <summary>
        /// Сложение.
        /// </summary>
        /// <param name="a">Одно слагаемое.</param>
        /// <param name="b">Другое слагаемое.</param>
        /// <returns>Результат сложения.</returns>
        public static IdentityMatrix Add(this IdentityMatrix a, IdentityMatrix b) =>
            new IdentityMatrix(a.Value + b.Value);

        /// <summary>
        /// Сложение.
        /// </summary>
        /// <param name="a">Одно слагаемое.</param>
        /// <param name="b">Другое слагаемое.</param>
        /// <returns>Результат сложения.</returns>
        public static Matrix Add(this Matrix a, Matrix b) => a.Zip(b, (x, y) => x + y);

        /// <summary>
        /// Умножение на -1.
        /// </summary>
        /// <param name="number">Что нужно умножить на -1.</param>
        /// <returns>Результат умножения на -1.</returns>
        public static Number Negate(this Number number) => new Number(-number.Value);

        /// <summary>
        /// Умножение на -1.
        /// </summary>
        /// <param name="matrix">Что нужно умножить на -1.</param>
        /// <returns>Результат умножения на -1.</returns>
        public static Matrix Negate(this Matrix matrix) => matrix.Multiply(new Number(-1));

        /// <summary>
        /// Умножение на -1.
        /// </summary>
        /// <param name="matrix">Что нужно умножить на -1.</param>
        /// <returns>Результат умножения на -1.</returns>
        public static IdentityMatrix Negate(this IdentityMatrix matrix) => new IdentityMatrix(-matrix.Value);

        /// <summary>
        /// Вычитание.
        /// </summary>
        /// <param name="a">Уменьшаемое.</param>
        /// <param name="b">Вычитаемое.</param>
        /// <returns>Результат вычитания из уменьшаемого вычитаемого.</returns>
        public static Number Subtract(this Number a, Number b) => new Number(a.Value - b.Value);

        /// <summary>
        /// Вычитание.
        /// </summary>
        /// <param name="a">Уменьшаемое.</param>
        /// <param name="b">Вычитаемое.</param>
        /// <returns>Результат вычитания из уменьшаемого вычитаемого.</returns>
        public static Matrix Subtract(this Matrix a, IdentityMatrix b) => a.Add(b.Negate());

        /// <summary>
        /// Вычитание.
        /// </summary>
        /// <param name="a">Уменьшаемое.</param>
        /// <param name="b">Вычитаемое.</param>
        /// <returns>Результат вычитания из уменьшаемого вычитаемого.</returns>
        public static Matrix Subtract(this IdentityMatrix a, Matrix b) => b.Add(b.Negate());

        /// <summary>
        /// Вычитание.
        /// </summary>
        /// <param name="a">Уменьшаемое.</param>
        /// <param name="b">Вычитаемое.</param>
        /// <returns>Результат вычитания из уменьшаемого вычитаемого.</returns>
        public static Matrix Subtract(this Matrix a, Matrix b) => a.Zip(b, (x, y) => x - y);

        /// <summary>
        /// Вычитание.
        /// </summary>
        /// <param name="a">Уменьшаемое.</param>
        /// <param name="b">Вычитаемое.</param>
        /// <returns>Результат вычитания из уменьшаемого вычитаемого.</returns>
        public static IdentityMatrix Subtract(this IdentityMatrix a, IdentityMatrix b) =>
            new IdentityMatrix(a.Value - b.Value);

        /// <summary>
        /// Умножение.
        /// </summary>
        /// <param name="identity">Один множитель.</param>
        /// <param name="matrix">Ещё один множитель.</param>
        /// <returns>Результат умножения.</returns>
        public static Matrix Multiply(this IdentityMatrix identity, Matrix matrix) => matrix.Multiply(identity);

        /// <summary>
        /// Умножение.
        /// </summary>
        /// <param name="identity">Один множитель.</param>
        /// <param name="matrix">Ещё один множитель.</param>
        /// <returns>Результат умножения.</returns>
        public static Matrix Multiply(this Matrix matrix, IdentityMatrix identity) =>
            matrix.Map(x => x * identity.Value);

        /// <summary>
        /// Умножение.
        /// </summary>
        /// <param name="a">Один множитель.</param>
        /// <param name="b">Ещё один множитель.</param>
        /// <returns>Результат умножения.</returns>
        public static IdentityMatrix Multiply(this IdentityMatrix a, IdentityMatrix b) =>
            new IdentityMatrix(a.Value * b.Value);

        /// <summary>
        /// Умножение.
        /// </summary>
        /// <param name="a">Один множитель.</param>
        /// <param name="b">Ещё один множитель.</param>
        /// <returns>Результат умножения.</returns>
        public static Number Multiply(this Number a, Number b) => new Number(a.Value * b.Value);

        /// <summary>
        /// Матричное умножение матриц.
        /// </summary>
        /// <param name="left">Матрица слева.</param>
        /// <param name="right">Матрица справа.</param>
        /// <returns>Резульат умножения матрицы слева на матрицу справа.</returns>
        /// <exception cref="ComputationError">Если матрицы неправильных размеров.</exception>
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
                    var sum = 0m;
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
        /// <param name="a">Одна матрица.</param>
        /// <param name="b">Другая матрица.</param>
        /// <returns>Результат умножения.</returns>
        public static Matrix MultiplyElements(this Matrix a, Matrix b) => a.Zip(b, (x, y) => x * y);

        /// <summary>
        /// Деление.
        /// </summary>
        /// <param name="left">Делимое.</param>
        /// <param name="right">Делитель.</param>
        /// <returns>Результат деления делимого на делитель.</returns>
        public static Number Divide(this Number left, Number right) => new Number(left.Value.SafeDiv(right.Value));

        /// <summary>
        /// Деление.
        /// </summary>
        /// <param name="left">Делимое.</param>
        /// <param name="right">Делитель.</param>
        /// <returns>Результат деления делимого на делитель.</returns>
        public static IdentityMatrix Divide(this IdentityMatrix left, IdentityMatrix right) =>
            new IdentityMatrix(left.Value.SafeDiv(right.Value));

        /// <summary>
        /// Деление.
        /// </summary>
        /// <param name="left">Делимое.</param>
        /// <param name="right">Делитель.</param>
        /// <returns>Результат деления делимого на делитель.</returns>
        public static Matrix Divide(this Matrix left, IdentityMatrix right) => left.Map(x => x.SafeDiv(right.Value));

        /// <summary>
        /// Возведение в степень.
        /// </summary>
        /// <param name="number">Основание.</param>
        /// <param name="power">Показатель.</param>
        /// <returns>Результат возведения в степень.</returns>
        public static Number Power(this Number number, decimal power)
        {
            return new Number((decimal) Math.Pow((double) number.Value, (double) power));
        }

        /// <summary>
        /// Возведение в степень.
        /// </summary>
        /// <param name="matrix">Основание.</param>
        /// <param name="power">Показатель.</param>
        /// <returns>Результат возведения в степень.</returns>
        public static IdentityMatrix Power(this IdentityMatrix matrix, decimal power)
        {
            return new IdentityMatrix((decimal) Math.Pow((double) matrix.Value, (double)power));
        }

        /// <summary>
        /// Возведение в степень.
        /// </summary>
        /// <param name="matrix">Основание.</param>
        /// <param name="power">Показатель.</param>
        /// <returns>Результат возведения в степень.</returns>
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

        /// <summary>
        /// Нахождение обратного.
        /// </summary>
        /// <param name="number">Для чего нужно найти обратное.</param>
        /// <returns>Обратное.</returns>
        public static Number Invert(this Number number) => new Number(1m.SafeDiv(number.Value));

        /// <summary>
        /// Нахождение обратного.
        /// </summary>
        /// <param name="identity">Для чего нужно найти обратное.</param>
        /// <returns>Обратное.</returns>
        public static IdentityMatrix Invert(this IdentityMatrix identity) =>
            new IdentityMatrix(1m.SafeDiv(identity.Value));

        /// <summary>
        /// Нахождение обратной матрицы.
        /// </summary>
        /// <param name="matrix">Обычная матрица.</param>
        /// <returns>Обратная к данной матрица.</returns>
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

        /// <summary>
        /// Решение СЛАУ.
        /// </summary>
        /// <param name="matrix">СЛАУ в матричном виде.</param>
        /// <returns>Результат решения.</returns>
        public static Solution Solve(this Matrix matrix) => new Solution(new Gauss(matrix, false).Calculate().Result);

        /// <summary>
        /// Приведение матрицы к каноническому виду.
        /// </summary>
        /// <param name="matrix">Матрица.</param>
        /// <returns>Канонический вид матрицы.</returns>
        public static Matrix Canonical(this Matrix matrix) => new Gauss(matrix, false).Calculate().Result;

        /// <summary>
        /// Определитель матрицы.
        /// </summary>
        /// <param name="matrix">Матрица.</param>
        /// <returns>Определитель</returns>
        /// <exception cref="ComputationError">Если матрица не квадратная.</exception>
        public static Number Determinant(Matrix matrix)
        {
            if (!matrix.IsSquare)
            {
                throw new ComputationError("Определитель определён только для квадратных матриц.")
                {
                    Right = matrix
                };
            }

            var gauss = new Gauss(matrix, false).Calculate().Result;
            var result = 1m;
            for (var i = 1; i <= matrix.MinDimension; i++)
            {
                result *= gauss[i, i];
            }

            return new Number(result);
        }

        /// <summary>
        /// След матрицы: https://ru.wikipedia.org/wiki/След_матрицы
        /// </summary>
        /// <param name="matrix">Матрица.</param>
        /// <returns>След матрицы.</returns>
        /// <exception cref="ComputationError">Если матрица не квадратная.</exception>
        public static Number Trace(Matrix matrix)
        {
            if (!matrix.IsSquare)
            {
                throw new ComputationError("След матрицы определён только для квадратных матриц.")
                {
                    Right = matrix
                };
            }

            var result = 0m;
            for (var i = 1; i < matrix.MinDimension; i++)
            {
                result += matrix[i, i];
            }

            return new Number(result);
        }

        /// <summary>
        /// Транспонирование матрицы.
        /// </summary>
        /// <param name="matrix">Матрица.</param>
        /// <returns>Траспонированная матрица.</returns>
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

        /// <summary>
        /// Приклеивание матрицы справа.
        /// </summary>
        /// <param name="left">Матрица, которая будет слева.</param>
        /// <param name="right">Матрица, которая будет справа.</param>
        /// <returns>Блочная матрица, составленная из данных двух.</returns>
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

        /// <summary>
        /// Приклеивание матрицы снизу.
        /// </summary>
        /// <param name="top">Матрица, которая будет сверхк.</param>
        /// <param name="bottom">Матрица, которая будет снизу.</param>
        /// <returns>Блочная матрица, составленная из данных двух.</returns>
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
