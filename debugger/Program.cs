// See https://aka.ms/new-console-template for more information

string prompt = @"
   (  (  ( .  (   (    (  
  ()) )\ )\ . )\  )\   )\ 
 ((_))(_)(_) ((_)((_) ((_)
(/ __| \ / // _ \| _ \ __|
| (__ \   /| (_) |  _/__ \
 \___| \_/  \___/|_| |___/

Entering debugging loop to keep container running. Press Ctrl+C to exit.
";
Console.WriteLine(prompt);

bool isRunning = true;
int counter = 0;
int max = 80;
int delay = 5000;

Console.CancelKeyPress += delegate {
    Console.WriteLine();
    Console.WriteLine("Program cancellation requested. Exiting...");
    isRunning = false;
};

while (isRunning)
{
    Console.Write(".");
    if (counter == max)
    {
        Console.WriteLine();
        counter = 0;
    } else {
        counter++;
    }
    System.Threading.Thread.Sleep(delay);
}
