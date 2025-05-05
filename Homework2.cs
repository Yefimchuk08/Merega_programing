using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    static void Main()
    {
        Console.WriteLine("Select a task to run (1-11):");
        Console.WriteLine("1 - TCP Greet Server\n2 - UDP Square Server\n3 - UDP Client Every 3 Seconds");
        Console.WriteLine("4 - TCP Log Server\n5 - UDP Random Client\n6 - TCP Timestamp Echo");
        Console.WriteLine("7 - UDP Even/Odd Server\n8 - UDP Port Checker\n9 - TCP Chat");
        Console.WriteLine("10 - TCP File Transfer\n11 - UDP Calculator");

        int task = int.Parse(Console.ReadLine());

        switch (task)
        {
            case 1: Task.Run(TcpGreetServer); break;
            case 2: Task.Run(UdpSquareServer); break;
            case 3: Task.Run(UdpClientEvery3Seconds); break;
            case 4: Task.Run(TcpLogServer); break;
            case 5: Task.Run(UdpRandomClient); break;
            case 6: Task.Run(TcpTimestampEchoServer).Wait(); break;
            case 7: Task.Run(UdpEvenOddServer); break;
            case 8: Task.Run(UdpPortCheckerClient); break;
            case 9: Task.Run(TcpChatServer); break;
            case 10: Task.Run(TcpFileReceiverServer); break;
            case 11: Task.Run(UdpCalculatorServer); break;
        }

        Console.ReadLine();
    }

    static async Task TcpGreetServer() { /* ... unchanged ... */ }
    static async Task UdpSquareServer() { /* ... unchanged ... */ }
    static async Task UdpClientEvery3Seconds() { /* ... unchanged ... */ }
    static async Task TcpLogServer() { /* ... unchanged ... */ }
    static async Task UdpRandomClient() { /* ... unchanged ... */ }
    static async Task TcpTimestampEchoServer() { /* ... unchanged ... */ }
    static async Task UdpEvenOddServer() { /* ... unchanged ... */ }
    static async Task UdpPortCheckerClient() { /* ... unchanged ... */ }
    static async Task TcpChatServer() { /* ... unchanged ... */ }
    static async Task TcpFileReceiverServer() { /* ... unchanged ... */ }

    // 11. Enhanced UDP Calculator Server
    static async Task UdpCalculatorServer()
    {
        UdpClient server = new UdpClient(5008);
        Console.WriteLine("UDP Calculator Server started.");
        var clients = new Dictionary<IPEndPoint, DateTime>();

        _ = Task.Run(async () => // Connection timeout checker
        {
            while (true)
            {
                var now = DateTime.Now;
                foreach (var client in clients.Keys.ToList())
                {
                    if ((now - clients[client]).TotalSeconds > 30)
                    {
                        Console.WriteLine($"Connection to {client} timed out.");
                        clients.Remove(client);
                    }
                }
                await Task.Delay(5000);
            }
        });

        while (true)
        {
            var result = await server.ReceiveAsync();
            var endpoint = result.RemoteEndPoint;
            clients[endpoint] = DateTime.Now;

            string expr = Encoding.UTF8.GetString(result.Buffer);
            string[] parts = expr.Split(' ');
            string response;

            if (parts.Length < 2 || parts.Length > 3 || !double.TryParse(parts[0], out double a) || (parts.Length == 3 && !double.TryParse(parts[2], out double b)))
            {
                response = "Error: Invalid input.";
            }
            else
            {
                string op = parts[1];
                double resultVal = 0;
                bool valid = true;
                switch (op)
                {
                    case "+": resultVal = a + b; break;
                    case "-": resultVal = a - b; break;
                    case "*": resultVal = a * b; break;
                    case "/":
                        if (b == 0) { response = "Error: Division by zero!"; valid = false; break; }
                        resultVal = a / b;
                        break;
                    case "^": resultVal = Math.Pow(a, b); break;
                    case "%": resultVal = a * b / 100.0; break;
                    case "sqrt": resultVal = Math.Sqrt(a); break;
                    default:
                        response = "Error: Unknown operation."; valid = false;
                        break;
                }
                if (valid) response = $"Result = {resultVal}";
            }

            string log = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] Request: A={parts[0]}, B={(parts.Length > 2 ? parts[2] : "")}, Operation={parts[1]}, Response={response}";
            File.AppendAllText("log.txt", log + Environment.NewLine);
            byte[] data = Encoding.UTF8.GetBytes(response);
            await server.SendAsync(data, data.Length, endpoint);
        }
    }
}
