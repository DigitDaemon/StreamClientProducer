using System;

namespace StreamClientProducer
{
    class Program
    {
        static ThreadController controller;
        static void Main(string[] args)
        {
            
            bool active = true;
            controller = new ThreadController();
            while (active)
            {
                var input = Console.ReadLine();
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

                if (command.Equals("Add-Channel")){
                    controller.addThread(parameter);
                }
                else if (command.Equals("Drop-Channel"))
                {
                    controller.dropThread(parameter);
                }
                else if (command.Equals("List-Channels"))
                {
                    Console.WriteLine("The active threads are:");
                    controller.listThreads();
                }
                else if (command.Equals("Count"))
                {
                    Console.WriteLine("The message queue has " + controller.queueSize() + " messages in it right now");
                    
                }
                else if (command.Equals("Exit"))
                {
                    controller.exit();
                    active = false;
                }
                else if (command.Equals("Blacklist"))
                {
                    controller.listBlacklist();
                }
            }

        }

    }
}
