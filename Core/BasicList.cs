using System;
using System.Collections.Generic;

namespace MatrixCalc.Core
{
    /// <summary>
    /// Список со всевозможными значениями внутри.
    /// </summary>
    public class BasicList : IBasic
    {
        /// <summary>
        /// Значения внутри.
        /// </summary>
        internal List<IBasic> Data;

        /// <summary>
        /// Создаёт пустой список
        /// </summary>
        public BasicList()
        {
            Data = new List<IBasic>();
        }

        /// <summary>
        /// Добавляет значение в конец списка
        /// </summary>
        public void Add(IBasic val)
        {
            Data.Add(val);
        }

        /// <inheritdoc />
        public virtual string[,] Display()
        {
            var result = new string[Data.Count, 1];
            for (var i = 0; i < Data.Count; i++)
            {
                result[i,0] = Data[i].ToString();
            }
            return result;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"List<{Data.Count} elements>";
        }
    }
}
