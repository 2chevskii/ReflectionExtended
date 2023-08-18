using System.Linq.Expressions;

internal class Program
{
    public        int m_Field;
    public static int Field;

    [Example]
    public static void Main(string[] args)
    {
        Expression<Action> expr = () => Program.Main(new[]{""});

        Expression<Func<int>> expr2 = () => Program.Field;


        Expression<Func<Program, int>> expr3 = p => p.m_Field;
    }
}

internal class ExampleAttribute : Attribute
{
    public string ExampleName { get; set; }

    public ExampleAttribute(string exampleName = "example")
    {
        ExampleName = exampleName;
    }
}
