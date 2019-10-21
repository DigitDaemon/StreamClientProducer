using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using KafkaNet;
using KafkaNet.Model;
using KafkaNet.Protocol;

namespace StreamClientProducer
{
     

    class TwitchProducer
    {
        static Uri uri = new Uri("http://Localhost:9092");
        static KafkaOptions options = new KafkaOptions(uri);
        static BrokerRouter router = new BrokerRouter(options);
        ConcurrentQueue<String> messageQueue;
        Producer client;
        private bool Active; 
        

        public TwitchProducer(ref ConcurrentQueue<String> messageQueue)
        {
            this.messageQueue = messageQueue;
            client = new Producer(router);
        }

        public void pThread()
        {
            this.Active = true;
            int count = 0;
            while (this.Active)
            {
                if (!messageQueue.IsEmpty)
                {
                    string message;
                    messageQueue.TryDequeue(out message);
                    Message msg = new Message(message);
                    if (message.Contains("!d "))
                    {
                        client.SendMessageAsync("twitch_command", new List<Message> { msg }).Wait();
                    }
                    else
                    {
                        client.SendMessageAsync("twitch_message", new List<Message> { msg }).Wait();
                    }
                }
                else
                {
                    Thread.Yield();
                }

                if (count > 60)
                {
                    Thread.Sleep(1000);
                    count = 0;
                    client.Stop();
                    client = new Producer(router);
                }

                count++;
            }

            
        }

        public void Kill()
        {
            this.Active = false;
        }
    }
}
