using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;

namespace SocialNetwork
{

    public partial class LogWindow : Window
    {
        MySqlConnection con = new MySqlConnection("server=localhost;port=3306;database=socialnetwork;user=root;password=root;charset=utf8");
        int UserId;
        bool admin;
        public LogWindow()
        {
            InitializeComponent();
        }
        public string getlogin
        {
            get { return UserName.Text; }
        }

        public string getpass
        {
            get { return Pass.Password; }
        }
        private void LogClick(object sender, RoutedEventArgs e)
        {
            try
            {
                con.Open();
            }
            catch
            {
                MessageBox.Show("server is not available");
            }
            MySqlCommand check = new MySqlCommand("Select * FROM users WHERE users.login = @username AND users.pass = @password", con);
            check.Parameters.AddWithValue("@username", getlogin);
            check.Parameters.AddWithValue("@password", getpass);
            MySqlDataReader reader = check.ExecuteReader();
            if (reader.HasRows)
            {
                reader.Read();
                UserId = int.Parse(reader["id"].ToString());
                admin = bool.Parse(reader["admin"].ToString());
                con.Close();
                con.Open();
                Field.Children.Clear();

                MySqlCommand name = new MySqlCommand("Select fname FROM profiles WHERE profiles.user_id =" + UserId, con);
                MySqlDataReader name_reader = name.ExecuteReader();
                name_reader.Read();
               

                TextBlock welcome = new TextBlock();
                welcome.Text = "Welcome back, " + name_reader["fname"] +"!";
                welcome.FontSize = 30;
                Canvas.SetLeft(welcome, Width / 2 - 170);
                Canvas.SetTop(welcome, Height / 2 - 40);

                DoubleAnimation myDoubleAnimation = new DoubleAnimation();
                myDoubleAnimation.From = 0;
                myDoubleAnimation.To = 400;
                myDoubleAnimation.Duration = new Duration(TimeSpan.FromSeconds(1));
                myDoubleAnimation.AutoReverse = true;
                Field.Children.Add(welcome);
                myDoubleAnimation.Completed += new EventHandler(Completed);
                welcome.BeginAnimation(TextBox.WidthProperty, myDoubleAnimation);
                con.Close();

            }
            else
            {
                MessageBox.Show("wrong password or username");
                con.Close();
            }
        }
        private void Completed(object sender, EventArgs e)
        {
            MainWindow win = new MainWindow(UserId,admin);
            win.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
            win.ShowDialog();
            this.Close();
        }
    }
}
