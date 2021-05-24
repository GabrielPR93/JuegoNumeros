using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ejercicio_9
{
    class Program
    {
        public static readonly object l = new object();
        public static List<Cliente> clientes = new List<Cliente>();
        public static List<int> numerosClientes = new List<int>();
        public static Random r = new Random();
        public static bool flag = true;
        public static int cont = 10;
        public static bool entra = true;
        static void Main(string[] args)
        {
            IPEndPoint ie = new IPEndPoint(IPAddress.Any, 31416);
            Socket s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {

                s.Bind(ie);
                s.Listen(5);
            }
            catch (SocketException e) when (e.ErrorCode == (int)SocketError.AddressAlreadyInUse)
            {
                Console.WriteLine("Port {1} in use: " + ie.Port);
                flag = false;
            }

            while (flag)
            {
                Socket cliente = s.Accept();
                lock (l)
                {

                    clientes.Add(new Cliente(cliente));
                }

                Thread hilo = new Thread(hiloCliente);
                hilo.Start(cliente);


            }

        }
        private static void enviarTodos(string mensaje)
        {
            if (mensaje != null)
            {
                lock (l)
                {
                    for (int i = 0; i < clientes.Count; i++)
                    {
                        try
                        {
                            using (NetworkStream ns = new NetworkStream(clientes[i].socketCliente))
                            using (StreamWriter sw = new StreamWriter(ns))
                            {
                                sw.WriteLine(mensaje);
                                sw.Flush();

                            }
                        }
                        catch (IOException e)
                        {
                            desconexion(clientes[i]);
                            entra = false;
                            Console.WriteLine(e.Message);

                        }
                    }
                }
            }
        }
        private static void enviarCliente(Cliente c, string mensaje)
        {
            if (mensaje != null)
            {
                lock (l)
                {
                    try
                    {
                        using (NetworkStream ns = new NetworkStream(c.socketCliente))
                        using (StreamWriter sw = new StreamWriter(ns))
                        {
                            sw.WriteLine(mensaje);
                            sw.Flush();
                        }
                    }
                    catch (IOException e)
                    {
                        desconexion(c);
                        entra = false;
                        Console.WriteLine(e.Message);
                    }
                }
            }
        }
        public static void tiempo()
        {

            while (cont >= 0)
            {
                enviarTodos("this game starts in: " + cont.ToString());

                cont--;
                Thread.Sleep(800);
            }

            for (int i = 0; i < clientes.Count; i++)
            {
                numerosRandom(clientes[i]);

            }
            comprobarGanador();
            reset();

        }

        public static void numerosRandom(Cliente c)
        {
            bool correcto = true;
            bool flagEnviar = false;

            while (correcto)
            {
                c.numeroCliente = r.Next(1, 21);

                lock (l)
                {
                    if (!numerosClientes.Contains(c.numeroCliente))
                    {
                        correcto = false;
                        numerosClientes.Add(c.numeroCliente);
                        flagEnviar = true;
                    }
                    if (flagEnviar)
                    {
                        enviarCliente(c, "You number is : " + c.numeroCliente);

                    }
                }
            }
        }

        public static void comprobarGanador()
        {

            int num = clientes[0].numeroCliente;

            if (clientes.Count > 0)
            {
                for (int x = 0; x < clientes.Count; x++)
                {
                    if (num < clientes[x].numeroCliente)
                    {
                        num = clientes[x].numeroCliente;
                    }
                }
                for (int i = 0; i < clientes.Count; i++)
                {

                    if (num == clientes[i].numeroCliente)
                    {

                        enviarCliente(clientes[i], "Congratulations you number is winner !!");
                    }
                    else
                    {
                        enviarCliente(clientes[i], "Number winner is: " + num);
                    }
                }

            }

        }
        public static void reset()
        {
            lock (l)
            {
                for (int i = 0; i < clientes.Count; i++)
                {
                    clientes[i].socketCliente.Close();
                }
                clientes.Clear();
                numerosClientes.Clear();
                cont = 10;
                entra = true;
            }
        }
        public static void desconexion(Cliente c)
        {
            lock (l)
            {

                c.socketCliente.Close();
                numerosClientes.Remove(c.numeroCliente);
                clientes.Remove(c);

            }
        }

        public static void hiloCliente(object socket)
        {
            bool flag = true;
            Socket scliente = (Socket)socket;
            IPEndPoint ieCliente = (IPEndPoint)scliente.RemoteEndPoint;
            Console.WriteLine("Connected with client {0} at port {1}", ieCliente.Address, ieCliente.Port);
            try
            {
                using (NetworkStream ns = new NetworkStream(scliente))
                using (StreamWriter sw = new StreamWriter(ns))
                {

                    sw.WriteLine("Wellcome to this game");
                    sw.Flush();
                  
                    if (clientes.Count == 2)
                    {
                        if (entra)
                        {
                            Thread hiloTiempo = new Thread(tiempo);
                            hiloTiempo.Start();

                        }

                    }
                    if (clientes.Count < 2)
                    {

                        enviarTodos("waiting for more players");
                    }

                }
            }
            catch (IOException e)
            {

                Console.WriteLine(e.Message);

            }


        }
    }
}
