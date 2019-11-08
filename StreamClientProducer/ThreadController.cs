using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace StreamClientProducer
{

    class ThreadController
    {
        ConcurrentQueue<String> messageQueue;
        static System.Timers.Timer trigger;
        List<string> Channels;
        List<bool> ThreadActiveBool;
        List<Thread> pool;
        List<ClientThread> ct;
        TwitchProducer producer;
        CommandConsumer command;
        Thread pro;
        Thread com;
        HashSet<string> blacklist;


        public ThreadController()
        {
            blacklist = new HashSet<string>();
            trigger = new System.Timers.Timer(100);
            trigger.AutoReset = true;
            trigger.Enabled = true;
            messageQueue = new ConcurrentQueue<String>();
            Channels = new List<string>();
            ThreadActiveBool = new List<bool>();
            ct = new List<ClientThread>();
            pool = new List<Thread>();
            producer = new TwitchProducer(ref messageQueue);
            pro = new Thread(producer.ProducerThread);
            pro.Name = "PRODUCER";
            pro.Start();
            command = new CommandConsumer(this, ref blacklist);
            com = new Thread(command.CommandThread);
            com.Name = "Command";
            com.Start();
        }

        public void addThread(string channel)
        {
            if (!channel.Equals(""))
            {
                Channels.Add(channel);
                int index = Channels.FindIndex(a => a.Equals(channel));
                ct.Add(new ClientThread(channel, 60, ref trigger, ref messageQueue, ref blacklist));
                pool.Add(new Thread(() => ct[index].CThread()));
                pool[Channels.FindIndex(a => a.Equals(channel))].Name = channel;
                pool[Channels.FindIndex(a => a.Equals(channel))].Start();
            }
            else
            {
                Console.WriteLine("Parrameter needed to exicute command Add-Channel");
            }
        }

        public void dropThread(string channel)
        {
            if (!channel.Equals(""))
            {
                try
                {
                    int index = Channels.FindIndex(a => a.Equals(channel));
                    ct[index].Kill();
                    pool.RemoveAt(index);
                    Channels.RemoveAt(index);
                    ct.RemoveAt(index);
                    Console.WriteLine(channel + " dropped");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                Console.WriteLine("Parameter needed to exicute Drop-Channel command");
            }
        }

        public void listThreads()
        {
            if (pool.Count > 0)
            {
                foreach(Thread thread in pool)
                {
                    Console.WriteLine(thread.Name + " " + thread.ThreadState);
                }
            }
            else
            {
                Console.WriteLine("No active channel threads");
            }

            Console.WriteLine("kafka producer" + pro.ThreadState);
            Console.WriteLine("command consumer" + com.ThreadState);
        }

        public int queueSize()
        {
            return messageQueue.Count;
        }

        public void exit()
        {
            while(Channels.Count > 0)
            {
                dropThread(Channels[0]);
            }
            pro.Priority = ThreadPriority.Highest;
            producer.KillAsync().Wait();
            command.Kill();
            System.Environment.Exit(0);
        }

        public void listBlacklist()
        {
            foreach(string name in blacklist)
            {
                Console.WriteLine("The blocked users are:");
                Console.WriteLine(name);
            }
        }
    }
}
