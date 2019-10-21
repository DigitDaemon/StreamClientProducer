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

        public ThreadController()
        {
            trigger = new System.Timers.Timer(100);
            trigger.AutoReset = true;
            trigger.Enabled = true;
            messageQueue = new ConcurrentQueue<String>();
            Channels = new List<string>();
            ThreadActiveBool = new List<bool>();
            ct = new List<ClientThread>();
            pool = new List<Thread>();
            producer = new TwitchProducer(ref messageQueue);
            Thread pro = new Thread(producer.pThread);
            pro.Name = "PRODUCER";
            pro.Start();
        }

        public void addThread(string channel)
        {
            Channels.Add(channel);
            int index = Channels.FindIndex(a => a.Equals(channel));
            ct.Add(new ClientThread(channel, 60, ref trigger, ref messageQueue));
            pool.Add(new Thread(() => ct[index].CThread()));
            pool[Channels.FindIndex(a => a.Equals(channel))].Name = channel;
            pool[Channels.FindIndex(a => a.Equals(channel))].Start();
        }

        public void dropThread(string channel)
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
                Console.WriteLine("No active threads");
            }
        }

        public void queueSize()
        {
            Console.WriteLine(messageQueue.Count);
        }

        public void exit()
        {
            while(Channels.Count > 0)
            {
                dropThread(Channels[0]);
            }
            producer.Kill();
        }
    }
}
