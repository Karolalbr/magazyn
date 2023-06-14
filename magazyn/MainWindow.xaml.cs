using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace SteelInventory
{
    public partial class MainWindow : Window
    {
        private SteelContext dbContext;
        private List<Steel> steelList;

        public MainWindow()
        {
            InitializeComponent();

            dbContext = new SteelContext();
            steelList = dbContext.Steels.ToList();

            SteelListView.ItemsSource = steelList;
            DisplayTotalWeight();
            DisplayTotalLength();
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(CodeTextBox.Text) ||
                string.IsNullOrWhiteSpace(WeightTextBox.Text) ||
                string.IsNullOrWhiteSpace(LengthTextBox.Text) ||
                string.IsNullOrWhiteSpace(ThicknessTextBox.Text))
            {
                MessageBox.Show("Wszystkie pola muszą być wypełnione.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            double weight, length, thickness;
            if (!double.TryParse(WeightTextBox.Text, out weight) ||
                !double.TryParse(LengthTextBox.Text, out length) ||
                !double.TryParse(ThicknessTextBox.Text, out thickness))
            {
                MessageBox.Show("Wprowadzono nieprawidłowy format danych.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var existingSteel = steelList.FirstOrDefault(s => s.Code == CodeTextBox.Text);
            if (existingSteel != null)
            {
                if (existingSteel.Thickness != (decimal)thickness)
                {
                    MessageBox.Show("Błąd: Grubość nie zgadza się z istniejącym kodem.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                existingSteel.Weight += (decimal)weight;
                existingSteel.Length += (decimal)length;
            }
            else
            {
                Steel steel = new Steel()
                {
                    Name = string.IsNullOrWhiteSpace(NameTextBox.Text) ? null : NameTextBox.Text,
                    Code = CodeTextBox.Text,
                    Weight = (decimal)weight,
                    Length = (decimal)length,
                    Thickness = (decimal)thickness,
                };

                dbContext.Steels.Add(steel);
            }

            dbContext.SaveChanges();

            steelList = dbContext.Steels.ToList();
            SteelListView.ItemsSource = null;
            SteelListView.ItemsSource = steelList;

            NameTextBox.Clear();
            CodeTextBox.Clear();
            WeightTextBox.Clear();
            LengthTextBox.Clear();
            ThicknessTextBox.Clear();

            DisplayTotalWeight();
            DisplayTotalLength();
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (SteelListView.SelectedItem == null)
            {
                MessageBox.Show("Nie wybrano żadnego elementu do usunięcia.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Steel selectedSteel = (Steel)SteelListView.SelectedItem;

            dbContext.Steels.Remove(selectedSteel);
            dbContext.SaveChanges();

            steelList = dbContext.Steels.ToList();
            SteelListView.ItemsSource = null;
            SteelListView.ItemsSource = steelList;

            DisplayTotalWeight();
            DisplayTotalLength();
        }

        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (SteelListView.SelectedItem == null)
            {
                MessageBox.Show("Nie wybrano żadnego elementu do edycji.", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            Steel selectedSteel = (Steel)SteelListView.SelectedItem;

            NameTextBox.Text = selectedSteel.Name;
            CodeTextBox.Text = selectedSteel.Code;
            WeightTextBox.Text = selectedSteel.WeightAsDouble.ToString();
            LengthTextBox.Text = selectedSteel.LengthAsDouble.ToString();
            ThicknessTextBox.Text = selectedSteel.ThicknessAsDouble.ToString();

            dbContext.Steels.Remove(selectedSteel);
            dbContext.SaveChanges();

            steelList = dbContext.Steels.ToList();
            SteelListView.ItemsSource = null;
            SteelListView.ItemsSource = steelList;
        }

        private void DisplayTotalWeight()
        {
            double totalWeight = steelList.Sum(s => s.WeightAsDouble);
            TotalWeightLabel.Text = "Łączna waga: " + totalWeight.ToString() + "t ";
        }

        private void DisplayTotalLength()
        {
            double totalLength = steelList.Sum(s => s.LengthAsDouble);
            TotalLengthLabel.Text = "Suma długości: " + totalLength.ToString() + "m ";
        }
    }

    public class Steel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public decimal Weight { get; set; }
        public decimal Length { get; set; } 
        public decimal Thickness { get; set; } 

        
        public double WeightAsDouble
        {
            get { return Convert.ToDouble(Weight); }
        }

        public double LengthAsDouble
        {
            get { return Convert.ToDouble(Length); }
        }

        public double ThicknessAsDouble
        {
            get { return Convert.ToDouble(Thickness); }
        }
    }

    public class SteelContext : DbContext
    {
        private string GetDatabaseFilePath()
        {
            string databaseFileName = "magazyn.mdf";
            string currentDirectory = Directory.GetCurrentDirectory();
            string projectDirectory = Directory.GetParent(Directory.GetParent(Directory.GetParent(currentDirectory).FullName).FullName).FullName;
            string databaseFilePath = Path.Combine(projectDirectory, databaseFileName);

            return databaseFilePath;
        }
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {

            string connectionString = ($"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={GetDatabaseFilePath()};Integrated Security=True;Connect Timeout=30");

            optionsBuilder.UseSqlServer(connectionString);
        }

        public DbSet<Steel> Steels { get; set; }
    }


}
