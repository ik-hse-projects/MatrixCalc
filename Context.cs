using System;
using System.Collections.Generic;
using System.Linq;
using MatrixCalc.Core;

namespace MatrixCalc
{
    /// <summary>
    /// Хранит все переменные, которые могут использоваться при вычислении. А также делает пару дргуих хороших вещей.
    /// </summary>
    public class Context
    {
        /// <summary>
        /// Хранилище переменных.
        /// </summary>
        private readonly Dictionary<string, IBasic> variables = new Dictionary<string, IBasic>();

        /// <summary>
        /// Родительский контекст. Переменные из него можно получить, но они в нём не меняются.
        /// </summary>
        private Context? parent;

        /// <summary>
        /// Было ли выведено предупреждение о поддерживаемых числах.
        /// </summary>
        public bool WarningShown;

        /// <summary>
        /// Выводит предупреждение и устанавливает соответствующий флаг.
        /// </summary>
        private void BigWarning()
        {
            Console.WriteLine();
            Console.WriteLine("Внимание!");
            Console.WriteLine("Корректность расчетов гарантируется только для чисел в диапазоне от 0.0001 до 1000 " +
                              "(по абсолютному значению).");
            Console.WriteLine();
            WarningShown = true;
        }

        /// <summary>
        /// Проверяет, является ли число «большим».
        /// </summary>
        private static bool IsBig(double number)
        {
            var abs = Math.Abs(number);
            return abs > 1000 || abs < 0.0001D;
        }

        /// <summary>
        /// Проверяет, является ли матрица «большой». Если да, то выводит предупреждение.
        /// </summary>
        public void WarnBigMatrix(Matrix matrix)
        {
            if (WarningShown)
            {
                return;
            }

            if (matrix.Data.Any(IsBig))
            {
                BigWarning();
            }
        }

        /// <summary>
        /// Проверяет, является ли число «большим». Если да, то выводит предупреждение.
        /// </summary>
        public void WarnBigNumber(IdentityMatrix number)
        {
            if (WarningShown)
            {
                return;
            }

            if (IsBig(number.Value))
            {
                BigWarning();
            }
        }

        /// <summary>
        /// Возвращает значение переменной по её имени.
        /// </summary>
        public IBasic GetVariable(string name)
        {
            if (variables.TryGetValue(name, out var result))
            {
                return result;
            }

            if (parent != null)
            {
                return parent.GetVariable(name);
            }

            throw new ComputationError($"Нет переменной с именем {name}");
        }

        /// <summary>
        /// Присваивает значение переменной.
        /// </summary>
        public void SetVariable(string name, IBasic? basic)
        {
            if (basic == null)
            {
                variables.Remove(name);
            }
            else
            {
                variables[name] = basic;
            }
        }

        /// <summary>
        /// Создаёт новый контекст, в котором будут все существующие переменные,
        /// причём имеется возможность легко откатить все изменения.
        /// </summary>
        public Context Snapshot()
        {
            return new Context
            {
                parent = this
            };
        }

        /// <summary>
        /// Надёжно сохраняет все изменения переменных и возвращает новый контекст.
        /// </summary>
        public Context Save()
        {
            if (parent == null)
            {
                return this;
            }

            foreach (var (name, value) in variables)
            {
                parent.variables[name] = value;
            }

            return parent;
        }

        /// <summary>
        /// Откатывает изменения переменных, если они не были надёжно сохранены.
        /// </summary>
        public Context Rollback()
        {
            return parent ?? this;
        }

        /// <summary>
        /// Возвращает перечисление по всем именам переменных.
        /// </summary>
        public IEnumerable<string> ListVariables()
        {
            var keys = variables.Keys.Where(k => k != "_");

            if (parent != null)
            {
                return keys.Concat(parent.ListVariables());
            }

            return keys;
        }
    }
}