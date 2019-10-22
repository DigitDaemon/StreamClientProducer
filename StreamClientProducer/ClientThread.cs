using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Timers;

namespace StreamClientProducer
{
    class ClientThread
    {
        const int ALLOWABLE_FAILURES = 10;
        const int RESET_CYCLES = 1000;
        static private string username = "DigitDaemon";
        private static string path = Path.Combine(Environment.CurrentDirectory.Replace(@"bin\Debug\netcoreapp2.1", ""), @"Data\");
        static private string password = File.ReadAllText(Path.Combine(path, "Token.txt"));//do not push this!!!
        static private char[] trimChar = new char[69] { 'a','b','c','d','e','f','g','h','i','j','k','l','m','n','o','p',
            'q','r','s','t','u','v','w','x','y','z','A','B','C','D','E','F','G','H','I','J','K','L',
            'M','N','O','P','Q','R','S','T','U','V','W','X','Y','Z',' ','.','@','#','!', '_', '-', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0'};
        ConcurrentQueue<String> messageQueue;
        protected bool ActiveThread;
        TcpClient client;
        StreamWriter writer;
        StreamReader reader;
        int cycles;
        string channel;
        System.Timers.Timer trigger;

        public ClientThread(String channel, int time, ref System.Timers.Timer trigger, ref ConcurrentQueue<string> queue)
        {
            Console.WriteLine(channel + " Thread Start");
            cycles = 0;
            this.messageQueue = queue;
            this.ActiveThread = true;
            this.channel = channel;
            this.trigger = trigger;
        }

        public void CThread()
        {
            Console.WriteLine("Channel Thread " + channel + " start");
            Connect();
            trigger.Elapsed += onTick;

            while (this.ActiveThread)
            {

            }


            closeThreadResources();
            trigger.Elapsed -= onTick;
            Console.WriteLine("thread end");
            
        }

       public void Kill()
        {
            this.ActiveThread = false;
        }

        private void closeThreadResources()
        {
            try
            {
                client.Close();
            }
            catch (Exception e)
            {

            }
            try
            {
                writer.Close();
            }
            catch (Exception e)
            {

            }
            try
            {
                reader.Close();
            }
            catch (Exception e)
            {

            }
        }

        private void flushThread()
        {
            closeThreadResources();
            Thread.Yield();
            Connect();
        }

        private void onTick(Object source, ElapsedEventArgs e)
        {
            cycles++;

            if (cycles > RESET_CYCLES)
            {
                flushThread();
                cycles = 0;
            }

            if (cycles % 100 == 0)
            {
                Thread.Yield();
                //Console.WriteLine(channel + " yield at 30");
            }

            if (!client.Connected)
            {
                Connect();
            }

            try
            {
                if (client.Available > 0 || reader.Peek() >= 0)
                {

                    var message = reader.ReadLine();
                    var uname = "";

                    message = message.Remove(0, 1);
                    if (message.Contains("@") && !message.Contains("JOIN"))
                    {
                        while (!message[0].Equals('!'))
                        {
                            uname += message[0];
                            message = message.Remove(0, 1);
                        }

                        message = message.TrimStart(trimChar);
                        message = message.Remove(0, 1);
                        messageQueue.Enqueue(channel + " " + uname + " " + message);
                        //Console.WriteLine(uname + ": " + message);
                    }
                    else
                    {
                        //Console.WriteLine(message);
                    }


                }
                else
                {
                    Thread.Yield();
                    //Console.WriteLine(channel + " sleep at no messages");
                }
            }
            catch (Exception j)
            {
                Console.WriteLine("~~~~~~~~~~~~~~~~~~~~ERROR~~~~~~~~~~~~~~~~~~");
                Console.WriteLine(channel);
                Console.WriteLine(j.Message);
                Console.WriteLine(j.StackTrace);
                flushThread();
            }

        }

        void Connect()
        {
            Console.WriteLine("Connect to " + channel + " start");
            //Console.WriteLine(channel + " Connect Start");
            int attempts = 0;
            //Connect to twitch irc
            do
            {

                try
                {
                    attempts++;
                    client = new TcpClient("irc.chat.twitch.tv.", 6667);
                    //Console.WriteLine(channel + " TcpClient");
                    writer = new StreamWriter(client.GetStream());
                    //Console.WriteLine(channel + " StreamWriter");
                    reader = new StreamReader(client.GetStream());
                    //Console.WriteLine(channel + " StreamReader");
                    //Log in
                    writer.WriteLine("PASS " + password + Environment.NewLine
                        + "NICK " + username + Environment.NewLine
                        + "USER " + username + " 8 * :" + username);
                    //Console.WriteLine(channel + " Login");
                    writer.WriteLine("JOIN #" + channel);
                    //Console.WriteLine(channel + " Join");
                    writer.Flush();
                    //Console.WriteLine(channel + " flush");

                    break;
                }
                catch (Exception e)
                {
                    Console.WriteLine("ERROR: " + e.Message);
                    if (attempts > ALLOWABLE_FAILURES)
                        throw new Exception("Network Error" + e.Message);
                    else
                        Thread.Sleep(1000);
                }
            } while (true);
            Console.WriteLine("Connect to " + channel + " end");
        }



    }
}
