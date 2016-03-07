using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace com.veeam.Compresser.Tests
{
    struct UserProfile
    {
        public readonly string name;
        public int age;

        public UserProfile(string name, int age)
        {
            this.name = name;
            this.age = age;
        }

        public override string ToString()
        {
            return string.Format("[ name: {0}, age: {1}]", name ?? "anonym", age);
        }
    }
}
