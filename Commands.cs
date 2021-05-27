using System.Collections.Generic;
using System.Linq;
using MatrixCalc.Core;

namespace MatrixCalc
{
    /// <summary>
    /// Базовая команда/оператор.
    /// </summary>
    public abstract class Command
    {
        /// <summary>
        /// Все известных команды. Сгруппированы по приоритетам, от высокого к низкому.
        /// </summary>
        public static readonly IReadOnlyList<IReadOnlyList<Command>> Commands = new[]
        {
            new Command[] {new GetVarCmd(), new GenerateRandomMatrix(), new EmptyList()},
            new Command[] {new IdentityCmd()},
            new Command[] {new Transpose(), new CanonicalForm(), new Trace()},
            new Command[] {new BlockMatrixHorizontal(), new BlockMatrixVertical()},
            new Command[] {new Determinant(), new Solve()},
            new Command[] {new Pow()},
            new Command[] {new MatrixElementMul(), new Mul(), new Div()},
            new Command[] {new Sum(), new Sub()},
            new Command[] {new ListAdd()},
            new Command[] {new SetVarCmd()}
        };

        /// <summary>
        /// Кэш. Содержит подготовленные для работы варианты написания. Автоматически вычисляется при необходимости.
        /// </summary>
        private Syntax[]? compiled;

        /// <summary>
        /// Список всех команд без группировки по приоритетам.
        /// </summary>
        public static IEnumerable<Command> FlatCommands => Commands.SelectMany(x => x);

        /// <summary>
        /// Возможные написания команды.
        /// </summary>
        protected abstract string[] Syntax { get; }

        /// <summary>
        /// Название команды человечесим языком.
        /// </summary>
        public abstract string Title { get; }

        /// <summary>
        /// Подробное описание команды.
        /// </summary>
        public abstract string Description { get; }

        /// <summary>
        /// Подготовленные варианты написания.
        /// </summary>
        public Syntax[] CompiledSyntax => compiled ??= Syntax.Select(MatrixCalc.Syntax.Parse).ToArray();

        /// <summary>
        /// Вычисляет значение команды.
        /// </summary>
        /// <param name="context">Конекст, в котором происходит вычисление.</param>
        /// <param name="args">Объект, который позволяет получить все аргументы, объявленные в <see cref="Syntax" /></param>
        /// <returns>Вычисленное значение.</returns>
        public abstract IBasic? Compute(Context context, dynamic args);

        // В дальнейшем я опускаю комментарии, поскольку совершенно не хочется раздувать код,
        // копируя содержимое Title и Description в комментарии. Считаю дублирование хуже, чем отсутсвие возможности
        // автоматически сгенрировать документацию, особенно если учесть, что обычно приватные классы не входят в неё.

        // Небольшое отступление про использование dynamic. Он идеально подходит для этого случая:
        // Во-первых, производительность нам не очень-то и важна на самом деле.
        // Во-вторых, у многих операций есть комбинации аргументов, каждую из которых надо обрабатывать по-своему.
        // Например, умножение:
        //     Матрица на число — получится матрица.
        //     Матрица на единичную матрицу — получится матрица.
        //     Единичная матрица на число — получится единичная матрица.
        //     Число на число — получится число.
        // Это всё легко можно различать с использованием перегрузок (что и происходит в Actions.cs),
        // но поскольку типы значений зависят от того, что именно вводит пользователь, то я вижу три варианта:
        // 1. switch: Много повторяющегося кода.
        // 2. рефлексия: ну это уже слишком продвинуто для третьего peer review, да и код всё равно получается
        //     не очень красивым и довольно сложным.
        //     Поэтому обычно стараюсь его избегать, хотя куцые возможности метапрограммирования так и манят.
        // 3. dynamic: Вирутальная машина дотнета автоматически выбирает наилучшую перегрузку метода в зависимости
        //     от того, какой тип на самом деле имеет dynamic (я гуглил). Примерно это и пришлось бы делать самому
        //     в случае рефлексии, но dynamic все сложности прячет под капот СиШарпа. Это очень круто!

        private class GenerateRandomMatrix : Command
        {
            protected override string[] Syntax => new[] {"rand"};
            public override string Title => "Генерация случайной матрицы";

            public override string Description =>
                "Генерирует случайную матрицу. Параметры генерации будут запрошены отдельно.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Program.GenerateMatrix(context);
            }
        }

        private class SetVarCmd : Command
        {
            protected override string[] Syntax => new[] {"{name:name} = {value:Basic?}"};
            public override string Title => "Присваивание";

            public override string Description =>
                "Присваивает переменной с именем `name` значение справа.\n" +
                "Если не указано значение справа, то будет предложено ввести матрицу вручную.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                if (args.value == null)
                {
                    var result = Program.ReadMatrix(context);
                    context.SetVariable(args.name, result);
                    return result;
                }

