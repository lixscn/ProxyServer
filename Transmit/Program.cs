﻿using System;

namespace Transmit
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            SocketService sks = new SocketService();
            sks.OnStart();

            Console.ReadLine();
        }
    }
}