// A simple C# program for traversal of a linked list
using System;
 
public class LinkedList {
    Node head; // head of list
 
    /* Linked list Node. This inner
    class is made static so that
    main() can access it */
    public class Node {
        public int data;
        public Node next;
        public Node(int d)
        {
            data = d;
            next = null;
 
        } // Constructor
    }
 
    /* This function prints contents of
    linked list starting from head */
    public void printList()
    {
        Node n = head;
        while (n != null) {
            Console.Write(n.data + " ");
            n = n.next;
        }
    }
 
    /* method to create a simple linked list with 3 nodes*/
    public static void Main(String[] args)
    {
        /* Start with the empty list. */
        LinkedList llist = new LinkedList();
 
        llist.head = new Node(1);
        Node second = new Node(2);
        Node third = new Node(3);
 
        llist.head.next = second; // Link first node with the second node
        second.next = third; // Link second node with the third node
 
        llist.printList();
    }
}
