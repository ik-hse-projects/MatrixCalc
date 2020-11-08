using System;
using System.Text;
using MatrixCalc.Core;

namespace MatrixCalc
{
    /// <summary>
    /// Базовый класс всех ошибок внутри MatrixCalc, связанных с некорректными действиями пользователя.
    /// </summary>
    public abstract class Error : Exception
    {
        protected Error(string? message) : base(message)
        {
        }

        public abstract string Kind { get; }

        public virtual string Display()
        {
            return Message;
        }
    }

    /// <summary>
    /// Критическая ошибка внутри программы.
    /// </summary>
    public class InternalError : Error
    {
        public InternalError(string? message) : base(message)
        {
        }

        public override string Kind => "внутри программы";
    }

    /// <summary>
    /// Ошибка при разборе выражения, введённого пользователем.
    /// </summary>
    public class ParsingError : Error
    {
        public ParsingError(string? message) : base(message)
        {
        }

        /// <summary>
        /// Текст, связанный с этой ошибкой.
        /// </summary>
        public string? Span { get; set; }

        public override string Kind => "при разборе выражения";

        public override string Display()
        {
            return Span != null ? $"{Message}\n>>> `... {Span} ...`" : Message;
        }
    }

    /// <summary>
    /// Ошибка в ходе вычисления.
    /// </summary>
    public class ComputationError : Error
    {
        public ComputationError(string? message) : base(message)
        {
        }

        /// <summary>
        /// Левый аргумент, если есть.
        /// </summary>
        public IBasic? Left { get; set; }

        /// <summary>
        /// Правый аргумент, если есть.
        /// </summary>
        public IBasic? Right { get; set; }

        /// <summary>
        /// Некорректное значение, о котором идёт речь.
        /// </summary>
        public IBasic? IncorrectValue { get; set; }

        public override string Kind => "при вычислении";

        public override string Display()
        {
            var result = new StringBuilder().AppendLine(Message);

            if (Left != null)
            {
                result.AppendLine($"То что было слева: {Left}");
            }

            if (Right != null)
            {
                result.AppendLine($"То что было справа: {Right}");
            }

            if (IncorrectValue != null)
            {
                result.AppendLine($"Некорректное значение: {IncorrectValue}");
            }

            // Убираем лишний перенос строки.
            result.Remove(result.Length - 1, 1);

            return result.ToString();
        }
    }

    /// <summary>
    /// Некоторые вспомогательные функции, направленные на качественную работу программы.
    /// </summary>
    public static class Safety
    {
        /// <summary>
        /// Делит <paramref name="left" /> на <paramref name="right" /> и возвращает результат деления.
        /// </summary>
        public static double SafeDiv(this double left, double right)
        {
            // Раньше расчеты велись в decimal, но позже по ряду причин пришлось перейти на double.
            // Эта функция довольно проста, но если вдруг потребуется снова перейти на decimal или int,
            // то можно будет легко добавить проверку на деление на ноль.
            return left / right;
        }

        /// <summary>
        /// Сравнивает число с нулём с некоторым допуском.
        /// </summary>
        public static bool IsZero(this double number)
        {
            return Math.Abs(number) < 1e-10;
        }
    }
}