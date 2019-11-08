using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;

namespace StreamClientProducer
{


    class TwitchProducer
    {
        static string server = "Localhost:9092";
        ConcurrentQueue<String> messageQueue;
        private bool Active;
        ProducerConfig config;

        public TwitchProducer(ref ConcurrentQueue<String> messageQueue)
        {
            this.messageQueue = messageQueue;
            config = new ProducerConfig { BootstrapServers = server };
            


        }

        public void ProducerThread()
        {
            Console.WriteLine("Producer Thread start");
            this.Active = true;
            int count = 0;
            var producer = new ProducerBuilder<string, string>(config).Build();

            while (this.Active)
            {
                if (!messageQueue.IsEmpty)
                {
                    string message;
                    messageQueue.TryDequeue(out message);
                    string key = message.Substring(0, message.IndexOf(" "));
                    if (message.Contains("!d"))
                    {
                        producer.ProduceAsync("twitch_command", new Message<string, string> { Key = key, Value = message }).Wait();
                    }
                    else
                    {
                        producer.ProduceAsync("twitch_message", new Message<string, string> { Key = key, Value = message }).Wait();
                    }
                }
                else
                {
                    Thread.Yield();
                }

                if (count > 60)
                {
                    Thread.Sleep(500);
                    count = 0;
                }

                count++;
            }
        }

            public async Task KillAsync()
            {
                while (messageQueue.Count > 0)
                {
                    Console.WriteLine(messageQueue.Count);
                }
                this.Active = false;

            }
        
    }
}
