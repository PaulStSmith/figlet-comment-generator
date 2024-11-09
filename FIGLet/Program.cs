namespace FIGlet
{
    internal class Program
    {
        static void Main()
        {
            var fnt = FIGFont.LoadFromFile("fonts\\small.flf");
            var renderer = new FIGletRenderer(fnt);
            Console.WriteLine(renderer.Render("Hello, World!"));
        }
    }
}
