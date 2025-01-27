namespace Practice.Models
{
    public class Render
    {
        public int Id { get; set; } // Уникальный идентификатор
        public int Angle { get; set; } // Угол поворота
        public int Light { get; set; } // Яркость света
        public byte[] Skin { get; set; } // Текстура
        public byte[] RenderedImage { get; set; } // Сгенерированное изображение
        public byte[] BlendFile { get; set; } // .blend файл
    }
}
