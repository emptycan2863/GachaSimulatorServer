public class IdLoginRequest {
    public string id { get; set; } = "";
}

public class IdLoginResponse {
    public bool success { get; set; }
    public string message { get; set; } = "";
    public UserInfo? user { get; set; }
}

public class UserInfo {
    public string id { get; set; } = "";
    public string name { get; set; } = "";
}