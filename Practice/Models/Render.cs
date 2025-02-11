namespace Practice.Models
{
    public class Render
    {
        public int Id { get; set; } // Уникальный идентификатор
        public int Angle_vertical { get; set; }
        public int Angle_horizontal { get; set; } // Угол поворота
        public int Light { get; set; } // Яркость света
        public byte[] Skin { get; set; } // Текстура
        public byte[] RenderedImage { get; set; } // Сгенерированное изображение
        public int Angle_light { get; set; }
    }
}
