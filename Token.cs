using System;
using System.Collections.Generic;
using MatrixCalc.Core;

namespace MatrixCalc
{
    /// <summary>
    /// Токен с каким-то содержимым.
    /// </summary>
    public abstract class Token
    {
        protected Token(string span)
        {
            Span = span;
        }

        /// <summary>
        /// Строка, соответсвующая этому токену, так как его вводил пользователь.
        /// </summary>
        public string Span { get; }
    }

    /// <summary>
    /// Токен с ещё не разобранным содержимым.
    /// </summary>
    public class Unparsed : Token
    {
        public Unparsed(string span) : base(span)
        {
        }

        /// <summary>
        /// Ищет переданную команду в содержимом токена и возвращает IEnumerable по новым токенам.
        /// В новых токенах будут и Unparsed, и ParsedCommand, если строка команда присутствовала.
        /// </summary>
        public IEnumerable<Token> Apply(Command command)
        {
            foreach (var syntax in command.CompiledSyntax)
            {
                for (var i = 0; i <= Span.Length - syntax.Command.Length; i++)
                {
                    var window = Span.Substring(i, syntax.Command.Length);
                    var comparisonType = syntax.Flags.CaseInsensitive
                        ? StringComparison.InvariantCultureIgnoreCase
                        : StringComparison.InvariantCulture;
                    if (string.Equals(window, syntax.Command, comparisonType))
                    {
                        var before = Span.Substring(0, i).Trim();
                        var after = Span.Substring(i + syntax.Command.Length).Trim();
                        if (before != "")
                        {
                            yield return new Unparsed(before);
                        }

                        yield return new ParsedCommand(syntax.Command, command, syntax);

                        if (after != "")
                        {
                            foreach (var token in new Unparsed(after).Apply(command))
                            {
                                yield return token;
                            }
                        }

                        yield break;
                    }
                }
            }

            // Если ничего не нашли, то оставляем всё как есть.
            yield return this;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Unparsed`{Span}`";
        }
    }

    /// <summary>
    /// Токен, который соответсвует какой-то определённой команде.
    /// </summary>
    public class ParsedCommand : Token
    {
        public ParsedCommand(string span, Command value, Syntax syntax) : base(span)
        {
            Command = value;
            Syntax = syntax;
        }

        /// <summary>
        /// Команда.
        /// </summary>
        public Command Command { get; }

        /// <summary>
        /// Синтаксис команды, который был сопоставлен исходному тексту.
        /// </summary>
        public Syntax Syntax { get; }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"Parsed<{Command}>";
        }
    }

    /// <summary>
    /// Токен, который соответсвует какому-то определённому значению.
    /// </summary>
    public class ComputedValue : Token
    {
        public ComputedValue(string span, IBasic? value) : base(span)
        {
            Value = value;
        }

        /// <summary>
        /// Значение.
        /// </summary>
        public IBasic? Value { get; }

        public override string ToString()
        {
            return $"Computed<{Value}>";
        }
    }
}