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
        List<Command> receivedCommands = connection.Receive();
        //Class-Instances
        ConsoleInput consoleInput = new ConsoleInput();
        LoginView loginView = new LoginView(connection, consoleInput);
        View registerView = new RegisterView(connection, consoleInput);


        // connection.Send(new SendMessageCommand("Bamse", "Hej hopp!"));
        // string? LoginNameInput = consoleInput.GetInput() ?? ""; //Console.ReadLine() ?? "";
        // string LoginPasswordInput = consoleInput.GetInput() ?? "";

        //connection.Send(new LoginCommand(LoginNameInput, LoginPasswordInput));
        // connection.Send(new SendMessageCommand("Bamse", "Hallåj"));

        Console.WriteLine("Welcome. What do you want to do?");
        Console.WriteLine("Login : press '1'");
        Console.WriteLine("Register : press '2'");
        Console.WriteLine("Exit : type 'exit'");

        while (true)
        {

            receivedCommands = connection.Receive();

            string state = "entry";
            if (state == "entry")
            {
                string? input = Console.ReadLine();
                if (input == null)
                {
                    Console.WriteLine("Not valid input, try again");
                    return;
                }

                switch (input)
                {
                    case "1":
                        loginView.Execute();
                        Console.WriteLine("Press any key to continue..."); //Async
                        Console.ReadKey();
                        state = loginView.ListenForAuth(receivedCommands, connection, state);
                        break;

                    case "2":
                        registerView.Execute();
                        state = "entry";
                        break;

                    case "exit":
                        state = "entry";
                        break;
                }
                // TODO: Ge meny
                // login -> state = "loggedin"
            }


            switch (state)
            {
                case "loggedin":
                    {
                        Console.WriteLine("logged in state");
                        // loopa alla meddelanden
                        foreach (Command receivedCommand in receivedCommands)
                        {
                            if (receivedCommand is SendMessageCommand)
                            {
                                Console.WriteLine("---");
                                SendMessageCommand message = (SendMessageCommand)receivedCommand;
                                Console.WriteLine($"{message.Sender} Sent: {message.Content}");
                            }
                        }
                    //connection.Send(new SendMessageCommand(Console.ReadLine(),""));
                        
                    }
                    break;

                case "register":
                    //do something
                    break;

                case "entry":
                    //do something
                    break;
            }


            // if (state == "loggedin")
            // {
            //     Console.WriteLine("logged in state");
            // }




            //Todo set Sender till connected User och Content
            // connection.Send(new SendMessageCommand(LoginNameInput, Console.ReadLine()!));

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

public class ConsoleInput
{
    public ConsoleInput() { }

    public string? GetInput()
    {
        string input = Console.ReadLine() ?? "";
        if (input == null || input == "")
        {
            Console.WriteLine("Not valid input, try again");
            return null;
        }
        else
        {
            return input;
        }
    }
}

public abstract class View
{
    public ConsoleInput ConsoleInput { get; set; }
    public IConnection Connection { get; set; }
    public View(IConnection connection, ConsoleInput consoleInput)
    {
        this.ConsoleInput = consoleInput;
        this.Connection = connection;
    }

    public abstract void Execute();
}

public class LoginView : View
{
    public LoginView(IConnection connection, ConsoleInput consoleInput)
       : base(connection, consoleInput) { }
    public override void Execute()
    {
        Console.WriteLine("Type your username");
        string? UsernameInput = ConsoleInput.GetInput() ?? "";

        Console.WriteLine("Type your password");
        string? passwordInput = ConsoleInput.GetInput() ?? "";

        Connection.Send(new LoginCommand(UsernameInput, passwordInput));
    }

     
    public string ListenForAuth(List<Command> receivedCommands, IConnection connection, string state)
    {
        state = "entry";
        receivedCommands = connection.Receive();
        foreach (Command receivedCommand in receivedCommands)
        {
            if (receivedCommand is SendMessageCommand)
            {
                SendMessageCommand message = (SendMessageCommand)receivedCommand;
                Console.WriteLine($"{message.Sender} Sent: {message.Content}");
                state = "loggedin";
            }
        }
        return state;
    }
    
}

public class RegisterView : View
{
    public RegisterView(IConnection connection, ConsoleInput consoleInput)
       : base(connection, consoleInput) { }
    public override void Execute()
    {
        Console.WriteLine("Type your new username");
        string UsernameInput = ConsoleInput.GetInput() ?? "";

        Console.WriteLine("Type your new password");
        string passwordInput = ConsoleInput.GetInput() ?? "";

        Connection.Send(new RegisterUserCommand(UsernameInput, passwordInput));
    }
}