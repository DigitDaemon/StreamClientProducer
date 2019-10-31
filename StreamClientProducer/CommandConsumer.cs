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
        HashSet<string> blacklist;

        public CommandConsumer(ThreadController controller, ref HashSet<string> blacklist)
        {
            this.blacklist = blacklist;
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
                        string command;
                        string parameter;
                        if (input.Contains(" "))
                        {
                            command = input.Substring(0, input.IndexOf(" ")).Trim();
                            parameter = input.Substring(input.IndexOf(" "), input.Length - command.Length).Trim();
                        }
                        else
                        {
                            command = input;
                            parameter = "";
                        }
                        Console.WriteLine("COMMAND----------> " + input);

                        if (command.Equals("Add-Channel"))
                        {
                            controller.addThread(parameter);
                        }
                        else if (command.Equals("Drop-Channel"))
                        {
                            controller.dropThread(parameter);
                        }
                        else if (command.Equals("SCPList-Channels"))
                        {
                            Console.WriteLine("The active threads are:");
                            controller.listThreads();
                        }
                        else if (command.Equals("SCPCount"))
                        {
                            Console.WriteLine("The message queue has " + controller.queueSize() + " messages in it right now");

                        }
                        else if (command.Equals("SCPExit") || command.Equals("System-Shutdown"))
                        {
                            controller.exit();
                            Active = false;
                            source.Cancel();
                        }
                        else if (command.Equals("Blacklist"))
                        {
                            blacklist.Add(parameter);
                        }
                        else if (command.Equals("Unblacklist"))
                        {
                            blacklist.Remove(parameter);
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
