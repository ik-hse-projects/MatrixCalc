using System;
using System.Linq;

namespace MatrixCalc
{
    /// <summary>
    /// Флаги содержат то, как следует интерпретировать синтаксис команды.
    /// </summary>
    public struct Flags
    {
        /// <summary>
        /// Следует ли игнорировать регистр при сопоставлении команды с введённым значением.
        /// </summary>
        public bool CaseInsensitive { get; private set; }

        /// <summary>
        /// Разбирает флаги из массива символов и возвращает их (флаги то есть).
        /// </summary>
        /// <param name="text">Массив символов.</param>
        public void Parse(char[] text)
        {
            foreach (var c in text)
            {
                if (c == 'i')
                {
                    CaseInsensitive = true;
                }
                else
                {
                    throw new Exception($"Некорректная буква флага в определении синтаксиса: {c}");
                }
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"(Case: {!CaseInsensitive})";
        }
    }

    /// <summary>
    /// Синтаксис той или иной команды.
    /// </summary>
    public class Syntax
    {
        /// <summary>
        /// Строка с самой командой.
        /// </summary>
        public readonly string Command;

        /// <summary>
        /// Флаги синтаксиса.
        /// </summary>
        public readonly Flags Flags;

        /// <summary>
        /// Левый аргумент, если есть.
        /// </summary>
        public readonly Placeholder? Left;

        /// <summary>
        /// Правый аргумент, если есть.
        /// </summary>
        public readonly Placeholder? Right;

        /// <summary>
        /// Приватный конструктор, который инициализирует все поля.
        /// </summary>
        private Syntax(Flags flags, Placeholder? left, string command, Placeholder? right)
        {
            Flags = flags;
            Left = left;
            Command = command;
            Right = right;
        }

        /// <summary>
        /// Разбирает синтаксис команды из переданной строки.
        /// </summary>
        public static Syntax Parse(string text)
        {
            Flags flags = default;
            if (text.Contains('/'))
            {
                var flagLetters = text.TakeWhile(c => c != '/').ToArray();
                flags.Parse(flagLetters);
                text = text.Substring(flagLetters.Length + 1);
            }

            Placeholder? left;
            Placeholder? right;

            (left, text) = Placeholder.Parse(text);

            var command = string.Join("", text.TakeWhile(ch => ch != '{'));
            text = text.Substring(command.Length);
            command = command.Trim();

            (right, text) = Placeholder.Parse(text);

            if (text.Length != 0)
            {
                throw new Exception("Не удалось полностью разобрать синтаксис.");
            }

            return new Syntax(flags, left, command, right);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Command[{Flags} {Left} {Command} {Right}]";
        }
    }
}