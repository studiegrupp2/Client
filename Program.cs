using System.Net;
using System.Net.Sockets;
namespace Client;
using Shared;
class Program
{
    static void Main(string[] args)
    {
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

        // Console.WriteLine("Welcome. What do you want to do?");
        // Console.WriteLine("Login : press '1'");
        // Console.WriteLine("Register : press '2'");
        // Console.WriteLine("Exit : type 'exit'");

        string state = "entry";
        string loggedInUser = "";
        // bool userLoggedIn = true; // för att kunna avsluta loopen

        while (true)
        {
            if (state == "entry")
            {
                Console.WriteLine("Welcome. What do you want to do?");
                Console.WriteLine("Login : press '1'");
                Console.WriteLine("Register : press '2'");
                Console.WriteLine("Exit : type '4'");
                string? input = Console.ReadLine();
                if (input == null)
                {
                    Console.WriteLine("Not valid input, try again");
                    return;
                }

                switch (input)
                {
                    case "1":
                        loggedInUser = loginView.Execute();
                        Console.WriteLine("Press any key to continue..."); //Async
                        Console.ReadKey();
                        state = loginView.ListenForAuth(receivedCommands, connection, state);

                        if (state == "loggedin")
                        {
                            Console.WriteLine("logged in state");
                            // loopa alla meddelanden
                            Console.WriteLine("To send message to all: type 1");
                            Console.WriteLine("To send private message: type 2");
                            Console.WriteLine("To logout: type 3");
                            Console.WriteLine("To exit chattapplication: type 4");
                        }
                        break;

                    case "2":
                        registerView.Execute();
                        state = "entry";
                        break;

                    case "3":
                        Console.WriteLine("Successfully logged out");
                        state = "entry";
                        break;

                    case "4":
                        connection.Send(new DisconnectCommand());
                        System.Environment.Exit(0);
                        break;

                }

            }


            switch (state)
            {
                case "loggedin":
                    {
                        //while()
                        // if (!Console.KeyAvailable) break;

                        receivedCommands = connection.Receive();
                        foreach (Command receivedCommand in receivedCommands)
                        {

                            if (receivedCommand is SendMessageCommand)
                            {
                                SendMessageCommand message = (SendMessageCommand)receivedCommand;
                                Console.WriteLine($"{message.Sender} Sent: {message.Content}");
                            }
                        }

                        if (!Console.KeyAvailable) break;

                        string? input = Console.ReadLine();
                        if (input == null)
                        {
                            Console.WriteLine("no input received");
                            return;
                        }

                        if (input == "1")
                        {
                            Console.WriteLine("enter your message:");
                            connection.Send(new SendMessageCommand(loggedInUser, Console.ReadLine()!));
                        }

                        else if (input == "2")
                        {
                            Console.WriteLine("Enter username for receiver:");
                            string receiver = Console.ReadLine() ?? "";
                            Console.WriteLine("Enter messege:");
                            connection.Send(new SendPrivateMessageCommand(loggedInUser, receiver, Console.ReadLine()!));
                        }
                        else if (input == "3")
                        {
                            connection.Send(new LogoutCommand(loggedInUser));
                            state = "entry";
                            // userLoggedIn = false;
                        }
                        else if (input == "4")
                        {
                            //exit connection application 
                        }

                        //while-loop

                        // connection.Send(new SendMessageCommand("", Console.ReadLine()!));
                        // connection.Send(new SendPrivateMessageCommand("",Console.ReadLine()!, Console.ReadLine()!));

                    }
                    break;


                case "entry":
                    //do something
                    break;
            }
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

    public abstract string Execute();
}

public class LoginView : View
{
    public LoginView(IConnection connection, ConsoleInput consoleInput)
       : base(connection, consoleInput) { }
    public override string Execute()
    {
        Console.WriteLine("Type your username");
        string? UsernameInput = ConsoleInput.GetInput() ?? "";

        Console.WriteLine("Type your password");
        string? passwordInput = ConsoleInput.GetInput() ?? "";

        Connection.Send(new LoginCommand(UsernameInput, passwordInput));

        return UsernameInput;
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
    public override string Execute()
    {
        Console.WriteLine("Type your new username");
        string UsernameInput = ConsoleInput.GetInput() ?? "";

        Console.WriteLine("Type your new password");
        string passwordInput = ConsoleInput.GetInput() ?? "";

        Connection.Send(new RegisterUserCommand(UsernameInput, passwordInput));

        return UsernameInput;
    }
}