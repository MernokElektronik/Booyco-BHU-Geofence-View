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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Booyco_HMI_Utility
{
    /// <summary>
    /// Interaction logic for LoginView.xaml
    /// </summary>
    public partial class LoginView : UserControl
    {   
        string PasswordBooycoAccess = "Booyco" ;
        string PasswordBooycoAccess_NextMonth = "Booyco";
        string PasswordMernokAccess = "Mernok";
        string PasswordMernokAccess_NextMonth = "Mernok";
        string PasswordDataLogAccess = "Datalog";
        string PasswordDataLogAccess_NextMonth = "Datalog";

        public LoginView()
        {
            InitializeComponent();
            int CalculatedHash = KeyGen(DateTime.Now.Year.ToString(), (DateTime.Now.Month* 999999).ToString(), "Booyco");
            int CalculatedHash_NextMonth = KeyGen(DateTime.Now.Year.ToString(), ((DateTime.Now.Month+1) * 999999).ToString(), "Booyco");
            PasswordBooycoAccess += (CalculatedHash.ToString("X6").Substring(0,6));
            PasswordBooycoAccess_NextMonth += (CalculatedHash_NextMonth.ToString("X6").Substring(0, 6));
            CalculatedHash = KeyGen(DateTime.Now.Year.ToString(), (DateTime.Now.Month * 888888).ToString(), "Mernok");
            CalculatedHash_NextMonth = KeyGen(DateTime.Now.Year.ToString(), ((DateTime.Now.Month+1) * 888888).ToString(), "Mernok");
            PasswordMernokAccess += (CalculatedHash.ToString("X6").Substring(0, 6));
            PasswordMernokAccess_NextMonth += (CalculatedHash_NextMonth.ToString("X6").Substring(0, 6));

            CalculatedHash = KeyGen(DateTime.Now.Year.ToString(), (DateTime.Now.Month * 777777).ToString(), "Datalog");
            CalculatedHash_NextMonth = KeyGen(DateTime.Now.Year.ToString(), ((DateTime.Now.Month + 1) * 777777).ToString(), "Datalog");
            PasswordDataLogAccess += (CalculatedHash.ToString("X6").Substring(0, 6));
            PasswordDataLogAccess_NextMonth += (CalculatedHash_NextMonth.ToString("X6").Substring(0, 6));
        }

        /// <summary>
        /// KeyGen: Generate key with three string inputs
        /// String a: First input for key generation
        /// String b: Second input for key generation
        /// String b: Third input for key generation
        /// return int: hash key generated from inputs
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        int KeyGen(string a, string b, string c)
        {
            var bHash = b.GetHashCode();
            var aHash = a.GetHashCode();
            var cHash = c.GetHashCode();
            var hash = 1;
            unchecked
            {
                hash = hash * 10 + aHash;
                hash = hash * 10 + bHash;
                hash = hash * 10 + cHash;
            }
            return hash;
        }


        /// <summary>
        /// ButtonLogin_Click: Login Button click event
        /// Check password and login if correct
        /// or 
        /// Log out to basic access control
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {

            if (GlobalSharedData.AccessLevel == (int)AccessLevelEnum.Full || GlobalSharedData.AccessLevel == (int)AccessLevelEnum.Basic )
            {
                GlobalSharedData.AccessLevel = (int)AccessLevelEnum.Limited;
                ButtonLogin.Content = "Log In";
                Label_Error.Content = "";
                ButtonLogin.IsEnabled = false;
                PasswordBox_Login.IsEnabled = true;
                Label_AccessLevel.Content = "Access Level: None";
            }
            else if (PasswordBox_Login.Password == PasswordBooycoAccess)
            {
                GlobalSharedData.AccessLevel = (int)AccessLevelEnum.Basic;
                ButtonLogin.Content = "Log Out";
                Label_Error.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 150, 0));
               
                Label_Error.Content = "Successfully logged in as Booyco technician.";
                Label_AccessLevel.Content = "Access Level: Booyco Technician";                

                PasswordBox_Login.IsEnabled = false;
            }
            else if (PasswordBox_Login.Password == PasswordDataLogAccess)
            {
                GlobalSharedData.AccessLevel = (int)AccessLevelEnum.Extended;
                ButtonLogin.Content = "Log Out";
                Label_Error.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 150, 0));

                Label_Error.Content = "Successfully logged.";
                Label_AccessLevel.Content = "Access Level: Booyco Technician";

                PasswordBox_Login.IsEnabled = false;
            }
            else if (PasswordBox_Login.Password == PasswordMernokAccess)
            {
            
                GlobalSharedData.AccessLevel = (int)AccessLevelEnum.Full;
                ButtonLogin.Content = "Log Out";
                Label_Error.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 0, 150, 0));

                Label_Error.Content = "Successfully logged in as Mernok technician.";
                Label_AccessLevel.Content = "Access Level: Mernok Technician";               
               
                PasswordBox_Login.IsEnabled = false;
            }
            else
            {
                Label_Error.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromArgb(255, 130, 0, 0));
                Label_Error.Content = "Incorrect Password, please try again.";

            }
            PasswordBox_Login.Clear();          
        }

        /// <summary>
        /// ButtonBack_Click: back Button Click Event
        /// Close View and open source view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
         
        }

        /// <summary>
        /// ButtonPrevious_MouseEnter: Mouse enter on Previous Button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPrevious_MouseEnter(object sender, MouseEventArgs e)
        {
            RectangleArrowLeft.Fill = new SolidColorBrush(Color.FromRgb(60, 6, 6));
           // ImagePicture.Opacity = 1;
        }

        /// <summary>
        /// ButtonPrevious_MouseLeave: Mouse leave on Previous Button event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPrevious_MouseLeave(object sender, MouseEventArgs e)
        {
            RectangleArrowLeft.Fill = new SolidColorBrush(Color.FromRgb(140, 9, 9));
           // ImagePicture.Opacity = 0.6;
        }

        /// <summary>
        /// ButtonPrevious_Click: Previous Button Click Event
        /// Close View
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ButtonPrevious_Click(object sender, RoutedEventArgs e)
        {
            ProgramFlow.ProgramWindow = ProgramFlow.SourseWindow;
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TextBox_Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            PasswordBox _passwordbox = (PasswordBox)sender;
            if (_passwordbox.Password.Count() > 0 || GlobalSharedData.AccessLevel != (int)AccessLevelEnum.Basic || GlobalSharedData.AccessLevel != (int)AccessLevelEnum.Full)
            {
                ButtonLogin.IsEnabled = true;
            }
            else
            {
                ButtonLogin.IsEnabled = false;
            }
        }

        private void Grid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(this.IsVisible)
            {

            }
            else
            {
                PasswordBox_Login.Clear();
                Label_Error.Content = "";
            }
        }

        private void PasswordBox_Login_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                ButtonLogin_Click(null, null);
            }
        }
    }
}
