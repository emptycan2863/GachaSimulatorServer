public class UserInfo {
    public string id { get; set; } = "";
    public string name { get; set; } = "";
    public int gold { get; set; } = 0;
    public List<MailInfo> mail { get; set; } = new List<MailInfo>();
}

public class MailInfo {
    public int type { get; set; } = 0;

    public int reward { get; set; } = 0;
}
