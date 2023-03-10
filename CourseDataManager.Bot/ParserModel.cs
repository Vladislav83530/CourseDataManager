using CourseDataManager.Bot.Models;

namespace CourseDataManager.Bot
{
    internal class ParserModel
    {
        public UserLogin ParseForLogin(string message)
        {
            var logPass = message.Split(" ");
            var user = new UserLogin
            {
                Email = logPass[1].Trim(),
                Password = logPass[3].Trim()
            };
            return user;
        }

        public UserRegister ParseForRegister(string message)
        {
            var registerInfo = message.Split(":");
            var user = new UserRegister
            {
                UserName = registerInfo[1].Trim().Split("\n")[0],
                UserSurname = registerInfo[2].Trim().Split("\n")[0],
                Email = registerInfo[3].Trim().Split("\n")[0],
                Password = registerInfo[4].Trim().Split("\n")[0]
            };
            return user;
        }

        public Link ParseLink(string message)
        {
            var linkInfo = message.Split(":");
            var link = new Link
            {
                Name = linkInfo[1].Trim().Split("\n")[0],
                Link_ = linkInfo[3].Trim().Split("\n")[0],
            };
            return link;
        }
    }
}
