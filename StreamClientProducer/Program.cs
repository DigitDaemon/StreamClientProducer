using System;

namespace StreamClientProducer
{
    class Program
    {
        static ThreadController controller;
        static void Main(string[] args)
        {
            controller = new ThreadController();
            bool active = true;
            while (active)
            {
                string input = Console.ReadLine();

                if (input.Equals("Add Thread")){
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
                    active = false;
                }
            }

        }
    }
}
