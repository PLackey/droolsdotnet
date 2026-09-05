using System;

namespace org.drools.dotnet.tests.Models
{
    [Serializable]
    public class Message
    {
        public static readonly int HELLO = 0;
        public static readonly int GOODBYE = 1;

        private string _message = string.Empty;
        private int _status;

        public string message
        {
            get { return _message; }
            set { _message = value; }
        }

        public int status
        {
            get { return _status; }
            set { _status = value; }
        }

        public override string ToString()
        {
            return $"Message[message={_message}, status={_status}]";
        }
    }

    [Serializable]
    public class TestNumber
    {
        private int _value;
        private string _category = string.Empty;

        public int value
        {
            get { return _value; }
            set { _value = value; }
        }

        public string category
        {
            get { return _category; }
            set { _category = value; }
        }

        public TestNumber() { }

        public TestNumber(int value)
        {
            this._value = value;
        }

        public override string ToString()
        {
            return $"TestNumber[value={_value}, category={_category}]";
        }
    }

    [Serializable]
    public class Person
    {
        private string _name = string.Empty;
        private int _age;
        private bool _adult;

        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        public int age
        {
            get { return _age; }
            set { _age = value; }
        }

        public bool adult
        {
            get { return _adult; }
            set { _adult = value; }
        }

        public Person() { }

        public Person(string name, int age)
        {
            this._name = name;
            this._age = age;
        }

        public override string ToString()
        {
            return $"Person[name={_name}, age={_age}, adult={_adult}]";
        }
    }
}