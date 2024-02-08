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
        RegisterView registerView = new RegisterView(connection, consoleInput);
        HelpInformation helpInformation = new HelpInformation();

        string state = "entry";
        string loggedInUser = "";

        while (true)
        {
            if (state == "entry")
            {
                helpInformation.PrintWelcomeInfo();
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
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        Console.WriteLine();
                        state = loginView.ListenForAuth(receivedCommands, connection, state);

                        if (state == "loggedin")
                        {
                            helpInformation.PrintHelp();
                        }
                        break;

                    case "2":
                        registerView.Execute();
                        Console.WriteLine("Press any key to continue...");
                        Console.ReadKey();
                        state = registerView.ListenForAuth(receivedCommands, connection, state);
                        break;

                    case "3":
                        Console.WriteLine("Successfully logged out");
                        state = "entry";
                        break;

                    case "4":
                        connection.Send(new DisconnectCommand());
                        System.Environment.Exit(0);
                        break;

                    case "help":
                        helpInformation.PrintHelp();
                        break;
                }
            }

            switch (state)
            {
                case "loggedin":
                    {
                        receivedCommands = connection.Receive();
                        foreach (Command receivedCommand in receivedCommands)
                        {
                            if (receivedCommand is SendPrivateMessageCommand)
                            {
                                SendPrivateMessageCommand message = (SendPrivateMessageCommand)receivedCommand;
                                Console.WriteLine($"Private message from {message.Sender}: {message.Content}");
                            }
                            if (receivedCommand is SendMessageCommand)
                            {
                                SendMessageCommand message = (SendMessageCommand)receivedCommand;
                                Console.WriteLine($"{message.Sender} sent: {message.Content}");
                            }
                        }

                        if (!Console.KeyAvailable) break;

                        string? input = Console.ReadLine();
                        switch (input)
                        {
                            case null:
                                Console.WriteLine("");
                                Console.WriteLine("No input received");
                                Console.WriteLine("To see commands: type 'help'");
                                break;

                            case "1":
                                Console.WriteLine("Enter your message:");
                                connection.Send(new SendMessageCommand(loggedInUser, Console.ReadLine()!));
                                break;

                            case "2":
                                Console.WriteLine("Enter username for receiver:");
                                string receiver = Console.ReadLine() ?? "";
                                Console.WriteLine("Enter message:");
                                connection.Send(new SendPrivateMessageCommand(loggedInUser, receiver, Console.ReadLine()!));
                                break;

                            case "3":
                                connection.Send(new LogoutCommand(loggedInUser));
                                state = "entry";
                                break;

                            case "help":
                                helpInformation.PrintHelp();
                                break;

                            default: //If the user does not type any of the above cases
                                Console.WriteLine("Not valid input");
                                Console.WriteLine("To see commands: type 'help'");
                                break;
                        }
                    }
                    break;
            }
        }
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

public class HelpInformation
{
    public void PrintHelp()
    {
        Console.WriteLine();
        Console.WriteLine("To send message to all: type 1");
        Console.WriteLine("To send private message: type 2");
        Console.WriteLine("To logout: type 3");
        Console.WriteLine("To see commands: type 'help'");
        Console.WriteLine();
    }
    public void PrintWelcomeInfo()
    {
        Console.WriteLine("Welcome. What do you want to do?");
        Console.WriteLine("Login : press '1'");
        Console.WriteLine("Register : press '2'");
        Console.WriteLine("Exit : type '4'");
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

    public virtual string ListenForAuth(List<Command> receivedCommands, IConnection connection, string state)
    {
        state = "entry";
        receivedCommands = connection.Receive();
        foreach (Command receivedCommand in receivedCommands)
        {
            if (receivedCommand is SendMessageCommand)
            {
                SendMessageCommand message = (SendMessageCommand)receivedCommand;

                if (message.Content != "Login failed. Wrong username or password.")
                {
                    Console.WriteLine($"{message.Sender} Sent: {message.Content}");
                    state = "loggedin";
                }
                else
                {
                    Console.WriteLine("Failed to login. Wrong username or password.");
                }
            }
            else if (receivedCommand is SendPrivateMessageCommand)
            {
                SendPrivateMessageCommand PrivateMessage = (SendPrivateMessageCommand)receivedCommand;
                if (PrivateMessage != null)
                {
                    Console.WriteLine($"Private message from {PrivateMessage.Sender}: {PrivateMessage.Content}");
                }
            }
        }
        return state;
    }
}

public class RegisterView : LoginView
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

    public override string ListenForAuth(List<Command> receivedCommands, IConnection connection, string state)
    {
        state = "entry";
        receivedCommands = connection.Receive();
        foreach (Command receivedCommand in receivedCommands)
        {
            if (receivedCommand is SendMessageCommand)
            {
                SendMessageCommand message = (SendMessageCommand)receivedCommand;
                Console.WriteLine($"{message.Sender} Sent: {message.Content}");
            }
        }
        return state;
    }
}