using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;


namespace StreamClientProducer
{
    class CommandConsumer
    {
        static string server = "Localhost:9092";
        private bool Active;
        string topic = "COMMANDS";
        ThreadController controller;
        ConsumerConfig config;
        CancellationToken canceltoken;
        CancellationTokenSource source;

        public CommandConsumer(ThreadController controller)
        {
            this.controller = controller;
            config = new ConsumerConfig
            {
                BootstrapServers = server,
                GroupId = Guid.NewGuid().ToString(),
                EnableAutoCommit = true,
                StatisticsIntervalMs = 5000,
                SessionTimeoutMs = 6000,
                AutoOffsetReset = AutoOffsetReset.Latest,
                EnablePartitionEof = true

            };
            source = new CancellationTokenSource();
            canceltoken = source.Token;
        }

        public void CommandThread()
        {
            Console.WriteLine("CommandThread Start");
            Active = true;
            var consumer = new ConsumerBuilder<string, string>(config).Build();
            var topicp = new TopicPartition(topic, 0);
            consumer.Assign(topicp);

            while (Active)
            {
                try
                {
                    var consumeresult = consumer.Consume(canceltoken);

                    if (!consumeresult.IsPartitionEOF)
                    {
                        var input = consumeresult.Value;

                        Console.WriteLine("COMMAND----------> " + input);
                        if (input.Equals("Add Thread"))
                        {
                            Console.Write("Name of the channel: ");
                            controller.addThread(Console.ReadLine());
                        }
                        else if (input.Equals("Drop Thread"))
                        {
                            Console.Write("Name of the channel: ");
                            controller.dropThread(Console.ReadLine());
                        }
                        else if (input.Equals("List Threads"))
                        {
                            Console.WriteLine("The active threads are:");
                            controller.listThreads();
                        }
                        else if (input.Equals("Count"))
                        {
                            controller.queueSize();
                        }
                        else if (input.Equals("Exit"))
                        {
                            controller.exit();
                            Active = false;
                            source.Cancel();
                        }
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
                catch (System.OperationCanceledException e)
                {

                }
     
            }
        }
    
        public void Kill()
        {
            source.Cancel();
            Active = false;
        }

    }

}
