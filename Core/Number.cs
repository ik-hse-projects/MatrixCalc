namespace MatrixCalc.Core
{
    /// <summary>
    /// Обычное число. Является подтипом единичной матрицы.
    /// </summary>
    public sealed class Number : IdentityMatrix
    {
        public Number(double value) : base(value)
        {
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Number<{Value}>";
        }

        /// <summary>
        /// Преобразовывает число в целое, если оно таковым является.
        /// </summary>
        public int Int()
        {
            if (!IsInteger())
            {
                throw new ParsingError($"{Value} не является целым числом.");
            }

            return (int) Value;
        }

        /// <summary>
        /// Преобразовывает число в целое неотрицательное число, если оно таковым является.
        /// </summary>
        public uint UInt()
        {
            if (!IsInteger())
            {
                throw new ParsingError($"{Value} не является целым числом.");
            }

            if (double.IsNegative(Value))
            {
                throw new ParsingError($"{Value} не является неотрицательным числом.");
            }

            return (uint) Value;
        }

        /// <summary>
        /// Проверяет, является ли число целым.
        /// </summary>
        public bool IsInteger()
        {
            return Value % 1 == 0;
        }

        /// <inheritdoc />
        public override string[,] Display()
        {
            return new[,]
            {
                {Value.ToString()}
            };
        }
    }
}