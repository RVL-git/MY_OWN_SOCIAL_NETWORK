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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MySql.Data.MySqlClient;
using System.Collections.ObjectModel;


namespace SocialNetwork
{
    public class FriendViewModel
    {
        public FriendViewModel(string name, string path, int id)
        {
            Name = name;
            Path = path;
            Id = id;
        }

        public int Id { get; set; }

        public string Name { get; set; }
        public string Path { get; set; }
    }
    public class MainFViewModel
    {
        public ObservableCollection<FriendViewModel> Images { get; set; } = new ObservableCollection<FriendViewModel>();

        public MainFViewModel(List<string> List)
        {
            foreach (string id in List)
            {
               
                MySqlConnection con = new MySqlConnection("server=localhost;port=3306;database=socialnetwork;user=root;password=root;charset=utf8");
                con.Open();
                MySqlCommand friends = new MySqlCommand("Select P.fname, P.lname, PH.path FROM profiles P, photos PH WHERE (P.user_id = " + id + " AND P.user_photo = PH.id )", con);
                MySqlDataReader friends_reader = friends.ExecuteReader();
                friends_reader.Read();
                Images.Add(new FriendViewModel(friends_reader["fname"].ToString() + " " + friends_reader["lname"].ToString(), friends_reader["path"].ToString(), int.Parse(id)));
                con.Close();
            }
        }
    }

    public class PostViewModel
    {
        public PostViewModel(string text, string title, int id)
        {
            Id = id;
            Title = title;
            Text = text;
        }