                context.SetVariable(args.name, args.value);
                return args.value;
            }
        }

        public class GetVarCmd : Command
        {
            protected override string[] Syntax => new[] {"${name:name}"};
            public override string Title => "Переменная";
            public override string Description => "Получает значение ранее присовенной переменной с именем `name`.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return context.GetVariable(args.name);
            }
        }

        private class IdentityCmd : Command
        {
            protected override string[] Syntax => new[] {@"{m:Number?} E {n:uint?}"};

            public override string Title => "Единичная матрица";

            public override string Description =>
                "Создаёт квадратную единичную матрицу.\n" +
                "Число справа — её размер (не обязательно); если указан, то будет создана обычная матрица.\n" +
                "Число слева — её множитель (по-умолчанию 1).";

            public override IBasic? Compute(Context context, dynamic args)
            {
                Number m = args.m == null ? new Number(1) : args.m;
                var id = new IdentityMatrix(m.Value);
                if (args.n != null)
                {
                    return id.ToMatrix((int) args.n, (int) args.n);
                }

                return id;
            }
        }

        private class Transpose : Command
        {
            protected override string[] Syntax => new[] {"i/{m:Matrix} ^ T", "i/{m:Matrix} ^T", "{m:Matrix}T"};

            public override string Title => "Транспонирование";
            public override string Description => "Транспонирует матрицу.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Actions.Transpose(args.m);
            }
        }

        private class Pow : Command
        {
            protected override string[] Syntax => new[] {"{o:Basic} ^ {p:int}"};

            public override string Title => "Возведение в степень.";
            public override string Description => "Возводит объект `o` в степень `p` (должна быть целым числом).";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Actions.Power(args.o, args.p);
            }
        }

        private class Sum : Command
        {
            protected override string[] Syntax => new[] {"{a:Basic} + {b:Basic}"};

            public override string Title => "Сложение";
            public override string Description => "Складывает два объекта.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Actions.Add(args.a, args.b);
            }
        }

        private class Sub : Command
        {
            protected override string[] Syntax => new[] {"{a:Basic?} - {b:Basic}"};

            public override string Title => "Вычитание";
            public override string Description => "Вычитает два объекта.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return args.a == null
                    ? Actions.Negate(args.b)
                    : Actions.Subtract(args.a, args.b);
            }
        }

        private class Mul : Command
        {
            protected override string[] Syntax => new[] {"{a:Basic} * {b:Basic}"};

            public override string Title => "Умножение";
            public override string Description => "Умножает два объекта.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Actions.Multiply(args.a, args.b);
            }
        }

        private class Div : Command
        {
            protected override string[] Syntax => new[] {"/{a:Basic} / {b:Number}"};

            public override string Title => "Деление";
            public override string Description => "Делит объект слева на число справа.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Actions.Divide(args.a, args.b);
            }
        }

        private class MatrixElementMul : Command
        {
            protected override string[] Syntax => new[] {"{a:Matrix} .* {b:Matrix}"};
            public override string Title => "Поэлементное умножение";

            public override string Description =>
                "Умножает элементы одной матрицы на соответствуюие элементы другой. Размеры должны совпадать.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Actions.MultiplyElements(args.a, args.b);
            }
        }

        private class Determinant : Command
        {
            protected override string[] Syntax => new[] {"i/det {m:Matrix}"};
            public override string Title => "Определитель";
            public override string Description => "Вычисляет определитель матрицы.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Actions.Determinant(args.m);
            }
        }

        private class Trace : Command
        {
            protected override string[] Syntax => new[] {"Tr {m:Matrix}", "trace {m:Matrix}"};
            public override string Title => "След матрицы";
            public override string Description => "Вычисляет след матрицы.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Actions.Trace(args.m);
            }
        }

        private class Solve : Command
        {
            protected override string[] Syntax => new[] {"i/solve {m:Matrix}"};

            public override string Title => "Решить СЛАУ";

            public override string Description =>
                "Находит решения СЛАУ, записанной в матричном виде. Правый столбец — столбец свободных членов.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Actions.Solve(args.m);
            }
        }

        private class ListAdd : Command
        {
            protected override string[] Syntax => new[] {"{xs:List} , {x:Basic}"};

            public override string Title => "Увеличить список";

            public override string Description =>
                "Создаёт список из списка слева и значения справа";

            public override IBasic? Compute(Context context, dynamic args)
            {
                // FIXME: Это плохо работает, если список положут в переменную.
                args.xs.Add(args.x);
                return args.xs;
            }
        }

        private class EmptyList : Command
        {
            protected override string[] Syntax => new[] {"[]"};

            public override string Title => "Создать список список";

            public override string Description => "Создаёт совершенно пустой список";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return new BasicList();
            }
        }

        private class BlockMatrixHorizontal : Command
        {
            protected override string[] Syntax => new[] {"{a:Matrix} | {b:Matrix}"};

            public override string Title => "Приклеить справа";

            public override string Description =>
                "Формирует блочную матрицу, которая состоит из матрицы `a` слева и матрицы `b` справа.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Actions.JoinHorizontal(args.a, args.b);
            }
        }

        private class BlockMatrixVertical : Command
        {
            protected override string[] Syntax => new[] {"{a:Matrix} _ {b:Matrix}"};

            public override string Title => "Приклеить снизу";

            public override string Description =>
                "Формирует блочную матрицу, которая состоит из матрицы `a` сверху и матрицы `b` снизу.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Actions.JoinVertical(args.a, args.b);
            }
        }

        private class CanonicalForm : Command
        {
            protected override string[] Syntax => new[] {"{m:Matrix} ~"};

            public override string Title => "Канонический вид";
            public override string Description => "Приводит матрицу к каноническому виду.";

            public override IBasic? Compute(Context context, dynamic args)
            {
                return Actions.Canonical(args.m);
            }
        }
    }
}
