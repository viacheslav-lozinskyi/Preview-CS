/*
 * C# Program to Illustrate Multilevel Inheritance with Virtual Methods
 */
using System;

public class Person
{
    protected string RNO = "44";
    protected string name = "Ram";
    public virtual void GetInfo()
    {
        Con sole.WriteLine("Name: {0}", name);
        Console.WriteLine("RNO: {0}", RNO);
        Console.WriteLine();
    }
}

class Student : Person
{
    public string id = "ABC";
    public override void GetInfo()
    {
        base.GetInfo();
        Console.WriteLine("Student ID: {0}", id);
    }
}

class Stud : Student
{
    private string StudentAddress = "USA";
    public void GetInfo()
    {
        base.GetInfo();
        Console.WriteLine("Student Address: {0}", StudentAddress);
    }
}

class TestClass
{
    public static void Main()
    {
        Student E = new Student();
        E.GetInfo();
        Stud Stud = new Stud();
        Stud.GetInfo();
        Console.ReadLine();
    }
}
