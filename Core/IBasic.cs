namespace MatrixCalc.Core
{
    // К сожалению, я серьезно накосячил с ООП в этом проекте.
    // Слишком много switch-case по типам, из-за чего добавить новый — не самая простая задачка.
    // Я пытался сделать нормально, но по каким-то причинам это не было сделано. А жаль.

    /// <summary>
    /// Интерфейс для всех типов данных, с которыми работает программа.
    /// </summary>
    public interface IBasic
    {
        /// <summary>
        /// Красиво выводит значение.
        /// </summary>
        public string[,] Display();
    }

    /// <summary>
    /// Вспомогательные методы для IBasic.
    /// </summary>
    public static class BasicExtensions
    {
        /// <summary>
        /// Пытается преобразовать переданное значение в число.
        /// </summary>
        /// <param name="basic">Значение, которое требуется преобразовать.</param>
        /// <param name="allowNull">
        /// Если true, то при передаче null, оно же и будет возвращено, а не выброшено исключение.
        /// </param>
        /// <exception cref="ComputationError">Если переданное значение не является числом.</exception>
        /// <returns></returns>
        public static Number? AsNumber(this IBasic? basic, bool allowNull = false)
        {
            // Посольку этот метод принимает в качестве значения basic в т.ч. и null,
            // его нельзя было красиво реализовать прямо в интерфейсе.
            return basic switch
            {
                null when allowNull => null,
                Number n => n,
                var other => throw new ComputationError("Ожидалось число")
                {
                    IncorrectValue = other
                }
            };
        }
    }
}