        public int Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }
    }
    public class MainPViewModel
    {
        public ObservableCollection<PostViewModel> Posts { get; set; } = new ObservableCollection<PostViewModel>();

        public MainPViewModel()
        {
            MySqlConnection con = new MySqlConnection("server=localhost;port=3306;database=socialnetwork;user=root;password=root;charset=utf8");
            con.Open();
            MySqlCommand posts = new MySqlCommand("Select P.id, P.title, P.content, P.published_at, PR.fname, PR.lname FROM posts P, profiles PR WHERE (PR.user_id = P.user_id ) ORDER BY P.published_at DESC", con);
            MySqlDataReader posts_reader = posts.ExecuteReader();
            while (posts_reader.Read())
                Posts.Add(new PostViewModel(posts_reader["content"].ToString(),  posts_reader["title"].ToString() + '\n' + posts_reader["fname"].ToString() +" "+ posts_reader["lname"].ToString()[0] +". " + posts_reader["published_at"].ToString().Substring(0, 5) +" "+ posts_reader["published_at"].ToString().Substring(10,5),  int.Parse(posts_reader["id"].ToString())));
            con.Close();
        }
    }

    public class DialogViewModel
    {
        public DialogViewModel(string name, string path, string lastmess, int id)
        {
            Name = name;
            Path = path;
            LastMessage = lastmess;
            Id = id;
        }

        public int Id { get; set; }

        public string Name { get; set; }
        public string Path { get; set; }
        public string LastMessage { get; set; }
    }
    public class MainDViewModel
    {
        public ObservableCollection<DialogViewModel> Dialogs { get; set; } = new ObservableCollection<DialogViewModel>();

        public MainDViewModel(List<string> List)
        {
            foreach (string id in List)
            {

                MySqlConnection con = new MySqlConnection("server=localhost;port=3306;database=socialnetwork;user=root;password=root;charset=utf8");
                con.Open();
                MySqlCommand dl = new MySqlCommand("Select D.name, PH.path, M.sender_id, M.text, M.sent_at FROM dialogs D, photos PH, messages M WHERE (D.id = " + id + " AND D.photo_id = PH.id AND M.dialog_id =" + id + ") ORDER BY M.sent_at DESC", con);
                MySqlDataReader dl_reader = dl.ExecuteReader();
                dl_reader.Read();
                string n = dl_reader["name"].ToString();
                string p = dl_reader["path"].ToString();
                string l = dl_reader["text"].ToString();

                int sender = int.Parse(dl_reader["sender_id"].ToString());
                con.Close();

                con.Open();
                MySqlCommand name = new MySqlCommand("SELECT fname, lname FROM profiles WHERE user_id =" + sender, con);
                MySqlDataReader name_reader = name.ExecuteReader();

                name_reader.Read();
                Dialogs.Add(new DialogViewModel(n, p, name_reader["fname"].ToString() + " " + name_reader["fname"].ToString()[0] + ". : " + l, int.Parse(id)));
                con.Close();
            }
        }
    }





    public partial class MainWindow : Window
    {

        int UserId;
        int ProfileId;
        bool admin;
        bool check;
        string Fname;
        string Lname;
        string Email;
        string Phone;
        Ellipse photo;
        Ellipse ProfilePhoto;
        MySqlConnection con = new MySqlConnection("server=localhost;port=3306;database=socialnetwork;user=root;password=root;charset=utf8");
        List <string> FriendsList = new List<string>();
        List<string> DialogsList = new List<string>();
        private MainFViewModel MainViewModelF;
        private MainDViewModel MainViewModelD;
        private MainPViewModel MainViewModelP;
        public MainWindow(int UserId, bool admin)
        {
           
            this.UserId = UserId;
            this.admin = admin; 
            InitializeComponent();
            ProfilePhoto = new Ellipse();
            messages_button.Visibility = Visibility.Hidden;
            posts_button.Visibility = Visibility.Hidden;
            friends_button.Visibility = Visibility.Hidden;

            //get profile info
            con.Open();
            MySqlCommand profile_info = new MySqlCommand("Select * FROM profiles WHERE profiles.user_id =" + UserId, con);
            MySqlDataReader info_reader = profile_info.ExecuteReader();
            info_reader.Read();
            int photo_id = int.Parse(info_reader["user_photo"].ToString());
            Fname = info_reader["fname"].ToString();
            Lname = info_reader["lname"].ToString();
            Email = info_reader["email"].ToString();
            Phone = info_reader["phone"].ToString();
            con.Close();
            //take user info into menu
            fname_label.Visibility = Visibility.Hidden;
            fname_label.Content = Fname + " " + Lname;
            phone_label.Visibility = Visibility.Hidden;
            phone_label.Content = "+ "+Phone;

            //get avatar 
            con.Open();
            MySqlCommand profile_photo = new MySqlCommand("Select path FROM photos WHERE photos.id =" + photo_id, con);
            MySqlDataReader photo_reader = profile_photo.ExecuteReader();
            photo_reader.Read();
            //take avatar into userphoto
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(photo_reader["path"].ToString(), UriKind.Relative);
            bi3.EndInit();
            photo = new Ellipse();
            photo.Fill = new ImageBrush(bi3);
            photo.Width = 80;
            photo.Height = 80;
            photo.Visibility = Visibility.Hidden;
            Canvas.SetLeft(photo, 35);
            Canvas.SetTop(photo, 10);
            Field.Children.Add(photo);
            
            //event for menu 
            visible.Completed += new EventHandler(visible_loaded);
            hidden.Completed += new EventHandler(hidden_loaded);
            con.Close();
        }
        private void visible_loaded(object sender, EventArgs e)
        {
            photo.Visibility = Visibility.Visible;
            fname_label.Visibility = Visibility.Visible;
            phone_label.Visibility = Visibility.Visible;
            messages_button.Visibility = Visibility.Visible;
            posts_button.Visibility = Visibility.Visible;
            friends_button.Visibility = Visibility.Visible;
        }
        private void hidden_loaded(object sender, EventArgs e)
        {
            photo.Visibility = Visibility.Hidden;
            fname_label.Visibility = Visibility.Hidden;
            phone_label.Visibility = Visibility.Hidden;
            messages_button.Visibility = Visibility.Hidden;
            posts_button.Visibility = Visibility.Hidden;
            friends_button.Visibility = Visibility.Hidden;
        }


        private void posts_click(object sender, RoutedEventArgs e)
        {
            posts_list.Visibility = Visibility.Visible;
            friends_list.Visibility = Visibility.Hidden;
            dialogs_list.Visibility = Visibility.Hidden;
            Dialog.Visibility = Visibility.Hidden;
            search.Visibility = Visibility.Hidden;
            searchbutton.Visibility = Visibility.Hidden;
            Profile.Children.Clear();
            DialogPanel.Children.Clear();
            ComPanel.Children.Clear();

            Post.Visibility = Visibility.Hidden;
            MainViewModelP = new MainPViewModel();
            posts_list.DataContext = MainViewModelP;

        }

        private void posts_list_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            FriendsList = new List<string>();
            friends_list.Visibility = Visibility.Hidden;
            search.Visibility = Visibility.Visible;
            searchbutton.Visibility = Visibility.Visible;

            dialogs_list.Visibility = Visibility.Hidden;
            posts_list.Visibility = Visibility.Hidden;
            Dialog.Visibility = Visibility.Hidden;
            Profile.Children.Clear();
            ComPanel.Children.Clear();

            Post.Visibility = Visibility.Visible;

            PostViewModel p = (PostViewModel)posts_list.SelectedItem;
            txt.Text = p.Text;
            title.Content = p.Title;
            con.Open();
            MySqlCommand post = new MySqlCommand("Select COUNT(user_id) AS cnt FROM posts_favorites WHERE post_id =" + p.Id, con);
            MySqlDataReader post_reader = post.ExecuteReader();
            post_reader.Read();
            likecount.Content = post_reader["cnt"].ToString() + "likes";
            con.Close();
            List<string> ComList = new List<string>();
            ComPanel.Children.Clear();

            con.Open();
            MySqlCommand c = new MySqlCommand("SELECT P.comment_text, P.commented_at, PR.fname, PR.lname FROM posts_comments P, profiles PR WHERE post_id =" + p.Id, con);
            MySqlDataReader c_reader = c.ExecuteReader();
            while (c_reader.Read())
            {
                var lab = new Label();
                lab.Content = c_reader["fname"].ToString() + c_reader["lname"].ToString()[0] + "." + '\n' + c_reader["comment_text"].ToString() + '\n' + (c_reader["commented_at"].ToString()).Substring(0,10);
                ComPanel.Children.Add(lab);
            }
            con.Close();
        }

        private void friends_click(object sender, RoutedEventArgs e)
        {

            FriendsList = new List<string>();
            friends_list.Visibility = Visibility.Visible;
            search.Visibility = Visibility.Visible;
            searchbutton.Visibility = Visibility.Visible;

            Post.Visibility = Visibility.Hidden;
            dialogs_list.Visibility = Visibility.Hidden;
            posts_list.Visibility = Visibility.Hidden;
            Dialog.Visibility = Visibility.Hidden;
            Profile.Children.Clear();
            DialogPanel.Children.Clear();
            ComPanel.Children.Clear();

            con.Open();
            MySqlCommand friends1 = new MySqlCommand("Select user1_id FROM users_relations WHERE users_relations.user2_id =" + UserId, con);
            MySqlDataReader friends1_reader = friends1.ExecuteReader();
            while (friends1_reader.Read())
                FriendsList.Add(friends1_reader["user1_id"].ToString());
            con.Close();
            con.Open();
            MySqlCommand friends2 = new MySqlCommand("Select user2_id FROM users_relations WHERE users_relations.user1_id =" + UserId, con);
            MySqlDataReader friends2_reader = friends2.ExecuteReader();
            while (friends2_reader.Read())
                FriendsList.Add(friends2_reader["user2_id"].ToString());
            con.Close();
            MainViewModelF = new MainFViewModel(FriendsList);
            friends_list.DataContext = MainViewModelF;
        }

        private void Searchbutton_Click(object sender, RoutedEventArgs e)
        {
            FriendsList = new List<string>();
            string []searchitem = search.Text.Split(' ');
            if (searchitem.Length == 1)
            {
                con.Open();
                MySqlCommand friends1 = new MySqlCommand("Select id FROM users U, profiles P WHERE (U.id = P.user_id) AND ((P.fname LIKE '" + searchitem[0] + "%') OR" + "( P.lname LIKE '" + searchitem[0] + "%'))", con);
                MySqlDataReader friends1_reader = friends1.ExecuteReader();
                while (friends1_reader.Read())
                    if(int.Parse(friends1_reader["id"].ToString()) != UserId)
                        FriendsList.Add(friends1_reader["id"].ToString());
                
                con.Close();
            }
            else if (searchitem.Length == 2)
            {
                con.Open();
                MySqlCommand friends2 = new MySqlCommand("Select id FROM users U, profiles P WHERE (U.id = P.user_id) AND ((P.fname LIKE '" + searchitem[0] + "%') OR" + "( P.lname LIKE '" + searchitem[1] + "%'))", con);
                MySqlDataReader friends2_reader = friends2.ExecuteReader();
                while (friends2_reader.Read())
                    if (int.Parse(friends2_reader["id"].ToString()) != UserId)
                        FriendsList.Add(friends2_reader["user2_id"].ToString());
                
                con.Close();
                MainViewModelF = new MainFViewModel(FriendsList);
                friends_list.DataContext = MainViewModelF;
            }
            MainViewModelF = new MainFViewModel(FriendsList);
            friends_list.DataContext = MainViewModelF;
        }
        private void friends_list_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            friends_list.Visibility = Visibility.Hidden;
            Profile.Visibility = Visibility.Visible;
            search.Visibility = Visibility.Hidden;
            searchbutton.Visibility = Visibility.Hidden;

            //ProfilePhoto.Visibility = Visibility.Visible;
            FriendViewModel user = (FriendViewModel)friends_list.SelectedItem;
            Fill_Profile(user.Id);
            ProfileId = user.Id;

        }
        private void messages_click(object sender, RoutedEventArgs e)
        {
            DialogsList = new List<string>();
            DialogPanel.Children.Clear();
            ComPanel.Children.Clear();

            Profile.Children.Clear();
            dialogs_list.Visibility = Visibility.Visible;
            friends_list.Visibility = Visibility.Hidden;
            posts_list.Visibility = Visibility.Hidden;
            search.Visibility = Visibility.Hidden;
            searchbutton.Visibility = Visibility.Hidden;
            Post.Visibility = Visibility.Hidden;
            con.Open();
            MySqlCommand dialogs = new MySqlCommand("Select dialog_id FROM participants WHERE participants.user_id =" + UserId, con);
            MySqlDataReader dialogs_reader = dialogs.ExecuteReader();
            while (dialogs_reader.Read())
                DialogsList.Add(dialogs_reader["dialog_id"].ToString());
            con.Close();
            MainViewModelD = new MainDViewModel(DialogsList);
            dialogs_list.DataContext = MainViewModelD;


        }
        private void dialogs_list_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {

            dialogs_list.Visibility = Visibility.Hidden;
            Dialog.Visibility = Visibility.Visible;
            DialogPanel.Children.Clear();
            con.Open();
            DialogViewModel Id = (DialogViewModel)dialogs_list.SelectedItem;

            MySqlCommand messages = new MySqlCommand("Select M.text, M.sent_at, P.fname, P.lname, M.sender_id FROM messages M, profiles P WHERE (M.dialog_id =" + Id.Id + " AND P.user_id = M.sender_id) ORDER BY M.sent_at", con);
            MySqlDataReader messages_reader = messages.ExecuteReader();
            while (messages_reader.Read())
            {
                var message = new ContentControl();
                //DateTime time = (DateTime)messages_reader["sent_at"];
                if (int.Parse(messages_reader["sender_id"].ToString()) != UserId)
                {
                    message.Style = (Style)this.Resources["BubbleLeftStyle"];
                    message.Content = messages_reader["fname"].ToString() + " " + messages_reader["lname"].ToString()[0] + "." + '\n' + messages_reader["text"] + '\n' + (messages_reader["sent_at"].ToString()).Substring(10,5);

                }
                else
                {
                    message.Style = (Style)this.Resources["BubbleRightStyle"];
                    message.Content = messages_reader["text"].ToString() + '\n' + (messages_reader["sent_at"].ToString()).Substring(10, 5);
                }
                DialogPanel.Children.Add(message);
                //MessageBox.Show(messages_reader["fname"].ToString() + " " + messages_reader["lname"].ToString()[0] + "." + '\n' + messages_reader["text"].ToString() + '\n' + messages_reader["sent_at"].ToString());
            }
            Dialog.Visibility = Visibility.Visible;
            friends_list.Visibility = Visibility.Hidden;
            Profile.Visibility = Visibility.Hidden;
            dialogs_list.Visibility = Visibility.Hidden;
            con.Close();
        }
        private void SendMessage_Click(object sender, RoutedEventArgs e)
        {
            var message = new ContentControl();
            message.Content = TBMessage.Text + '\n' + DateTime.Now.ToShortTimeString();
            message.Style = (Style)this.Resources["BubbleRightStyle"];
            DialogPanel.Children.Add(message);
        }



        public void Fill_Profile(int id)
        {
            con.Open();
            Profile.Children.Clear();
            MySqlCommand profile_info = new MySqlCommand("Select * FROM profiles WHERE profiles.user_id =" + id, con);
            MySqlDataReader info_reader = profile_info.ExecuteReader();
            info_reader.Read();
            int photo_id = int.Parse(info_reader["user_photo"].ToString());

            Label PFname = new Label();
            PFname.Content = info_reader["fname"].ToString();
            Canvas.SetLeft(PFname, Width/2 + 20);
            Canvas.SetTop(PFname, Height/2 - 130);
            Profile.Children.Add(PFname);

            Label PLname = new Label();
            PLname.Content = info_reader["lname"].ToString();
            Canvas.SetLeft(PLname, Width / 2 + 20);
            Canvas.SetTop(PLname, Height / 2 - 110);
            Profile.Children.Add(PLname);

            Label PPhone = new Label();
            PPhone.Content = "+ " + info_reader["phone"].ToString();
            Canvas.SetLeft(PPhone, Width / 2 + 20);
            Canvas.SetTop(PPhone, Height / 2  - 90);
            Profile.Children.Add(PPhone);

            Label PEmail = new Label();
            PEmail.Content = info_reader["email"].ToString();
            Canvas.SetLeft(PEmail, Width / 2 + 20);
            Canvas.SetTop(PEmail, Height / 2 - 70);
            Profile.Children.Add(PEmail);

            con.Close();

            Button AddFriend = new Button();
            check = false;
            con.Open();
            MySqlCommand friends1 = new MySqlCommand("Select user1_id FROM users_relations WHERE (user1_id = " + id + " AND user2_id =" + UserId + " )", con);
            MySqlDataReader friends1_reader = friends1.ExecuteReader();
            if (friends1_reader.Read())
                check = true;
            con.Close();
            con.Open();
            MySqlCommand friends2 = new MySqlCommand("Select user2_id FROM users_relations WHERE (user2_id = " + id + " AND user1_id =" + UserId + " )", con);
            MySqlDataReader friends2_reader = friends2.ExecuteReader();
            if (friends2_reader.Read())
                check = true;
            con.Close();

            if (check)
                AddFriend.Content = "unfriend";
            else
                AddFriend.Content = "add friend";

            AddFriend.Width = 150;
            AddFriend.Height = 30;
            AddFriend.Click += AddFriendClick;
            Canvas.SetLeft(AddFriend,Width / 2 + 20);
            Canvas.SetTop(AddFriend, Height / 2 - 30);
            AddFriend.Background = Brushes.White;
            Profile.Children.Add(AddFriend);

            //get avatar 
            con.Open();
            MySqlCommand profile_photo = new MySqlCommand("Select path FROM photos WHERE photos.id =" + photo_id, con);
            MySqlDataReader photo_reader = profile_photo.ExecuteReader();
            photo_reader.Read();
            //take avatar into userphoto
            BitmapImage bi3 = new BitmapImage();
            bi3.BeginInit();
            bi3.UriSource = new Uri(photo_reader["path"].ToString(), UriKind.Relative);
            bi3.EndInit();
            bi3.Freeze();
            ProfilePhoto.Fill = new ImageBrush(bi3);
            ProfilePhoto.Width = Height/2 - 40;
            ProfilePhoto.Height = Height / 2 - 40;
            
            Canvas.SetLeft(ProfilePhoto, Width/2 - ProfilePhoto.Width);
            Canvas.SetTop(ProfilePhoto, Height/2 - ProfilePhoto.Height/2 - 50);
            Profile.Children.Add(ProfilePhoto);
            con.Close();
        }
        private void AddFriendClick(object sender, RoutedEventArgs e)
        {

            if (check)
            { 
                con.Open();
                MySqlCommand friends = new MySqlCommand("DELETE FROM users_relations WHERE (user1_id = " + ProfileId + " AND user2_id =" + UserId + " ) OR ( user1_id = " + UserId + " AND user2_id = " + ProfileId + ")", con);
                friends.ExecuteNonQuery();
                con.Close();

                Fill_Profile(ProfileId);
                return;
            }
            if (!check)
            {
                con.Open();
                MySqlCommand friends = new MySqlCommand("INSERT INTO users_relations (user1_id, user2_id) VALUES('" +UserId + "','" + ProfileId +"')", con);
                friends.ExecuteNonQuery();
                con.Close();

                Fill_Profile(ProfileId);
                return;
            }
        }

    }
}
