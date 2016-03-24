namespace TheDailyWtf.ViewModels
{
    public class CommentsLoginViewModel : HomeIndexViewModel
    {
        public CommentsLoginViewModel(string name, string token)
        {
            this.ShowLeaderboardAd = false;
            this.UserName = name;
            this.UserToken = token;
        }

        public string UserName { get; private set; }
        public string UserToken { get; private set; }
        public string TokenType { get { return UserToken.Split(':')[0]; } }
    }
}