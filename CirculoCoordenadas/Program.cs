using System;
using System.Globalization;
using System.IO;

class Program
{
    static void Main()
    {
        // Solicita las coordenadas del centro y el radio
        Console.WriteLine("Ingrese las coordenadas del centro en formato N000.00.00.000 W000.00.00.000:");
        string centro = Console.ReadLine();

        Console.WriteLine("Ingrese el radio en metros:");
        double radio = double.Parse(Console.ReadLine());

        // Convierte las coordenadas a decimal
        (double latCentro, double lonCentro) = ConvertirCoordenadasADecimal(centro);

        // Calcula los segmentos del círculo
        var segmentos = CalcularSegmentosCirculo(latCentro, lonCentro, radio);

        // Genera el archivo de texto con los segmentos
        GenerarArchivoDeSegmentos(segmentos);

        Console.WriteLine("Archivo generado con éxito.");
    }

    static (double, double) ConvertirCoordenadasADecimal(string coordenadas)
    {
        var partes = coordenadas.Split(' ');
        double lat = ConvertirParteADecimal(partes[0]);
        double lon = ConvertirParteADecimal(partes[1]);
        return (lat, lon);
    }

    static double ConvertirParteADecimal(string parte)
    {
        var grados = double.Parse(parte.Substring(1, 3));
        var minutos = double.Parse(parte.Substring(5, 2));
        var segundos = double.Parse(parte.Substring(8, 2) + "." + parte.Substring(11, 3), CultureInfo.InvariantCulture);
        var signo = (parte[0] == 'S' || parte[0] == 'W') ? -1 : 1;

        return signo * (grados + minutos / 60 + segundos / 3600);
    }

    static (double, double)[] CalcularSegmentosCirculo(double latCentro, double lonCentro, double radio)
    {
        const int numSegmentos = 36; // Dividir el círculo en 36 segmentos (10 grados cada uno)
        var segmentos = new (double, double)[numSegmentos + 1];

        for (int i = 0; i <= numSegmentos; i++)
        {
            double angulo = i * 10 * Math.PI / 180; // Convertir a radianes
            var (lat, lon) = CalcularPuntoEnElCírculo(latCentro, lonCentro, radio, angulo);
            segmentos[i] = (lat, lon);
        }

        return segmentos;
    }

    static (double, double) CalcularPuntoEnElCírculo(double latCentro, double lonCentro, double radio, double angulo)
    {
        double R = 6378137; // Radio de la Tierra en metros
        double latCentroRad = latCentro * Math.PI / 180;
        double lonCentroRad = lonCentro * Math.PI / 180;

        double distanciaAng = radio / R; // Distancia angular en radianes

        double latPuntoRad = Math.Asin(Math.Sin(latCentroRad) * Math.Cos(distanciaAng) + Math.Cos(latCentroRad) * Math.Sin(distanciaAng) * Math.Cos(angulo));
        double lonPuntoRad = lonCentroRad + Math.Atan2(Math.Sin(angulo) * Math.Sin(distanciaAng) * Math.Cos(latCentroRad), Math.Cos(distanciaAng) - Math.Sin(latCentroRad) * Math.Sin(latPuntoRad));

        double latPunto = latPuntoRad * 180 / Math.PI;
        double lonPunto = lonPuntoRad * 180 / Math.PI;

        return (latPunto, lonPunto);
    }

    static void GenerarArchivoDeSegmentos((double, double)[] segmentos)
    {
        using (StreamWriter sw = new StreamWriter("segmentos.txt"))
        {
            for (int i = 0; i < segmentos.Length - 1; i++)
            {
                string coordInicio = ConvertirDecimalACoordenadas(segmentos[i]);
                string coordFin = ConvertirDecimalACoordenadas(segmentos[i + 1]);
                sw.WriteLine($"{coordInicio} {coordFin} PARKING");
            }
        }
    }

    static string ConvertirDecimalACoordenadas((double lat, double lon) coord)
    {
        string lat = ConvertirDecimalAParte(coord.lat, 'N', 'S');
        string lon = ConvertirDecimalAParte(coord.lon, 'E', 'W');
        return $"{lat} {lon}";
    }

    static string ConvertirDecimalAParte(double valor, char positivo, char negativo)
    {
        char direccion = valor >= 0 ? positivo : negativo;
        valor = Math.Abs(valor);
        int grados = (int)valor;
        int minutos = (int)((valor - grados) * 60);
        double segundos = ((valor - grados) * 60 - minutos) * 60;
        return $"{direccion}{grados:000}.{minutos:00}.{segundos:00.000}".Replace('.', ',');
    }
}
