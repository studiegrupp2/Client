namespace Client;

class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, grupp2!");
        Shared.IConnection connection = Shared.SocketConnection.Connect(
            new byte[] { 127, 0, 0, 1 },
            27800
        );


    }
}
