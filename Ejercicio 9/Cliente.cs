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
    class Cliente
    {
       
        public static string msg;
        public IPEndPoint iPEndPointCliente { set; get; }
        public Socket socketCliente { set; get; }
        public int numeroCliente;
     

        public Cliente(Socket socket)
        {
            this.socketCliente = socket;
            this.iPEndPointCliente = (IPEndPoint)this.socketCliente.RemoteEndPoint;
        }

        public void SCliente()
        {
            try
            {

                using (NetworkStream ns=new NetworkStream(socketCliente))
                using (StreamReader sr=new StreamReader(ns))
            
                {
                    msg = sr.ReadLine();
                    Console.WriteLine(msg);

                }
            }
            catch (IOException e)
            {
                Console.WriteLine(e.Message);
                desconexion();
               
            }
          
           
        }
        public void desconexion()
        {
            Program.desconexion(this);
        }
    }
}
