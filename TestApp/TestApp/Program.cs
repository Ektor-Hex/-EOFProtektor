using System;

namespace TestApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== TestApp - Aplicación de Prueba ===");
            Console.WriteLine("Esta aplicación sirve para probar EOF Protektor");
            Console.WriteLine();

            // Método 1: Suma simple
            int resultado1 = Sumar(5, 10);
            Console.WriteLine($"Suma: 5 + 10 = {resultado1}");

            // Método 2: Multiplicación
            int resultado2 = Multiplicar(7, 8);
            Console.WriteLine($"Multiplicación: 7 * 8 = {resultado2}");

            // Método 3: Operaciones complejas
            double resultado3 = OperacionCompleja(100, 50);
            Console.WriteLine($"Operación compleja: {resultado3}");

            // Método 4: Validación
            bool esValido = ValidarDatos("TestData");
            Console.WriteLine($"Validación: {(esValido ? "VÁLIDO" : "INVÁLIDO")}");

            // Clase auxiliar
            var calculadora = new Calculadora();
            int potencia = calculadora.Potencia(2, 8);
            Console.WriteLine($"Potencia: 2^8 = {potencia}");

            Console.WriteLine();
            Console.WriteLine("Presiona cualquier tecla para salir...");
            Console.ReadKey();
        }

        static int Sumar(int a, int b)
        {
            return a + b;
        }

        static int Multiplicar(int a, int b)
        {
            int resultado = 0;
            for (int i = 0; i < b; i++)
            {
                resultado += a;
            }
            return resultado;
        }

        static double OperacionCompleja(double x, double y)
        {
            double temp = x * 2;
            temp = temp + y;
            temp = temp / 3;
            return Math.Round(temp, 2);
        }

        static bool ValidarDatos(string datos)
        {
            if (string.IsNullOrEmpty(datos))
                return false;

            if (datos.Length < 5)
                return false;

            return true;
        }
    }

    public class Calculadora
    {
        public int Potencia(int baseNum, int exponente)
        {
            int resultado = 1;
            for (int i = 0; i < exponente; i++)
            {
                resultado *= baseNum;
            }
            return resultado;
        }

        public double RaizCuadrada(double numero)
        {
            return Math.Sqrt(numero);
        }

        public int Factorial(int n)
        {
            if (n <= 1)
                return 1;
            return n * Factorial(n - 1);
        }
    }

    public class ValidadorComplejo
    {
        private string secretKey = "MySecretKey123";

        public bool ValidarConClave(string input)
        {
            return input.Contains(secretKey);
        }

        public string EncriptarSimple(string texto)
        {
            char[] chars = texto.ToCharArray();
            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = (char)(chars[i] + 3);
            }
            return new string(chars);
        }
    }
}
