/* This program is just using some basic code*/

using System;

class Program {
  public static void Main (string[] args) {

    // Print name
    Console.WriteLine ("Paxton");
    Console.WriteLine ("Proctor\n");

    // Square root
    double X = 12345;
    Console.WriteLine (Math.Sqrt(X));

    // Write a 100 alternating members example: -1, 2, -3, ...
    for(int i = 0; i < 100; i++){
      Console.WriteLine ("-" + (i+=1));
      Console.WriteLine (i+1);
    }

    // DateTime
    DateTime now = DateTime.Now;
    Console.WriteLine (now);
    Console.ReadLine();
    
    // Write age + 10
    Console.WriteLine ("Enter your age");
    string age = Console.ReadLine();
    Console.WriteLine (int.Parse(age) + 10);
    
    // Write a Program that compares two real numbers with precision of .000001
    Console.WriteLine ("First Number: ");
    String A = Console.ReadLine();
    Console.WriteLine ("Second Number: ");
    String B = Console.ReadLine();
    bool equal = Math.Abs(double.Parse(A) - double.Parse(B)) < 0.000001;
    Console.WriteLine (equal);
  }
}
