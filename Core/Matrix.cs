using System;
using System.Collections.Generic;

namespace MatrixCalc.Core
{
    /// <summary>
    /// Обычная матрица с числами внутри.
    /// </summary>
    public class Matrix : IBasic
    {
        /// <summary>
        /// Числа внутри.
        /// </summary>
        internal double[] Data;

        /// <summary>
        /// Внутренний конструктор, который заполняет все поля.
        /// </summary>
        protected Matrix((int rows, int columns) size, double[] data)
        {
            Data = data;
            Size = size;
        }

        /// <summary>
        /// Создаёт нулевую матрицу указанного размера.
        /// </summary>
        /// <param name="rows">Кол-во строк.</param>
        /// <param name="columns">Кол-во столбцов.</param>
        public Matrix(int rows, int columns)
        {
            Data = new double[rows * columns];
            Size = (rows, columns);
        }

        /// <summary>
        /// Создаёт нулевую матрицу указанного размера.
        /// </summary>
        public Matrix((int rows, int columns) size) : this(size.rows, size.columns)
        {
        }

        /// <summary>
        /// Размер матрицы.
        /// </summary>
        public (int rows, int columns) Size { get; }

        /// <summary>
        /// Наименьшая из размерностей матрицы: min(rows, columns).
        /// </summary>
        public int MinDimension => Size.rows < Size.columns ? Size.rows : Size.columns;

        /// <summary>
        /// Является ли эта матрица квадратной.
        /// </summary>
        public bool IsSquare => Size.columns == Size.rows;

        /// <summary>
        /// Получает и присваивает значение матрицы в указанном месте.
        /// Индексы начинаются с единицы.
        /// </summary>
        /// <param name="row">Строка матрицы.</param>
        /// <param name="column">Столбец матрицы.</param>
        public double this[int row, int column]
        {
            get => Data[GetIndex(row, column)];
            set => Data[GetIndex(row, column)] = value;
        }

        /// <inheritdoc />
        public virtual string[,] Display()
        {
            if (Size.rows == 0 || Size.columns == 0)
            {
                return new[,] {{""}}.Parentheses();
            }

            const int maxSize = 10 + 1;

            var result = new string[Math.Min(Size.rows, maxSize), Math.Min(Size.columns, maxSize)];
            for (var row = 0; row < Size.rows; row++)
            {
                for (var column = 0; column < Size.columns; column++)
                {
                    if (column == maxSize - 1)
                    {
                        result[row, column] = "…";
                        break;
                    }

                    result[row, column] = this[row + 1, column + 1].ToString("0.#######");
                }

                if (row == maxSize - 1)
                {
                    for (var column = 0; column < maxSize; column++)
                    {
                        result[row, column] = "…";
                    }

                    break;
                }
            }

            return result.AlignCenter().Grid(Displaying.BoxedGrid).Parentheses();
        }

        /// <summary>
        /// Переводит координаты вида "строка"+"столбец" в индекс в обычном массиве Data.
        /// </summary>
        private int GetIndex(int row, int column)
        {
            // Я тысячу раз пожалел, что сделал так, как принято в математике: начал отсчет с единицы

            if (row < 1 || row > Size.rows)
            {
                throw new ArgumentOutOfRangeException(nameof(row));
            }

            if (column < 1 || column > Size.columns)
            {
                throw new ArgumentOutOfRangeException(nameof(column));
            }

            return (row - 1) * Size.columns + (column - 1);
        }

        /// <summary>
        /// Создаёт новую матрицу, которая содержит такие же значения, что и текущая.
        /// </summary>
        public Matrix Clone()
        {
            var result = new Matrix(Size);
            for (var i = 0; i < Data.Length; i++)
            {
                result.Data[i] = Data[i];
            }

            return result;
        }

        /// <summary>
        /// Создаёт новую матрицу на основе применения функции к соответсвующим друг другу значениям двух матриц.
        /// </summary>
        /// <param name="other">Другая матрица, размер должен совпадать с размером текущей.</param>
        /// <param name="func">Функция.</param>
        public Matrix Zip(Matrix other, Func<double, double, double> func)
        {
            if (other.Size != Size)
            {
                throw new InternalError("У матриц не совпадают размеры.");
            }

            var res = new Matrix(Size);
            for (var i = 0; i < Data.Length; i++)
            {
                // Один из плюсов хранения значений в одбчном плоском массиве:
                // если не важны номера строк и столбцов, то можно делать вот так:
                res.Data[i] = func(Data[i], other.Data[i]);
            }

            return res;
        }

        /// <summary>
        /// Применяет указанную функцию ко всем элементам матрицы, заменяя их.
        /// </summary>
        public void MapInplace(Func<double, double> func)
        {
            for (var i = 0; i < Data.Length; i++)
            {
                Data[i] = func(Data[i]);
            }
        }

        /// <summary>
        /// Создаёт новую матрицу на основе существующей, применяя указанную функцию ко васем элементам матрицы.
        /// Например, `m.Map(x => x+1)` создаст матрицу, в которой все элементы увеличены на единицу.
        /// </summary>
        public Matrix Map(Func<double, double> func)
        {
            var res = Clone();
            res.MapInplace(func);
            return res;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Matrix<{Size.rows} x {Size.columns}>";
        }

        /// <summary>
        /// Заполняет матрицу на основании строк [файла].
        /// </summary>
        public Matrix FillFrom(IEnumerator<string> lines)
        {
            var row = 1;
            while (lines.MoveNext())
            {
                var line = lines.Current;
                if (lines.Current == "")
                {
                    if (row == Size.rows + 1)
                    {
                        return this;
                    }

                    throw new ComputationError("Не хватает в данных матрицы.");
                }

                if (row > Size.rows)
                {
                    throw new ComputationError("Слишком много строк в данных матрицы.");
                }

                var splitted = line.Split();
                if (splitted.Length != Size.columns)
                {
                    throw new ComputationError("Неправильное количество столбцов в данных матрицы.");
                }

                for (var column = 1; column <= splitted.Length; column++)
                {
                    var value = splitted[column - 1];
                    if (!double.TryParse(value, out var parsed))
                    {
                        throw new ComputationError($"Не удалось разобрать число: {value}.");
                    }

                    this[row, column] = parsed;
                }

                row++;
            }

            throw new ComputationError("Неожиданный конец файла.");
        }
    }
}