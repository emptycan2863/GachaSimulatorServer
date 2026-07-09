public class IdLoginRequest {
    public string id { get; set; } = "";
}

public class UserInfo {
    public string id { get; set; } = "";
}

public class UserDbModel {
    public string id { get; set; } = "";
    public UserInfo user { get; set; } = new UserInfo();
}

public class LoginResponse {
    public bool success { get; set; }
    public string id { get; set; } = "";
    public UserInfo? user { get; set; }
}