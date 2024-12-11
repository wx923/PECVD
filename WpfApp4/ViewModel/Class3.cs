using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WpfApp4.ViewModel
{
    internal class Class3
    {
    }
    public class MainViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Person> Persons { get; set; }
        public IEnumerable<string> GenderOptions { get; } = new[] { "Male", "Female", "Other" };

        public MainViewModel()
        {
            Persons = new ObservableCollection<Person>
        {
            new Person { Name = "John", Age = 30, Gender = "Male" },
            new Person { Name = "Jane", Age = 25, Gender = "Female" }
        };
        }

        public void AddPerson()
        {
            Persons.Add(new Person());
        }

        public void RemovePerson(Person person)
        {
            Persons.Remove(person);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
    public class Person : INotifyPropertyChanged
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        private int age;
        public int Age
        {
            get { return age; }
            set { age = value; OnPropertyChanged(nameof(Age)); }
        }

        private string gender;
        public string Gender
        {
            get { return gender; }
            set { gender = value; OnPropertyChanged(nameof(Gender)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
