namespace MatrixCalc.Core
{
    /// <summary>
    /// Единичная матрица, умноженная на число.
    /// </summary>
    public class IdentityMatrix : IBasic
    {
        /// <summary>
        /// Создаёт новую единичную матрицу, умноженную на число.
        /// </summary>
        public IdentityMatrix(double value)
        {
            Value = value;
        }

        /// <summary>
        /// Число, которое расположено на главной диагонали.
        /// </summary>
        public double Value { get; }

        /// <inheritdoc />
        public virtual string[,] Display()
        {
            var value = Value.ToString();
            return new[,]
            {
                {value, "0 ", "⋯ ", "0 ", "0"},
                {"0 ", value, "⋯ ", "0 ", "0"},
                {"⋮ ", "⋮ ", "⋱ ", "⋮ ", "⋮"},
                {"0 ", "0 ", "⋯ ", value, "0"},
                {"0 ", "0 ", "⋯ ", "0 ", value}
            }.AlignCenter().Grid(Displaying.SpacedGrid).Parentheses();
        }

        /// <summary>
        /// Преобразовывает единичную матрицу произвольного размера в матрицу указанного размера.
        /// </summary>
        /// <param name="rows">Кол-во строк в новой матрице.</param>
        /// <param name="columns">Кол-во столбцов в новой матрице.</param>
        public Matrix ToMatrix(int rows, int columns)
        {
            var result = new Matrix(rows, columns);
            for (var i = 1; i <= result.MinDimension; i++)
            {
                result[i, i] = Value;
            }

            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"IdMatrix<{Value}>";
        }
    }
}