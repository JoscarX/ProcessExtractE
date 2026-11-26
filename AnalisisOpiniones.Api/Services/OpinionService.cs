using AnalisisOpiniones.Api.Models;

namespace AnalisisOpiniones.Api.Services
{
    public sealed class OpinionService
    {
        private readonly IReadOnlyList<Opinion> _opinions = Array.AsReadOnly(new Opinion[]
        {
            CreateOpinion(1, 1, "Lo llevo a la oficina compartida y nadie se queja del sonido.", 5),
            CreateOpinion(2, 1, "Por el precio esperaba que incluyera un reposamuñecas.", 3),
            CreateOpinion(3, 1, "Tras un mes, la barra espaciadora empezó a quedarse pegada.", 2),
            CreateOpinion(4, 1, "Las letras se ven bien incluso con poca luz.", 4),
            CreateOpinion(5, 1, "El formato compacto me sirve, aunque echo de menos el teclado numérico.", 3),
            CreateOpinion(6, 2, "El clic principal sigue firme después de varias semanas de juego.", 5),
            CreateOpinion(7, 2, "Pierde el seguimiento sobre mi escritorio de vidrio.", 2),
            CreateOpinion(8, 2, "La superficie mate se limpia sin dejar huellas.", 4),
            CreateOpinion(9, 2, "Es una opción razonable si solo lo usas para navegar.", 3),
            CreateOpinion(10, 2, "Los botones laterales quedan lejos para quien tiene la mano pequeña.", 2),
            CreateOpinion(11, 3, "El ajuste de inclinación me permite compartir la pantalla con facilidad.", 5),
            CreateOpinion(12, 3, "El acabado combina bien con el escritorio y no se raya al plegarlo.", 4),
            CreateOpinion(13, 3, "Con un equipo pesado, la bisagra bajó durante una videollamada.", 1),
            CreateOpinion(14, 3, "Ocupa poco en el bolso, aunque pesa más de lo esperado.", 3),
            CreateOpinion(15, 3, "Lo uso a diario y las gomas siguen en su sitio.", 5),
            CreateOpinion(16, 4, "El tono neutro ayuda a distinguir colores cuando hago manualidades.", 5),
            CreateOpinion(17, 4, "El adaptador se calienta demasiado para dejarla encendida varias horas.", 2),
            CreateOpinion(18, 4, "El embalaje llegó bien y el montaje tomó unos minutos.", 4),
            CreateOpinion(19, 4, "En una mesa amplia, el haz cubre solo la mitad de mi zona de trabajo.", 3),
            CreateOpinion(20, 4, "El brazo cruje al moverlo y el precio me parece alto para ese acabado.", 2)
        });

        public IReadOnlyList<Opinion> GetOpinions()
        {
            return _opinions;
        }

        private static Opinion CreateOpinion(int id, int productId, string comment, int rating)
        {
            var createdAt = new DateTimeOffset(2026, 11, id, 12, 0, 0, TimeSpan.FromHours(-3));

            return new Opinion(id, productId, comment, rating, createdAt);
        }
    }
}
