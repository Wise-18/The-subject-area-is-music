namespace CL
{
    /// <summary>
    /// Исполнитель
    /// </summary>
    public class Singer
    {
        /// <summary>
        /// ID исполнителя
        /// </summary>
        public int Id { get; set; }
        /// <summary>
        /// Имя исполнителя
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Количество песен
        /// </summary>
        public int NumberOfSongs { get; set; }

        public override string ToString() => $"{Id};{Name};{NumberOfSongs}";
    }
}