namespace FIGLet
{
    internal class Program
    {
        static void Main()
        {
            var fnt = FIGFont.LoadFromFile("fonts\\standard.flf");
            var renderer = new FIGLetRenderer(fnt);
            Console.WriteLine(renderer.Render("C#"));
        }
    }
}
