using System;
using System.Collections.Generic;

namespace WindowsRemoteManager
{
    class Command
    {
        public string ID;
        public string[] Instructions;

        public Command(string ID, string[] Instructions)
        {
            this.ID = ID; this.Instructions = Instructions;
        }
    }
}
