namespace DTO{

    public class User{
        public int Id{get;set;}
        public string? NickName {get;set;}
        public string? Password {get;set;}
    }
    public class UserChangeRequest{
        public string? Password{get;set;}
        public string? NewNickname{get;set;}
    }

}