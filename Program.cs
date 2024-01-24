namespace Client;
using Shared;
class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Hello, grupp2!");
        Shared.IConnection connection = Shared.SocketConnection.Connect(
            new byte[] { 127, 0, 0, 1 },
            27800
        );
        connection.Send(new RegisterUserCommand("Ironman", "stark123"));
        connection.Send(new LoginCommand("Ironman", "stark123"));
        
    }
}
