using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using MatrixCalc.Core;

namespace MatrixCalc
{
    /// <summary>
    /// Половинка парсера, которая работает с токенами.
    /// </summary>
    public class LowParser
    {
        /// <summary>
        /// Список известных команд.
        /// </summary>
        private readonly IReadOnlyList<IReadOnlyList<Command>> commands;

        /// <summary>
        /// Контекст, в котором происходят вычисления.
        /// </summary>
        private readonly Context context;

        /// <summary>
        /// Текущие токены.
        /// </summary>
        private List<Token> tokens;

        /// <summary>
        /// Создаёт LowParser на основе данных токенов, используя переданный контекст.
        /// </summary>
        public LowParser(Context context, List<Token> tokens)
        {
            commands = Command.Commands;
            this.tokens = tokens;
            this.context = context;
        }

        /// <summary>
        /// Полностью разбирает введённые токены и возвращает результат вычислений.
        /// </summary>
        public IBasic? Parse()
        {
            // Вообще-то по задумке одного шага Reduce должно быть достаточно.
            // Но я делаю несколько, поскольку раньше это было необходимо,
            // а сейчас до дедлайна остается слишком мало времени, чтобы ловить не совсем понятные баги.
            for (var i = 0; i < 1000; i++)
            {
                switch (tokens.Count)
                {
                    case 0:
                        return null;
                    case 1 when tokens[0] is ComputedValue parsed:
                        return parsed.Value;
                    default:
                        Reduce();
                        break;
                }
            }

            throw new ParsingError(
                "Не удалось разобрать выражение. Возможно оно слишком сложно или вовсе некорректно.");
        }

        /// <summary>
        /// Находит и вычисляет все ParsedCommand среди токенов.
        /// </summary>
        private void RunAllCommands()
        {
            for (var i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                if (token is ParsedCommand command)
                {
                    var args = new ExpandoObject();
                    var argsAsCollection = (ICollection<KeyValuePair<string, dynamic>>) args;

                    argsAsCollection.Add(new KeyValuePair<string, dynamic>("span", command.Span));
                    var span = "";
                    if (AddArg(command.Command, command.Syntax.Left, i - 1, argsAsCollection, ref span))
                    {
                        i--;
                    }

                    span += command.Span;

                    AddArg(command.Command, command.Syntax.Right, i + 1, argsAsCollection, ref span);

                    var value = command.Command.Compute(context, args);
                    tokens[i] = new ComputedValue(span, value);
                    if (!context.WarningShown)
                    {
                        switch (value)
                        {
                            case IdentityMatrix matrix:
                                context.WarnBigNumber(matrix);
                                break;
                            case Matrix matrix:
                                context.WarnBigMatrix(matrix);
                                break;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Находит переданную команду среди Unparsed токенов и добавляет соответствующие ParsedCommand.
        /// </summary>
        private void ApplyCommand(Command command)
        {
            var newTokens = new List<Token>();
            foreach (var token in tokens)
            {
                if (token is Unparsed unparsed)
                {
                    newTokens.AddRange(unparsed.Apply(command));
                }
                else
                {
                    newTokens.Add(token);
                }
            }

            tokens = newTokens;
        }

        /// <summary>
        /// Делает один шаг вычислений.
        /// </summary>
        private void Reduce()
        {
            foreach (var commandGroup in commands)
            {
                foreach (var command in commandGroup)
                {
                    ApplyCommand(command);
                }

                RunAllCommands();
                // Убираем пустые/пробельные токены.
                tokens = tokens
                    .Where(t => !(t is Unparsed unparsed && string.IsNullOrWhiteSpace(unparsed.Span)))
                    .ToList();
            }
        }

        /// <summary>
        /// Функция берет `i`-ый токен, пытается его интерпретировать как аргумент команды `command`,
        /// описание которого передаётся в `placeholder`. Если это удалось, то помещает аргумент с соответсвующим именем
        /// в argsAsCollection. Прибавляет в конец `span` Span токена.
        /// </summary>
        /// <exception cref="ComputationError">Если аргумент не того типа, который требует команда.</exception>
        /// <exception cref="ParsingError">Если аргумент отмечен как обязательный, но его не удалось найти.</exception>
        /// <returns>Был ли найден аргумент.</returns>
        private bool AddArg(Command command, Placeholder? placeholder, int i,
            ICollection<KeyValuePair<string, dynamic>> argsAsCollection, ref string span)
        {
            // Ох, какая же эта функция страшная...

            if (placeholder == null)
            {
                return false;
            }

            object? argumentValue = null;
            if (i >= 0 && i < tokens.Count)
            {
                var token = tokens[i];
                switch (token)
                {
                    case ComputedValue prev:
                    {
                        argumentValue = prev.Value;
                        tokens.RemoveAt(i);
                        break;
                    }
                    case Unparsed unparsed when placeholder.Type == "name":
                    {
                        // Имена не могут быть интерпретированы как ComputedValue, поэтому приходится их аккуратно
                        // выдирать из Unparsed.
                        var name = new string(unparsed.Span
                            .TakeWhile(ch => char.IsLetterOrDigit(ch) || ch == '_')
                            .ToArray());
                        argumentValue = name;
                        var remaining = unparsed.Span.Substring(name.Length);
                        if (string.IsNullOrWhiteSpace(remaining))
                        {
                            tokens.RemoveAt(i);
                        }
                        else
                        {
                            tokens[i] = new Unparsed(remaining);
                        }

                        break;
                    }
                    default:
                    {
                        argumentValue = null;
                        break;
                    }
                }

                span += token.Span;
            }

            if (argumentValue == null && !placeholder.Nullable)
            {
                throw new ParsingError(
                    $"Не удалось найти необходимый операнд при попытке применить оператор {command}.")
                {
                    Span = span
                };
            }

            argsAsCollection.Add(new KeyValuePair<string, object?>(placeholder.Name, placeholder.Cast(argumentValue)));
            return argumentValue != null;
        }
    }
}