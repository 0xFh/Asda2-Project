using System;

namespace WCell.Core.Database
{
    public class ConsoleStringMover : SingleStringMover
    {
        public override string Read()
        {
            return Console.In.ReadLine();
        }

        public override void Write(string s)
        {
            throw new Exception("Cannot write to ConsoleStringMover");
        }
    }
}