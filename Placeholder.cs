using System;
using MatrixCalc.Core;

namespace MatrixCalc
{
    /// <summary>
    /// Обозначает место для аргумента команды.
    /// </summary>
    public class Placeholder
    {
        /// <summary>
        /// Имя аргумента.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Можент ли быть этот аргумент опущен.
        /// </summary>
        public readonly bool Nullable;

        /// <summary>
        /// Тип значения.
        /// </summary>
        public readonly string Type;

        /// <summary>
        /// Создаёт новый Placeholder.
        /// </summary>
        private Placeholder(string name, string type, bool nullable)
        {
            Name = name;
            Type = type;
            Nullable = nullable;
        }

        /// <summary>
        /// Создаёт новый Placehodler, разбирая переданную строку.
        /// </summary>
        /// <returns>
        /// Возвращает разобранный Placeholder и неразобранный остаток строки
        /// (всё что следует после объявления placeholder).
        /// </returns>
        public static (Placeholder?, string) Parse(string text)
        {
            if (text.Length == 0)
            {
                return (null, text);
            }

            var start = 1;

            if (text[0] != '{')
            {
                return (null, text);
            }

            var end = text.IndexOf('}');

            var inner = text.Substring(start, end - start);
            var splitted = inner.Split(':');
            var name = splitted[0];
            var type = splitted[1];

            var nullable = false;
            if (type[^1] == '?')
            {
                nullable = true;
                type = type.Substring(0, type.Length - 1);
            }

            return (new Placeholder(name, type, nullable), text.Substring(end + 1));
        }

        /// <summary>
        /// Преобразовывает переданный объект в указанный тип и возвращает его.
        /// Если это невозможно, то выбрасывает ComputationError.
        /// </summary>
        private static T TryCast<T>(object obj)
        {
            if (obj is T result)
            {
                return result;
            }

            var expected = typeof(T).Name;
            var found = obj.GetType().Name;
            throw new ComputationError($"Ожидался операнд типа {expected}, но был передан {found}.");
        }

        /// <summary>
        /// Преобразовывает переданный объект в тип, который требует этот Placeholder, и возвращает его.
        /// </summary>
        public object? Cast(object? obj)
        {
            if (obj == null)
            {
                return null;
            }

            return Type switch
            {
                "name" => TryCast<string>(obj),
                "int" => TryCast<Number>(obj).Int(),
                "uint" => TryCast<Number>(obj).UInt(),
                "double" => TryCast<Number>(obj).Value,
                "Matrix" => TryCast<Matrix>(obj),
                "Number" => TryCast<Number>(obj),
                "Basic" => TryCast<IBasic>(obj),
                _ => throw new ArgumentOutOfRangeException($"Invalid type: {Type}")
            };
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var result = Type != "Basic" ? $"{Type} {Name}" : Name;
            return Nullable ? $"[{result}]" : $"<{result}>";
        }
    }
}