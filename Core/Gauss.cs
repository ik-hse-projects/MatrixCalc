namespace MatrixCalc.Core
{
    /// <summary>
    /// Реализация элементарных преобразований над строками.
    /// </summary>
    public static class ElementaryMutations
    {
        /// <summary>
        /// Меняет местами строки `n` и `m`.
        /// </summary>
        public static void Swap(this Matrix matrix, int n, int m)
        {
            for (var i = 1; i <= matrix.Size.columns; i++)
            {
                var a = matrix[n, i];
                var b = matrix[m, i];
                matrix[n, i] = b;
                matrix[m, i] = a;
            }
        }

        /// <summary>
        /// Добавляет к строке `row` строку `another`, умноженную на `multiplier`. Реузльтат помещается в строку `row`.
        /// </summary>
        public static void AddOneRowToAnother(this Matrix matrix, int row, int another, decimal multiplier)
        {
            for (var i = 1; i <= matrix.Size.columns; i++)
            {
                var value = matrix[row, i];
                value += multiplier * matrix[another, i];
                matrix[row, i] = value;
            }
        }

        /// <summary>
        /// Умножает строку на число.
        /// </summary>
        public static void MultiplyRow(this Matrix matrix, int row, decimal multiplyBy)
        {
            for (var i = 1; i <= matrix.Size.columns; i++)
            {
                var value = matrix[row, i];
                value *= multiplyBy;
                matrix[row, i] = value;
            }
        }
    }

    /// <summary>
    /// Результат применеия метода Гаусса.
    /// </summary>
    public readonly struct GaussResult
    {
        public readonly Matrix Result;
        public readonly Matrix? Trace;

        public GaussResult(Matrix result, Matrix? trace)
        {
            Result = result;
            Trace = trace;
        }
    }

    /// <summary>
    /// Реализация метода Гаусса.
    /// </summary>
    public class Gauss
    {
        /// <summary>
        /// Кол-во столбцов в матрице.
        /// </summary>
        private readonly int maxCol;

        /// <summary>
        /// Кол-во строк в матрице.
        /// </summary>
        private readonly int maxRow;

        /// <summary>
        /// Матрица, которая будет приведена к каноничесекому виду.
        /// </summary>
        private readonly Matrix result;

        /// <summary>
        /// Единичная матрица, над которой будут произведены все те же преобразования, что и над основной.
        /// </summary>
        private readonly Matrix? trace;

        /// <summary>
        /// Инициализирует метод Гаусса.
        /// </summary>
        /// <param name="matrix">Матрица.</param>
        /// <param name="trace">Требуется ли создать матрицу элементарных преобразований.</param>
        public Gauss(Matrix matrix, bool trace)
        {
            (maxRow, maxCol) = matrix.Size;
            result = matrix.Clone();
            if (trace)
            {
                this.trace = new IdentityMatrix(1).ToMatrix(maxRow, maxCol);
            }
        }

        /// <summary>
        /// Умножает строку на число.
        /// </summary>
        private void MultiplyRow(int row, decimal multiplier)
        {
            result.MultiplyRow(row, multiplier);
            trace?.MultiplyRow(row, multiplier);
        }

        /// <summary>
        /// Прибавляет к одной строке другую, умноженную на число.
        /// </summary>
        private void AddOneRowToAnother(int row, int another, decimal multiplier)
        {
            result.AddOneRowToAnother(row, another, multiplier);
            trace?.AddOneRowToAnother(row, another, multiplier);
        }

        /// <summary>
        /// Меняет местами две строки.
        /// </summary>
        private void Swap(int n, int m)
        {
            result.Swap(n, m);
            trace?.Swap(n, m);
        }

        /// <summary>
        /// Методом Гаусса приводит данную матрицу к каноническому виду.
        /// Описание алгоритма в человечском виде: https://wiki.bularond.ru/ru/algebra/second_lesson#теорема-о-методе-гаусса
        /// </summary>
        public GaussResult Calculate()
        {
            var column = 1;
            var row = 1;
            while (row <= maxRow && column <= maxCol)
            {
                Step1:
                var current = result[row, column];
                if (!current.IsZero())
                {
                    // Step 1.
                    MultiplyRow(row, 1 / current);

                    // Мы только что поедлили на current, поэтому здесь должна быть единица.
                    // Но decimal не очень надёжны, так что лучше сделать её явно:
                    result[row, column] = 1;

                    for (var i = 1; i <= maxRow; i++)
                    {
                        if (i != row)
                        {
                            var other = result[i, column];
                            if (!other.IsZero())
                            {
                                AddOneRowToAnother(i, row, -other);
                            }

                            // У нас тут и так должен быть ноль, но погрешности вычислений могут всё испортить.
                            result[i, column] = 0;
                        }
                    }

                    column++;
                    row++;
                }
                else
                {
                    // Step 2.
                    for (var i = row + 1; i <= maxRow; i++)
                    {
                        if (!result[i, column].IsZero())
                        {
                            Swap(row, i);
                            goto Step1;
                        }
                    }

                    // Step 3.
                    column++;
                }
            }

            return new GaussResult(result, trace);
        }
    }
}