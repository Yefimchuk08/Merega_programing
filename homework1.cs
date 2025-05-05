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

    static async Task TcpGreetServer()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5001);
        server.Start();
        Console.WriteLine("TCP Greet Server started.");
        while (true)
        {
            var client = await server.AcceptTcpClientAsync();
            var stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int size = await stream.ReadAsync(buffer);
            string name = Encoding.UTF8.GetString(buffer, 0, size);
            string response = $"Hello, {name}!";
            await stream.WriteAsync(Encoding.UTF8.GetBytes(response));
            client.Close();
        }
    }

    static async Task UdpSquareServer()
    {
        UdpClient server = new UdpClient(5002);
        Console.WriteLine("UDP Square Server started.");
        while (true)
        {
            var result = await server.ReceiveAsync();
            int number = int.Parse(Encoding.UTF8.GetString(result.Buffer));
            int square = number * number;
            byte[] response = Encoding.UTF8.GetBytes(square.ToString());
            await server.SendAsync(response, response.Length, result.RemoteEndPoint);
        }
    }

    static async Task UdpClientEvery3Seconds()
    {
        UdpClient client = new UdpClient();
        while (true)
        {
            string msg = $"Ping {DateTime.Now:HH:mm:ss}";
            byte[] data = Encoding.UTF8.GetBytes(msg);
            await client.SendAsync(data, data.Length, "127.0.0.1", 5002);
            Console.WriteLine("Sent: " + msg);
            await Task.Delay(3000);
        }
    }

    static async Task TcpLogServer()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5003);
        server.Start();
        Console.WriteLine("TCP Log Server started.");
        while (true)
        {
            var client = await server.AcceptTcpClientAsync();
            var stream = client.GetStream();
            byte[] buffer = new byte[1024];
            int size = await stream.ReadAsync(buffer);
            string msg = Encoding.UTF8.GetString(buffer, 0, size);
            File.AppendAllText("log.txt", msg + Environment.NewLine);
            client.Close();
        }
    }

    static async Task UdpRandomClient()
    {
        UdpClient client = new UdpClient();
        Random rand = new Random();
        int number = rand.Next(1, 100);
        byte[] data = Encoding.UTF8.GetBytes(number.ToString());
        await client.SendAsync(data, data.Length, "127.0.0.1", 5002);
        var result = await client.ReceiveAsync();
        Console.WriteLine($"Sent: {number}, Received: {Encoding.UTF8.GetString(result.Buffer)}");
    }

    static async Task TcpTimestampEchoServer()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5004);
        server.Start();
        Console.WriteLine("TCP Timestamp Echo Server started.");
        var client = await server.AcceptTcpClientAsync();
        var stream = client.GetStream();
        for (int i = 0; i < 3; i++)
        {
            byte[] buffer = new byte[1024];
            int size = await stream.ReadAsync(buffer);
            string msg = Encoding.UTF8.GetString(buffer, 0, size);
            string response = $"[{DateTime.Now:HH:mm:ss}] Received: {msg}";
            await stream.WriteAsync(Encoding.UTF8.GetBytes(response));
            Thread.Sleep(2000);
        }
        client.Close();
    }

    static async Task UdpEvenOddServer()
    {
        UdpClient server = new UdpClient(5005);
        Console.WriteLine("UDP Even/Odd Server started.");
        while (true)
        {
            var result = await server.ReceiveAsync();
            int num = int.Parse(Encoding.UTF8.GetString(result.Buffer));
            string response = (num % 2 == 0) ? "Even" : "Odd";
            await server.SendAsync(Encoding.UTF8.GetBytes(response), response.Length, result.RemoteEndPoint);
        }
    }

    static async Task UdpPortCheckerClient()
    {
        UdpClient client = new UdpClient();
        int[] ports = { 5001, 5002, 5005, 5010 };
        foreach (int port in ports)
        {
            string ping = "ping";
            byte[] data = Encoding.UTF8.GetBytes(ping);
            await client.SendAsync(data, data.Length, "127.0.0.1", port);
            client.Client.ReceiveTimeout = 500;
            try
            {
                var result = await client.ReceiveAsync();
                Console.WriteLine($"Port {port} OK: {Encoding.UTF8.GetString(result.Buffer)}");
            }
            catch
            {
                Console.WriteLine($"Port {port} NOT responding.");
            }
        }
    }

    static async Task TcpChatServer()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5006);
        server.Start();
        Console.WriteLine("TCP Chat Server started.");
        var client = await server.AcceptTcpClientAsync();
        var stream = client.GetStream();
        byte[] buffer = new byte[1024];
        while (true)
        {
            int size = await stream.ReadAsync(buffer);
            string msg = Encoding.UTF8.GetString(buffer, 0, size);
            if (msg.ToLower() == "exit") break;
            string response = $"Accepted: {msg}";
            await stream.WriteAsync(Encoding.UTF8.GetBytes(response));
        }
        client.Close();
    }

    static async Task TcpFileReceiverServer()
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5007);
        server.Start();
        Console.WriteLine("TCP File Receiver started.");
        while (true)
        {
            var client = await server.AcceptTcpClientAsync();
            var stream = client.GetStream();
            byte[] nameBuf = new byte[256];
            int nameSize = await stream.ReadAsync(nameBuf);
            string filename = Encoding.UTF8.GetString(nameBuf, 0, nameSize);

            using (FileStream fs = new FileStream("ReceivedFiles/" + filename, FileMode.Create))
            {
                byte[] buffer = new byte[1024];
                int bytes;
                while ((bytes = await stream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                    fs.Write(buffer, 0, bytes);
            }

            await stream.WriteAsync(Encoding.UTF8.GetBytes("File received: " + filename));
            client.Close();
        }
    }


    static async Task UdpCalculatorServer()
    {
        UdpClient server = new UdpClient(5008);
        Console.WriteLine("UDP Calculator Server started.");
        while (true)
        {
            var result = await server.ReceiveAsync();
            string expr = Encoding.UTF8.GetString(result.Buffer);
            string[] parts = expr.Split(' ');
            double a = double.Parse(parts[0]);
            string op = parts[1];
            double b = double.Parse(parts[2]);
            double res = op switch
            {
                "+" => a + b,
                "-" => a - b,
                "*" => a * b,
                "/" => b != 0 ? a / b : double.NaN,
                "^" => Math.Pow(a, b),
                _ => double.NaN
            };
            byte[] response = Encoding.UTF8.GetBytes(res.ToString());
            await server.SendAsync(response, response.Length, result.RemoteEndPoint);
        }
    }
}
