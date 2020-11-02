using System.Collections.Generic;
using System.Text;
using MatrixCalc.Core;

namespace MatrixCalc
{
    /// <summary>
    /// Половинка парсера, которая работает с тем, что вводит пользователь.
    /// </summary>
    public class HighParser
    {
        private readonly Context context;
        private readonly StringBuilder number = new StringBuilder();
        private readonly HighParser? parent;
        private readonly StringBuilder span = new StringBuilder();
        private readonly List<Token> tokens = new List<Token>();
        private readonly StringBuilder unparsed = new StringBuilder();
        private bool wasWhitespace;

        /// <summary>
        /// Создаёт "корневой парсер".
        /// </summary>
        private HighParser(Context context)
        {
            this.context = context;
            parent = null;
        }

        /// <summary>
        /// Создаёт парсер, который обрабатывает содержимое скобок.
        /// </summary>
        private HighParser(Context context, HighParser parent)
        {
            this.context = context;
            this.parent = parent;
        }

        /// <summary>
        /// Вложенность скобок.
        /// </summary>
        public int Depth => parent?.Depth + 1 ?? 1;

        /// <summary>
        /// Помещает один символ в парсер.
        /// </summary>
        /// <returns>Парсер, который должен обрабатывать следующий символ.</returns>
        private HighParser Feed(char ch)
        {
            if (ch == '(')
            {
                FinishNumber();
                FinishUnparsed();
                return new HighParser(context, this);
            }

            if (ch == ')')
            {
                if (parent == null)
                {
                    throw new ParsingError("Некорректная расстановка скобок.");
                }

                FinishNumber();
                FinishUnparsed();
                var evaluated = Finish();
                parent.tokens.Add(new ComputedValue(span.ToString(), evaluated));
                return parent;
            }

            if (char.IsWhiteSpace(ch))
            {
                if (wasWhitespace)
                {
                    return this;
                }

                wasWhitespace = true;
            }
            else
            {
                wasWhitespace = false;
            }

            span.Append(ch);

            if (char.IsDigit(ch) || number.Length != 0 && (ch == '.' || ch == ','))
            {
                FinishUnparsed();
                number.Append(ch);
            }
            else
            {
                FinishNumber();
                unparsed.Append(ch);
            }

            return this;
        }

        /// <summary>
        /// Завершает чтение числа и помещает его в токены.
        /// </summary>
        private void FinishNumber()
        {
            if (number.Length == 0)
            {
                return;
            }

            var span = number.ToString();
            if (!double.TryParse(span, out var parsed))
            {
                throw new ParsingError($"Некорректное число: {span}");
            }

            tokens.Add(new ComputedValue(span, new Number(parsed)));
            number.Clear();
        }

        /// <summary>
        /// Заврешает чтение необработанного потока символов.
        /// </summary>
        private void FinishUnparsed()
        {
            if (unparsed.Length == 0)
            {
                return;
            }

            tokens.Add(new Unparsed(unparsed.ToString()));
            unparsed.Clear();
        }

        /// <summary>
        /// Завершает чтение символов и возвращает значение, которое получилось в ходе вычислений.
        /// </summary>
        private IBasic? Finish()
        {
            FinishNumber();
            FinishUnparsed();

            var value = new LowParser(context, tokens).Parse();
            return value;
        }

        /// <summary>
        /// Интерпретирует строку и возвращает результат вычислений.
        /// </summary>
        public static IBasic? Parse(Context context, string data)
        {
            var current = new HighParser(context);
            foreach (var ch in data)
            {
                current = current.Feed(ch);
            }

            if (current.parent != null)
            {
                throw new ParsingError("Некорректная расстановка скобок.");
            }

            return current.Finish();
        }
    }
}