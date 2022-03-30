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
  }
}
