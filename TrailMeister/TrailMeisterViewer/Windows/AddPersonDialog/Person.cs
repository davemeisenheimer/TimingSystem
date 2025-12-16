using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrailMeisterViewer.Windows.AddPersonDialog
{
    internal class Person
    {
        public Person(string firstName, string lastName, string association)
        {
            FirstName = firstName;
            LastName = lastName;
            Association = association;
        }


        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Association { get; set; }
    }
}
