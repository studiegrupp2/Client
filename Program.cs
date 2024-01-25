using System.Net;
using System.Net.Sockets;
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
        
        // connection.Send(new SendMessageCommand("Bamse", "Hej hopp!"));

        string LoginNameInput = Console.ReadLine() ?? "";
        string LoginPasswordInput = Console.ReadLine() ?? "";
    
        connection.Send(new LoginCommand(LoginNameInput, LoginPasswordInput));
        
        
        

        

        string state = "entry";
    
   
   while (true)
        {
            List<Command> receivedCommands = connection.Receive();

            foreach (Command receivedCommand in receivedCommands)
            {
                if (receivedCommand is SendMessageCommand)
                {
                    SendMessageCommand message = (SendMessageCommand)receivedCommand;
                    Console.WriteLine($"{message.Sender} Sent: {message.Content}");
                }
                
            }

            Console.WriteLine("Write a message: ");


//Todo set Sender till connected User och Content
            connection.Send(new SendMessageCommand(Console.ReadLine()!, ""));

            // if (state == "entry") {
            //     // TODO: Ge meny
            //     // login -> state = "loggedin"
            // } else if (state == "loggedin") {
            //     // 
            // }
            // connection.Receive();
            // string input = Console.ReadLine() ?? "";
            //byte[] buffer = System.Text.Encoding.UTF8.GetBytes(input);

            // connection.Send(buffer);

            // buffer = new byte[10000];
            // int read = socket.Receive(buffer);

            // string content = System.Text.Encoding.UTF8.GetString(buffer, 0, read);
            // Console.WriteLine("Echo: " + content);

            
          
        }
        // Console.WriteLine("Type anything to close");
        // Console.ReadLine();
    }

    
    
}
